using UnityEngine;
using Unity.Netcode;
using LastZone.Gameplay.Weapons;

namespace LastZone.Gameplay.Player
{
    /// <summary>
    /// Reads keyboard/mouse input and forwards it to PlayerController, PlayerCamera, and Weapon.
    /// PC-first; replace with touch joystick input for Android later.
    /// </summary>
    public class PlayerInput : NetworkBehaviour
    {
        [Header("References (assign in Inspector)")]
        public PlayerController controller;
        public PlayerCamera playerCamera;
        public Weapon currentWeapon;

        void Update()
        {
            if (!IsOwner) return;

            // --- Movement axes ---
            float h = Input.GetAxis("Horizontal"); // A/D
            float v = Input.GetAxis("Vertical");   // W/S

            // --- Stance / sprint ---
            bool sprint = Input.GetKey(KeyCode.LeftShift);
            bool crouch = Input.GetKey(KeyCode.C);
            bool prone  = Input.GetKey(KeyCode.Z);
            bool jump   = Input.GetKeyDown(KeyCode.Space);

            // --- Combat ---
            bool aim    = Input.GetButton("Fire2"); // Right mouse
            bool fire   = Input.GetButton("Fire1"); // Left mouse
            bool reload = Input.GetKeyDown(KeyCode.R);

            // --- Weapon switch ---
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchWeapon(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchWeapon(2);

            // --- Toggle debug HUD ---
            // (handled by DebugHUD itself via F3, but could also go here)

            // Forward to controller
            if (controller != null)
            {
                controller.SetInput(h, v, sprint, crouch, prone, jump);
            }

            // Forward to camera
            if (playerCamera != null)
            {
                playerCamera.SetAiming(aim);
            }

            // Forward to weapon (fire/reload handled inside Weapon.Update,
            // but we could also call weapon methods here for cleaner separation)
        }

        void SwitchWeapon(int index)
        {
            // TODO: Switch between weapons in inventory
        }
    }
}
