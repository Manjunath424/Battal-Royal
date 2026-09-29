namespace LastZone.Services
{
    // Stubs so the project compiles. Replace with real backend later.
    public class AuthService
    {
        public bool IsSignedIn { get; private set; }
        public string PlayerId { get; private set; } = "local-player";
        public void SignInLocal() { IsSignedIn = true; }
    }

    public class ProfileService
    {
        public string DisplayName = "Player";
    }

    public class MatchmakingService
    {
    }
}
