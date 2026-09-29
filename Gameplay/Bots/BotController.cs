using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;
using LastZone.Gameplay.Weapons;

namespace LastZone.Gameplay.Bots
{
    /// <summary>
    /// Server-authoritative bot AI with states: Idle, Roam, Chase, Attack, Flee.
    /// Runs only on the server; clients see movement via NetworkTransform.
    /// </summary>
    public class BotController : NetworkBehaviour
    {
        [Header("Bot Settings")]
        public float detectionRadius = 40f;
        public float attackRadius = 30f;
        public float moveSpeed = 5f;
        public float fireRate = 8f;         // effective fire rate for this bot
        public float reactionDelay = 0.3f;  // seconds added between shots
        [Range(0f, 1f)]
        public float accuracy = 0.6f;       // 60% hit chance at medium range

        enum BotState { Idle, Roam, Chase, Attack, Flee }
        BotState state = BotState.Idle;

        CharacterController characterController;
        PlayerHealth health;
        Weapon currentWeapon;
        Transform target;
        float nextFireTime;
        Vector3 roamTarget;
        float roamTimer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer)
            {
                enabled = false; // Bot logic only runs on server
                return;
            }

            characterController = GetComponent<CharacterController>();
            health = GetComponent<PlayerHealth>();
            currentWeapon = GetComponentInChildren<Weapon>();
            PickNewRoamTarget();
            roamTimer = Random.Range(3f, 6f);
        }

        void Update()
        {
            if (!IsServer) return;
            if (health == null || health.isDead) return;

            FindTarget();
            UpdateState();
            Move();
            Attack();
        }

        void FindTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
            Transform closest = null;
            float closestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                // Skip self
                if (hit.transform == transform) continue;

                // Only target players (not other bots for simplicity)
                var playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth == null || playerHealth.isDead) continue;

                // Skip other bots (they have BotController)
                if (hit.GetComponent<BotController>() != null) continue;

                float d = Vector3.Distance(transform.position, hit.transform.position);
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = hit.transform;
                }
            }

            target = (closest != null && closestDist <= attackRadius) ? closest : null;
        }

        void UpdateState()
        {
            if (health.currentHealth < 30f && target != null)
                state = BotState.Flee;
            else if (target != null)
                state = BotState.Attack;
            else
                state = BotState.Roam;
        }

        void Move()
        {
            Vector3 direction = Vector3.zero;

            switch (state)
            {
                case BotState.Idle:
                    break;

                case BotState.Roam:
                    roamTimer -= Time.deltaTime;
                    if (roamTimer <= 0f)
                    {
                        PickNewRoamTarget();
                        roamTimer = Random.Range(3f, 6f);
                    }
                    direction = (roamTarget - transform.position);
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0.1f)
                        direction = direction.normalized;
                    else
                        direction = Vector3.zero;
                    break;

                case BotState.Chase:
                case BotState.Attack:
                    if (target != null)
                    {
                        direction = (target.position - transform.position);
                        direction.y = 0f;
                        direction = direction.normalized;
                    }
                    break;

                case BotState.Flee:
                    if (target != null)
                    {
                        direction = (transform.position - target.position);
                        direction.y = 0f;
                        direction = direction.normalized;
                    }
                    else
                    {
                        direction = -transform.forward;
                    }
                    break;
            }

            if (direction.sqrMagnitude > 0.01f)
            {
                // Apply gravity
                Vector3 motion = direction * moveSpeed * Time.deltaTime;
                motion.y = -9.81f * Time.deltaTime; // simple gravity
                characterController.Move(motion);
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            }
        }

        void Attack()
        {
            if (state != BotState.Attack || target == null || currentWeapon == null)
                return;

            if (Time.time < nextFireTime) return;

            // Accuracy check: roll against accuracy to simulate missing
            if (Random.value > accuracy) 
            {
                // "Missed" — schedule next attempt but don't fire
                nextFireTime = Time.time + (1f / fireRate) + reactionDelay;
                return;
            }

            Vector3 dir = (target.position - currentWeapon.muzzle.position).normalized;
            currentWeapon.FireServerRpc(dir);
            nextFireTime = Time.time + (1f / fireRate) + reactionDelay;
        }

        void PickNewRoamTarget()
        {
            Vector2 rand = Random.insideUnitCircle * 20f;
            roamTarget = transform.position + new Vector3(rand.x, 0f, rand.y);
        }
    }
}
