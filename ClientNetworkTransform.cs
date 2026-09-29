using Unity.Netcode.Components;

namespace LastZone.Networking
{
    /// Owner-authoritative transform sync. Use on the PLAYER prefab (the owner moves it
    /// with CharacterController). Bots use the normal server-authoritative NetworkTransform.
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
}
