using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Veil.Input;
using Veil.Movement;
using Veil.Movement.Actions;

namespace Veil.Tests.PlayMode
{
    public class JumpActionPlayModeTests
    {
        [UnityTest]
        public IEnumerator JumpAction_FromGround_ReachesApexNearJumpHeight()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);

            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var input = ScriptableObject.CreateInstance<InputReader>();
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 2f, 0f);
            var motor = playerGo.AddComponent<CharacterMotor>();
            motor.Settings = settings;

            // Land the capsule first (mirrors CharacterMotorPlayModeTests' landing pattern) so
            // the jump starts from a genuinely grounded state, not the spawn height.
            for (int i = 0; i < 60; i++)
            {
                motor.Move(new Vector3(0f, -5f, 0f), Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }
            Assert.IsTrue(motor.IsGrounded, "Capsule must be grounded before jumping.");

            var ctx = new MovementContext(motor, input, settings) { Velocity = Vector3.zero };
            var jumpAction = new JumpAction();

            // Trigger the jump directly (fire-and-forget impulse) — this is the closest automated
            // proxy for the "does the jump feel right" question, isolated from unrelated
            // state-machine/action-controller sequencing so it purely tests the impulse+gravity math.
            Assert.IsTrue(jumpAction.CanExecute(ctx, Veil.Movement.States.MovementStateId.Grounded));
            jumpAction.Execute(ctx);

            float startY = playerGo.transform.position.y;
            float peakY = startY;

            // Full expected airtime is ~0.54s (see feel-verification math); run well past that so
            // the true peak is captured even with FixedUpdate's discrete integration step.
            for (int i = 0; i < 90; i++)
            {
                float verticalVelocity = Mathf.Max(settings.MaxFallSpeed, ctx.Velocity.y + settings.Gravity * Time.fixedDeltaTime);
                ctx.Velocity = new Vector3(ctx.Velocity.x, verticalVelocity, ctx.Velocity.z);
                motor.Move(ctx.Velocity, Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();

                if (playerGo.transform.position.y > peakY) peakY = playerGo.transform.position.y;
            }

            float apexHeight = peakY - startY;
            float expectedApexHeight = settings.JumpHeight;

            Assert.AreEqual(expectedApexHeight, apexHeight, 0.15f,
                $"Jump apex height ({apexHeight:F3}m) should be within tolerance of MovementSettings.JumpHeight ({expectedApexHeight:F3}m).");

            Object.Destroy(ground);
            Object.Destroy(playerGo);
        }
    }
}
