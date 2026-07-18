using System;
using UnityEngine;
using Veil.Input;

namespace Veil.Interaction
{
    /// <summary>Spherecasts from the camera each frame to find the focused interactable, and fires interact on input.</summary>
    public sealed class InteractionCaster : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera eye;
        [SerializeField] private InputReader input;
        [SerializeField] private float range = 2.5f;
        [SerializeField] private float sphereRadius = 0.15f;
        [SerializeField] private LayerMask interactableMask = ~0;

        /// <summary>Fires when the focused interactable changes (including to/from null); a future UI binds its prompt here.</summary>
        public event Action<IInteractable> FocusChanged;

        private readonly RaycastHit[] _hitBuffer = new RaycastHit[8];
        private IInteractable _current;

        private void OnEnable()
        {
            if (input != null) input.InteractPressed += OnInteractPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.InteractPressed -= OnInteractPressed;
        }

        private void Update()
        {
            IInteractable found = null;
            int count = Physics.SphereCastNonAlloc(eye.transform.position, sphereRadius, eye.transform.forward, _hitBuffer, range, interactableMask, QueryTriggerInteraction.Collide);
            if (count > 0)
            {
                int closest = 0;
                for (int i = 1; i < count; i++)
                {
                    if (_hitBuffer[i].distance < _hitBuffer[closest].distance) closest = i;
                }
                found = _hitBuffer[closest].collider.GetComponentInParent<IInteractable>();
            }

            if (!ReferenceEquals(found, _current))
            {
                _current = found;
                FocusChanged?.Invoke(_current);
            }
        }

        private void OnInteractPressed() => _current?.Interact(gameObject);
    }
}
