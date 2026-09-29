using UnityEngine;

namespace LastZone.Gameplay.Zone
{
    [CreateAssetMenu(fileName = "ZoneSchedule", menuName = "LastZone/Zone Schedule")]
    public class ZoneSchedule : ScriptableObject
    {
        public Stage[] stages;

        [System.Serializable]
        public class Stage
        {
            public float waitTime;
            public float shrinkTime;
            public float endRadiusPercent;
            public float damagePerSecond;
        }
    }
}
