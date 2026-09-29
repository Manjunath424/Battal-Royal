using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;

namespace LastZone.Gameplay.Weapons
{
    /// Lives on a CHILD of the player/bot prefab (same NetworkObject - it is NOT its own network prefab).
    public class Weapon : NetworkBehaviour
    {
        public WeaponDef weaponDef;
        public Transform muzzle;
        public LineRenderer tracer;
        [Tooltip("Set at runtime by PlayerInput (the camera).")]
        public Transform aimTransform;
        public float aimRayStartOffset = 3.5f; // skips the player's own body when aiming from the camera

        readonly NetworkVariable<int> ammo = new NetworkVariable<int>(0);
        public int currentAmmo => ammo.Value;

        float nextFireTime;       // owner side
        float serverNextFire;     // server side rate limiter
        bool isReloading;         // server side
        bool useLocalInput;

        public override void OnNetworkSpawn()
        {
            // Bots are owned by the server too, so IsOwner alone would let them read the host's mouse.
            useLocalInput = IsOwner && GetComponentInParent<PlayerInput>() != null;

            if (IsServer && weaponDef != null) ammo.Value = weaponDef.magazineSize;
            if (tracer != null)
            {
                tracer.positionCount = 2;
                tracer.enabled = false;
            }
        }

        void Update()
        {
            if (!useLocalInput || weaponDef == null) return;

            if (Input.GetButton("Fire1") && Time.time >= nextFireTime && ammo.Value > 0)
            {
                nextFireTime = Time.time + 1f / weaponDef.fireRate;
                FireServerRpc(GetAimDirection());
            }

            if (Input.GetKeyDown(KeyCode.R) && ammo.Value < weaponDef.magazineSize)
                ReloadServerRpc();
        }

        Vector3 GetAimDirection()
        {
            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            if (aimTransform == null) return transform.forward;

            Vector3 start = aimTransform.position + aimTransform.forward * aimRayStartOffset;
            Vector3 point = Physics.Raycast(start, aimTransform.forward, out RaycastHit h, weaponDef.effectiveRange,
                                            ~0, QueryTriggerInteraction.Ignore)
                ? h.point
                : start + aimTransform.forward * weaponDef.effectiveRange;
            return (point - origin).normalized;
        }

        [ServerRpc]
        public void FireServerRpc(Vector3 direction) => TryFire(direction);

        /// SERVER ONLY. Bots call this directly.
        public bool TryFire(Vector3 direction)
        {
            if (!IsServer || weaponDef == null) return false;
            if (isReloading || ammo.Value <= 0) return false;
            if (Time.time < serverNextFire) return false;
            serverNextFire = Time.time + 0.5f / weaponDef.fireRate;

            ammo.Value--;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
            Vector3 origin = muzzle != null ? muzzle.position : transform.position;
            Vector3 endPoint = origin + direction * weaponDef.effectiveRange;

            RaycastHit[] hits = Physics.RaycastAll(origin, direction, weaponDef.effectiveRange,
                                                   ~0, QueryTriggerInteraction.Ignore);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            Transform self = NetworkObject.transform;

            foreach (var hit in hits)
            {
                if (hit.collider.transform.IsChildOf(self)) continue; // don't shoot yourself
                endPoint = hit.point;

                var victim = hit.collider.GetComponentInParent<PlayerHealth>();
                if (victim != null)
                {
                    float dmg = weaponDef.damage;
                    if (hit.collider.name.IndexOf("head", StringComparison.OrdinalIgnoreCase) >= 0)
                        dmg *= weaponDef.headshotMultiplier;
                    victim.TakeDamage(dmg, OwnerClientId);
                }
                break;
            }

            FireClientRpc(endPoint);
            return true;
        }

        [ClientRpc]
        void FireClientRpc(Vector3 endPoint)
        {
            if (tracer != null) StartCoroutine(ShowTracer(endPoint));
        }

        IEnumerator ShowTracer(Vector3 end)
        {
            tracer.SetPosition(0, muzzle != null ? muzzle.position : transform.position);
            tracer.SetPosition(1, end);
            tracer.enabled = true;
            yield return new WaitForSeconds(0.05f);
            tracer.enabled = false;
        }

        [ServerRpc]
        void ReloadServerRpc() => StartReload();

        /// SERVER ONLY.
        public void StartReload()
        {
            if (!IsServer || isReloading || weaponDef == null) return;
            if (ammo.Value >= weaponDef.magazineSize) return;
            StartCoroutine(ReloadRoutine());
        }

        IEnumerator ReloadRoutine()
        {
            isReloading = true;
            yield return new WaitForSeconds(weaponDef.reloadTime);
            ammo.Value = weaponDef.magazineSize;
            isReloading = false;
        }
    }
}
