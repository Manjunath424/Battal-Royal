using UnityEngine;
using Unity.Netcode;

namespace LastZone.Gameplay.Player
{
    /// Owner-authoritative movement. Player prefab needs ClientNetworkTransform to sync position.
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement")]
        public float runSpeed = 5.5f;
        public float sprintSpeed = 7.5f;
        public float crouchSpeed = 2f;
        public float proneSpeed = 1f;
        public float jumpForce = 6f;
        public float gravity = -20f;
        public float staminaMax = 100f;
        public float staminaDrainRate = 20f;
        public float staminaRegenRate = 12.5f;

        [Header("State")]
        public bool isCrouching;
        public bool isProne;
        public bool isSprinting;

        CharacterController cc;
        PlayerHealth health;
        Vector3 moveInput;
        float yaw;
        float verticalVelocity;
        float stamina;
        bool jumpRequested;

        public override void OnNetworkSpawn()
        {
            cc = GetComponent<CharacterController>();
            health = GetComponent<PlayerHealth>();
            stamina = staminaMax;
            if (!IsOwner) enabled = false;
        }

        public void SetInput(float h, float v, bool sprint, bool crouch, bool prone, bool jump, float cameraYaw)
        {
            moveInput = new Vector3(h, 0f, v);
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
            yaw = cameraYaw;

            isCrouching = crouch;
            isProne = prone;
            isSprinting = sprint && stamina > 0f && moveInput.sqrMagnitude > 0.01f && !crouch && !prone;
            if (jump) jumpRequested = true;
        }

        void Update()
        {
            if (!IsOwner || cc == null) return;
            if (health != null && health.isDead) return;

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            HandleStamina();
            Move();
        }

        void HandleStamina()
        {
            if (isSprinting)
                stamina = Mathf.Max(0f, stamina - staminaDrainRate * Time.deltaTime);
            else
                stamina = Mathf.Min(staminaMax, stamina + staminaRegenRate * Time.deltaTime);

            if (stamina <= 0f) isSprinting = false;
        }

        void Move()
        {
            float speed = isProne ? proneSpeed : isCrouching ? crouchSpeed : isSprinting ? sprintSpeed : runSpeed;
            Vector3 horizontal = Quaternion.Euler(0f, yaw, 0f) * moveInput * speed;

            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            if (jumpRequested)
            {
                if (cc.isGrounded && !isProne) verticalVelocity = jumpForce;
                jumpRequested = false;
            }
            verticalVelocity += gravity * Time.deltaTime;

            cc.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }
}
