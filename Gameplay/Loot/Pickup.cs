using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;

namespace LastZone.Gameplay.Loot
{
    /// <summary>
    /// Pickup item (armor vest, medkit, etc.). Sits in the world until a player
    /// walks over it and triggers collection via ServerRpc.
    /// </summary>
    public class Pickup : NetworkBehaviour
    {
        [Header("Item Info")]
        public string itemName = "Pickup";

        [Header("Effects")]
        public float armorAmount;
        public float healthAmount;

        [Header("Auto-collect")]
        public float collectRadius = 1.5f;

        void Update()
        {
            if (!IsServer) return;

            // Simple proximity-based auto-collect
            var players = FindObjectsOfType<PlayerHealth>(true);
            foreach (var health in players)
            {
                if (health.isDead) continue;
                float dist = Vector3.Distance(transform.position, health.transform.position);
                if (dist <= collectRadius)
                {
                    ApplyTo(health);
                    GetComponent<NetworkObject>().Despawn();
                    return;
                }
            }
        }

        /// <summary>
        /// Manually triggered pickup (e.g. via interact key).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PickUpServerRpc(ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;

            // Find the player's health component by their NetworkObject
            foreach (var player in FindObjectsOfType<PlayerHealth>(true))
            {
                if (player.OwnerClientId == clientId)
                {
                    ApplyTo(player);
                    GetComponent<NetworkObject>().Despawn();
                    return;
                }
            }
        }

        void ApplyTo(PlayerHealth health)
        {
            if (armorAmount > 0f) health.AddArmor(armorAmount);
            if (healthAmount > 0f) health.Heal(healthAmount);
        }
    }
}
