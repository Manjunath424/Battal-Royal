using UnityEngine;

namespace LastZone.Gameplay.Player
{
    /// Plain MonoBehaviour on a CHILD object of the player prefab that has Camera + AudioListener.
    /// PlayerInput enables it for the owner and disables the whole child for remote players.
    public class PlayerCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 3f;
        public float height = 1.5f;
        public float lookSensitivity = 2f;

        [Header("Aim")]
        public float aimDistance = 1.5f;
        public float aimHeight = 1.6f;
        public bool isAiming;

        float yaw;
        float pitch;
        bool locked;

        public float Yaw => yaw;

        public void Init(Transform followTarget)
        {
            target = followTarget;
            yaw = target.eulerAngles.y;
            pitch = 10f;
            SetCursorLocked(true);
        }

        public void SetAiming(bool aiming) => isAiming = aiming;

        void SetCursorLocked(bool value)
        {
            locked = value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !value;
        }

        void LateUpdate()
        {
            if (target == null) return;

            if (Input.GetKeyDown(KeyCode.Escape)) SetCursorLocked(false);
            else if (!locked && Input.GetMouseButtonDown(0)) SetCursorLocked(true);

            if (locked)
            {
                yaw += Input.GetAxis("Mouse X") * lookSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
                pitch = Mathf.Clamp(pitch, -40f, 60f);
            }

            float d = isAiming ? aimDistance : distance;
            float h = isAiming ? aimHeight : height;

            Vector3 pivot = target.position + Vector3.up * h;
            Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = pivot + rot * new Vector3(0f, 0f, -d);
            transform.rotation = rot;
        }
    }
}
