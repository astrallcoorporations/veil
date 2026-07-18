using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Veil.Input;
using Veil.Movement;
using Veil.Movement.Actions;
using Veil.Movement.States;

namespace Veil.Tests.EditMode
{
    public class SlideActionTests
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

        // InputReader.SprintHeld/CrouchHeld are private-set, normally driven by the Input
        // System at runtime via VeilControls.IGameplayActions callbacks. Reflection lets this
        // fixture drive them directly without constructing a real InputAction.CallbackContext.
        private static void SetHeld(InputReader input, string propertyName, bool value)
        {
            typeof(InputReader).GetProperty(propertyName)
                .GetSetMethod(nonPublic: true)
                .Invoke(input, new object[] { value });
        }

        private MovementContext MakeContext(bool grounded, bool sprinting, Vector3 velocity)
        {
            var motor = new FakeMotor { IsGrounded = grounded };
            var input = ScriptableObject.CreateInstance<InputReader>();
            SetHeld(input, nameof(InputReader.SprintHeld), sprinting);
            SetHeld(input, nameof(InputReader.CrouchHeld), sprinting);
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var ctx = new MovementContext(motor, input, settings) { Velocity = velocity };
            return ctx;
        }

        [Test]
        public void CanExecute_False_WhenAirborne()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: false, sprinting: true, velocity: new Vector3(0, 0, 8));

            Assert.IsFalse(action.CanExecute(ctx, MovementStateId.Air));
        }

        [Test]
        public void CanExecute_True_WhenGroundedAndFastEnough()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: true, sprinting: true, velocity: new Vector3(0, 0, 8));

            Assert.IsTrue(action.CanExecute(ctx, MovementStateId.Grounded));
        }

        [Test]
        public void Execute_SetsIsActiveTrue()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: true, sprinting: true, velocity: new Vector3(0, 0, 8));

            action.Execute(ctx);

            Assert.IsTrue(action.IsActive);
        }

        [Test]
        public void Tick_EndsSlide_WhenSpeedDropsBelowMinimum()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: true, sprinting: true, velocity: new Vector3(0, 0, 1.4f));
            // Execute boosts speed to 1.4 + SlideInitialBoost(3) = 4.4; a large DeltaTime here
            // ensures friction genuinely decays it to/below MinSlideSpeed(1.5) within one Tick,
            // rather than relying on a crouch release to end the slide.
            ctx.DeltaTime = 2f;

            action.Execute(ctx);
            action.Tick(ctx);

            Assert.IsFalse(action.IsActive);
        }
    }
}
