using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Crouched movement — reduced speed and capsule height.</summary>
    public sealed class CrouchState : IMovementState
    {
        public void Enter(MovementContext ctx) => ctx.Motor.SetHeight(ctx.Settings.CrouchHeight);

        public void Tick(MovementContext ctx)
        {
            Vector3 wishDir = (ctx.Forward * ctx.Input.MoveInput.y + ctx.Right * ctx.Input.MoveInput.x).normalized;
            Vector3 target = wishDir * ctx.Settings.CrouchSpeed;
            ctx.Velocity = Vector3.MoveTowards(ctx.Velocity, target, ctx.Settings.GroundAcceleration * ctx.DeltaTime);
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
