using UnityEngine;
using UnityEngine.UI;
using LastZone.Gameplay.Player;
using LastZone.Gameplay.Zone;
using LastZone.Core;

namespace LastZone.UI.HUD
{
    /// <summary>
    /// In-game HUD showing health, armor, players alive, zone timer, and kill feed.
    /// Assign localPlayerHealth at runtime when the local player spawns.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Player References (set at runtime)")]
        public PlayerHealth localPlayerHealth;

        [Header("Scene References (assign in Inspector)")]
        public ZoneController zoneController;

        [Header("UI Elements")]
        public Slider healthSlider;
        public Slider armorSlider;
        public Text playersAliveText;
        public Text zoneTimerText;
        public Text killFeedText;

        // Cached alive count (updated periodically)
        float aliveCheckTimer;
        int cachedAliveCount;

        void Update()
        {
            UpdateHealthUI();
            UpdateZoneUI();
            UpdateAliveCount();
        }

        void UpdateHealthUI()
        {
            if (localPlayerHealth == null) return;

            if (healthSlider != null)
                healthSlider.value = localPlayerHealth.GetHealthNormalized();

            if (armorSlider != null)
                armorSlider.value = localPlayerHealth.GetArmorNormalized();
        }

        void UpdateZoneUI()
        {
            if (zoneController == null || zoneTimerText == null) return;

            string phase = zoneController.isShrinking ? "SHRINKING" : "SAFE";
            float timeLeft = zoneController.GetTimeRemaining();
            zoneTimerText.text = $"Zone {zoneController.currentStageIndex + 1} | {phase} | {timeLeft:F0}s";
        }

        void UpdateAliveCount()
        {
            if (playersAliveText == null) return;

            aliveCheckTimer -= Time.deltaTime;
            if (aliveCheckTimer <= 0f)
            {
                aliveCheckTimer = 0.5f; // Check every 0.5s
                cachedAliveCount = 0;
                var healths = FindObjectsOfType<PlayerHealth>(true);
                foreach (var h in healths)
                {
                    if (!h.isDead) cachedAliveCount++;
                }
            }
            playersAliveText.text = $"Alive: {cachedAliveCount}";
        }

        /// <summary>
        /// Add a kill feed entry. Call from GameManager or death events.
        /// </summary>
        public void AddKillFeedEntry(string killerName, string victimName)
        {
            if (killFeedText == null) return;
            killFeedText.text = $"{killerName} eliminated {victimName}\n{killFeedText.text}";
            // Keep only last 5 lines
            string[] lines = killFeedText.text.Split('\n');
            if (lines.Length > 5)
            {
                killFeedText.text = string.Join("\n", lines, 0, 5);
            }
        }
    }
}
