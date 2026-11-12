using UnityEngine;

namespace HollowRidge.Player
{
    /// <summary>
    /// All tunable player movement values. Later systems (injury, stamina, cold,
    /// carry weight) will apply multipliers on top of these rather than editing them.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementSettings", menuName = "Hollow Ridge/Player Movement Settings")]
    public class PlayerMovementSettings : ScriptableObject
    {
        [Header("Speed (metres per second)")]
        [Min(0f)] public float walkSpeed = 3.5f;
        [Min(0f)] public float sprintSpeed = 6f;
        [Min(0f)] public float crouchSpeed = 1.8f;
        [Tooltip("How quickly horizontal speed changes (m/s per second).")]
        [Min(0f)] public float acceleration = 40f;
        [Tooltip("Fraction of ground acceleration available while airborne (0-1).")]
        [Range(0f, 1f)] public float airControl = 0.3f;

        [Header("Jump and gravity")]
        [Tooltip("Peak height of a jump in metres.")]
        [Min(0f)] public float jumpHeight = 1.1f;
        [Tooltip("Gravity strength in m/s^2 (positive number).")]
        [Min(0f)] public float gravity = 20f;
        [Tooltip("Largest downward speed in m/s.")]
        [Min(0f)] public float terminalVelocity = 50f;
        [Tooltip("Small downward push while grounded so the controller stays glued to slopes and steps.")]
        [Min(0f)] public float groundedStickForce = 2f;

        [Header("Body and collider")]
        [Min(0.5f)] public float standHeight = 1.8f;
        [Min(0.5f)] public float crouchHeight = 1.0f;
        [Min(0.1f)] public float radius = 0.3f;
        [Tooltip("Highest step the player walks up automatically (metres).")]
        [Min(0f)] public float stepOffset = 0.35f;
        [Range(0f, 89f)] public float slopeLimit = 45f;
        [Min(0.01f)] public float skinWidth = 0.05f;

        [Header("Camera")]
        [Tooltip("Camera height above the feet when standing.")]
        public float standCameraHeight = 1.65f;
        [Tooltip("Camera height above the feet when crouching.")]
        public float crouchCameraHeight = 0.85f;
        [Tooltip("How fast the body and camera blend between stand and crouch.")]
        [Min(0.1f)] public float crouchTransitionSpeed = 10f;

        [Header("Look")]
        [Tooltip("Degrees per pixel of mouse movement.")]
        [Min(0f)] public float mouseSensitivity = 0.1f;
        [Tooltip("Degrees per second at full stick deflection.")]
        [Min(0f)] public float gamepadSensitivity = 140f;
        public float minPitch = -85f;
        public float maxPitch = 85f;
        public bool invertY = false;

        [Header("Crouch behaviour")]
        [Tooltip("True = press to toggle crouch. False = hold to crouch.")]
        public bool crouchIsToggle = false;
    }
}