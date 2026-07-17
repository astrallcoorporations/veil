using NUnit.Framework;
using UnityEngine;
using Veil.Movement;

namespace Veil.Tests.EditMode
{
    public class MovementSettingsTests
    {
        [Test]
        public void OnValidate_ClampsCrouchSpeedBelowSprintSpeed()
        {
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            settings.SprintSpeed = 5f;
            settings.CrouchSpeed = 50f; // invalid: crouch faster than sprint
            settings.OnValidate();

            Assert.LessOrEqual(settings.CrouchSpeed, settings.SprintSpeed);
        }

        [Test]
        public void OnValidate_ClampsMaxSlopeAngleTo0_90()
        {
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            settings.MaxWalkableSlopeAngle = 200f;
            settings.OnValidate();

            Assert.LessOrEqual(settings.MaxWalkableSlopeAngle, 90f);
        }
    }
}
