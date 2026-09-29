using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;

namespace LastZone.Gameplay
{
    /// <summary>
    /// Handles spawning players and bots at designated spawn points.
    /// Server-authoritative.
    /// </summary>
    public class SpawnManager : NetworkBehaviour
    {
        [Header("Spawn Points (assign in Inspector)")]
        public Transform[] spawnPoints;

        [Header("Prefabs (must be registered as NetworkPrefabs)")]
        public GameObject playerPrefab;
        public GameObject botPrefab;

        [Header("Bot Settings")]
        public int botsToSpawn = 4;

        int nextSpawnIndex;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsServer) return;

            // Spawn bots at some spawn points
            int botCount = Mathf.Min(botsToSpawn, spawnPoints.Length);
            for (int i = 0; i < botCount; i++)
            {
                SpawnBot(spawnPoints[i]);
            }
            nextSpawnIndex = botCount;
        }

        /// <summary>
        /// Call from connection approval or GameManager when a new client connects.
        /// </summary>
        public void SpawnPlayer(ulong clientId)
        {
            if (!IsServer) return;
            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("[SpawnManager] No spawn points assigned!");
                return;
            }

            Transform sp = spawnPoints[nextSpawnIndex % spawnPoints.Length];
            nextSpawnIndex++;

            var player = Instantiate(playerPrefab, sp.position, sp.rotation);
            var netObj = player.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.SpawnAsPlayerObject(clientId);
            }
            else
            {
                Debug.LogError("[SpawnManager] playerPrefab is missing NetworkObject component!");
                Destroy(player);
            }
        }

        void SpawnBot(Transform spawnPoint)
        {
            if (botPrefab == null) return;

            var bot = Instantiate(botPrefab, spawnPoint.position, spawnPoint.rotation);
            var netObj = bot.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn(); // Server-owned
            }
            else
            {
                Debug.LogError("[SpawnManager] botPrefab is missing NetworkObject component!");
                Destroy(bot);
            }
        }
    }
}
