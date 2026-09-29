using Unity.Netcode.Components;

namespace LastZone.Gameplay.Player
{
    // Owner-authoritative transform for the PLAYER prefab (bots use the normal NetworkTransform).
    // NGO 2.x: you can skip this file and set "Authority Mode = Owner" on the built-in NetworkTransform.
#pragma warning disable CS0618
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative() => false;
    }
#pragma warning restore CS0618
}
