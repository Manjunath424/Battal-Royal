using UnityEngine;

namespace LastZone.Gameplay.Weapons
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "LastZone/Weapon")]
    public class WeaponDef : ScriptableObject
    {
        public string weaponName;
        public float damage = 20f;
        public float fireRate = 10f;   // rounds per second
        public int magazineSize = 30;
        public float reloadTime = 1.7f;
        public float effectiveRange = 70f;
        public float headshotMultiplier = 2f;
        public float spreadHip = 2f;   // not used yet
        public float spreadADS = 0.5f; // not used yet
    }
}
