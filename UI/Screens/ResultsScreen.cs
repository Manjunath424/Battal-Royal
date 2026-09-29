using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace LastZone.UI.Screens
{
    /// <summary>
    /// Post-match results screen. Call SetResults() from GameManager when match ends.
    /// </summary>
    public class ResultsScreen : MonoBehaviour
    {
        [Header("UI Elements")]
        public Text placementText;
        public Text killsText;
        public Text damageText;
        public Text matchTimeText;

        [Header("Buttons")]
        public Button returnToMenuButton;

        void Start()
        {
            if (returnToMenuButton != null)
                returnToMenuButton.onClick.AddListener(OnReturnToMenu);
        }

        public void OnReturnToMenu()
        {
            // Disconnect from network before returning
            if (Unity.Netcode.NetworkManager.Singleton != null)
            {
                Unity.Netcode.NetworkManager.Singleton.Shutdown();
            }
            SceneManager.LoadScene("Menu");
        }

        /// <summary>
        /// Populate the results UI. Call from GameManager.EndMatch().
        /// </summary>
        public void SetResults(int placement, int kills, float damage, float matchTimeSeconds)
        {
            if (placementText != null)
                placementText.text = placement == 1 ? "WINNER WINNER!" : $"#{placement}";

            if (killsText != null)
                killsText.text = $"Kills: {kills}";

            if (damageText != null)
                damageText.text = $"Damage: {damage:F0}";

            if (matchTimeText != null)
            {
                int mins = Mathf.FloorToInt(matchTimeSeconds / 60f);
                int secs = Mathf.FloorToInt(matchTimeSeconds % 60f);
                matchTimeText.text = $"Time: {mins}:{secs:D2}";
            }

            gameObject.SetActive(true);
        }
    }
}
