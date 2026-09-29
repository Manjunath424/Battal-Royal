using UnityEngine;
using Unity.Netcode;

namespace LastZone.Gameplay.Player
{
    /// <summary>
    /// Server-authoritative health and armor system.
    /// Armor absorbs 50% of incoming damage until depleted.
    /// </summary>
    public class PlayerHealth : NetworkBehaviour
    {
        [Header("Health")]
        public float maxHealth = 100f;
        public float currentHealth;

        [Header("Armor")]
        public float maxArmor = 100f;
        public float currentArmor;

        [Header("State")]
        public bool isDead;

        /// <summary>Fired on server when this entity dies. Args: (victimNetId, killerNetId)</summary>
        public event System.Action<ulong, ulong> OnDeath;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            currentHealth = maxHealth;
            currentArmor = 0f;
            isDead = false;
        }

        /// <summary>
        /// Apply damage on the server. Armor absorbs 50% until depleted.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TakeDamageServerRpc(float damage, ulong sourceId)
        {
            if (isDead) return;

            // Armor absorbs up to 50% of damage
            float armorAbsorb = Mathf.Min(currentArmor, damage * 0.5f);
            currentArmor -= armorAbsorb;
            float actualDamage = damage - armorAbsorb;
            currentHealth -= actualDamage;

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                isDead = true;
                Die(sourceId);
            }
        }

        void Die(ulong sourceId)
        {
            OnDeath?.Invoke(NetworkObjectId, sourceId);
            DieClientRpc();
            // TODO: Spawn loot drop, ragdoll, death VFX
        }

        [ClientRpc]
        void DieClientRpc()
        {
            // Play death animation/VFX on all clients
            isDead = true;
        }

        /// <summary>Add armor, clamped to maxArmor.</summary>
        public void AddArmor(float amount)
        {
            currentArmor = Mathf.Min(maxArmor, currentArmor + amount);
        }

        /// <summary>Heal HP, clamped to maxHealth. Cannot heal if dead.</summary>
        public void Heal(float amount)
        {
            if (isDead) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        /// <summary>Returns health as 0-1 ratio for UI sliders.</summary>
        public float GetHealthNormalized() => currentHealth / maxHealth;

        /// <summary>Returns armor as 0-1 ratio for UI sliders.</summary>
        public float GetArmorNormalized() => currentArmor / maxArmor;
    }
}
