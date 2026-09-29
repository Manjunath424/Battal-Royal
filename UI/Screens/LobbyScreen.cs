using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace LastZone.UI.Screens
{
    /// <summary>
    /// Lobby screen for hosting or joining a game over LAN.
    /// Wire buttons and fields in the Inspector.
    /// </summary>
    public class LobbyScreen : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;    // "Play" / "Quit" buttons
        public GameObject lobbyPanel;   // Host / Join / IP input

        [Header("Lobby Fields")]
        public InputField ipAddressInput;
        public InputField portInput;
        public Button hostButton;
        public Button joinButton;
        public Button backButton;

        [Header("Status")]
        public Text statusText;

        void Start()
        {
            ShowMain();

            if (ipAddressInput != null)
                ipAddressInput.text = "127.0.0.1";
            if (portInput != null)
                portInput.text = "7777";
        }

        public void OnPlayClicked()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (lobbyPanel != null) lobbyPanel.SetActive(true);
        }

        public void OnHostClicked()
        {
            SetStatus("Starting host...");
            StartHost();
        }

        public void OnJoinClicked()
        {
            SetStatus("Connecting...");
            StartClient();
        }

        public void OnBackClicked()
        {
            ShowMain();
        }

        void ShowMain()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
        }

        void StartHost()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                SetStatus("ERROR: No NetworkManager in scene!");
                return;
            }

            ConfigureTransport(nm);
            bool success = nm.StartHost();
            if (success)
            {
                LoadMatchScene();
            }
            else
            {
                SetStatus("Failed to start host.");
            }
        }

        void StartClient()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null)
            {
                SetStatus("ERROR: No NetworkManager in scene!");
                return;
            }

            ConfigureTransport(nm);
            nm.StartClient();
            // Scene will be loaded by the host via NetworkSceneManager,
            // or we can load it manually:
            LoadMatchScene();
        }

        void ConfigureTransport(NetworkManager nm)
        {
            var transport = nm.GetComponent<UnityTransport>();
            if (transport != null)
            {
                string ip = ipAddressInput != null ? ipAddressInput.text : "127.0.0.1";
                ushort port = 7777;
                if (portInput != null && ushort.TryParse(portInput.text, out ushort parsed))
                    port = parsed;

                transport.ConnectionData.Address = ip;
                transport.ConnectionData.Port = port;
            }
        }

        void LoadMatchScene()
        {
            SceneManager.LoadScene("Island");
        }

        void SetStatus(string msg)
        {
            if (statusText != null)
                statusText.text = msg;
        }
    }
}
