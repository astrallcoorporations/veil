using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Pure spring-damper math for held-object motion. Allocation-free.</summary>
    public static class GrabPhysicsMath
    {
        /// <summary>Returns the velocity that pulls <paramref name="current"/> toward <paramref name="target"/> like a critically-damped spring.</summary>
        public static Vector3 ComputeHoldVelocity(Vector3 current, Vector3 target, Vector3 currentVelocity, float springStrength, float damping, float deltaTime)
        {
            Vector3 displacement = target - current;
            Vector3 springForce = displacement * springStrength;
            Vector3 dampingForce = -currentVelocity * damping;
            return currentVelocity + (springForce + dampingForce) * deltaTime;
        }
    }
}
