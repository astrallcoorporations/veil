using NUnit.Framework;
using UnityEngine;
using Veil.Input;
using Veil.Movement;
using Veil.Movement.States;

namespace Veil.Tests.EditMode
{
    public class MovementStateMachineTests
    {
        private class FakeMotor : IMotor
        {
            public bool IsGrounded { get; set; } = true;
            public Vector3 GroundNormal { get; set; } = Vector3.up;
            public Vector3 Velocity { get; set; }
            public void Move(Vector3 velocity, float deltaTime) => Velocity = velocity;
            public bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit) { hit = default; return false; }
            public void SetHeight(float height) { }
        }

        [Test]
        public void StartsInGroundedState()
        {
            var ctx = new MovementContext(new FakeMotor(), ScriptableObject.CreateInstance<InputReader>(), ScriptableObject.CreateInstance<MovementSettings>());
            var sm = new MovementStateMachine(ctx);

            Assert.AreEqual(MovementStateId.Grounded, sm.Current);
        }

        [Test]
        public void TransitionsToAir_WhenMotorLeavesGround()
        {
            var motor = new FakeMotor { IsGrounded = true };
            var ctx = new MovementContext(motor, ScriptableObject.CreateInstance<InputReader>(), ScriptableObject.CreateInstance<MovementSettings>());
            var sm = new MovementStateMachine(ctx);

            motor.IsGrounded = false;
            sm.Tick(ctx);

            Assert.AreEqual(MovementStateId.Air, sm.Current);
        }
    }
}
