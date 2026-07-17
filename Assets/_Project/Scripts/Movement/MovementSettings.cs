using UnityEngine;

namespace Veil.Movement
{
    /// <summary>
    /// All tunable movement values for the player controller. Every number the
    /// movement/action/motor code needs lives here — no magic numbers in logic code.
    /// </summary>
    [CreateAssetMenu(fileName = "MovementSettings", menuName = "VEIL/Movement Settings")]
    public sealed class MovementSettings : ScriptableObject
    {
        [Header("Locomotion Speeds (m/s)")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float crouchSpeed = 2.5f;

        [Header("Acceleration")]
        [SerializeField] private float groundAcceleration = 60f;
        [SerializeField] private float airAcceleration = 20f;
        [SerializeField] private float airControlFactor = 0.35f;

        [Header("Gravity")]
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float maxFallSpeed = -40f;

        [Header("Capsule")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float capsuleRadius = 0.35f;
        [SerializeField, Range(0f, 89f)] private float maxWalkableSlopeAngle = 50f;

        [Header("Slide")]
        [SerializeField] private float slideInitialBoost = 3f;
        [SerializeField] private float slideFriction = 1.5f;
        [SerializeField] private float minSlideSpeed = 1.5f;

        [Header("Vault / Mantle")]
        [SerializeField] private float vaultMaxHeight = 1.1f;
        [SerializeField] private float mantleMaxHeight = 2.2f;
        [SerializeField] private float ledgeDetectRange = 0.8f;

        public float WalkSpeed { get => walkSpeed; set => walkSpeed = value; }
        public float SprintSpeed { get => sprintSpeed; set => sprintSpeed = value; }
        public float CrouchSpeed { get => crouchSpeed; set => crouchSpeed = value; }
        public float GroundAcceleration => groundAcceleration;
        public float AirAcceleration => airAcceleration;
        public float AirControlFactor => airControlFactor;
        public float Gravity => gravity;
        public float MaxFallSpeed => maxFallSpeed;
        public float StandingHeight => standingHeight;
        public float CrouchHeight => crouchHeight;
        public float CapsuleRadius => capsuleRadius;
        public float MaxWalkableSlopeAngle { get => maxWalkableSlopeAngle; set => maxWalkableSlopeAngle = value; }
        public float SlideInitialBoost => slideInitialBoost;
        public float SlideFriction => slideFriction;
        public float MinSlideSpeed => minSlideSpeed;
        public float VaultMaxHeight => vaultMaxHeight;
        public float MantleMaxHeight => mantleMaxHeight;
        public float LedgeDetectRange => ledgeDetectRange;

        /// <summary>Clamps interdependent values so an invalid Inspector edit can't produce broken movement.</summary>
        public void OnValidate()
        {
            crouchSpeed = Mathf.Min(crouchSpeed, sprintSpeed);
            walkSpeed = Mathf.Min(walkSpeed, sprintSpeed);
            maxWalkableSlopeAngle = Mathf.Clamp(maxWalkableSlopeAngle, 0f, 90f);
            vaultMaxHeight = Mathf.Max(0f, vaultMaxHeight);
            mantleMaxHeight = Mathf.Max(vaultMaxHeight, mantleMaxHeight);
        }
    }
}
