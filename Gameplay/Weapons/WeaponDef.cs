using UnityEngine;

namespace LastZone.Gameplay.Weapons
{
    /// <summary>
    /// ScriptableObject defining weapon stats. Create via Assets → Create → LastZone → Weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "LastZone/Weapon")]
    public class WeaponDef : ScriptableObject
    {
        [Header("Identity")]
        public string weaponName = "Weapon";

        [Header("Damage")]
        public float damage = 20f;
        public float headshotMultiplier = 2f;

        [Header("Fire")]
        public float fireRate = 10f;        // rounds per second
        public int magazineSize = 30;
        public float reloadTime = 1.7f;     // seconds

        [Header("Range & Accuracy")]
        public float effectiveRange = 70f;  // meters
        public float spreadHip = 2f;        // degrees
        public float spreadADS = 0.5f;      // degrees
    }
}
