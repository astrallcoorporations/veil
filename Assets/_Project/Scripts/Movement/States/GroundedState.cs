using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Standing/walking/sprinting on walkable ground.</summary>
    public sealed class GroundedState : IMovementState
    {
        /// <summary>
        /// Zeroes vertical velocity at the point of becoming grounded. This is the
        /// correct semantic moment to kill any residual vertical speed (e.g. from a
        /// landing) — Tick deliberately leaves Velocity.y untouched so it can't
        /// clobber a same-tick jump impulse before the state machine transitions to Air.
        /// </summary>
        public void Enter(MovementContext ctx) => ctx.Velocity = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);

        public void Tick(MovementContext ctx)
        {
            float speed = ctx.Input.SprintHeld ? ctx.Settings.SprintSpeed : ctx.Settings.WalkSpeed;
            Vector3 wishDir = (ctx.Forward * ctx.Input.MoveInput.y + ctx.Right * ctx.Input.MoveInput.x).normalized;
            Vector3 horizontal = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            horizontal = Vector3.MoveTowards(horizontal, wishDir * speed, ctx.Settings.GroundAcceleration * ctx.DeltaTime);
            ctx.Velocity = new Vector3(horizontal.x, ctx.Velocity.y, horizontal.z);
        }

        public void Exit(MovementContext ctx) { }

        public MovementStateId? RequestedTransition(MovementContext ctx)
        {
            if (!ctx.Motor.IsGrounded) return MovementStateId.Air;
            if (ctx.Input.CrouchHeld) return MovementStateId.Crouch;
            return null;
        }
    }
}
