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
        public IEnumerator Motor_RestingOnGround_ReportsGrounded()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.transform.localScale = new Vector3(20f, 1f, 20f);
            ground.transform.position = new Vector3(0f, -0.5f, 0f);

            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var playerGo = new GameObject("Player");
            playerGo.transform.position = new Vector3(0f, 2f, 0f);
            var motor = playerGo.AddComponent<CharacterMotor>();
            motor.Settings = settings;

            for (int i = 0; i < 60; i++)
            {
                motor.Move(Vector3.zero, Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.IsTrue(motor.IsGrounded);

            Object.Destroy(ground);
            Object.Destroy(playerGo);
        }
    }
}
