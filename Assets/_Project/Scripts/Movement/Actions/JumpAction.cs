using UnityEngine;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>
    /// Instantaneous upward jump impulse. Lowest action priority so vault/mantle/slide
    /// take precedence when available — jump is the fallback, not the primary traversal tool.
    /// </summary>
    public sealed class JumpAction : IMovementAction
    {
        /// <summary>Higher wins when multiple actions are simultaneously valid.</summary>
        public int Priority => 10;

        /// <summary>True while this action is currently driving movement.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether this action's preconditions are currently met.</summary>
        public bool CanExecute(MovementContext ctx, MovementStateId currentState) =>
            !IsActive && currentState == MovementStateId.Grounded && ctx.Motor.IsGrounded;

        /// <summary>Applies an upward velocity impulse derived from gravity and jump height, preserving horizontal velocity.</summary>
        public void Execute(MovementContext ctx)
        {
            float jumpSpeed = Mathf.Sqrt(2f * -ctx.Settings.Gravity * ctx.Settings.JumpHeight);
            ctx.Velocity = new Vector3(ctx.Velocity.x, jumpSpeed, ctx.Velocity.z);
        }

        /// <summary>No-op — jump is a single-frame impulse, not a multi-frame action.</summary>
        public void Tick(MovementContext ctx) { }

        /// <summary>No-op — jump has nothing to cancel once fired.</summary>
        public void Cancel(MovementContext ctx) { }
    }
}
