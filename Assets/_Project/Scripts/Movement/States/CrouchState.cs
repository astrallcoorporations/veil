using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Crouched movement — reduced speed and capsule height.</summary>
    public sealed class CrouchState : IMovementState
    {
        /// <summary>
        /// Sets the crouch capsule height and zeroes vertical velocity at the point of
        /// becoming grounded-and-crouched. Tick deliberately leaves Velocity.y untouched
        /// so it can't clobber a same-tick vertical impulse before the state machine
        /// transitions away — mirrors <see cref="GroundedState"/>'s fix for the same bug class.
        /// </summary>
        public void Enter(MovementContext ctx)
        {
            ctx.Motor.SetHeight(ctx.Settings.CrouchHeight);
            ctx.Velocity = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
        }

        public void Tick(MovementContext ctx)
        {
            Vector3 wishDir = (ctx.Forward * ctx.Input.MoveInput.y + ctx.Right * ctx.Input.MoveInput.x).normalized;
            Vector3 horizontal = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            horizontal = Vector3.MoveTowards(horizontal, wishDir * ctx.Settings.CrouchSpeed, ctx.Settings.GroundAcceleration * ctx.DeltaTime);
            ctx.Velocity = new Vector3(horizontal.x, ctx.Velocity.y, horizontal.z);
        }

        public void Exit(MovementContext ctx) => ctx.Motor.SetHeight(ctx.Settings.StandingHeight);

        public MovementStateId? RequestedTransition(MovementContext ctx)
        {
            if (!ctx.Motor.IsGrounded) return MovementStateId.Air;
            if (!ctx.Input.CrouchHeld) return MovementStateId.Grounded;
            return null;
        }
    }
}
