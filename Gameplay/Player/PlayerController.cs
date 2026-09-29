using UnityEngine;
using Unity.Netcode;

namespace LastZone.Gameplay.Player
{
    /// <summary>
    /// Handles player movement, stamina, and stance (crouch/prone/sprint).
    /// Receives input from PlayerInput via SetInput() — does NOT read Input directly.
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement Speeds (m/s)")]
        public float walkSpeed = 3.5f;
        public float runSpeed = 5.5f;
        public float sprintSpeed = 7.5f;
        public float crouchSpeed = 2f;
        public float proneSpeed = 1f;

        [Header("Jump")]
        public float jumpForce = 4f;
        public float gravity = -9.81f;

        [Header("Stamina")]
        public float staminaMax = 100f;
        public float staminaDrainRate = 20f;   // per second while sprinting
        public float staminaRegenRate = 12.5f;  // per second while not sprinting

        [Header("State (read-only at runtime)")]
        public bool isCrouching;
        public bool isProne;
        public bool isSprinting;

        CharacterController characterController;
        Vector3 moveInput;
        float staminaCurrent;
        float verticalVelocity;
        bool jumpRequested;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            characterController = GetComponent<CharacterController>();
            staminaCurrent = staminaMax;

            // Disable Update on non-owner instances (server interpolates these)
            if (!IsOwner) enabled = false;
        }

        /// <summary>
        /// Called by PlayerInput each frame with the current input state.
        /// </summary>
        public void SetInput(float h, float v, bool sprint, bool crouch, bool prone, bool jump)
        {
            moveInput = new Vector3(h, 0f, v);
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

            isSprinting = sprint && staminaCurrent > 0f && moveInput.sqrMagnitude > 0.01f;
            isCrouching = crouch && !isProne;
            isProne = prone;

            if (jump && characterController.isGrounded && !isProne)
            {
                jumpRequested = true;
            }
        }

        void Update()
        {
            if (!IsOwner) return;

            HandleStamina();
            HandleGravityAndJump();
            Move();
        }

        void HandleStamina()
        {
            if (isSprinting && moveInput.sqrMagnitude > 0.01f)
                staminaCurrent = Mathf.Max(0f, staminaCurrent - staminaDrainRate * Time.deltaTime);
            else
                staminaCurrent = Mathf.Min(staminaMax, staminaCurrent + staminaRegenRate * Time.deltaTime);

            if (staminaCurrent <= 0f) isSprinting = false;
        }

        void HandleGravityAndJump()
        {
            if (characterController.isGrounded)
            {
                verticalVelocity = -0.5f; // small downward force to keep grounded

                if (jumpRequested)
                {
                    verticalVelocity = jumpForce;
                    jumpRequested = false;
                }
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }

        void Move()
        {
            float speed;
            if (isProne)
                speed = proneSpeed;
            else if (isCrouching)
                speed = crouchSpeed;
            else if (isSprinting)
                speed = sprintSpeed;
            else
                speed = runSpeed;

            Vector3 horizontalMotion = transform.TransformDirection(moveInput) * speed;
            Vector3 motion = new Vector3(horizontalMotion.x, verticalVelocity, horizontalMotion.z);

            characterController.Move(motion * Time.deltaTime);
        }

        /// <summary>Returns stamina as 0-1 ratio for UI.</summary>
        public float GetStaminaNormalized() => staminaCurrent / staminaMax;
    }
}
