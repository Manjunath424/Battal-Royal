using UnityEngine;

namespace LastZone.Gameplay.Zone
{
    /// <summary>
    /// Defines the zone shrink schedule. Create via Assets → Create → LastZone → Zone Schedule.
    /// </summary>
    [CreateAssetMenu(fileName = "ZoneSchedule", menuName = "LastZone/Zone Schedule")]
    public class ZoneSchedule : ScriptableObject
    {
        public Stage[] stages;

        [System.Serializable]
        public class Stage
        {
            [Tooltip("Seconds to wait before this stage starts shrinking")]
            public float waitTime = 90f;

            [Tooltip("Seconds the shrink takes")]
            public float shrinkTime = 60f;

            [Tooltip("Zone radius as a fraction of mapRadius after shrink completes (0-1)")]
            [Range(0f, 1f)]
            public float endRadiusPercent = 0.5f;

            [Tooltip("HP/s damage to players outside the zone during this stage")]
            public float damagePerSecond = 1f;
        }
    }
}
