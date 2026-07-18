using NUnit.Framework;
using UnityEngine;
using Veil.Movement;
using Veil.Movement.Actions;

namespace Veil.Tests.EditMode
{
    public class LedgeDetectorTests
    {
        private MovementSettings MakeSettings()
        {
            var s = ScriptableObject.CreateInstance<MovementSettings>();
            return s; // vaultMaxHeight=1.1, mantleMaxHeight=2.2 (defaults)
        }

        [Test]
        public void Decide_NoForwardHit_ReturnsNone()
        {
            Assert.AreEqual(LedgeActionType.None, LedgeDetector.Decide(false, 0f, MakeSettings()));
        }

        [Test]
        public void Decide_LowLedge_ReturnsVault()
        {
            Assert.AreEqual(LedgeActionType.Vault, LedgeDetector.Decide(true, 0.8f, MakeSettings()));
        }

        [Test]
        public void Decide_MidLedge_ReturnsMantle()
        {
            Assert.AreEqual(LedgeActionType.Mantle, LedgeDetector.Decide(true, 1.8f, MakeSettings()));
        }

        [Test]
        public void Decide_TooHigh_ReturnsNone()
        {
            Assert.AreEqual(LedgeActionType.None, LedgeDetector.Decide(true, 3f, MakeSettings()));
        }
    }
}
