using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;

namespace LastZone.Gameplay.Zone
{
    /// Scene-placed NetworkObject (NetworkObject component is REQUIRED). Server simulates, clients read NetworkVariables.
    public class ZoneController : NetworkBehaviour
    {
        public ZoneSchedule schedule;
        public Transform zoneCenter;      // optional visual marker, follows the synced center
        public float mapRadius = 750f;
        public bool autoStart = false;    // GameManager calls StartZone() when the match starts

        readonly NetworkVariable<float> radius = new NetworkVariable<float>(0f);
        readonly NetworkVariable<Vector3> center = new NetworkVariable<Vector3>(Vector3.zero);
        readonly NetworkVariable<int> stage = new NetworkVariable<int>(0);
        readonly NetworkVariable<float> timer = new NetworkVariable<float>(0f);
        readonly NetworkVariable<bool> shrinking = new NetworkVariable<bool>(false);

        public float currentRadius => radius.Value;
        public Vector3 currentCenter => center.Value;
        public int currentStageIndex => stage.Value;
        public float stageTimer => timer.Value;
        public bool isShrinking => shrinking.Value;

        bool running, finished;
        float remaining, startRadius, endRadius;
        Vector3 startCenter, endCenter;

        bool HasSchedule => schedule != null && schedule.stages != null && schedule.stages.Length > 0;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            radius.Value = mapRadius;
            center.Value = zoneCenter != null ? zoneCenter.position : transform.position;
            stage.Value = 0;
            shrinking.Value = false;
            if (HasSchedule)
            {
                remaining = schedule.stages[0].waitTime;
                timer.Value = remaining;
            }
            running = autoStart;
        }

        public void StartZone()
        {
            if (IsServer && HasSchedule) running = true;
        }

        void Update()
        {
            if (zoneCenter != null) zoneCenter.position = center.Value;
            if (!IsServer || !IsSpawned || !running || !HasSchedule) return;

            if (!finished) Tick(Time.deltaTime);
            ApplyZoneDamage(Time.deltaTime);
        }

        void Tick(float dt)
        {
            remaining -= dt;
            var s = schedule.stages[stage.Value];

            if (!shrinking.Value)
            {
                if (remaining <= 0f) BeginShrink(s);
            }
            else
            {
                float dur = Mathf.Max(0.01f, s.shrinkTime);
                float t = Mathf.Clamp01(1f - remaining / dur);
                radius.Value = Mathf.Lerp(startRadius, endRadius, t);
                center.Value = Vector3.Lerp(startCenter, endCenter, t);
                if (remaining <= 0f) EndShrink();
            }
            timer.Value = Mathf.Max(0f, remaining);
        }

        void BeginShrink(ZoneSchedule.Stage s)
        {
            shrinking.Value = true;
            remaining = s.shrinkTime;
            startRadius = radius.Value;
            endRadius = mapRadius * s.endRadiusPercent;
            startCenter = center.Value;

            // New circle always fits inside the old one
            float maxOffset = Mathf.Max(0f, startRadius - endRadius);
            Vector2 r = Random.insideUnitCircle * maxOffset;
            endCenter = startCenter + new Vector3(r.x, 0f, r.y);
        }

        void EndShrink()
        {
            radius.Value = endRadius;
            center.Value = endCenter;
            shrinking.Value = false;

            if (stage.Value + 1 >= schedule.stages.Length)
            {
                finished = true; // keep last stage's damage active
                return;
            }
            stage.Value++;
            remaining = schedule.stages[stage.Value].waitTime;
        }

        void ApplyZoneDamage(float dt)
        {
            float dps = schedule.stages[stage.Value].damagePerSecond;
            Vector3 c = center.Value;
            foreach (var p in PlayerHealth.All)
            {
                if (p == null || p.isDead) continue;
                Vector3 d = p.transform.position - c;
                d.y = 0f;
                if (d.magnitude > radius.Value) p.TakeDamage(dps * dt, 0);
            }
        }
    }
}
