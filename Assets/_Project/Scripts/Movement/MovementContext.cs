using UnityEngine;
using Veil.Input;

namespace Veil.Movement
{
    /// <summary>
    /// Mutable per-frame data shared by the state machine and every action.
    /// One instance per player, owned and ticked by <see cref="CharacterMotor"/>.
    /// </summary>
    public sealed class MovementContext
    {
        /// <summary>The motor this context drives.</summary>
        public IMotor Motor { get; }

        /// <summary>Player input for this frame.</summary>
        public InputReader Input { get; }

        /// <summary>Tunable movement values.</summary>
        public MovementSettings Settings { get; }

        /// <summary>Current desired velocity; states/actions read and write this each tick.</summary>
        public Vector3 Velocity;

        /// <summary>Time elapsed since the last tick, in seconds.</summary>
        public float DeltaTime;

        public MovementContext(IMotor motor, InputReader input, MovementSettings settings)
        {
            Motor = motor;
            Input = input;
            Settings = settings;
        }
    }
}
