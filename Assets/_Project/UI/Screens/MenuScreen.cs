using UnityEngine;

namespace LastZone.UI.Screens
{
    public class MenuScreen : MonoBehaviour
    {
        public LobbyScreen lobbyScreen;

        // Play must open the lobby (host/join). Loading "Island" directly would bypass NetworkManager.
        public void OnPlayClicked()
        {
            if (lobbyScreen != null) lobbyScreen.OnPlayClicked();
        }

        public void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}
