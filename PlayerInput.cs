using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Weapons;

namespace LastZone.Gameplay.Player
{
    /// Keyboard/mouse input (old Input Manager). Replace this class for touch on Android later.
    public class PlayerInput : NetworkBehaviour
    {
        [Header("References (auto-found if empty)")]
        public PlayerController controller;
        public PlayerCamera playerCamera;
        public Weapon currentWeapon;

        public override void OnNetworkSpawn()
        {
            if (controller == null) controller = GetComponent<PlayerController>();
            if (playerCamera == null) playerCamera = GetComponentInChildren<PlayerCamera>(true);
            if (currentWeapon == null) currentWeapon = GetComponentInChildren<Weapon>();

            if (!IsOwner)
            {
                // Remote players must not have an active camera / audio listener
                if (playerCamera != null && playerCamera.gameObject != gameObject)
                    playerCamera.gameObject.SetActive(false);
                enabled = false;
                return;
            }

            if (playerCamera != null) playerCamera.Init(transform);
            if (currentWeapon != null && playerCamera != null) currentWeapon.aimTransform = playerCamera.transform;
        }

        void Update()
        {
            if (!IsOwner) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            bool sprint = Input.GetKey(KeyCode.LeftShift);
            bool crouch = Input.GetKey(KeyCode.C);
            bool prone = Input.GetKey(KeyCode.Z);
            bool jump = Input.GetKeyDown(KeyCode.Space);
            bool aim = Input.GetButton("Fire2");

            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);

            float yaw = playerCamera != null ? playerCamera.Yaw : transform.eulerAngles.y;
            if (controller != null) controller.SetInput(h, v, sprint, crouch, prone, jump, yaw);
            if (playerCamera != null) playerCamera.SetAiming(aim);
            // Fire / reload are read inside Weapon.
        }

        void SwitchWeapon(int index)
        {
            // TODO: multiple-weapon inventory
        }
    }
}
