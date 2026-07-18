using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Anything the player can raycast-interact with (doors, levers, pickups).</summary>
    public interface IInteractable
    {
        /// <summary>Short prompt text, e.g. "Open Door" — a future UI binds to this.</summary>
        string GetPrompt();

        /// <summary>Performs the interaction.</summary>
        void Interact(GameObject interactor);
    }
}
