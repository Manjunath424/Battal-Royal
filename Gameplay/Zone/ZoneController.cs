using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;

namespace LastZone.Gameplay.Zone
{
    /// <summary>
    /// Server-authoritative zone controller. Manages shrink phases, zone center,
    /// and applies damage to players outside the safe zone.
    /// </summary>
    public class ZoneController : NetworkBehaviour
    {
        [Header("Configuration")]
        public ZoneSchedule schedule;
        public Transform zoneCenter;
        public float mapRadius = 750f;

        [Header("Runtime State (read-only)")]
        public float currentRadius;
        public int currentStageIndex;
        public float stageTimer;
        public bool isShrinking;

        float startRadiusForShrink; // radius at the moment shrinking begins

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;

            if (schedule == null || schedule.stages == null || schedule.stages.Length == 0)
            {
                Debug.LogError("[ZoneController] No ZoneSchedule assigned or schedule has no stages!");
                enabled = false;
                return;
            }

            currentRadius = mapRadius;
            currentStageIndex = 0;
            stageTimer = schedule.stages[0].waitTime;
            isShrinking = false;
            startRadiusForShrink = mapRadius;
        }

        void Update()
        {
            if (!IsServer) return;
            if (schedule == null || currentStageIndex >= schedule.stages.Length) return;

            stageTimer -= Time.deltaTime;

            if (stageTimer <= 0f)
            {
                if (!isShrinking)
                {
                    // Wait phase ended → begin shrinking
                    isShrinking = true;
                    startRadiusForShrink = currentRadius;
                    stageTimer = schedule.stages[currentStageIndex].shrinkTime;
                }
                else
                {
                    // Shrink phase ended → snap to target radius, advance stage
                    float endRadius = mapRadius * schedule.stages[currentStageIndex].endRadiusPercent;
                    currentRadius = endRadius;
                    PickNewZoneCenter();

                    currentStageIndex++;
                    if (currentStageIndex >= schedule.stages.Length)
                    {
                        // All stages done — zone stays at final size
                        isShrinking = false;
                        return;
                    }

                    isShrinking = false;
                    stageTimer = schedule.stages[currentStageIndex].waitTime;
                }
            }

            // Smoothly interpolate radius during shrink
            if (isShrinking)
            {
                float shrinkDuration = schedule.stages[currentStageIndex].shrinkTime;
                float endRadius = mapRadius * schedule.stages[currentStageIndex].endRadiusPercent;
                float t = 1f - (stageTimer / shrinkDuration);
                currentRadius = Mathf.Lerp(startRadiusForShrink, endRadius, t);
            }

            ApplyZoneDamage();
        }

        void PickNewZoneCenter()
        {
            // Shift center randomly within the current circle
            Vector2 rand = Random.insideUnitCircle * (currentRadius * 0.3f);
            zoneCenter.position += new Vector3(rand.x, 0f, rand.y);
        }

        void ApplyZoneDamage()
        {
            if (currentStageIndex >= schedule.stages.Length) return;

            float damagePS = schedule.stages[currentStageIndex].damagePerSecond;
            float frameDamage = damagePS * Time.deltaTime;

            // Find all player controllers and damage those outside the zone
            var players = FindObjectsOfType<PlayerController>(true);
            foreach (var player in players)
            {
                float dist = Vector3.Distance(player.transform.position, zoneCenter.position);
                if (dist > currentRadius)
                {
                    var health = player.GetComponent<PlayerHealth>();
                    if (health != null && !health.isDead)
                    {
                        health.TakeDamageServerRpc(frameDamage, 0); // sourceId 0 = zone
                    }
                }
            }
        }

        /// <summary>Returns remaining time in the current phase (wait or shrink).</summary>
        public float GetTimeRemaining() => Mathf.Max(0f, stageTimer);
    }
}
