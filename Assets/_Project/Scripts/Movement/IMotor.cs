using UnityEngine;

namespace Veil.Movement
{
    /// <summary>
    /// Abstraction over the physical capsule motor. States and actions program
    /// against this interface, not <see cref="CharacterMotor"/> directly, so they
    /// can be unit tested with a fake implementation instead of real physics.
    /// </summary>
    public interface IMotor
    {
        /// <summary>True if the capsule is currently resting on walkable ground.</summary>
        bool IsGrounded { get; }

        /// <summary>Surface normal of the ground currently supporting the capsule.</summary>
        Vector3 GroundNormal { get; }

        /// <summary>Current resolved velocity, in m/s.</summary>
        Vector3 Velocity { get; }

        /// <summary>Moves the capsule by <paramref name="velocity"/> * <paramref name="deltaTime"/>, resolving collisions.</summary>
        void Move(Vector3 velocity, float deltaTime);

        /// <summary>Casts the capsule shape forward; used by ledge detection.</summary>
        bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit);

        /// <summary>Sets the capsule height (standing vs crouch).</summary>
        void SetHeight(float height);
    }
}
