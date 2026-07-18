using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Marks a rigidbody object as grabbable by <see cref="GrabController"/>.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class GrabbableObject : MonoBehaviour, IGrabbable
    {
        private Rigidbody _body;

        /// <summary>The rigidbody being carried.</summary>
        public Rigidbody Body => _body != null ? _body : (_body = GetComponent<Rigidbody>());

        /// <summary>Called when grabbed; disables gravity while held.</summary>
        public void OnGrabbed() => Body.useGravity = false;

        /// <summary>Called when released or thrown; restores gravity.</summary>
        public void OnReleased() => Body.useGravity = true;
    }
}
