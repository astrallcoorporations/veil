using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Swings open on interact; placeholder greybox behaviour for M1.</summary>
    public sealed class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private float openAngleDegrees = 90f;
        [SerializeField] private float openSpeedDegreesPerSecond = 120f;
        private bool _isOpen;
        private float _currentAngle;

        /// <summary>Whether the door is currently in its open state.</summary>
        public bool IsOpen => _isOpen;

        /// <inheritdoc />
        public string GetPrompt() => _isOpen ? "Close Door" : "Open Door";

        /// <inheritdoc />
        public void Interact(GameObject interactor) => _isOpen = !_isOpen;

        /// <summary>
        /// Directly sets the door's open state, e.g. from a wired puzzle component like
        /// <see cref="DoorLeverLink"/>. Independent of <see cref="Interact"/> — both simply
        /// assign <c>_isOpen</c>, so the door still opens/closes on its own interaction too.
        /// </summary>
        /// <param name="open">True to open the door, false to close it.</param>
        public void SetOpen(bool open) => _isOpen = open;

        private void Update()
        {
            float target = _isOpen ? openAngleDegrees : 0f;
            _currentAngle = Mathf.MoveTowards(_currentAngle, target, openSpeedDegreesPerSecond * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0f, _currentAngle, 0f);
        }
    }
}
