using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace LastZone.Core
{
    /// Put in a "Boot" scene (build index 0) or in the Menu scene. Persists across scenes.
    public class Bootstrap : MonoBehaviour
    {
        public static Bootstrap Instance { get; private set; }
        public ServiceLocator Services { get; private set; }
        public string menuScene = "Menu";

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Services = new ServiceLocator();
            Services.Initialize();

            // Only load Menu if we are not already in it (avoids a reload loop)
            if (SceneManager.GetActiveScene().name != menuScene)
                SceneManager.LoadScene(menuScene);
        }

        /// Safely shuts down networking and returns to the menu.
        public static void GoToMenu(float delay)
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(Instance.ReturnRoutine(delay));
                return;
            }
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening) nm.Shutdown();
            SceneManager.LoadScene("Menu");
        }

        IEnumerator ReturnRoutine(float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening)
            {
                nm.Shutdown();
                yield return null;
            }
            SceneManager.LoadScene(menuScene);
        }
    }
}
