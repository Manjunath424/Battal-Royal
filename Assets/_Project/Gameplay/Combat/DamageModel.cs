using UnityEngine;

namespace LastZone.Gameplay.Combat
{
    /// Optional helper (not used by Weapon yet - its falloff reaches 0 at max range).
    public static class DamageModel
    {
        public static float CalculateDamage(float baseDamage, float distance, float maxRange, float hitZoneMultiplier)
        {
            float falloff = Mathf.Clamp01(1f - (distance / maxRange));
            return baseDamage * falloff * hitZoneMultiplier;
        }
    }
}
