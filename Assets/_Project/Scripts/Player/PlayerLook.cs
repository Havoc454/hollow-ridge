using UnityEngine;

namespace HollowRidge.Player
{
    /// <summary>
    /// Yaw rotates the player body, pitch rotates the camera rig. Mouse deltas are
    /// already per-frame amounts, so they are NOT scaled by deltaTime; gamepad stick
    /// values are rates, so they are.
    /// </summary>
    [RequireComponent(typeof(PlayerInputReader))]
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] PlayerMovementSettings settings;
        [Tooltip("The child that holds the camera. Only this object pitches.")]
        [SerializeField] Transform cameraRig;

        PlayerInputReader _input;
        float _pitch;

        void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
        }

        void OnEnable()
        {
            _input.MenuPressed += ToggleCursor;
            SetCursorLocked(true);
        }

        void OnDisable()
        {
            _input.MenuPressed -= ToggleCursor;
            SetCursorLocked(false);
        }

        void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked) return;

            Vector2 look = _input.Look;
            Vector2 delta = _input.LookIsPointerDelta
                ? look * settings.mouseSensitivity
                : look * (settings.gamepadSensitivity * Time.deltaTime);

            // Yaw: turn the whole body around its vertical axis.
            transform.Rotate(0f, delta.x, 0f, Space.Self);

            // Pitch: only the camera rig, clamped so it can never flip.
            _pitch += settings.invertY ? delta.y : -delta.y;
            _pitch = Mathf.Clamp(_pitch, settings.minPitch, settings.maxPitch);
            cameraRig.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        // Temporary stand-in for the real pause menu (Phase 11).
        void ToggleCursor()
        {
            SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
        }

        static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}