using UnityEngine;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>Mantles up onto a higher ledge: forward+up displacement over a longer fixed duration than vault.</summary>
    public sealed class MantleAction : IMovementAction
    {
        private const float DurationSeconds = 0.6f;

        private float _elapsed;
        private Vector3 _startPos;
        private Vector3 _endPos;

        /// <summary>Higher wins when multiple actions are simultaneously valid.</summary>
        public int Priority => 30;

        /// <summary>True while this action is currently driving movement.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Whether this action's preconditions are currently met.</summary>
        public bool CanExecute(MovementContext ctx, MovementStateId currentState)
        {
            if (IsActive) return false;
            if (currentState == MovementStateId.Air) return false;

            Vector3 forward = ctx.Velocity.sqrMagnitude > 0.01f ? ctx.Velocity.normalized : Vector3.forward;
            if (!ctx.Motor.CapsuleCast(forward, ctx.Settings.LedgeDetectRange, out RaycastHit hit)) return false;

            float ledgeHeight = hit.point.y;
            return LedgeDetector.Decide(true, ledgeHeight, ctx.Settings) == LedgeActionType.Mantle;
        }

        /// <summary>Begins the action.</summary>
        public void Execute(MovementContext ctx)
        {
            IsActive = true;
            _elapsed = 0f;
            _startPos = ctx.Velocity;
            _endPos = ctx.Velocity.normalized * ctx.Settings.WalkSpeed + Vector3.up * 0.1f;
        }

        /// <summary>Advances the action while active; must set IsActive false internally when finished.</summary>
        public void Tick(MovementContext ctx)
        {
            _elapsed += ctx.DeltaTime;
            float t = Mathf.Clamp01(_elapsed / DurationSeconds);
            ctx.Velocity = Vector3.Lerp(_startPos, _endPos, t);

            if (t >= 1f) IsActive = false;
        }

        /// <summary>Cancels the action early, e.g. when interrupted.</summary>
        public void Cancel(MovementContext ctx) => IsActive = false;
    }
}
