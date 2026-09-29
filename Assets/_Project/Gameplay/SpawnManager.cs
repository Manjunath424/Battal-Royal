using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace LastZone.Gameplay
{
    /// Scene-placed NetworkObject in Island. NetworkManager's "Player Prefab" field must be LEFT EMPTY -
    /// this class spawns the player objects (so they appear at spawn points in the Island scene).
    /// playerPrefab / botPrefab must be in the NetworkManager's Network Prefabs list.
    public class SpawnManager : NetworkBehaviour
    {
        public Transform[] spawnPoints;
        public GameObject playerPrefab;
        public GameObject botPrefab;
        public int botCount = 7;

        readonly HashSet<ulong> spawnedClients = new HashSet<ulong>();
        int playerSlot;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            NetworkManager.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            NetworkManager.OnClientConnectedCallback += OnClientConnected;

            SpawnBots();
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager == null) return;
            if (NetworkManager.SceneManager != null)
                NetworkManager.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        void OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode,
                                  List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            foreach (var id in clientsCompleted) SpawnPlayer(id);
        }

        void OnClientConnected(ulong clientId) => SpawnPlayer(clientId); // late joiners

        public void SpawnPlayer(ulong clientId)
        {
            if (!IsServer || playerPrefab == null) return;
            if (!spawnedClients.Add(clientId)) return;

            Pose p = GetSpawnPose(playerSlot++);
            var go = Instantiate(playerPrefab, p.position, p.rotation);
            go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }

        void SpawnBots()
        {
            if (botPrefab == null) return;
            for (int i = 0; i < botCount; i++)
            {
                // bots use spawn points from the END of the array so players get the first ones
                int idx = spawnPoints.Length > 0 ? (spawnPoints.Length - 1 - i % spawnPoints.Length) : 0;
                Pose p = GetSpawnPose(idx);
                var go = Instantiate(botPrefab, p.position, p.rotation);
                go.GetComponent<NetworkObject>().Spawn(true);
            }
        }

        Pose GetSpawnPose(int index)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
                return new Pose(new Vector3(index * 2f, 1f, 0f), Quaternion.identity);
            Transform sp = spawnPoints[index % spawnPoints.Length];
            return new Pose(sp.position, sp.rotation);
        }
    }
}
