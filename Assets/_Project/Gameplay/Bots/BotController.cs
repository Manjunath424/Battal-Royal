using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;
using LastZone.Gameplay.Weapons;

namespace LastZone.Gameplay.Bots
{
    /// Bot prefab: NetworkObject, CharacterController, PlayerHealth, BotController, NetworkTransform (server-auth),
    /// a child Weapon. NO PlayerController / PlayerInput / PlayerCamera / ClientNetworkTransform.
    /// No NavMesh - bots walk straight and do not avoid obstacles yet.
    public class BotController : NetworkBehaviour
    {
        [Header("Bot Settings")]
        public float detectionRadius = 40f;
        public float attackRadius = 30f;
        public float stopDistance = 12f;
        public float moveSpeed = 5f;
        public float reactionDelay = 0.3f;
        public float aimErrorDegrees = 3f;
        public float fleeHealth = 30f;
        public float gravity = -20f;

        enum BotState { Roam, Chase, Attack, Flee }
        BotState state = BotState.Roam;

        CharacterController cc;
        PlayerHealth health;
        Weapon weapon;
        PlayerHealth target;
        Vector3 roamTarget;
        float roamTimer;
        float nextFireTime;
        float vy;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) { enabled = false; return; }
            cc = GetComponent<CharacterController>();
            health = GetComponent<PlayerHealth>();
            weapon = GetComponentInChildren<Weapon>();
            PickNewRoamTarget();
        }

        void Update()
        {
            if (!IsServer || !IsSpawned || health == null || health.isDead) return;

            FindTarget();
            UpdateState();
            Move();
            Attack();
        }

        void FindTarget()
        {
            PlayerHealth best = null;
            float bestSqr = detectionRadius * detectionRadius;
            foreach (var p in PlayerHealth.All)
            {
                if (p == null || p == health || p.isDead) continue;
                float sqr = (p.transform.position - transform.position).sqrMagnitude;
                if (sqr < bestSqr) { bestSqr = sqr; best = p; }
            }
            if (best != null && target == null) nextFireTime = Time.time + reactionDelay;
            target = best;
        }

        void UpdateState()
        {
            if (target == null) { state = BotState.Roam; return; }
            float d = Vector3.Distance(transform.position, target.transform.position);
            if (health.currentHealth < fleeHealth) state = BotState.Flee;
            else state = d <= attackRadius ? BotState.Attack : BotState.Chase;
        }

        void Move()
        {
            Vector3 dir = Vector3.zero;
            Vector3 look = Vector3.zero;

            switch (state)
            {
                case BotState.Roam:
                    roamTimer -= Time.deltaTime;
                    Vector3 toRoam = roamTarget - transform.position; toRoam.y = 0f;
                    if (roamTimer <= 0f || toRoam.magnitude < 1.5f)
                    {
                        PickNewRoamTarget();
                        roamTimer = Random.Range(3f, 6f);
                        toRoam = roamTarget - transform.position; toRoam.y = 0f;
                    }
                    dir = toRoam.normalized; look = dir;
                    break;

                case BotState.Chase:
                    dir = Flat(target.transform.position - transform.position); look = dir;
                    break;

                case BotState.Attack:
                    Vector3 toT = Flat(target.transform.position - transform.position);
                    look = toT;
                    float dist = Vector3.Distance(transform.position, target.transform.position);
                    if (dist > stopDistance) dir = toT;
                    break;

                case BotState.Flee:
                    dir = Flat(transform.position - target.transform.position);
                    look = Flat(target.transform.position - transform.position);
                    break;
            }

            if (cc.isGrounded && vy < 0f) vy = -2f;
            vy += gravity * Time.deltaTime;
            cc.Move((dir * moveSpeed + Vector3.up * vy) * Time.deltaTime);

            if (look.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(look), 10f * Time.deltaTime);
        }

        void Attack()
        {
            if (state != BotState.Attack || target == null || weapon == null || weapon.weaponDef == null) return;
            if (Time.time < nextFireTime) return;

            Vector3 origin = weapon.muzzle != null ? weapon.muzzle.position : transform.position + Vector3.up;
            Vector3 aimPoint = target.transform.position + Vector3.up * 1.2f;
            Vector3 dir = (aimPoint - origin).normalized;
            dir = Quaternion.Euler(Random.Range(-aimErrorDegrees, aimErrorDegrees),
                                   Random.Range(-aimErrorDegrees, aimErrorDegrees), 0f) * dir;

            if (weapon.TryFire(dir))
                nextFireTime = Time.time + 1f / weapon.weaponDef.fireRate;
            else if (weapon.currentAmmo <= 0)
                weapon.StartReload();
        }

        static Vector3 Flat(Vector3 v) { v.y = 0f; return v.normalized; }

        void PickNewRoamTarget()
        {
            Vector2 r = Random.insideUnitCircle * 20f;
            roamTarget = transform.position + new Vector3(r.x, 0f, r.y);
        }
    }
}
