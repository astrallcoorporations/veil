using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Veil.Input;
using Veil.Movement;
using Veil.Movement.Actions;
using Veil.Movement.States;

namespace Veil.Tests.PlayMode
{
    public class VaultMantlePlayModeTests
    {
        [UnityTest]
        public IEnumerator VaultAction_CanExecute_WhenLowObstacleAhead()
        {
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.transform.localScale = new Vector3(2f, 0.8f, 0.5f);
            // Near face at z = 1.0 - 0.25 = 0.75, inside the capsule's front-surface reach
            // (radius 0.35) plus MovementSettings.LedgeDetectRange (0.8) = 1.15m of forward
            // cast travel. Obstacle base at y = 0 so its 0.8m height overlaps the standing
            // capsule's vertical span (see player spawn note below).
            obstacle.transform.position = new Vector3(0f, 0.4f, 1.0f);

            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var playerGo = new GameObject("Player");
            var motor = playerGo.AddComponent<CharacterMotor>();
            motor.Settings = settings;
            // CharacterMotor treats transform.position.y as the capsule's feet height (its
            // capsule center is offset up by half the standing height - see CharacterMotor.Awake
            // and CharacterMotorPlayModeTests). Spawning at y = 0 puts the capsule's vertical
            // span at [0, StandingHeight], which overlaps the obstacle's [0, 0.8] base-to-top
            // range so the forward capsule cast can actually make contact with it.
            playerGo.transform.position = new Vector3(0f, 0f, 0f);
            playerGo.transform.forward = Vector3.forward;

            yield return null; // let Awake run

            var action = new VaultAction();
            var ctx = new MovementContext(motor, ScriptableObject.CreateInstance<InputReader>(), settings) { Velocity = Vector3.forward * 3f };

            bool canExecute = action.CanExecute(ctx, MovementStateId.Grounded);

            Assert.IsTrue(canExecute);

            Object.Destroy(obstacle);
            Object.Destroy(playerGo);
        }
    }
}
