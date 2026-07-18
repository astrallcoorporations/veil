using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>
    /// A self-contained, interruptible one-shot movement move (vault, mantle, slide).
    /// Only fires from valid macro-states, and only one action may be active at a time.
    /// </summary>
    public interface IMovementAction
    {
        /// <summary>Higher wins when multiple actions are simultaneously valid.</summary>
        int Priority { get; }

        /// <summary>True while this action is currently driving movement.</summary>
        bool IsActive { get; }

        /// <summary>Whether this action's preconditions are currently met.</summary>
        bool CanExecute(MovementContext ctx, MovementStateId currentState);

        /// <summary>Begins the action.</summary>
        void Execute(MovementContext ctx);

        /// <summary>Advances the action while active; must set IsActive false internally when finished.</summary>
        void Tick(MovementContext ctx);

        /// <summary>Cancels the action early, e.g. when interrupted.</summary>
        void Cancel(MovementContext ctx);
    }
}
