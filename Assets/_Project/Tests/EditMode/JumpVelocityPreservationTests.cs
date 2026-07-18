using NUnit.Framework;
using UnityEngine;
using Veil.Input;
using Veil.Movement;
using Veil.Movement.Actions;
using Veil.Movement.States;

namespace Veil.Tests.EditMode
{
    /// <summary>
    /// Proves the fix for the jump-velocity-clobber bug documented in
    /// task-jump-feel-report.md Section 4: <see cref="GroundedState"/>.Tick used to apply
    /// Vector3.MoveTowards to the FULL velocity vector (including Y), dragging a fresh jump
    /// impulse toward zero during the one extra Grounded tick that elapses before the state
    /// machine catches up to the motor reporting the player airborne. Unlike
    /// JumpActionTests (which calls JumpAction.Execute in isolation), this test drives the
    /// real production sequencing -- ActionController.TryTriggerBestAction followed by
    /// PlayerController.TickMovement, in the same order/ownership PlayerController.Update
    /// uses -- so it actually exercises the bug window.
    /// </summary>
    public class JumpVelocityPreservationTests
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
        public void JumpVelocity_SurvivesTheExtraGroundedTick_BeforeMotorReportsAirborne()
        {
            var motor = new FakeMotor { IsGrounded = true };
            var input = ScriptableObject.CreateInstance<InputReader>();
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var ctx = new MovementContext(motor, input, settings)
            {
                Forward = Vector3.forward,
                Right = Vector3.right,
            };

            var stateMachine = new MovementStateMachine(ctx);
            var actionController = new ActionController();
            actionController.RegisterAction(new JumpAction());

            float expectedJumpSpeed = Mathf.Sqrt(2f * -settings.Gravity * settings.JumpHeight);
            const float deltaTime = 1f / 30f; // worst-case frame rate from the bug report's measurement table.

            // Frame N (input event, pre-Update): mirrors PlayerController.OnActionTriggerPressed,
            // which the Input System dispatches ahead of Update() each frame.
            actionController.TryTriggerBestAction(ctx, stateMachine.Current);
            Assert.AreEqual(expectedJumpSpeed, ctx.Velocity.y, 0.001f,
                "JumpAction.Execute should set full vertical velocity immediately.");

            // Frame N, inside Update -> TickMovement: an action is active, so the state
            // machine's Tick (and therefore RequestedTransition) is skipped entirely this
            // frame. actionController.Tick then clears JumpAction immediately, since it is a
            // single-frame fire-and-forget action (IsActive is always false).
            // PlayerController.Update() sets ctx.DeltaTime from Time.deltaTime immediately
            // before calling TickMovement each frame -- TickMovement's own deltaTime
            // parameter is not applied to ctx internally, so tests must mirror that here.
            ctx.DeltaTime = deltaTime;
            PlayerController.TickMovement(ctx, stateMachine, actionController, deltaTime);
            Assert.IsFalse(actionController.HasActiveAction);
            Assert.AreEqual(MovementStateId.Grounded, stateMachine.Current,
                "State machine has not ticked yet, so it is still nominally Grounded.");

            // Frame N+1: the "one extra Grounded tick" from the bug report. The real
            // CharacterMotor has not yet detected liftoff (its downward ground-cast
            // tolerance can still read grounded for a frame), so IsGrounded stays true here
            // -- this is the exact bug window where GroundedState.Tick used to clobber Y.
            ctx.DeltaTime = deltaTime;
            PlayerController.TickMovement(ctx, stateMachine, actionController, deltaTime);

            Assert.AreEqual(expectedJumpSpeed, ctx.Velocity.y, 0.01f,
                "GroundedState.Tick must not drag vertical velocity toward zero -- it has no business touching Y.");
            Assert.AreEqual(MovementStateId.Grounded, stateMachine.Current,
                "Motor still reports grounded, so no transition should have happened yet.");

            // The motor now catches up and reports liftoff; the very next tick should
            // transition to Air carrying the jump velocity preserved above. The transition
            // check runs after GroundedState.Tick within the same call, so AirState.Tick
            // (and its gravity integration) has not run yet -- Y should still be untouched.
            motor.IsGrounded = false;
            ctx.DeltaTime = deltaTime;
            PlayerController.TickMovement(ctx, stateMachine, actionController, deltaTime);

            Assert.AreEqual(MovementStateId.Air, stateMachine.Current);
            Assert.AreEqual(expectedJumpSpeed, ctx.Velocity.y, 0.01f,
                "Vertical velocity should still be intact once the state machine hands off to Air.");
        }
    }
}
