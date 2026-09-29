using UnityEngine;
using Unity.Netcode;

namespace LastZone.Gameplay.Player
{
    /// <summary>
    /// Third-person camera with over-the-shoulder ADS zoom.
    /// Attach to a child GameObject with a Camera component on the player prefab.
    /// </summary>
    public class PlayerCamera : NetworkBehaviour
    {
        [Header("Follow Target")]
        public Transform target; // Assign player's root or a head bone

        [Header("Normal View")]
        public float distance = 3f;
        public float height = 1.5f;

        [Header("ADS View")]
        public float aimDistance = 1.2f;
        public float aimHeight = 0.8f;

        [Header("Controls")]
        public float rotationSpeed = 10f;
        public float minPitch = -40f;
        public float maxPitch = 60f;

        [Header("State")]
        public bool isAiming;

        float yaw;
        float pitch;
        Camera cam;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!IsOwner)
            {
                // Disable camera for remote players
                cam = GetComponent<Camera>();
                if (cam != null) cam.enabled = false;
                var listener = GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
                enabled = false;
                return;
            }

            cam = GetComponent<Camera>();
            if (target == null) target = transform.parent; // fallback to parent

            yaw = transform.eulerAngles.y;
            pitch = 0f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void LateUpdate()
        {
            if (!IsOwner || target == null) return;

            // Mouse look
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // Rotate the player body horizontally to face camera direction
            if (target.parent != null)
            {
                target.parent.rotation = Quaternion.Euler(0f, yaw, 0f);
            }

            // Camera orbit
            float currentDistance = isAiming ? aimDistance : distance;
            float currentHeight = isAiming ? aimHeight : height;

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, currentHeight, -currentDistance);
            Vector3 camPosition = target.position + offset;

            transform.position = camPosition;
            transform.LookAt(target.position + Vector3.up * 0.5f);
        }

        public void SetAiming(bool aiming)
        {
            isAiming = aiming;
        }
    }
}
