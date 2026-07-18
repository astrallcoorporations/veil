namespace Veil.Movement.States
{
    /// <summary>One macro-locomotion state. Implementations must not allocate in <see cref="Tick"/>.</summary>
    public interface IMovementState
    {
        /// <summary>Called once when the state becomes active.</summary>
        void Enter(MovementContext ctx);

        /// <summary>Called every motor tick while this state is active.</summary>
        void Tick(MovementContext ctx);

        /// <summary>Called once when the state stops being active.</summary>
        void Exit(MovementContext ctx);

        /// <summary>Returns the state to transition to this tick, or null to stay.</summary>
        MovementStateId? RequestedTransition(MovementContext ctx);
    }
}
