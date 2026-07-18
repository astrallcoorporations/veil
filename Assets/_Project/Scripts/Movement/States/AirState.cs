using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Airborne — reduced air control, gravity accumulates.</summary>
    public sealed class AirState : IMovementState
    {
        public void Enter(MovementContext ctx) { }

        public void Tick(MovementContext ctx)
        {
            Vector3 wishDir = new Vector3(ctx.Input.MoveInput.x, 0f, ctx.Input.MoveInput.y).normalized;
            Vector3 horizontal = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            horizontal = Vector3.MoveTowards(horizontal, wishDir * ctx.Settings.SprintSpeed, ctx.Settings.AirAcceleration * ctx.Settings.AirControlFactor * ctx.DeltaTime);

            float verticalVelocity = Mathf.Max(ctx.Settings.MaxFallSpeed, ctx.Velocity.y + ctx.Settings.Gravity * ctx.DeltaTime);
            ctx.Velocity = new Vector3(horizontal.x, verticalVelocity, horizontal.z);
        }

        public void Exit(MovementContext ctx) { }

        public MovementStateId? RequestedTransition(MovementContext ctx)
        {
            if (ctx.Motor.IsGrounded) return MovementStateId.Grounded;
            return null;
        }
    }
}
