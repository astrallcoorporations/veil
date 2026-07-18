using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>
    /// Wires a specific <see cref="Lever"/> to a specific <see cref="Door"/> so pulling the
    /// lever actually opens/closes the door. Kept as its own composed component (rather than
    /// teaching <see cref="Lever"/> about <see cref="Door"/> directly) so both interactables
    /// stay decoupled and reusable in puzzles that don't involve a door at all.
    /// </summary>
    public sealed class DoorLeverLink : MonoBehaviour
    {
        [SerializeField] private Lever lever;
        [SerializeField] private Door door;

        private void OnEnable()
        {
            if (lever != null) lever.StateChanged += HandleLeverStateChanged;
        }

        private void OnDisable()
        {
            if (lever != null) lever.StateChanged -= HandleLeverStateChanged;
        }

        private void HandleLeverStateChanged(bool isOn)
        {
            if (door != null) door.SetOpen(isOn);
        }
    }
}
