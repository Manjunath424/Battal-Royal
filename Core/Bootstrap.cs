using UnityEngine;
using UnityEngine.SceneManagement;

namespace LastZone.Core
{
    public class Bootstrap : MonoBehaviour
    {
        public static Bootstrap Instance { get; private set; }
        public ServiceLocator Services { get; private set; }

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

            SceneManager.LoadScene("Menu");
        }
    }
}
