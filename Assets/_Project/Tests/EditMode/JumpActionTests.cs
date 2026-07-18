using NUnit.Framework;
using UnityEngine;
using Veil.Input;
using Veil.Movement;
using Veil.Movement.Actions;
using Veil.Movement.States;

namespace Veil.Tests.EditMode
{
    public class JumpActionTests
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

        private MovementContext MakeContext(bool grounded, Vector3 velocity)
        {
            var motor = new FakeMotor { IsGrounded = grounded };
            var input = ScriptableObject.CreateInstance<InputReader>();
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var ctx = new MovementContext(motor, input, settings) { Velocity = velocity };
            return ctx;
        }

        [Test]
        public void CanExecute_False_WhenAirborne()
        {
            var action = new JumpAction();
            var ctx = MakeContext(grounded: false, velocity: new Vector3(0, 0, 4));

            Assert.IsFalse(action.CanExecute(ctx, MovementStateId.Air));
        }

        [Test]
        public void CanExecute_True_WhenGrounded()
        {
            var action = new JumpAction();
            var ctx = MakeContext(grounded: true, velocity: new Vector3(0, 0, 4));

            Assert.IsTrue(action.CanExecute(ctx, MovementStateId.Grounded));
        }

        [Test]
        public void Execute_SetsVerticalVelocity_MatchingPhysicsFormula()
        {
            var action = new JumpAction();
            var ctx = MakeContext(grounded: true, velocity: new Vector3(1.5f, 0f, 4f));

            // Project default gravity is -30; MovementSettings.JumpHeight defaults to 1.1.
            float expectedJumpSpeed = Mathf.Sqrt(2f * -ctx.Settings.Gravity * ctx.Settings.JumpHeight);

            action.Execute(ctx);

            Assert.AreEqual(expectedJumpSpeed, ctx.Velocity.y, 0.001f);
            Assert.AreEqual(1.5f, ctx.Velocity.x, 0.0001f, "Jump must not reset horizontal momentum.");
            Assert.AreEqual(4f, ctx.Velocity.z, 0.0001f, "Jump must not reset horizontal momentum.");
        }
    }
}
