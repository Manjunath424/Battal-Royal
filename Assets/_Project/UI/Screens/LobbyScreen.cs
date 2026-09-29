using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace LastZone.UI.Screens
{
    public class LobbyScreen : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject mainPanel;
        public GameObject lobbyPanel;

        [Header("Lobby Fields")]
        public InputField ipAddressInput;
        public string matchScene = "Island";
        public ushort port = 7777;

        void Start()
        {
            ShowMain();
            if (ipAddressInput != null) ipAddressInput.text = "127.0.0.1";
        }

        public void OnPlayClicked() { mainPanel.SetActive(false); lobbyPanel.SetActive(true); }
        public void OnBackClicked() => ShowMain();

        void ShowMain() { mainPanel.SetActive(true); lobbyPanel.SetActive(false); }

        public void OnHostClicked()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) { Debug.LogError("No NetworkManager in the Menu scene."); return; }

            var ut = nm.GetComponent<UnityTransport>();
            if (ut != null) ut.SetConnectionData("127.0.0.1", port, "0.0.0.0"); // listen on all interfaces

            if (!nm.StartHost()) { Debug.LogError("StartHost failed."); return; }

            // Only the SERVER loads the scene; clients are synced automatically by NGO.
            nm.SceneManager.LoadScene(matchScene, LoadSceneMode.Single);
        }

        public void OnJoinClicked()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) { Debug.LogError("No NetworkManager in the Menu scene."); return; }

            string ip = ipAddressInput != null && !string.IsNullOrWhiteSpace(ipAddressInput.text)
                ? ipAddressInput.text.Trim() : "127.0.0.1";
            var ut = nm.GetComponent<UnityTransport>();
            if (ut != null) ut.SetConnectionData(ip, port);

            if (!nm.StartClient()) Debug.LogError("StartClient failed.");
            // No LoadScene here - the host's scene is synchronized to us.
        }
    }
}
