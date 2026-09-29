namespace LastZone.Services
{
    /// <summary>
    /// Stub matchmaking service. Replace with Lobby/Relay or custom matchmaking later.
    /// </summary>
    public class MatchmakingService
    {
        public bool IsSearching { get; private set; }

        public void StartSearch()
        {
            IsSearching = true;
            // TODO: Integrate with Unity Lobby or custom backend
        }

        public void StopSearch()
        {
            IsSearching = false;
        }
    }
}
