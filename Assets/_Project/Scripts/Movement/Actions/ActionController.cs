using System.Collections.Generic;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>
    /// Owns the set of one-shot movement actions, resolves which one fires when
    /// multiple are simultaneously valid, and ticks/cancels the active one.
    /// </summary>
    public sealed class ActionController
    {
        private readonly List<IMovementAction> _actions = new List<IMovementAction>();
        private IMovementAction _active;

        /// <summary>Registers an action to be considered for triggering.</summary>
        public void RegisterAction(IMovementAction action) => _actions.Add(action);

        /// <summary>
        /// Attempts to trigger the highest-priority valid action. If an action is
        /// already active, no new action can start until it finishes or is cancelled.
        /// </summary>
        public void TryTriggerBestAction(MovementContext ctx, MovementStateId currentState)
        {
            if (_active != null) return;

            IMovementAction best = null;
            for (int i = 0; i < _actions.Count; i++)
            {
                var candidate = _actions[i];
                if (!candidate.CanExecute(ctx, currentState)) continue;
                if (best == null || candidate.Priority > best.Priority) best = candidate;
            }

            if (best == null) return;

            best.Execute(ctx);
            _active = best;
        }

        /// <summary>Ticks the active action, if any, clearing it once it reports inactive.</summary>
        public void Tick(MovementContext ctx)
        {
            if (_active == null) return;
            _active.Tick(ctx);
            if (!_active.IsActive) _active = null;
        }

        /// <summary>Cancels the active action, if any.</summary>
        public void CancelActive(MovementContext ctx)
        {
            if (_active == null) return;
            _active.Cancel(ctx);
            _active = null;
        }
    }
}
