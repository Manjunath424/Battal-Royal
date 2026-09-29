using LastZone.Services;

namespace LastZone.Core
{
    public class ServiceLocator
    {
        public AuthService Auth { get; private set; }
        public ProfileService Profile { get; private set; }
        public MatchmakingService Matchmaking { get; private set; }

        public void Initialize()
        {
            Auth = new AuthService();
            Profile = new ProfileService();
            Matchmaking = new MatchmakingService();
        }
    }
}
