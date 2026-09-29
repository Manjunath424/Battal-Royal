using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastZone.UI.Screens
{
    /// <summary>
    /// Main menu screen. Wire "Play" and "Quit" buttons in the Inspector.
    /// </summary>
    public class MenuScreen : MonoBehaviour
    {
        public void OnPlayClicked()
        {
            // If you have a LobbyScreen, show it instead:
            // lobbyScreen.OnPlayClicked();
            SceneManager.LoadScene("Island");
        }

        public void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
