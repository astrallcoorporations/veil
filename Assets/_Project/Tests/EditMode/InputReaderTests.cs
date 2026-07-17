using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Veil.Input;

namespace Veil.Tests.EditMode
{
    public class InputReaderTests : InputTestFixture
    {
        private InputReader _reader;

        public override void Setup()
        {
            base.Setup();
            _reader = ScriptableObject.CreateInstance<InputReader>();
            _reader.Enable();
        }

        public override void TearDown()
        {
            _reader.Disable();
            Object.DestroyImmediate(_reader);
            base.TearDown();
        }

        [Test]
        public void MoveInput_DefaultsToZero()
        {
            Assert.AreEqual(Vector2.zero, _reader.MoveInput);
        }

        [Test]
        public void JumpVaultPressed_FiresOnce_WhenButtonPressed()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            int fireCount = 0;
            _reader.JumpVaultPressed += () => fireCount++;

            Press(keyboard.spaceKey);
            Release(keyboard.spaceKey);

            Assert.AreEqual(1, fireCount);
        }
    }
}
