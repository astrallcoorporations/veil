using UnityEngine;
using Veil.Movement;

namespace Veil.Camera
{
    /// <summary>Drives an actual Camera's FOV, tilt, and bob from live motor state each frame.</summary>
    public sealed class CameraController : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera targetCamera;
        [SerializeField] private CameraSettings settings;
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private float sprintSpeedForJuice = 8f;

        private float _time;

        private void Reset()
        {
            targetCamera = GetComponentInChildren<UnityEngine.Camera>();
        }

        private void Update()
        {
            if (targetCamera == null || settings == null || motor == null) return;

            _time += Time.deltaTime;
            float speed = new Vector3(motor.Velocity.x, 0f, motor.Velocity.z).magnitude;

            float targetFov = CameraJuice.CalculateFovKick(settings.BaseFov, speed, sprintSpeedForJuice, settings.MaxFovKick);
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFov, settings.FovLerpSpeed * Time.deltaTime);

            Vector3 bob = CameraJuice.CalculateBobOffset(_time, speed, settings.BobFrequency, settings.BobAmplitude);
            targetCamera.transform.localPosition = bob;
        }

        /// <summary>Applies roll tilt; called separately so slide state (owned by ActionController) can drive it without CameraController depending on Actions.</summary>
        public void ApplyTilt(float horizontalInput, bool isSliding)
        {
            float targetTilt = CameraJuice.CalculateTilt(horizontalInput, settings.MaxLeanTiltDegrees, isSliding, settings.SlideTiltDegrees);
            Vector3 euler = targetCamera.transform.localEulerAngles;
            float currentZ = euler.z > 180f ? euler.z - 360f : euler.z;
            float newZ = Mathf.Lerp(currentZ, targetTilt, settings.TiltLerpSpeed * Time.deltaTime);
            targetCamera.transform.localRotation = Quaternion.Euler(euler.x, euler.y, newZ);
        }
    }
}
