using UnityEngine;

namespace Veil.Movement
{
    /// <summary>
    /// Kinematic capsule motor. Custom collide-and-slide implementation (not Unity's
    /// built-in CharacterController) so slope handling and slide physics are fully
    /// controllable. Applies velocity only — has no opinion on what velocity should be;
    /// that decision belongs to the state machine and actions.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class CharacterMotor : MonoBehaviour, IMotor
    {
        private const int MaxSlideIterations = 4;
        private const float SkinWidth = 0.02f;

        [SerializeField] private MovementSettings settings;
        [SerializeField] private LayerMask collisionMask = ~0;

        private CapsuleCollider _capsule;
        private readonly RaycastHit[] _hitBuffer = new RaycastHit[8];

        /// <summary>Movement tuning data driving capsule height and slope limits.</summary>
        public MovementSettings Settings { get => settings; set => settings = value; }

        /// <inheritdoc />
        public bool IsGrounded { get; private set; }

        /// <inheritdoc />
        public Vector3 GroundNormal { get; private set; } = Vector3.up;

        /// <inheritdoc />
        public Vector3 Velocity { get; private set; }

        private void Awake()
        {
            _capsule = GetComponent<CapsuleCollider>();
            _capsule.height = settings != null ? settings.StandingHeight : 1.8f;
            _capsule.radius = settings != null ? settings.CapsuleRadius : 0.35f;
            _capsule.center = new Vector3(0f, _capsule.height * 0.5f, 0f);
        }

        /// <inheritdoc />
        public void Move(Vector3 velocity, float deltaTime)
        {
            Velocity = velocity;
            Vector3 remaining = velocity * deltaTime;

            for (int i = 0; i < MaxSlideIterations && remaining.sqrMagnitude > 0.0000001f; i++)
            {
                if (!CapsuleCastInternal(remaining.normalized, remaining.magnitude + SkinWidth, out RaycastHit hit))
                {
                    transform.position += remaining;
                    remaining = Vector3.zero;
                    break;
                }

                float travel = Mathf.Max(0f, hit.distance - SkinWidth);
                transform.position += remaining.normalized * travel;
                remaining = MotorMath.SlideAlongSurface(remaining - remaining.normalized * travel, hit.normal);
            }

            UpdateGroundState();
        }

        /// <inheritdoc />
        public bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit) =>
            CapsuleCastInternal(direction, maxDistance, out hit);

        /// <inheritdoc />
        public void SetHeight(float height)
        {
            _capsule.height = height;
            _capsule.center = new Vector3(0f, height * 0.5f, 0f);
        }

        private bool CapsuleCastInternal(Vector3 direction, float maxDistance, out RaycastHit hit)
        {
            Vector3 point0 = transform.position + _capsule.center + Vector3.up * (_capsule.height * 0.5f - _capsule.radius);
            Vector3 point1 = transform.position + _capsule.center - Vector3.up * (_capsule.height * 0.5f - _capsule.radius);

            int count = Physics.CapsuleCastNonAlloc(point0, point1, _capsule.radius, direction, _hitBuffer, maxDistance, collisionMask, QueryTriggerInteraction.Ignore);
            if (count == 0)
            {
                hit = default;
                return false;
            }

            int closest = 0;
            for (int i = 1; i < count; i++)
            {
                if (_hitBuffer[i].distance < _hitBuffer[closest].distance) closest = i;
            }
            hit = _hitBuffer[closest];
            return true;
        }

        private void UpdateGroundState()
        {
            float slopeLimit = settings != null ? settings.MaxWalkableSlopeAngle : 50f;
            if (CapsuleCastInternal(Vector3.down, SkinWidth * 4f, out RaycastHit hit) && MotorMath.IsWalkable(hit.normal, slopeLimit))
            {
                IsGrounded = true;
                GroundNormal = hit.normal;
            }
            else
            {
                IsGrounded = false;
                GroundNormal = Vector3.up;
            }
        }
    }
}
