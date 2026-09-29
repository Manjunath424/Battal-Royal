using System;
using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Player;
using LastZone.Gameplay.Zone;

namespace LastZone.Core
{
    /// Scene-placed NetworkObject in the Island scene (needs a NetworkObject component).
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static event Action<ulong, bool> MatchEnded; // winnerClientId, hasWinner

        [Header("Scenes")]
        public string menuScene = "Menu";
        public string matchScene = "Island";

        [Header("Match Settings")]
        public int minPlayersToStart = 2;      // bots count too
        public float countdownTime = 10f;
        public float returnToMenuDelay = 8f;

        public enum MatchState { Waiting, Countdown, Playing, Ended }

        readonly NetworkVariable<MatchState> state = new NetworkVariable<MatchState>(MatchState.Waiting);
        readonly NetworkVariable<float> matchTime = new NetworkVariable<float>(0f);
        readonly NetworkVariable<float> countdown = new NetworkVariable<float>(0f);

        public MatchState currentState => state.Value;
        public float matchTimer => matchTime.Value;
        public float countdownTimer => countdown.Value;

        public override void OnNetworkSpawn()
        {
            Instance = this;
            if (IsServer)
            {
                state.Value = MatchState.Waiting;
                matchTime.Value = 0f;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (!IsServer || !IsSpawned) return;

            switch (state.Value)
            {
                case MatchState.Waiting:
                    if (PlayerHealth.All.Count >= minPlayersToStart)
                    {
                        state.Value = MatchState.Countdown;
                        countdown.Value = countdownTime;
                    }
                    break;

                case MatchState.Countdown:
                    countdown.Value -= Time.deltaTime;
                    if (countdown.Value <= 0f) StartMatch();
                    break;

                case MatchState.Playing:
                    matchTime.Value += Time.deltaTime;
                    CheckMatchEnd();
                    break;
            }
        }

        void StartMatch()
        {
            state.Value = MatchState.Playing;
            matchTime.Value = 0f;
            var zone = FindObjectOfType<ZoneController>();
            if (zone != null) zone.StartZone();
        }

        void CheckMatchEnd()
        {
            PlayerHealth last = null;
            int alive = 0;
            foreach (var p in PlayerHealth.All)
            {
                if (p != null && !p.isDead) { alive++; last = p; }
            }
            if (alive <= 1) EndMatch(last);
        }

        void EndMatch(PlayerHealth winner)
        {
            state.Value = MatchState.Ended;
            bool has = winner != null;
            MatchEndedClientRpc(has ? winner.OwnerClientId : 0, has);
        }

        [ClientRpc]
        void MatchEndedClientRpc(ulong winnerClientId, bool hasWinner)
        {
            MatchEnded?.Invoke(winnerClientId, hasWinner);
            Bootstrap.GoToMenu(returnToMenuDelay);
        }
    }
}
