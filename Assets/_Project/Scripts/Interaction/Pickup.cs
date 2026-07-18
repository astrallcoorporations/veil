using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Simple pickup — destroys itself on interact; placeholder for M1.</summary>
    public sealed class Pickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Item";

        /// <inheritdoc />
        public string GetPrompt() => $"Pick Up {displayName}";

        /// <inheritdoc />
        public void Interact(GameObject interactor) => Destroy(gameObject);
    }
}
