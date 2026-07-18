using UnityEngine;
using Veil.Input;

namespace Veil.Interaction
{
    /// <summary>
    /// Physics grab/carry/throw for <see cref="IGrabbable"/> rigidbodies. Kept separate
    /// from <see cref="InteractionCaster"/> so static-trigger interaction and physics-carry
    /// interaction evolve independently.
    /// </summary>
    public sealed class GrabController : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float grabRange = 2.5f;
        [SerializeField] private float springStrength = 200f;
        [SerializeField] private float damping = 12f;
        [SerializeField] private float throwForce = 8f;
        [SerializeField] private LayerMask grabbableMask = ~0;

        private IGrabbable _held;

        /// <summary>Overrides the hold point transform (used by tests and prefab wiring).</summary>
        public void SetHoldPoint(Transform point) => holdPoint = point;

        private void OnEnable()
        {
            if (input != null)
            {
                input.GrabPressed += OnGrabPressed;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.GrabPressed -= OnGrabPressed;
            }
        }

        private void FixedUpdate() => TickHold(Time.fixedDeltaTime);

        private void OnGrabPressed()
        {
            if (_held != null)
            {
                Release(throwing: true);
                return;
            }

            if (Physics.SphereCast(transform.position, 0.2f, transform.forward, out RaycastHit hit, grabRange, grabbableMask))
            {
                var grabbable = hit.collider.GetComponentInParent<IGrabbable>();
                if (grabbable != null) Grab(grabbable);
            }
        }

        /// <summary>Begins holding the given grabbable.</summary>
        public void Grab(IGrabbable grabbable)
        {
            _held = grabbable;
            _held.OnGrabbed();
        }

        /// <summary>Advances the spring-damper hold for one physics step.</summary>
        public void TickHold(float deltaTime)
        {
            if (_held == null || holdPoint == null) return;

            Vector3 newVelocity = GrabPhysicsMath.ComputeHoldVelocity(
                _held.Body.position, holdPoint.position, _held.Body.linearVelocity, springStrength, damping, deltaTime);
            _held.Body.linearVelocity = newVelocity;
        }

        /// <summary>Releases the held object, optionally applying a throw impulse.</summary>
        public void Release(bool throwing)
        {
            if (_held == null) return;

            if (throwing) _held.Body.linearVelocity += transform.forward * throwForce;
            _held.OnReleased();
            _held = null;
        }
    }
}
