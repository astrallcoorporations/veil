using System;
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Toggleable lever; placeholder greybox behaviour, exposes an event other objects (e.g. a Door) can bind to for M1 puzzle wiring.</summary>
    public sealed class Lever : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool startsOn;
        private bool _isOn;

        /// <summary>Fires with the new on/off state each time the lever is used.</summary>
        public event Action<bool> StateChanged;

        private void Awake() => _isOn = startsOn;

        public string GetPrompt() => _isOn ? "Pull Lever (Off)" : "Pull Lever (On)";

        public void Interact(GameObject interactor)
        {
            _isOn = !_isOn;
            StateChanged?.Invoke(_isOn);
        }
    }
}
