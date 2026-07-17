using UnityEngine;

namespace Veil.Movement
{
    /// <summary>
    /// Pure, allocation-free movement math shared by the motor, states, and actions.
    /// Kept free of MonoBehaviour/UnityEngine.Object dependencies so it is directly
    /// unit-testable and safe to call from any per-frame hot path.
    /// </summary>
    public static class MotorMath
    {
        /// <summary>True if a surface with the given normal is walkable at the given max slope angle.</summary>
        public static bool IsWalkable(Vector3 normal, float maxSlopeAngleDegrees)
        {
            float angle = Vector3.Angle(normal, Vector3.up);
            return angle <= maxSlopeAngleDegrees;
        }

        /// <summary>Projects a move delta onto a collision plane so motion doesn't penetrate the surface.</summary>
        public static Vector3 SlideAlongSurface(Vector3 moveDelta, Vector3 hitNormal)
        {
            return Vector3.ProjectOnPlane(moveDelta, hitNormal);
        }

        /// <summary>Applies ground friction to a slide, never dropping below <paramref name="minSpeed"/> while horizontal speed exceeds it.</summary>
        public static Vector3 ComputeSlideVelocity(Vector3 currentVelocity, Vector3 groundNormal, float friction, float minSpeed, float deltaTime)
        {
            Vector3 planar = Vector3.ProjectOnPlane(currentVelocity, groundNormal);
            float speed = planar.magnitude;
            if (speed <= minSpeed) return planar;

            float decayedSpeed = Mathf.Max(minSpeed, speed - friction * deltaTime);
            return planar.normalized * decayedSpeed;
        }
    }
}
