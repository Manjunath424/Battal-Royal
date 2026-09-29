using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;

namespace LastZone.Gameplay.Loot
{
    /// Prefab needs: NetworkObject + a Collider with "Is Trigger" ticked. Register as a network prefab.
    public class Pickup : NetworkBehaviour
    {
        public string itemName;
        public float armorAmount;
        public float healthAmount;
        public float maxPickupDistance = 3f;

        bool collected;

        void OnTriggerEnter(Collider other)
        {
            if (!IsServer || !IsSpawned || collected) return;
            var p = other.GetComponentInParent<PlayerHealth>();
            if (p != null && !p.isDead) Collect(p);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PickUpServerRpc(ServerRpcParams rpcParams = default)
        {
            if (collected) return;
            if (!NetworkManager.ConnectedClients.TryGetValue(rpcParams.Receive.SenderClientId, out var client)) return;
            if (client.PlayerObject == null) return;
            if (Vector3.Distance(client.PlayerObject.transform.position, transform.position) > maxPickupDistance) return;

            var p = client.PlayerObject.GetComponent<PlayerHealth>();
            if (p != null && !p.isDead) Collect(p);
        }

        void Collect(PlayerHealth p)
        {
            collected = true;
            if (armorAmount > 0f) p.AddArmor(armorAmount);
            if (healthAmount > 0f) p.Heal(healthAmount);
            NetworkObject.Despawn(true);
        }
    }
}
