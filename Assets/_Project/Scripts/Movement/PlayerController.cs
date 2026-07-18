using UnityEngine;
using Veil.Camera;
using Veil.Input;
using Veil.Interaction;
using Veil.Movement.Actions;
using Veil.Movement.States;

namespace Veil.Movement
{
    /// <summary>
    /// Owns and ticks one player's full movement stack: motor, state machine, and
    /// action controller. Also drives camera tilt from live slide state. This is the
    /// only class that wires Movement, Camera, and Actions together — everything else
    /// stays decoupled behind interfaces.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private InputReader input;
        [SerializeField] private MovementSettings movementSettings;
        [SerializeField] private CameraController cameraController;

        private MovementContext _ctx;
        private MovementStateMachine _stateMachine;
        private ActionController _actionController;
        private SlideAction _slideAction;

        private void Awake()
        {
            _ctx = new MovementContext(motor, input, movementSettings);
            _stateMachine = new MovementStateMachine(_ctx);
            _actionController = new ActionController();

            _slideAction = new SlideAction();
            _actionController.RegisterAction(_slideAction);
            _actionController.RegisterAction(new VaultAction());
            _actionController.RegisterAction(new MantleAction());
        }

        private void OnEnable()
        {
            input.Enable();
            input.JumpVaultPressed += OnActionTriggerPressed;
            input.SlidePressed += OnActionTriggerPressed;
        }

        private void OnDisable()
        {
            input.JumpVaultPressed -= OnActionTriggerPressed;
            input.SlidePressed -= OnActionTriggerPressed;
            input.Disable();
        }

        private void OnActionTriggerPressed() => _actionController.TryTriggerBestAction(_ctx, _stateMachine.Current);

        private void Update()
        {
            _ctx.DeltaTime = Time.deltaTime;
            TickMovement(_ctx, _stateMachine, _actionController, Time.deltaTime);
            motor.Move(_ctx.Velocity, Time.deltaTime);

            if (cameraController != null)
            {
                cameraController.ApplyTilt(input.MoveInput.x, _slideAction.IsActive);
            }
        }

        /// <summary>
        /// Advances one movement tick: the state machine drives velocity unless an
        /// action is currently active, in which case the action owns velocity instead.
        /// Static and side-effect-visible-via-ctx so it's directly unit-testable
        /// without a live MonoBehaviour/scene.
        /// </summary>
        public static void TickMovement(MovementContext ctx, MovementStateMachine stateMachine, ActionController actionController, float deltaTime)
        {
            if (!actionController.HasActiveAction)
            {
                stateMachine.Tick(ctx);
            }
            actionController.Tick(ctx);
        }
    }
}
