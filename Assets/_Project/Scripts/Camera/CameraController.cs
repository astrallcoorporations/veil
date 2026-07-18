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
        private Vector3 _baseLocalPosition;

        private void Reset()
        {
            targetCamera = GetComponentInChildren<UnityEngine.Camera>();
        }

        private void Awake()
        {
            if (targetCamera != null)
                _baseLocalPosition = targetCamera.transform.localPosition;
        }

        private void Update()
        {
            if (targetCamera == null || settings == null || motor == null) return;

            _time += Time.deltaTime;
            float speed = new Vector3(motor.Velocity.x, 0f, motor.Velocity.z).magnitude;

            float targetFov = CameraJuice.CalculateFovKick(settings.BaseFov, speed, sprintSpeedForJuice, settings.MaxFovKick);
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, targetFov, settings.FovLerpSpeed * Time.deltaTime);

            Vector3 bob = CameraJuice.CalculateBobOffset(_time, speed, settings.BobFrequency, settings.BobAmplitude);
            targetCamera.transform.localPosition = _baseLocalPosition + bob;
        }

        /// <summary>Applies pitch (from mouse look) and roll tilt (from strafe/slide) in one rotation write so they don't fight.</summary>
        public void ApplyLook(float pitch, float horizontalInput, bool isSliding)
        {
            float targetTilt = CameraJuice.CalculateTilt(horizontalInput, settings.MaxLeanTiltDegrees, isSliding, settings.SlideTiltDegrees);
            float currentZ = targetCamera.transform.localEulerAngles.z;
            if (currentZ > 180f) currentZ -= 360f;
            float newZ = Mathf.Lerp(currentZ, targetTilt, settings.TiltLerpSpeed * Time.deltaTime);
            targetCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, newZ);
        }
    }
}
