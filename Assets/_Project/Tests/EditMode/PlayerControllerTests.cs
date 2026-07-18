using NUnit.Framework;
using UnityEngine;
using Veil.Input;
using Veil.Movement;
using Veil.Movement.Actions;

namespace Veil.Tests.EditMode
{
    public class PlayerControllerTests
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
        public void Tick_WithNoActiveAction_LetsStateMachineDriveVelocity()
        {
            var motor = new FakeMotor();
            var input = ScriptableObject.CreateInstance<InputReader>();
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var ctx = new MovementContext(motor, input, settings);
            var stateMachine = new Veil.Movement.States.MovementStateMachine(ctx);
            var actionController = new ActionController();

            PlayerController.TickMovement(ctx, stateMachine, actionController, 0.1f);

            Assert.IsFalse(actionController.HasActiveAction);
        }
    }
}
