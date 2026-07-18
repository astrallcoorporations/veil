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

        /// <inheritdoc />
        public string GetPrompt() => _isOpen ? "Close Door" : "Open Door";

        /// <inheritdoc />
        public void Interact(GameObject interactor) => _isOpen = !_isOpen;

        private void Update()
        {
            float target = _isOpen ? openAngleDegrees : 0f;
            _currentAngle = Mathf.MoveTowards(_currentAngle, target, openSpeedDegreesPerSecond * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0f, _currentAngle, 0f);
        }
    }
}
