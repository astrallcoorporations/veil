using NUnit.Framework;
using UnityEngine;
using Veil.Interaction;

namespace Veil.Tests.EditMode
{
    public class GrabPhysicsMathTests
    {
        [Test]
        public void ComputeHoldVelocity_MovesTowardTarget()
        {
            Vector3 current = Vector3.zero;
            Vector3 target = new Vector3(1f, 0f, 0f);

            Vector3 velocity = GrabPhysicsMath.ComputeHoldVelocity(current, target, Vector3.zero, springStrength: 10f, damping: 1f, deltaTime: 0.1f);

            Assert.Greater(velocity.x, 0f);
        }

        [Test]
        public void ComputeHoldVelocity_AtTarget_WithZeroVelocity_ReturnsNearZero()
        {
            Vector3 velocity = GrabPhysicsMath.ComputeHoldVelocity(Vector3.zero, Vector3.zero, Vector3.zero, springStrength: 10f, damping: 1f, deltaTime: 0.1f);

            Assert.AreEqual(0f, velocity.magnitude, 0.001f);
        }
    }
}
