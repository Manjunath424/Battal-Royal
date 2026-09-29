using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using LastZone.Gameplay.Player;
using LastZone.Core;

namespace LastZone.UI.HUD
{
    /// <summary>
    /// Debug overlay showing FPS, ping, alive count, and match state.
    /// Toggle with F3 key. Attach to a UI panel under your HUD Canvas.
    /// </summary>
    public class DebugHUD : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text fpsText;
        public Text pingText;
        public Text aliveCountText;
        public Text matchStateText;

        [Header("Settings")]
        public bool showDebug = true;
        public KeyCode toggleKey = KeyCode.F3;

        float frameCount;
        float elapsed;
        int fps;

        // Cache: only count alive players every 0.5s, not every frame
        float aliveCheckTimer;
        int cachedAliveCount;

        void Update()
        {
            // Toggle with key
            if (Input.GetKeyDown(toggleKey))
            {
                showDebug = !showDebug;
            }

            // Show/hide the panel
            if (!showDebug)
            {
                SetChildrenActive(false);
                return;
            }
            else
            {
                SetChildrenActive(true);
            }

            // --- FPS ---
            frameCount++;
            elapsed += Time.unscaledDeltaTime; // unscaled so it works during pause
            if (elapsed >= 0.5f)
            {
                fps = Mathf.RoundToInt(frameCount / elapsed);
                frameCount = 0;
                elapsed = 0f;

                // Also update alive count on the same interval to save perf
                cachedAliveCount = 0;
                var healths = FindObjectsOfType<PlayerHealth>(true);
                foreach (var h in healths)
                {
                    if (!h.isDead) cachedAliveCount++;
                }
            }

            if (fpsText != null)
                fpsText.text = $"FPS: {fps}";

            // --- Ping ---
            if (pingText != null)
            {
                var nm = NetworkManager.Singleton;
                if (nm != null && nm.IsConnectedClient)
                {
                    if (nm.IsServer || nm.IsHost)
                    {
                        pingText.text = "Ping: 0 ms (Host)";
                    }
                    else
                    {
                        // NetworkManager.NetworkConfig.NetworkTransport may expose RTT
                        // For UnityTransport, RTT isn't directly public in older versions.
                        // Use a simple workaround or show N/A.
                        pingText.text = "Ping: N/A";
                    }
                }
                else
                {
                    pingText.text = "Ping: --";
                }
            }

            // --- Alive count ---
            if (aliveCountText != null)
                aliveCountText.text = $"Alive: {cachedAliveCount}";

            // --- Match state ---
            if (matchStateText != null)
            {
                if (GameManager.Instance != null)
                    matchStateText.text = $"State: {GameManager.Instance.currentState}";
                else
                    matchStateText.text = "State: --";
            }
        }

        void SetChildrenActive(bool active)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(active);
        }
    }
}
