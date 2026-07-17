using UnityEngine;

namespace Veil.Camera
{
    /// <summary>All tunable camera-juice values (FOV kick, tilt, bob).</summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "VEIL/Camera Settings")]
    public sealed class CameraSettings : ScriptableObject
    {
        [Header("FOV Kick")]
        [SerializeField] private float baseFov = 90f;
        [SerializeField] private float maxFovKick = 12f;
        [SerializeField] private float fovLerpSpeed = 8f;

        [Header("Tilt")]
        [SerializeField] private float maxLeanTiltDegrees = 4f;
        [SerializeField] private float slideTiltDegrees = 10f;
        [SerializeField] private float tiltLerpSpeed = 10f;

        [Header("Bob")]
        [SerializeField] private float bobFrequency = 1.8f;
        [SerializeField] private float bobAmplitude = 0.05f;

        public float BaseFov => baseFov;
        public float MaxFovKick => maxFovKick;
        public float FovLerpSpeed => fovLerpSpeed;
        public float MaxLeanTiltDegrees => maxLeanTiltDegrees;
        public float SlideTiltDegrees => slideTiltDegrees;
        public float TiltLerpSpeed => tiltLerpSpeed;
        public float BobFrequency => bobFrequency;
        public float BobAmplitude => bobAmplitude;
    }
}
