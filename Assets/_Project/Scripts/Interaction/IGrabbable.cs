using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Anything the player can physically grab, carry, and throw.</summary>
    public interface IGrabbable
    {
        /// <summary>The rigidbody being carried.</summary>
        Rigidbody Body { get; }

        /// <summary>Called when grabbed; used to e.g. disable other physics behaviour while held.</summary>
        void OnGrabbed();

        /// <summary>Called when released or thrown.</summary>
        void OnReleased();
    }
}
