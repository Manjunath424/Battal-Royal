namespace LastZone.Services
{
    /// <summary>
    /// Stub profile service. Holds player display name, stats, etc.
    /// </summary>
    public class ProfileService
    {
        public string DisplayName { get; set; } = "Player";
        public int TotalKills { get; set; }
        public int TotalWins { get; set; }
    }
}
