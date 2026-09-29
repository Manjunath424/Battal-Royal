using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using LastZone.Core;
using LastZone.Gameplay.Player;
using LastZone.Gameplay.Zone;

namespace LastZone.UI.HUD
{
    /// Local player is found at runtime (a scene object cannot reference a prefab instance spawned later).
    public class GameHUD : MonoBehaviour
    {
        public ZoneController zoneController;   // scene object - can be assigned in Inspector

        [Header("UI Elements (Slider Max Value = 1)")]
        public Slider healthSlider;
        public Slider armorSlider;
        public Text playersAliveText;
        public Text zoneTimerText;
        public Text killFeedText;

        PlayerHealth localHealth;

        void Update()
        {
            if (localHealth == null) localHealth = FindLocalHealth();

            if (localHealth != null)
            {
                if (healthSlider != null) healthSlider.value = localHealth.currentHealth / localHealth.maxHealth;
                if (armorSlider != null) armorSlider.value = localHealth.currentArmor / localHealth.maxArmor;
            }

            if (zoneController != null && zoneTimerText != null)
            {
                int secs = Mathf.CeilToInt(zoneController.stageTimer);
                string phase = zoneController.isShrinking ? "Shrinking" : "Next shrink";
                zoneTimerText.text = $"Stage {zoneController.currentStageIndex + 1}  {phase}: {secs / 60}:{secs % 60:00}";
            }

            if (playersAliveText != null)
                playersAliveText.text = $"Alive: {PlayerHealth.AliveCount()}";
        }

        static PlayerHealth FindLocalHealth()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null || !nm.IsListening || nm.LocalClient == null) return null;
            var po = nm.LocalClient.PlayerObject;
            return po != null ? po.GetComponent<PlayerHealth>() : null;
        }
    }
}
