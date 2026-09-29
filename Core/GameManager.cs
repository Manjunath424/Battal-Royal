using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using LastZone.Gameplay.Player;

namespace LastZone.Core
{
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scenes")]
        public string menuScene = "Menu";
        public string matchScene = "Island";

        [Header("Match Settings")]
        public int minPlayersToStart = 2;
        public float countdownTime = 10f;

        public enum MatchState { Waiting, Countdown, Playing, Ended }
        public MatchState currentState = MatchState.Waiting;

        public float matchTimer;
        public float countdownTimer;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!IsServer) return;

            currentState = MatchState.Waiting;
            matchTimer = 0f;
        }

        void Update()
        {
            if (!IsServer) return;

            switch (currentState)
            {
                case MatchState.Waiting:
                    // Use FindObjectsByType (Unity 2022.2+) for better perf; fallback to FindObjectsOfType
                    int alivePlayers = FindObjectsOfType<PlayerController>(true).Length;
                    if (alivePlayers >= minPlayersToStart)
                    {
                        currentState = MatchState.Countdown;
                        countdownTimer = countdownTime;
                    }
                    break;

                case MatchState.Countdown:
                    countdownTimer -= Time.deltaTime;
                    if (countdownTimer <= 0f)
                    {
                        StartMatch();
                    }
                    break;

                case MatchState.Playing:
                    matchTimer += Time.deltaTime;
                    CheckMatchEnd();
                    break;

                case MatchState.Ended:
                    // Waiting for return-to-menu invoke
                    break;
            }
        }

        void StartMatch()
        {
            currentState = MatchState.Playing;
            matchTimer = 0f;
            // TODO: Spawn zone, enable combat, notify clients
        }

        void CheckMatchEnd()
        {
            var players = FindObjectsOfType<PlayerController>(true);
            int alive = 0;
            PlayerController lastAlive = null;

            foreach (var p in players)
            {
                var health = p.GetComponent<PlayerHealth>();
                if (health != null && !health.isDead)
                {
                    alive++;
                    lastAlive = p;
                }
            }

            if (alive <= 1)
            {
                EndMatch(lastAlive);
            }
        }

        void EndMatch(PlayerController winner)
        {
            currentState = MatchState.Ended;
            // TODO: Send results to backend, show results UI
            Invoke(nameof(ReturnToMenu), 5f);
        }

        void ReturnToMenu()
        {
            SceneManager.LoadScene(menuScene);
        }
    }
}
