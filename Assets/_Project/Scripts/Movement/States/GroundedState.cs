using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Standing/walking/sprinting on walkable ground.</summary>
    public sealed class GroundedState : IMovementState
    {
        public void Enter(MovementContext ctx) { }

        public void Tick(MovementContext ctx)
        {
            float speed = ctx.Input.SprintHeld ? ctx.Settings.SprintSpeed : ctx.Settings.WalkSpeed;
            Vector3 wishDir = (ctx.Forward * ctx.Input.MoveInput.y + ctx.Right * ctx.Input.MoveInput.x).normalized;
            Vector3 target = wishDir * speed;
            ctx.Velocity = Vector3.MoveTowards(ctx.Velocity, target, ctx.Settings.GroundAcceleration * ctx.DeltaTime);
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
