using UnityEngine;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>Slide triggered by crouching while sprinting on the ground; ends when speed decays below the minimum.</summary>
    public sealed class SlideAction : IMovementAction
    {
        /// <summary>Higher wins when multiple actions are simultaneously valid.</summary>
        public int Priority => 20;

        /// <summary>True while this action is currently driving movement.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether this action's preconditions are currently met.</summary>
        public bool CanExecute(MovementContext ctx, MovementStateId currentState)
        {
            if (IsActive) return false;
            if (currentState != MovementStateId.Grounded) return false;
            if (!ctx.Input.SprintHeld || !ctx.Input.CrouchHeld) return false;

            Vector3 planar = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            return planar.magnitude >= ctx.Settings.MinSlideSpeed;
        }

        /// <summary>Begins the action.</summary>
        public void Execute(MovementContext ctx)
        {
            IsActive = true;
            ctx.Motor.SetHeight(ctx.Settings.CrouchHeight);
            Vector3 planar = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            ctx.Velocity = planar.normalized * (planar.magnitude + ctx.Settings.SlideInitialBoost);
        }

        /// <summary>Advances the action while active; must set IsActive false internally when finished.</summary>
        public void Tick(MovementContext ctx)
        {
            ctx.Velocity = MotorMath.ComputeSlideVelocity(ctx.Velocity, ctx.Motor.GroundNormal, ctx.Settings.SlideFriction, ctx.Settings.MinSlideSpeed, ctx.DeltaTime);

            Vector3 planar = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            if (planar.magnitude <= ctx.Settings.MinSlideSpeed || !ctx.Input.CrouchHeld)
            {
                IsActive = false;
                ctx.Motor.SetHeight(ctx.Settings.StandingHeight);
            }
        }

        /// <summary>Cancels the action early, e.g. when interrupted.</summary>
        public void Cancel(MovementContext ctx)
        {
            IsActive = false;
            ctx.Motor.SetHeight(ctx.Settings.StandingHeight);
        }
    }
}
