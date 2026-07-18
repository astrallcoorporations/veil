using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Veil.Camera;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Veil.Tests.EditMode
{
    public class CameraJuiceTests
    {
        [Test]
        public void CalculateFovKick_ZeroSpeed_ReturnsBaseFov()
        {
            float fov = CameraJuice.CalculateFovKick(baseFov: 90f, speed: 0f, maxSpeed: 8f, maxKick: 12f);
            Assert.AreEqual(90f, fov, 0.01f);
        }

        [Test]
        public void CalculateFovKick_MaxSpeed_ReturnsBasePlusMaxKick()
        {
            float fov = CameraJuice.CalculateFovKick(baseFov: 90f, speed: 8f, maxSpeed: 8f, maxKick: 12f);
            Assert.AreEqual(102f, fov, 0.01f);
        }

        [Test]
        public void CalculateTilt_Sliding_ReturnsSlideTilt()
        {
            float tilt = CameraJuice.CalculateTilt(horizontalInput: 0f, maxTiltDegrees: 4f, isSliding: true, slideTiltDegrees: 10f);
            Assert.AreEqual(10f, tilt, 0.01f);
        }

        [Test]
        public void CalculateBobOffset_IsAllocationFree()
        {
            Assert.That(() => { CameraJuice.CalculateBobOffset(1f, 5f, 1.8f, 0.05f); }, Is.Not.AllocatingGCMemory());
        }
    }
}
