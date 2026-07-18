using System.Collections.Generic;

namespace Veil.Movement.States
{
    /// <summary>Owns the current macro-locomotion state and ticks it each frame.</summary>
    public sealed class MovementStateMachine
    {
        private readonly Dictionary<MovementStateId, IMovementState> _states;
        private readonly MovementContext _ctx;

        /// <summary>The currently active macro-state.</summary>
        public MovementStateId Current { get; private set; }

        public MovementStateMachine(MovementContext ctx)
        {
            _ctx = ctx;
            _states = new Dictionary<MovementStateId, IMovementState>
            {
                { MovementStateId.Grounded, new GroundedState() },
                { MovementStateId.Air, new AirState() },
                { MovementStateId.Crouch, new CrouchState() },
            };
            Current = MovementStateId.Grounded;
            _states[Current].Enter(_ctx);
        }

        /// <summary>Ticks the active state and applies any requested transition.</summary>
        public void Tick(MovementContext ctx)
        {
            _states[Current].Tick(ctx);

            MovementStateId? requested = _states[Current].RequestedTransition(ctx);
            if (requested.HasValue && requested.Value != Current)
            {
                _states[Current].Exit(ctx);
                Current = requested.Value;
                _states[Current].Enter(ctx);
            }
        }
    }
}
