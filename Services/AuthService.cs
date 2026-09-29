namespace LastZone.Services
{
    /// <summary>
    /// Stub authentication service. Replace with Firebase/PlayFab/custom auth later.
    /// </summary>
    public class AuthService
    {
        public bool IsAuthenticated { get; private set; }
        public string PlayerId { get; private set; }

        public void LoginAsGuest()
        {
            PlayerId = System.Guid.NewGuid().ToString();
            IsAuthenticated = true;
        }

        public void Logout()
        {
            PlayerId = null;
            IsAuthenticated = false;
        }
    }
}
