using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Veil.Movement;

namespace Veil.Tests.PlayMode
{
    public class CharacterMotorPlayModeTests
    {
        [UnityTest]
        public IEnumerator Motor_FallingOntoGround_ReportsGroundedAndLandsAtSurface()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);

            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 2f, 0f);
            var motor = playerGo.AddComponent<CharacterMotor>();
            motor.Settings = settings;

            // Command a real falling velocity every frame so the capsule genuinely travels
            // down through the collide-and-slide path (Vector3.zero would trivially "pass"
            // regardless of whether ground detection or movement actually works).
            for (int i = 0; i < 60; i++)
            {
                motor.Move(new Vector3(0f, -5f, 0f), Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(motor.IsGrounded);
            // Ground top face is at y = 0; the capsule's feet track transform.position.y.
            // Asserting near-zero (not still ~2, the spawn height) proves the capsule actually
            // reached the ground via a real (non-self) hit rather than being blocked in place.
            Assert.Less(Mathf.Abs(playerGo.transform.position.y), 0.1f,
                "Capsule should have landed at the ground surface (y ~ 0), not remained at spawn height.");

            Object.Destroy(ground);
            Object.Destroy(playerGo);
        }

        [UnityTest]
        public IEnumerator Motor_MovingTowardWall_StopsAtWallAndDoesNotPassThrough()
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.transform.localScale = new Vector3(1f, 5f, 20f);
            wall.transform.position = new Vector3(3f, 0f, 0f); // wall face at x = 2.5

            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 1f, 0f);
            var motor = playerGo.AddComponent<CharacterMotor>();
            motor.Settings = settings;

            for (int i = 0; i < 120; i++)
            {
                motor.Move(new Vector3(2f, 0f, 0f), Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            float finalX = playerGo.transform.position.x;
            // The capsule (radius 0.35) should have travelled well away from its spawn at x = 0
            // and then been stopped by the wall face at x = 2.5, never passing through it.
            Assert.Greater(finalX, 1.5f, "Capsule should have moved substantially toward the wall.");
            Assert.Less(finalX, 2.5f, "Capsule should not have passed through the wall.");

            Object.Destroy(wall);
            Object.Destroy(playerGo);
        }
    }
}
