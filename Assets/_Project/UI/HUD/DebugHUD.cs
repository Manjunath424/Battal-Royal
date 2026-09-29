using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using LastZone.Core;
using LastZone.Gameplay.Player;

namespace LastZone.UI.HUD
{
    /// Put this script on a persistent object and assign 'panel' (child holding the texts). F3 toggles the panel.
    public class DebugHUD : MonoBehaviour
    {
        public GameObject panel;
        public Text fpsText;
        public Text pingText;
        public Text aliveCountText;
        public Text matchStateText;
        public bool showDebug = true;

        float frames, elapsed;
        int fps;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3)) showDebug = !showDebug;
            if (panel != null && panel.activeSelf != showDebug) panel.SetActive(showDebug);
            if (!showDebug) return;

            frames++;
            elapsed += Time.unscaledDeltaTime;
            if (elapsed >= 0.5f)
            {
                fps = Mathf.RoundToInt(frames / elapsed);
                frames = 0; elapsed = 0f;
            }
            if (fpsText != null) fpsText.text = $"FPS: {fps}";

            if (pingText != null)
            {
                var nm = NetworkManager.Singleton;
                if (nm == null || !nm.IsListening) pingText.text = "Ping: offline";
                else if (nm.IsServer) pingText.text = "Ping: host";
                else
                {
                    var ut = nm.NetworkConfig.NetworkTransport as UnityTransport;
                    pingText.text = ut != null ? $"Ping: {ut.GetCurrentRtt(NetworkManager.ServerClientId)} ms" : "Ping: N/A";
                }
            }

            if (aliveCountText != null) aliveCountText.text = $"Alive: {PlayerHealth.AliveCount()}";
            if (matchStateText != null)
                matchStateText.text = GameManager.Instance != null ? $"State: {GameManager.Instance.currentState}" : "State: -";
        }
    }
}
