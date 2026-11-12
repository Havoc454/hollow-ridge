using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HollowRidge.Player
{
    /// <summary>
    /// The only script that touches the Input Actions asset. Everything else reads
    /// values and events from here, so input can be swapped, disabled or (later)
    /// duplicated per player without touching other scripts.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour, HollowRidgeInput.IPlayerActions
    {
        HollowRidgeInput _input;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        /// <summary>True when Look is a per-frame pointer delta (mouse), false for stick values.</summary>
        public bool LookIsPointerDelta { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool CrouchHeld { get; private set; }

        public event Action JumpPressed;
        public event Action CrouchPressed;
        public event Action MenuPressed;

        void Awake()
        {
            _input = new HollowRidgeInput();
            _input.Player.SetCallbacks(this);
        }

        void OnEnable() => _input.Player.Enable();

        void OnDisable()
        {
            _input.Player.Disable();
            Move = Look = Vector2.zero;
            SprintHeld = CrouchHeld = false;
        }

        void OnDestroy() => _input?.Dispose();

        public void OnMove(InputAction.CallbackContext context)
        {
            Move = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            Look = context.ReadValue<Vector2>();
            if (context.performed)
                LookIsPointerDelta = context.control.device is Pointer;
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed) JumpPressed?.Invoke();
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            SprintHeld = context.ReadValueAsButton();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            CrouchHeld = context.ReadValueAsButton();
            if (context.performed) CrouchPressed?.Invoke();
        }

        public void OnMenu(InputAction.CallbackContext context)
        {
            if (context.performed) MenuPressed?.Invoke();
        }
    }
}