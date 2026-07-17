using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Veil.Movement;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Veil.Tests.EditMode
{
    public class MotorMathTests
    {
        [Test]
        public void IsWalkable_FlatGround_ReturnsTrue()
        {
            Assert.IsTrue(MotorMath.IsWalkable(Vector3.up, 50f));
        }

        [Test]
        public void IsWalkable_VerticalWall_ReturnsFalse()
        {
            Assert.IsFalse(MotorMath.IsWalkable(Vector3.right, 50f));
        }

        [Test]
        public void SlideAlongSurface_RemovesVelocityIntoWall()
        {
            var moveDelta = new Vector3(1f, 0f, 0f);
            var wallNormal = new Vector3(-1f, 0f, 0f);

            var result = MotorMath.SlideAlongSurface(moveDelta, wallNormal);

            Assert.AreEqual(0f, result.x, 0.0001f);
        }

        [Test]
        public void ComputeSlideVelocity_DecaysTowardMinSpeed()
        {
            var start = new Vector3(0f, 0f, 10f);
            var result = MotorMath.ComputeSlideVelocity(start, Vector3.up, friction: 1.5f, minSpeed: 1.5f, deltaTime: 1f);

            Assert.Less(result.magnitude, start.magnitude);
            Assert.GreaterOrEqual(result.magnitude, 1.5f);
        }

        [Test]
        public void SlideAlongSurface_IsAllocationFree()
        {
            Assert.That(() => { MotorMath.SlideAlongSurface(Vector3.forward, Vector3.up); }, Is.Not.AllocatingGCMemory());
        }
    }
}
