using UnityEngine;

namespace LastZone.Gameplay.Combat
{
    /// <summary>
    /// Static utility for damage calculations: distance falloff, hit-zone multipliers.
    /// </summary>
    public static class DamageModel
    {
        /// <summary>
        /// Full damage calculation with distance falloff and hit-zone multiplier.
        /// </summary>
        public static float CalculateDamage(float baseDamage, float distance, float maxRange, float hitZoneMultiplier)
        {
            float falloff = CalculateFalloff(distance, maxRange);
            return baseDamage * falloff * hitZoneMultiplier;
        }

        /// <summary>
        /// Returns a 0-1 multiplier based on distance vs effective range.
        /// Full damage up to 50% of range, then linear falloff to 0 at max range.
        /// </summary>
        public static float CalculateFalloff(float distance, float maxRange)
        {
            if (maxRange <= 0f) return 1f;
            // Full damage up to half range, then linear drop
            float halfRange = maxRange * 0.5f;
            if (distance <= halfRange) return 1f;
            return Mathf.Clamp01(1f - ((distance - halfRange) / halfRange));
        }
    }
}
