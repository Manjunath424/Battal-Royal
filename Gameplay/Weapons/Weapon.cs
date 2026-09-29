using UnityEngine;
using Unity.Netcode;
using System.Collections;
using LastZone.Gameplay.Player;

namespace LastZone.Gameplay.Weapons
{
    /// <summary>
    /// Hitscan weapon with server-authoritative fire and client-side VFX.
    /// Attach to a child of the player prefab with a muzzle Transform and LineRenderer.
    /// </summary>
    public class Weapon : NetworkBehaviour
    {
        [Header("Definition (assign ScriptableObject)")]
        public WeaponDef weaponDef;

        [Header("References")]
        public Transform muzzle;
        public LineRenderer tracer;

        [Header("Runtime (read-only)")]
        public int currentAmmo;
        public bool isReloading;

        float nextFireTime;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (weaponDef != null)
                currentAmmo = weaponDef.magazineSize;
            if (tracer != null)
                tracer.enabled = false;
        }

        void Update()
        {
            if (!IsOwner) return;

            // Fire (owner requests server to fire)
            if (Input.GetButton("Fire1") && CanFire())
            {
                Vector3 dir = GetAimDirection();
                FireServerRpc(dir);
                nextFireTime = Time.time + 1f / weaponDef.fireRate;
            }

            // Reload
            if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo < weaponDef.magazineSize)
            {
                ReloadServerRpc();
            }
        }

        bool CanFire()
        {
            return Time.time >= nextFireTime
                && !isReloading
                && currentAmmo > 0
                && weaponDef != null;
        }

        Vector3 GetAimDirection()
        {
            // Aim from screen center via camera raycast
            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f));
                if (Physics.Raycast(ray, out RaycastHit hit, weaponDef.effectiveRange))
                    return (hit.point - muzzle.position).normalized;
                return ray.direction;
            }
            return muzzle.forward;
        }

        /// <summary>
        /// Server-side fire. Also callable by BotController for bot shooting.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void FireServerRpc(Vector3 direction)
        {
            if (isReloading || currentAmmo <= 0 || weaponDef == null) return;
            currentAmmo--;

            // Hitscan raycast
            if (Physics.Raycast(muzzle.position, direction, out RaycastHit hit, weaponDef.effectiveRange))
            {
                var health = hit.collider.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    float dmg = weaponDef.damage;

                    // Distance falloff
                    float dist = Vector3.Distance(muzzle.position, hit.point);
                    dmg *= Combat.DamageModel.CalculateFalloff(dist, weaponDef.effectiveRange);

                    // Headshot check
                    if (hit.collider.CompareTag("Head"))
                        dmg *= weaponDef.headshotMultiplier;

                    health.TakeDamageServerRpc(dmg, OwnerClientId);
                }
            }

            // Replicate VFX to all clients
            FireClientRpc(direction);
        }

        [ClientRpc]
        void FireClientRpc(Vector3 direction)
        {
            if (tracer != null)
                StartCoroutine(ShowTracer(direction));
            // TODO: Muzzle flash particle, gunshot audio
        }

        IEnumerator ShowTracer(Vector3 dir)
        {
            if (tracer == null) yield break;
            tracer.SetPosition(0, muzzle.position);
            tracer.SetPosition(1, muzzle.position + dir * weaponDef.effectiveRange);
            tracer.enabled = true;
            yield return new WaitForSeconds(0.05f);
            tracer.enabled = false;
        }

        [ServerRpc]
        void ReloadServerRpc()
        {
            if (isReloading) return;
            isReloading = true;
            // Use Invoke (MonoBehaviour.Invoke), NOT "InvokeRpc" which doesn't exist
            Invoke(nameof(FinishReload), weaponDef.reloadTime);
        }

        void FinishReload()
        {
            currentAmmo = weaponDef.magazineSize;
            isReloading = false;
        }
    }
}
