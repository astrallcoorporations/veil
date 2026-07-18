using UnityEngine;

namespace Veil.Camera
{
    /// <summary>Pure, allocation-free camera-juice math: FOV kick, tilt, and procedural bob.</summary>
    public static class CameraJuice
    {
        /// <summary>Linearly interpolates FOV from base toward base+maxKick as speed approaches maxSpeed.</summary>
        public static float CalculateFovKick(float baseFov, float speed, float maxSpeed, float maxKick)
        {
            float t = maxSpeed > 0f ? Mathf.Clamp01(speed / maxSpeed) : 0f;
            return baseFov + maxKick * t;
        }

        /// <summary>Camera roll in degrees: slide tilt takes priority over lean-from-strafe tilt.</summary>
        public static float CalculateTilt(float horizontalInput, float maxTiltDegrees, bool isSliding, float slideTiltDegrees)
        {
            if (isSliding) return slideTiltDegrees;
            return -Mathf.Clamp(horizontalInput, -1f, 1f) * maxTiltDegrees;
        }

        /// <summary>Vertical/lateral procedural bob offset, scaled by current speed.</summary>
        public static Vector3 CalculateBobOffset(float time, float speed, float bobFrequency, float bobAmplitude)
        {
            float cycle = time * bobFrequency;
            float y = Mathf.Sin(cycle * 2f) * bobAmplitude * speed;
            float x = Mathf.Cos(cycle) * bobAmplitude * 0.5f * speed;
            return new Vector3(x, y, 0f);
        }
    }
}
