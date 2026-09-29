using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LastZone.Gameplay.Player
{
    /// Used by both players and bots. Health/armor are NetworkVariables so every client sees them.
    public class PlayerHealth : NetworkBehaviour
    {
        public static readonly List<PlayerHealth> All = new List<PlayerHealth>();
        public static event Action<PlayerHealth, ulong> Died; // victim, source client id

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { All.Clear(); Died = null; }

        public float maxHealth = 100f;
        public float maxArmor = 100f;
        public float startArmor = 0f;

        readonly NetworkVariable<float> health = new NetworkVariable<float>(100f);
        readonly NetworkVariable<float> armor = new NetworkVariable<float>(0f);
        readonly NetworkVariable<bool> dead = new NetworkVariable<bool>(false);

        public float currentHealth => health.Value;
        public float currentArmor => armor.Value;
        public bool isDead => dead.Value;

        public static int AliveCount()
        {
            int n = 0;
            foreach (var p in All) if (p != null && !p.isDead) n++;
            return n;
        }

        public override void OnNetworkSpawn()
        {
            All.Add(this);
            if (IsServer)
            {
                health.Value = maxHealth;
                armor.Value = Mathf.Clamp(startArmor, 0f, maxArmor);
                dead.Value = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            All.Remove(this);
        }

        /// SERVER ONLY. (Was a ServerRpc before - that fails ownership checks when the server damages other players.)
        public void TakeDamage(float damage, ulong sourceClientId)
        {
            if (!IsServer || dead.Value || damage <= 0f) return;

            float armorAbsorb = Mathf.Min(armor.Value, damage * 0.5f);
            armor.Value -= armorAbsorb;
            health.Value -= damage - armorAbsorb;

            if (health.Value <= 0f)
            {
                health.Value = 0f;
                dead.Value = true;
                Died?.Invoke(this, sourceClientId);
            }
        }

        public void AddArmor(float amount)
        {
            if (!IsServer || dead.Value) return;
            armor.Value = Mathf.Min(maxArmor, armor.Value + amount);
        }

        public void Heal(float amount)
        {
            if (!IsServer || dead.Value) return;
            health.Value = Mathf.Min(maxHealth, health.Value + amount);
        }
    }
}
