using UnityEngine;

namespace HollowRidge.Player
{
    /// <summary>
    /// Moves the CharacterController using input from PlayerInputReader and values
    /// from PlayerMovementSettings. No static references: everything is on this prefab.
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public class PlayerMotor : MonoBehaviour
    {
        [SerializeField] PlayerMovementSettings settings;
        [Tooltip("The child that holds the camera. Its height follows crouching.")]
        [SerializeField] Transform cameraRig;
        [Tooltip("What counts as 'something above me' when trying to stand up.")]
        [SerializeField] LayerMask obstructionMask = ~0;

        CharacterController _controller;
        PlayerInputReader _input;

        Vector3 _horizontalVelocity;
        float _verticalVelocity;
        bool _jumpRequested;

        float _currentHeight;
        bool _crouchToggled;
        readonly Collider[] _overlapBuffer = new Collider[8];

        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }
        public Vector3 Velocity => _horizontalVelocity + Vector3.up * _verticalVelocity;
        public PlayerMovementSettings Settings => settings;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputReader>();

            _controller.minMoveDistance = 0f;
            _controller.skinWidth = settings.skinWidth;
            _controller.radius = settings.radius;
            _controller.stepOffset = settings.stepOffset;
            _controller.slopeLimit = settings.slopeLimit;

            _currentHeight = settings.standHeight;
            ApplyHeight();
        }

        void OnEnable()
        {
            _input.JumpPressed += OnJumpPressed;
            _input.CrouchPressed += OnCrouchPressed;
        }

        void OnDisable()
        {
            _input.JumpPressed -= OnJumpPressed;
            _input.CrouchPressed -= OnCrouchPressed;
            _crouchToggled = false;
        }

        void OnJumpPressed() => _jumpRequested = true;

        void OnCrouchPressed()
        {
            if (settings.crouchIsToggle) _crouchToggled = !_crouchToggled;
        }

        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            UpdateCrouch(dt);
            UpdateHorizontal(dt);
            UpdateVertical(dt);

            Vector3 motion = (_horizontalVelocity + Vector3.up * _verticalVelocity) * dt;
            CollisionFlags flags = _controller.Move(motion);
            IsGrounded = _controller.isGrounded;

            // Bumped our head on something while rising: stop going up.
            if ((flags & CollisionFlags.Above) != 0 && _verticalVelocity > 0f)
                _verticalVelocity = 0f;
        }

        // ---------- Crouch ----------

        void UpdateCrouch(float dt)
        {
            bool wantsCrouch = settings.crouchIsToggle ? _crouchToggled : _input.CrouchHeld;

            if (wantsCrouch)
                IsCrouching = true;
            else if (IsCrouching && HasRoomToStand())
                IsCrouching = false; // stands automatically as soon as there is room

            float target = IsCrouching ? settings.crouchHeight : settings.standHeight;
            float blend = 1f - Mathf.Exp(-settings.crouchTransitionSpeed * dt);
            _currentHeight = Mathf.Lerp(_currentHeight, target, blend);
            if (Mathf.Abs(_currentHeight - target) < 0.001f) _currentHeight = target;

            ApplyHeight();
        }

        // Resizes the collider (feet stay fixed) and moves the camera to match.
        void ApplyHeight()
        {
            _controller.height = _currentHeight;
            _controller.center = new Vector3(0f, _currentHeight * 0.5f, 0f);

            if (cameraRig != null)
            {
                float range = Mathf.Max(0.001f, settings.standHeight - settings.crouchHeight);
                float t = Mathf.Clamp01((_currentHeight - settings.crouchHeight) / range);
                float camY = Mathf.Lerp(settings.crouchCameraHeight, settings.standCameraHeight, t);
                Vector3 p = cameraRig.localPosition;
                cameraRig.localPosition = new Vector3(p.x, camY, p.z);
            }
        }

        // Checks only the extra space standing would take up (above the crouched
        // capsule), so floors and ramps under the feet never block standing.
        bool HasRoomToStand()
        {
            float r = settings.radius * 0.95f;
            Vector3 origin = transform.position;
            Vector3 bottom = origin + Vector3.up * (settings.crouchHeight - r);
            Vector3 top = origin + Vector3.up * (settings.standHeight - r);

            int count = Physics.OverlapCapsuleNonAlloc(
                bottom, top, r, _overlapBuffer, obstructionMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                Collider c = _overlapBuffer[i];
                if (c == _controller || c.transform.IsChildOf(transform)) continue;
                return false;
            }
            return true;
        }

        // ---------- Movement ----------

        // Later phases (stamina, exhaustion, injury) plug in here.
        bool CanSprint()
        {
            return true;
        }

        float GetTargetSpeed()
        {
            if (IsCrouching)
            {
                IsSprinting = false;
                return settings.crouchSpeed;
            }

            // Sprint only while pushing forward, and only when allowed.
            IsSprinting = _input.SprintHeld && _input.Move.y > 0.1f && CanSprint();
            return IsSprinting ? settings.sprintSpeed : settings.walkSpeed;
        }

        void UpdateHorizontal(float dt)
        {
            Vector2 move = _input.Move;
            Vector3 wish = transform.right * move.x + transform.forward * move.y;
            Vector3 target = wish * GetTargetSpeed();

            float accel = settings.acceleration * (IsGrounded ? 1f : settings.airControl);
            _horizontalVelocity = Vector3.MoveTowards(_horizontalVelocity, target, accel * dt);
        }

        void UpdateVertical(float dt)
        {
            bool jump = _jumpRequested;
            _jumpRequested = false; // a press only counts on the frame it happens

            if (IsGrounded && jump && !IsCrouching)
            {
                // v = sqrt(2 * g * h) gives a peak of exactly jumpHeight.
                _verticalVelocity = Mathf.Sqrt(2f * settings.gravity * settings.jumpHeight);
            }
            else if (IsGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -settings.groundedStickForce;
            }
            else
            {
                _verticalVelocity -= settings.gravity * dt;
                if (_verticalVelocity < -settings.terminalVelocity)
                    _verticalVelocity = -settings.terminalVelocity;
            }
        }
    }
}