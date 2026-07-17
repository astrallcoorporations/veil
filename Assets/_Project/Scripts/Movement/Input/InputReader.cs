using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Veil.Input
{
    /// <summary>
    /// ScriptableObject wrapper around the generated <see cref="VeilControls"/> Input System
    /// actions. Every gameplay system reads player input through this asset instead of
    /// binding to Input System actions directly, so input can be swapped, mocked, or
    /// rebound without touching movement/camera/interaction code.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "VEIL/Input Reader")]
    public sealed class InputReader : ScriptableObject, VeilControls.IGameplayActions
    {
        private VeilControls _controls;

        /// <summary>Current move axis, range [-1,1] per component.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Current look delta for this frame.</summary>
        public Vector2 LookInput { get; private set; }

        /// <summary>True while the sprint button is held.</summary>
        public bool SprintHeld { get; private set; }

        /// <summary>True while the crouch button is held.</summary>
        public bool CrouchHeld { get; private set; }

        /// <summary>Fires on the frame the jump/vault button is pressed.</summary>
        public event Action JumpVaultPressed;

        /// <summary>Fires on the frame the slide button is pressed.</summary>
        public event Action SlidePressed;

        /// <summary>Fires on the frame the interact button is pressed.</summary>
        public event Action InteractPressed;

        /// <summary>Fires on the frame the grab button is pressed.</summary>
        public event Action GrabPressed;

        /// <summary>Enables the underlying Input System action map. Call from OnEnable of the owning behaviour.</summary>
        public void Enable()
        {
            if (_controls == null)
            {
                _controls = new VeilControls();
                _controls.Gameplay.SetCallbacks(this);
            }
            _controls.Gameplay.Enable();
        }

        /// <summary>Disables the underlying Input System action map. Call from OnDisable of the owning behaviour.</summary>
        public void Disable()
        {
            _controls?.Gameplay.Disable();
        }

        void VeilControls.IGameplayActions.OnMove(InputAction.CallbackContext context) =>
            MoveInput = context.ReadValue<Vector2>();

        void VeilControls.IGameplayActions.OnLook(InputAction.CallbackContext context) =>
            LookInput = context.ReadValue<Vector2>();

        void VeilControls.IGameplayActions.OnSprint(InputAction.CallbackContext context) =>
            SprintHeld = context.ReadValueAsButton();

        void VeilControls.IGameplayActions.OnCrouch(InputAction.CallbackContext context) =>
            CrouchHeld = context.ReadValueAsButton();

        void VeilControls.IGameplayActions.OnSlide(InputAction.CallbackContext context)
        {
            if (context.performed) SlidePressed?.Invoke();
        }

        void VeilControls.IGameplayActions.OnJumpVault(InputAction.CallbackContext context)
        {
            if (context.performed) JumpVaultPressed?.Invoke();
        }

        void VeilControls.IGameplayActions.OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed) InteractPressed?.Invoke();
        }

        void VeilControls.IGameplayActions.OnGrab(InputAction.CallbackContext context)
        {
            if (context.performed) GrabPressed?.Invoke();
        }
    }
}
