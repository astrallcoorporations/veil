# VEIL Milestone 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first playable slice of VEIL — a Mirror's Edge-feel first-person controller (sprint/crouch/slide/vault/mantle), physics grab/carry interaction, and a small greybox stealth-sandbox test level, in Unity 6 URP.

**Architecture:** Hybrid FSM + Actions. A dumb kinematic `CharacterMotor` (custom collide-and-slide capsule, not Unity's built-in `CharacterController`) applies velocity. A `MovementStateMachine` owns coarse macro-state (Grounded/Air/Crouch). An `ActionController` owns one-shot interruptible moves (Vault/Mantle/Slide) as `IMovementAction` objects, priority-resolved. Hot-path math (slope clamping, slide velocity, ledge decision, camera juice, grab spring) is extracted into pure static classes so it's EditMode-testable without a scene, and zero-GC-checkable via Unity's `Is.Not.AllocatingGCMemory()` constraint.

**Tech Stack:** Unity 6000.0 LTS, URP, C#, new Input System, Unity Test Framework (NUnit, EditMode + PlayMode).

## Global Constraints

These apply to every task below; not repeated per-task.

- One responsibility per class.
- Composition over inheritance — no base-class movement hierarchies; behavior is composed via `IMovementState` / `IMovementAction` / `IMotor` interfaces.
- No magic numbers — every tunable lives in `MovementSettings` or `CameraSettings` ScriptableObjects, never a literal in logic code.
- XML doc comments (`///`) on every public type and member.
- `[SerializeField]` private fields, never public fields, for Inspector-exposed state.
- Keep classes under ~300 lines; split further if a class does more than one job.
- No premature optimization beyond the Performance Target constraints below, which are hard requirements from the start, not later polish.
- Movement responsiveness always beats animation accuracy — no animation/root-motion work in this milestone; nothing in code may block on an animation event to resolve movement.
- 144 FPS target on mid-range hardware; zero GC allocations in per-frame movement/camera hot paths (`CharacterMotor`, `MovementStateMachine`, states, `ActionController`, actions, `CameraJuice`); no per-frame LINQ anywhere in those paths.
- Placeholder art/geometry only (primitives/ProBuilder) — no final assets in this milestone.

---

## File Structure

```
Assets/_Project/
  Scripts/
    Movement/
      IMotor.cs
      MovementContext.cs
      MotorMath.cs
      CharacterMotor.cs
      MovementSettings.cs
      States/
        MovementStateId.cs
        IMovementState.cs
        MovementStateMachine.cs
        GroundedState.cs
        AirState.cs
        CrouchState.cs
      Actions/
        IMovementAction.cs
        ActionController.cs
        SlideAction.cs
        LedgeDetector.cs
        VaultAction.cs
        MantleAction.cs
      Input/
        VeilControls.inputactions
        InputReader.cs
    Camera/
      CameraSettings.cs
      CameraJuice.cs
      CameraController.cs
    Interaction/
      IInteractable.cs
      NearestInteractableSelector.cs
      InteractionCaster.cs
      Door.cs
      Lever.cs
      Pickup.cs
      IGrabbable.cs
      GrabbableObject.cs
      GrabPhysicsMath.cs
      GrabController.cs
    LevelGen/
      Editor/
        LevelBuilder.cs
  Levels/
    M1_StealthSandbox.unity
  Prefabs/
    Player.prefab
Packages/manifest.json
ProjectSettings/  (Unity-generated)
Assets/_Project/Tests/
  EditMode/
    MotorMathTests.cs
    MovementStateMachineTests.cs
    ActionControllerTests.cs
    SlideActionTests.cs
    LedgeDetectorTests.cs
    CameraJuiceTests.cs
    NearestInteractableSelectorTests.cs
    GrabPhysicsMathTests.cs
    MovementSettingsTests.cs
    LevelBuilderTests.cs
    InputReaderTests.cs
  PlayMode/
    CharacterMotorPlayModeTests.cs
    VaultMantlePlayModeTests.cs
    GrabControllerPlayModeTests.cs
docs/PLAYTEST_CHECKLIST.md
```

---

### Task 1: Unity Project Bootstrap

**Files:**
- Create: `ProjectSettings/` (Unity-generated via `-createProject`)
- Create: `Packages/manifest.json`
- Create: `Assets/_Project/` (empty folders per structure above)
- Create: `.gitignore`

**Interfaces:**
- Consumes: nothing (first task).
- Produces: a compiling empty Unity 6 URP project with Input System installed, and the folder skeleton every later task writes into. Later tasks assume `Assets/_Project/Scripts/...` and `Assets/_Project/Tests/...` already exist.

- [ ] **Step 1: Create the project via Editor CLI**

Run (adjust the Editor path to whatever `docs/superpowers/plans/UNITY_EDITOR_PATH.txt` records once Task 0's install completes — see note below):

```
"<UnityEditorPath>\Unity.exe" -batchmode -quit -createProject "C:\Users\thats\OneDrive\Documents\game" -logFile "C:\Users\thats\OneDrive\Documents\game\create_project.log"
```

Expected: process exits 0, `ProjectSettings/ProjectVersion.txt` and `Assets/` now exist. Check the log for `Project created successfully` or absence of `Fatal Error`.

> **Note:** the exact Unity Editor path is whatever the machine's install resolves to (e.g. `C:\Program Files\Unity\Hub\Editor\6000.0.xx\Editor\Unity.exe`). Confirm it exists (`Test-Path`) before running this step; if Unity isn't installed yet, install Unity Hub + a Unity 6000.0 LTS Editor with default modules first — this step cannot proceed without it.

- [ ] **Step 2: Add URP and Input System packages**

Edit `Packages/manifest.json`, ensure these entries exist under `"dependencies"` (keep whatever versions Unity 6000.0 LTS ships by default when you add them via Package Manager — do not hand-pin unrelated versions):

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "17.0.3",
    "com.unity.inputsystem": "1.11.2",
    "com.unity.test-framework": "1.4.5",
    "com.unity.probuilder": "6.0.4"
  }
}
```

- [ ] **Step 3: Create the folder skeleton**

Create every folder listed in the File Structure section above as empty directories under `Assets/_Project/` (Unity will populate `.meta` files for each on next Editor open — a placeholder `.gitkeep` file in each otherwise-empty folder is fine here since these are structural, not code placeholders).

- [ ] **Step 4: Verify the project compiles headless**

```
"<UnityEditorPath>\Unity.exe" -batchmode -quit -projectPath "C:\Users\thats\OneDrive\Documents\game" -logFile "C:\Users\thats\OneDrive\Documents\game\verify_open.log"
```

Expected: exit code 0, log contains no `error CS` lines.

- [ ] **Step 5: Write `.gitignore` and commit**

```
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Mm]emoryCaptures/
*.csproj
*.sln
*.tmp
*.log
.vs/
.vscode/
UserSettings/
```

```bash
git add .gitignore Packages/manifest.json ProjectSettings Assets/_Project
git commit -m "chore: bootstrap Unity 6 URP project for VEIL M1"
```

---

### Task 2: Input System — `InputReader`

**Files:**
- Create: `Assets/_Project/Scripts/Movement/Input/VeilControls.inputactions`
- Create: `Assets/_Project/Scripts/Movement/Input/InputReader.cs`
- Test: `Assets/_Project/Tests/EditMode/InputReaderTests.cs`

**Interfaces:**
- Consumes: `UnityEngine.InputSystem` package (Task 1).
- Produces: `InputReader` (ScriptableObject), consumed by every later Movement/Interaction task:
  - `Vector2 MoveInput { get; }`
  - `Vector2 LookInput { get; }`
  - `bool SprintHeld { get; }`
  - `bool CrouchHeld { get; }`
  - `event Action JumpVaultPressed`
  - `event Action SlidePressed`
  - `event Action InteractPressed`
  - `event Action GrabPressed`
  - `void Enable()` / `void Disable()`

- [ ] **Step 1: Create the Input Actions asset**

In the Unity Editor (or by hand-authoring the `.inputactions` JSON, then letting Unity regenerate its `.meta`), create `VeilControls.inputactions` with one action map `"Gameplay"` containing:
- `Move` (Value/Vector2, WASD + left stick composite)
- `Look` (Value/Vector2, mouse delta + right stick)
- `Sprint` (Button, Left Shift / gamepad L3)
- `Crouch` (Button, Left Ctrl / gamepad B)
- `Slide` (Button, same binding as Crouch — slide triggers off crouch-while-sprinting, resolved in `SlideAction`, not here)
- `JumpVault` (Button, Space / gamepad A)
- `Interact` (Button, E / gamepad X)
- `Grab` (Button, F / gamepad Y)

Set "C# Class Namespace" to `Veil.Input`, class name `VeilControls`, and enable "Generate C# Class" so `VeilControls.cs` is produced under the same folder.

- [ ] **Step 2: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Veil.Tests.EditMode
{
    public class InputReaderTests : InputTestFixture
    {
        private InputReader _reader;

        public override void Setup()
        {
            base.Setup();
            _reader = ScriptableObject.CreateInstance<InputReader>();
            _reader.Enable();
        }

        public override void TearDown()
        {
            _reader.Disable();
            Object.DestroyImmediate(_reader);
            base.TearDown();
        }

        [Test]
        public void MoveInput_DefaultsToZero()
        {
            Assert.AreEqual(Vector2.zero, _reader.MoveInput);
        }

        [Test]
        public void JumpVaultPressed_FiresOnce_WhenButtonPressed()
        {
            var keyboard = InputSystem.AddDevice<Keyboard>();
            int fireCount = 0;
            _reader.JumpVaultPressed += () => fireCount++;

            Press(keyboard.spaceKey);
            Release(keyboard.spaceKey);

            Assert.AreEqual(1, fireCount);
        }
    }
}
```

- [ ] **Step 3: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter InputReaderTests -logFile editmode_input.log`
Expected: FAIL — `InputReader` does not exist yet (compile error reported in the run).

- [ ] **Step 4: Implement `InputReader`**

```csharp
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Veil.Input
{
    /// <summary>
    /// ScriptableObject wrapper around the generated <see cref="VeilControls"/> Input System
    /// actions. Every gameplay system reads player input through this asset instead of
    /// binding to Input System actions directly, so input can be swapped, mocked, or
    /// rebound without touching movement/camera/interaction code.
    /// </summary>
    [CreateAssetMenu(fileName = "InputReader", menuName = "VEIL/Input Reader")]
    public sealed class InputReader : ScriptableObject, VeilControls.IGameplayActions
    {
        private VeilControls _controls;

        /// <summary>Current move axis, range [-1,1] per component.</summary>
        public Vector2 MoveInput { get; private set; }

        /// <summary>Current look delta for this frame.</summary>
        public Vector2 LookInput { get; private set; }

        /// <summary>True while the sprint button is held.</summary>
        public bool SprintHeld { get; private set; }

        /// <summary>True while the crouch button is held.</summary>
        public bool CrouchHeld { get; private set; }

        /// <summary>Fires on the frame the jump/vault button is pressed.</summary>
        public event Action JumpVaultPressed;

        /// <summary>Fires on the frame the slide button is pressed.</summary>
        public event Action SlidePressed;

        /// <summary>Fires on the frame the interact button is pressed.</summary>
        public event Action InteractPressed;

        /// <summary>Fires on the frame the grab button is pressed.</summary>
        public event Action GrabPressed;

        /// <summary>Enables the underlying Input System action map. Call from OnEnable of the owning behaviour.</summary>
        public void Enable()
        {
            if (_controls == null)
            {
                _controls = new VeilControls();
                _controls.Gameplay.SetCallbacks(this);
            }
            _controls.Gameplay.Enable();
        }

        /// <summary>Disables the underlying Input System action map. Call from OnDisable of the owning behaviour.</summary>
        public void Disable()
        {
            _controls?.Gameplay.Disable();
        }

        void VeilControls.IGameplayActions.OnMove(InputAction.CallbackContext context) =>
            MoveInput = context.ReadValue<Vector2>();

        void VeilControls.IGameplayActions.OnLook(InputAction.CallbackContext context) =>
            LookInput = context.ReadValue<Vector2>();

        void VeilControls.IGameplayActions.OnSprint(InputAction.CallbackContext context) =>
            SprintHeld = context.ReadValueAsButton();

        void VeilControls.IGameplayActions.OnCrouch(InputAction.CallbackContext context) =>
            CrouchHeld = context.ReadValueAsButton();

        void VeilControls.IGameplayActions.OnSlide(InputAction.CallbackContext context)
        {
            if (context.performed) SlidePressed?.Invoke();
        }

        void VeilControls.IGameplayActions.OnJumpVault(InputAction.CallbackContext context)
        {
            if (context.performed) JumpVaultPressed?.Invoke();
        }

        void VeilControls.IGameplayActions.OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed) InteractPressed?.Invoke();
        }

        void VeilControls.IGameplayActions.OnGrab(InputAction.CallbackContext context)
        {
            if (context.performed) GrabPressed?.Invoke();
        }
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter InputReaderTests -logFile editmode_input.log`
Expected: PASS, 2/2 tests green.

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Movement/Input Assets/_Project/Tests/EditMode/InputReaderTests.cs
git commit -m "feat: add InputReader wrapping generated Input System actions"
```

---

### Task 3: `MovementSettings` and `CameraSettings`

**Files:**
- Create: `Assets/_Project/Scripts/Movement/MovementSettings.cs`
- Create: `Assets/_Project/Scripts/Camera/CameraSettings.cs`
- Test: `Assets/_Project/Tests/EditMode/MovementSettingsTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `MovementSettings` and `CameraSettings` ScriptableObjects, referenced by every Movement/Camera task from here on. Field list below is the contract later tasks rely on — do not rename.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;

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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter MovementSettingsTests -logFile editmode_settings.log`
Expected: FAIL — `MovementSettings` does not exist.

- [ ] **Step 3: Implement `MovementSettings`**

```csharp
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
```

- [ ] **Step 4: Implement `CameraSettings`**

```csharp
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
```

- [ ] **Step 5: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter MovementSettingsTests -logFile editmode_settings.log`
Expected: PASS, 2/2 tests green.

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Movement/MovementSettings.cs Assets/_Project/Scripts/Camera/CameraSettings.cs Assets/_Project/Tests/EditMode/MovementSettingsTests.cs
git commit -m "feat: add MovementSettings and CameraSettings tunable data assets"
```

---

### Task 4: `IMotor`, `MovementContext`, `MotorMath`

**Files:**
- Create: `Assets/_Project/Scripts/Movement/IMotor.cs`
- Create: `Assets/_Project/Scripts/Movement/MovementContext.cs`
- Create: `Assets/_Project/Scripts/Movement/MotorMath.cs`
- Test: `Assets/_Project/Tests/EditMode/MotorMathTests.cs`

**Interfaces:**
- Consumes: `MovementSettings` (Task 3), `InputReader` (Task 2).
- Produces:
  - `IMotor` — the abstraction every state/action programs against, so Task 6/7/8/9 can be tested with a fake motor instead of real physics: `bool IsGrounded { get; }`, `Vector3 GroundNormal { get; }`, `Vector3 Velocity { get; }`, `void Move(Vector3 velocity, float deltaTime)`, `bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit)`, `void SetHeight(float height)`.
  - `MovementContext` — mutable per-frame bag passed to every state/action: `IMotor Motor`, `InputReader Input`, `MovementSettings Settings`, `Vector3 Velocity` (read/write), `float DeltaTime`.
  - `MotorMath` static class: `bool IsWalkable(Vector3 normal, float maxSlopeAngleDegrees)`, `Vector3 SlideAlongSurface(Vector3 moveDelta, Vector3 hitNormal)`, `Vector3 ComputeSlideVelocity(Vector3 currentVelocity, Vector3 groundNormal, float friction, float minSpeed, float deltaTime)`.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Veil.Tests.EditMode
{
    public class MotorMathTests
    {
        [Test]
        public void IsWalkable_FlatGround_ReturnsTrue()
        {
            Assert.IsTrue(MotorMath.IsWalkable(Vector3.up, 50f));
        }

        [Test]
        public void IsWalkable_VerticalWall_ReturnsFalse()
        {
            Assert.IsFalse(MotorMath.IsWalkable(Vector3.right, 50f));
        }

        [Test]
        public void SlideAlongSurface_RemovesVelocityIntoWall()
        {
            var moveDelta = new Vector3(1f, 0f, 0f);
            var wallNormal = new Vector3(-1f, 0f, 0f);

            var result = MotorMath.SlideAlongSurface(moveDelta, wallNormal);

            Assert.AreEqual(0f, result.x, 0.0001f);
        }

        [Test]
        public void ComputeSlideVelocity_DecaysTowardMinSpeed()
        {
            var start = new Vector3(0f, 0f, 10f);
            var result = MotorMath.ComputeSlideVelocity(start, Vector3.up, friction: 1.5f, minSpeed: 1.5f, deltaTime: 1f);

            Assert.Less(result.magnitude, start.magnitude);
            Assert.GreaterOrEqual(result.magnitude, 1.5f);
        }

        [Test]
        public void SlideAlongSurface_IsAllocationFree()
        {
            Assert.That(() => MotorMath.SlideAlongSurface(Vector3.forward, Vector3.up), Is.Not.AllocatingGCMemory());
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter MotorMathTests -logFile editmode_motormath.log`
Expected: FAIL — `MotorMath` does not exist.

- [ ] **Step 3: Implement `IMotor`**

```csharp
using UnityEngine;

namespace Veil.Movement
{
    /// <summary>
    /// Abstraction over the physical capsule motor. States and actions program
    /// against this interface, not <see cref="CharacterMotor"/> directly, so they
    /// can be unit tested with a fake implementation instead of real physics.
    /// </summary>
    public interface IMotor
    {
        /// <summary>True if the capsule is currently resting on walkable ground.</summary>
        bool IsGrounded { get; }

        /// <summary>Surface normal of the ground currently supporting the capsule.</summary>
        Vector3 GroundNormal { get; }

        /// <summary>Current resolved velocity, in m/s.</summary>
        Vector3 Velocity { get; }

        /// <summary>Moves the capsule by <paramref name="velocity"/> * <paramref name="deltaTime"/>, resolving collisions.</summary>
        void Move(Vector3 velocity, float deltaTime);

        /// <summary>Casts the capsule shape forward; used by ledge detection.</summary>
        bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit);

        /// <summary>Sets the capsule height (standing vs crouch).</summary>
        void SetHeight(float height);
    }
}
```

- [ ] **Step 4: Implement `MovementContext`**

```csharp
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
```

- [ ] **Step 5: Implement `MotorMath`**

```csharp
using UnityEngine;

namespace Veil.Movement
{
    /// <summary>
    /// Pure, allocation-free movement math shared by the motor, states, and actions.
    /// Kept free of MonoBehaviour/UnityEngine.Object dependencies so it is directly
    /// unit-testable and safe to call from any per-frame hot path.
    /// </summary>
    public static class MotorMath
    {
        /// <summary>True if a surface with the given normal is walkable at the given max slope angle.</summary>
        public static bool IsWalkable(Vector3 normal, float maxSlopeAngleDegrees)
        {
            float angle = Vector3.Angle(normal, Vector3.up);
            return angle <= maxSlopeAngleDegrees;
        }

        /// <summary>Projects a move delta onto a collision plane so motion doesn't penetrate the surface.</summary>
        public static Vector3 SlideAlongSurface(Vector3 moveDelta, Vector3 hitNormal)
        {
            return Vector3.ProjectOnPlane(moveDelta, hitNormal);
        }

        /// <summary>Applies ground friction to a slide, never dropping below <paramref name="minSpeed"/> while horizontal speed exceeds it.</summary>
        public static Vector3 ComputeSlideVelocity(Vector3 currentVelocity, Vector3 groundNormal, float friction, float minSpeed, float deltaTime)
        {
            Vector3 planar = Vector3.ProjectOnPlane(currentVelocity, groundNormal);
            float speed = planar.magnitude;
            if (speed <= minSpeed) return planar;

            float decayedSpeed = Mathf.Max(minSpeed, speed - friction * deltaTime);
            return planar.normalized * decayedSpeed;
        }
    }
}
```

- [ ] **Step 6: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter MotorMathTests -logFile editmode_motormath.log`
Expected: PASS, 5/5 tests green (including the GC-allocation constraint).

- [ ] **Step 7: Commit**

```bash
git add Assets/_Project/Scripts/Movement/IMotor.cs Assets/_Project/Scripts/Movement/MovementContext.cs Assets/_Project/Scripts/Movement/MotorMath.cs Assets/_Project/Tests/EditMode/MotorMathTests.cs
git commit -m "feat: add IMotor abstraction, MovementContext, and pure MotorMath helpers"
```

---

### Task 5: `CharacterMotor`

**Files:**
- Create: `Assets/_Project/Scripts/Movement/CharacterMotor.cs`
- Test: `Assets/_Project/Tests/PlayMode/CharacterMotorPlayModeTests.cs`

**Interfaces:**
- Consumes: `IMotor` (Task 4), `MotorMath` (Task 4), `MovementSettings` (Task 3).
- Produces: `CharacterMotor : MonoBehaviour, IMotor` — a `CapsuleCollider`-based kinematic motor. Later tasks (`MovementStateMachine`, actions) reference it only through `IMotor`.

- [ ] **Step 1: Write the failing PlayMode test**

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform PlayMode -testFilter CharacterMotorPlayModeTests -logFile playmode_motor.log`
Expected: FAIL — `CharacterMotor` does not exist.

- [ ] **Step 3: Implement `CharacterMotor`**

```csharp
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
        private RaycastHit[] _hitBuffer = new RaycastHit[8];

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
```

- [ ] **Step 4: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform PlayMode -testFilter CharacterMotorPlayModeTests -logFile playmode_motor.log`
Expected: PASS, 1/1 test green.

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/Movement/CharacterMotor.cs Assets/_Project/Tests/PlayMode/CharacterMotorPlayModeTests.cs
git commit -m "feat: add CharacterMotor kinematic collide-and-slide capsule"
```

---

### Task 6: `MovementStateMachine` and States

**Files:**
- Create: `Assets/_Project/Scripts/Movement/States/MovementStateId.cs`
- Create: `Assets/_Project/Scripts/Movement/States/IMovementState.cs`
- Create: `Assets/_Project/Scripts/Movement/States/MovementStateMachine.cs`
- Create: `Assets/_Project/Scripts/Movement/States/GroundedState.cs`
- Create: `Assets/_Project/Scripts/Movement/States/AirState.cs`
- Create: `Assets/_Project/Scripts/Movement/States/CrouchState.cs`
- Test: `Assets/_Project/Tests/EditMode/MovementStateMachineTests.cs`

**Interfaces:**
- Consumes: `IMotor`, `MovementContext` (Task 4).
- Produces:
  - `enum MovementStateId { Grounded, Air, Crouch }`
  - `interface IMovementState { void Enter(MovementContext ctx); void Tick(MovementContext ctx); void Exit(MovementContext ctx); MovementStateId? RequestedTransition(MovementContext ctx); }` — `RequestedTransition` returns non-null when the state itself decides it's time to leave (e.g. Grounded sees `!IsGrounded` and requests `Air`).
  - `class MovementStateMachine { MovementStateId Current { get; } void Tick(MovementContext ctx); }` — consumed by `ActionController` (Task 7) to gate which actions may fire.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using Veil.Input;

namespace Veil.Tests.EditMode
{
    public class MovementStateMachineTests
    {
        private class FakeMotor : IMotor
        {
            public bool IsGrounded { get; set; } = true;
            public Vector3 GroundNormal { get; set; } = Vector3.up;
            public Vector3 Velocity { get; set; }
            public void Move(Vector3 velocity, float deltaTime) => Velocity = velocity;
            public bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit) { hit = default; return false; }
            public void SetHeight(float height) { }
        }

        [Test]
        public void StartsInGroundedState()
        {
            var ctx = new MovementContext(new FakeMotor(), ScriptableObject.CreateInstance<InputReader>(), ScriptableObject.CreateInstance<MovementSettings>());
            var sm = new MovementStateMachine(ctx);

            Assert.AreEqual(MovementStateId.Grounded, sm.Current);
        }

        [Test]
        public void TransitionsToAir_WhenMotorLeavesGround()
        {
            var motor = new FakeMotor { IsGrounded = true };
            var ctx = new MovementContext(motor, ScriptableObject.CreateInstance<InputReader>(), ScriptableObject.CreateInstance<MovementSettings>());
            var sm = new MovementStateMachine(ctx);

            motor.IsGrounded = false;
            sm.Tick(ctx);

            Assert.AreEqual(MovementStateId.Air, sm.Current);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter MovementStateMachineTests -logFile editmode_statemachine.log`
Expected: FAIL — types don't exist.

- [ ] **Step 3: Implement `MovementStateId` and `IMovementState`**

```csharp
namespace Veil.Movement.States
{
    /// <summary>Coarse macro-locomotion state. Kept small deliberately — one-shot moves (vault/mantle/slide) live in the Actions layer, not here.</summary>
    public enum MovementStateId
    {
        Grounded,
        Air,
        Crouch
    }
}
```

```csharp
namespace Veil.Movement.States
{
    /// <summary>One macro-locomotion state. Implementations must not allocate in <see cref="Tick"/>.</summary>
    public interface IMovementState
    {
        /// <summary>Called once when the state becomes active.</summary>
        void Enter(MovementContext ctx);

        /// <summary>Called every motor tick while this state is active.</summary>
        void Tick(MovementContext ctx);

        /// <summary>Called once when the state stops being active.</summary>
        void Exit(MovementContext ctx);

        /// <summary>Returns the state to transition to this tick, or null to stay.</summary>
        MovementStateId? RequestedTransition(MovementContext ctx);
    }
}
```

- [ ] **Step 4: Implement `GroundedState`, `AirState`, `CrouchState`**

```csharp
using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Standing/walking/sprinting on walkable ground.</summary>
    public sealed class GroundedState : IMovementState
    {
        public void Enter(MovementContext ctx) { }

        public void Tick(MovementContext ctx)
        {
            float speed = ctx.Input.SprintHeld ? ctx.Settings.SprintSpeed : ctx.Settings.WalkSpeed;
            Vector3 wishDir = new Vector3(ctx.Input.MoveInput.x, 0f, ctx.Input.MoveInput.y).normalized;
            Vector3 target = wishDir * speed;
            ctx.Velocity = Vector3.MoveTowards(ctx.Velocity, target, ctx.Settings.GroundAcceleration * ctx.DeltaTime);
        }

        public void Exit(MovementContext ctx) { }

        public MovementStateId? RequestedTransition(MovementContext ctx)
        {
            if (!ctx.Motor.IsGrounded) return MovementStateId.Air;
            if (ctx.Input.CrouchHeld) return MovementStateId.Crouch;
            return null;
        }
    }
}
```

```csharp
using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Airborne — reduced air control, gravity accumulates.</summary>
    public sealed class AirState : IMovementState
    {
        public void Enter(MovementContext ctx) { }

        public void Tick(MovementContext ctx)
        {
            Vector3 wishDir = new Vector3(ctx.Input.MoveInput.x, 0f, ctx.Input.MoveInput.y).normalized;
            Vector3 horizontal = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            horizontal = Vector3.MoveTowards(horizontal, wishDir * ctx.Settings.SprintSpeed, ctx.Settings.AirAcceleration * ctx.Settings.AirControlFactor * ctx.DeltaTime);

            float verticalVelocity = Mathf.Max(ctx.Settings.MaxFallSpeed, ctx.Velocity.y + ctx.Settings.Gravity * ctx.DeltaTime);
            ctx.Velocity = new Vector3(horizontal.x, verticalVelocity, horizontal.z);
        }

        public void Exit(MovementContext ctx) { }

        public MovementStateId? RequestedTransition(MovementContext ctx)
        {
            if (ctx.Motor.IsGrounded) return MovementStateId.Grounded;
            return null;
        }
    }
}
```

```csharp
using UnityEngine;

namespace Veil.Movement.States
{
    /// <summary>Crouched movement — reduced speed and capsule height.</summary>
    public sealed class CrouchState : IMovementState
    {
        public void Enter(MovementContext ctx) => ctx.Motor.SetHeight(ctx.Settings.CrouchHeight);

        public void Tick(MovementContext ctx)
        {
            Vector3 wishDir = new Vector3(ctx.Input.MoveInput.x, 0f, ctx.Input.MoveInput.y).normalized;
            Vector3 target = wishDir * ctx.Settings.CrouchSpeed;
            ctx.Velocity = Vector3.MoveTowards(ctx.Velocity, target, ctx.Settings.GroundAcceleration * ctx.DeltaTime);
        }

        public void Exit(MovementContext ctx) => ctx.Motor.SetHeight(ctx.Settings.StandingHeight);

        public MovementStateId? RequestedTransition(MovementContext ctx)
        {
            if (!ctx.Motor.IsGrounded) return MovementStateId.Air;
            if (!ctx.Input.CrouchHeld) return MovementStateId.Grounded;
            return null;
        }
    }
}
```

- [ ] **Step 5: Implement `MovementStateMachine`**

```csharp
using System.Collections.Generic;

namespace Veil.Movement.States
{
    /// <summary>Owns the current macro-locomotion state and ticks it each frame.</summary>
    public sealed class MovementStateMachine
    {
        private readonly Dictionary<MovementStateId, IMovementState> _states;
        private readonly MovementContext _ctx;

        /// <summary>The currently active macro-state.</summary>
        public MovementStateId Current { get; private set; }

        public MovementStateMachine(MovementContext ctx)
        {
            _ctx = ctx;
            _states = new Dictionary<MovementStateId, IMovementState>
            {
                { MovementStateId.Grounded, new GroundedState() },
                { MovementStateId.Air, new AirState() },
                { MovementStateId.Crouch, new CrouchState() },
            };
            Current = MovementStateId.Grounded;
            _states[Current].Enter(_ctx);
        }

        /// <summary>Ticks the active state and applies any requested transition.</summary>
        public void Tick(MovementContext ctx)
        {
            _states[Current].Tick(ctx);

            MovementStateId? requested = _states[Current].RequestedTransition(ctx);
            if (requested.HasValue && requested.Value != Current)
            {
                _states[Current].Exit(ctx);
                Current = requested.Value;
                _states[Current].Enter(ctx);
            }
        }
    }
}
```

- [ ] **Step 6: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter MovementStateMachineTests -logFile editmode_statemachine.log`
Expected: PASS, 2/2 tests green.

- [ ] **Step 7: Commit**

```bash
git add Assets/_Project/Scripts/Movement/States Assets/_Project/Tests/EditMode/MovementStateMachineTests.cs
git commit -m "feat: add MovementStateMachine with Grounded/Air/Crouch states"
```

---

### Task 7: `ActionController` and `IMovementAction`

**Files:**
- Create: `Assets/_Project/Scripts/Movement/Actions/IMovementAction.cs`
- Create: `Assets/_Project/Scripts/Movement/Actions/ActionController.cs`
- Test: `Assets/_Project/Tests/EditMode/ActionControllerTests.cs`

**Interfaces:**
- Consumes: `MovementContext`, `MovementStateMachine` (Tasks 4, 6).
- Produces:
  - `interface IMovementAction { int Priority { get; } bool IsActive { get; } bool CanExecute(MovementContext ctx, MovementStateId currentState); void Execute(MovementContext ctx); void Tick(MovementContext ctx); void Cancel(MovementContext ctx); }`
  - `class ActionController { void RegisterAction(IMovementAction action); void Tick(MovementContext ctx, MovementStateId currentState); }` — consumed by Tasks 8/9 (`SlideAction`, `VaultAction`, `MantleAction`) which register themselves, and by Task 14's Player prefab wiring.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using Veil.Movement.States;

namespace Veil.Tests.EditMode
{
    public class ActionControllerTests
    {
        private class FakeAction : IMovementAction
        {
            public int Priority { get; set; }
            public bool IsActive { get; private set; }
            public bool AllowExecute { get; set; } = true;
            public int ExecuteCount { get; private set; }
            public int CancelCount { get; private set; }

            public bool CanExecute(MovementContext ctx, MovementStateId currentState) => AllowExecute && !IsActive;
            public void Execute(MovementContext ctx) { IsActive = true; ExecuteCount++; }
            public void Tick(MovementContext ctx) { }
            public void Cancel(MovementContext ctx) { IsActive = false; CancelCount++; }
        }

        [Test]
        public void ExecutesHighestPriorityValidAction()
        {
            var low = new FakeAction { Priority = 1 };
            var high = new FakeAction { Priority = 10 };
            var controller = new ActionController();
            controller.RegisterAction(low);
            controller.RegisterAction(high);

            controller.TryTriggerBestAction(null, MovementStateId.Grounded);

            Assert.AreEqual(1, high.ExecuteCount);
            Assert.AreEqual(0, low.ExecuteCount);
        }

        [Test]
        public void ActiveAction_BlocksLowerPriorityFromInterrupting()
        {
            var running = new FakeAction { Priority = 10 };
            var interrupter = new FakeAction { Priority = 5 };
            var controller = new ActionController();
            controller.RegisterAction(running);
            controller.RegisterAction(interrupter);

            controller.TryTriggerBestAction(null, MovementStateId.Grounded);
            controller.TryTriggerBestAction(null, MovementStateId.Grounded);

            Assert.AreEqual(1, running.ExecuteCount);
            Assert.AreEqual(0, interrupter.ExecuteCount);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter ActionControllerTests -logFile editmode_actioncontroller.log`
Expected: FAIL — types don't exist.

- [ ] **Step 3: Implement `IMovementAction`**

```csharp
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>
    /// A self-contained, interruptible one-shot movement move (vault, mantle, slide).
    /// Only fires from valid macro-states, and only one action may be active at a time.
    /// </summary>
    public interface IMovementAction
    {
        /// <summary>Higher wins when multiple actions are simultaneously valid.</summary>
        int Priority { get; }

        /// <summary>True while this action is currently driving movement.</summary>
        bool IsActive { get; }

        /// <summary>Whether this action's preconditions are currently met.</summary>
        bool CanExecute(MovementContext ctx, MovementStateId currentState);

        /// <summary>Begins the action.</summary>
        void Execute(MovementContext ctx);

        /// <summary>Advances the action while active; must set IsActive false internally when finished.</summary>
        void Tick(MovementContext ctx);

        /// <summary>Cancels the action early, e.g. when interrupted.</summary>
        void Cancel(MovementContext ctx);
    }
}
```

- [ ] **Step 4: Implement `ActionController`**

```csharp
using System.Collections.Generic;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>
    /// Owns the set of one-shot movement actions, resolves which one fires when
    /// multiple are simultaneously valid, and ticks/cancels the active one.
    /// </summary>
    public sealed class ActionController
    {
        private readonly List<IMovementAction> _actions = new List<IMovementAction>();
        private IMovementAction _active;

        /// <summary>Registers an action to be considered for triggering.</summary>
        public void RegisterAction(IMovementAction action) => _actions.Add(action);

        /// <summary>
        /// Attempts to trigger the highest-priority valid action. If an action is
        /// already active, no new action can start until it finishes or is cancelled.
        /// </summary>
        public void TryTriggerBestAction(MovementContext ctx, MovementStateId currentState)
        {
            if (_active != null) return;

            IMovementAction best = null;
            for (int i = 0; i < _actions.Count; i++)
            {
                var candidate = _actions[i];
                if (!candidate.CanExecute(ctx, currentState)) continue;
                if (best == null || candidate.Priority > best.Priority) best = candidate;
            }

            if (best == null) return;

            best.Execute(ctx);
            _active = best;
        }

        /// <summary>Ticks the active action, if any, clearing it once it reports inactive.</summary>
        public void Tick(MovementContext ctx)
        {
            if (_active == null) return;
            _active.Tick(ctx);
            if (!_active.IsActive) _active = null;
        }

        /// <summary>Cancels the active action, if any.</summary>
        public void CancelActive(MovementContext ctx)
        {
            if (_active == null) return;
            _active.Cancel(ctx);
            _active = null;
        }
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter ActionControllerTests -logFile editmode_actioncontroller.log`
Expected: PASS, 2/2 tests green.

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Movement/Actions/IMovementAction.cs Assets/_Project/Scripts/Movement/Actions/ActionController.cs Assets/_Project/Tests/EditMode/ActionControllerTests.cs
git commit -m "feat: add ActionController with priority-resolved one-shot movement actions"
```

---

### Task 8: `SlideAction`

**Files:**
- Create: `Assets/_Project/Scripts/Movement/Actions/SlideAction.cs`
- Test: `Assets/_Project/Tests/EditMode/SlideActionTests.cs`

**Interfaces:**
- Consumes: `IMovementAction` (Task 7), `MotorMath.ComputeSlideVelocity` (Task 4), `MovementContext` (Task 4).
- Produces: `SlideAction : IMovementAction`, `Priority = 20`. Registered into `ActionController` in Task 14's Player prefab wiring.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using Veil.Input;
using Veil.Movement.States;

namespace Veil.Tests.EditMode
{
    public class SlideActionTests
    {
        private class FakeMotor : IMotor
        {
            public bool IsGrounded { get; set; } = true;
            public Vector3 GroundNormal { get; set; } = Vector3.up;
            public Vector3 Velocity { get; set; }
            public void Move(Vector3 velocity, float deltaTime) => Velocity = velocity;
            public bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit) { hit = default; return false; }
            public void SetHeight(float height) { }
        }

        private MovementContext MakeContext(bool grounded, bool sprinting, Vector3 velocity)
        {
            var motor = new FakeMotor { IsGrounded = grounded };
            var input = ScriptableObject.CreateInstance<InputReader>();
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var ctx = new MovementContext(motor, input, settings) { Velocity = velocity };
            return ctx;
        }

        [Test]
        public void CanExecute_False_WhenAirborne()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: false, sprinting: true, velocity: new Vector3(0, 0, 8));

            Assert.IsFalse(action.CanExecute(ctx, MovementStateId.Air));
        }

        [Test]
        public void CanExecute_True_WhenGroundedAndFastEnough()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: true, sprinting: true, velocity: new Vector3(0, 0, 8));

            Assert.IsTrue(action.CanExecute(ctx, MovementStateId.Grounded));
        }

        [Test]
        public void Execute_SetsIsActiveTrue()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: true, sprinting: true, velocity: new Vector3(0, 0, 8));

            action.Execute(ctx);

            Assert.IsTrue(action.IsActive);
        }

        [Test]
        public void Tick_EndsSlide_WhenSpeedDropsBelowMinimum()
        {
            var action = new SlideAction();
            var ctx = MakeContext(grounded: true, sprinting: true, velocity: new Vector3(0, 0, 1.4f));

            action.Execute(ctx);
            action.Tick(ctx);

            Assert.IsFalse(action.IsActive);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter SlideActionTests -logFile editmode_slide.log`
Expected: FAIL — `SlideAction` does not exist.

- [ ] **Step 3: Implement `SlideAction`**

```csharp
using UnityEngine;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>Slide triggered by crouching while sprinting on the ground; ends when speed decays below the minimum.</summary>
    public sealed class SlideAction : IMovementAction
    {
        public int Priority => 20;
        public bool IsActive { get; private set; }

        public bool CanExecute(MovementContext ctx, MovementStateId currentState)
        {
            if (IsActive) return false;
            if (currentState != MovementStateId.Grounded) return false;
            if (!ctx.Input.SprintHeld || !ctx.Input.CrouchHeld) return false;

            Vector3 planar = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            return planar.magnitude >= ctx.Settings.MinSlideSpeed;
        }

        public void Execute(MovementContext ctx)
        {
            IsActive = true;
            ctx.Motor.SetHeight(ctx.Settings.CrouchHeight);
            Vector3 planar = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            ctx.Velocity = planar.normalized * (planar.magnitude + ctx.Settings.SlideInitialBoost);
        }

        public void Tick(MovementContext ctx)
        {
            ctx.Velocity = MotorMath.ComputeSlideVelocity(ctx.Velocity, ctx.Motor.GroundNormal, ctx.Settings.SlideFriction, ctx.Settings.MinSlideSpeed, ctx.DeltaTime);

            Vector3 planar = new Vector3(ctx.Velocity.x, 0f, ctx.Velocity.z);
            if (planar.magnitude <= ctx.Settings.MinSlideSpeed || !ctx.Input.CrouchHeld)
            {
                IsActive = false;
                ctx.Motor.SetHeight(ctx.Settings.StandingHeight);
            }
        }

        public void Cancel(MovementContext ctx)
        {
            IsActive = false;
            ctx.Motor.SetHeight(ctx.Settings.StandingHeight);
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter SlideActionTests -logFile editmode_slide.log`
Expected: PASS, 4/4 tests green.

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/Movement/Actions/SlideAction.cs Assets/_Project/Tests/EditMode/SlideActionTests.cs
git commit -m "feat: add SlideAction"
```

---

### Task 9: `LedgeDetector`, `VaultAction`, `MantleAction`

**Files:**
- Create: `Assets/_Project/Scripts/Movement/Actions/LedgeDetector.cs`
- Create: `Assets/_Project/Scripts/Movement/Actions/VaultAction.cs`
- Create: `Assets/_Project/Scripts/Movement/Actions/MantleAction.cs`
- Test: `Assets/_Project/Tests/EditMode/LedgeDetectorTests.cs`
- Test: `Assets/_Project/Tests/PlayMode/VaultMantlePlayModeTests.cs`

**Interfaces:**
- Consumes: `MovementSettings` (Task 3), `IMotor.CapsuleCast` (Task 4/5), `IMovementAction` (Task 7).
- Produces:
  - `enum LedgeActionType { None, Vault, Mantle }`
  - `static LedgeDetector.Decide(bool forwardHit, float ledgeHeight, MovementSettings settings) : LedgeActionType` — pure decision logic, EditMode-testable with synthetic inputs.
  - `VaultAction`, `MantleAction : IMovementAction`, `Priority = 30` (higher than Slide since a vault/mantle should interrupt a slide approach).

- [ ] **Step 1: Write the failing EditMode test**

```csharp
using NUnit.Framework;
using UnityEngine;

namespace Veil.Tests.EditMode
{
    public class LedgeDetectorTests
    {
        private MovementSettings MakeSettings()
        {
            var s = ScriptableObject.CreateInstance<MovementSettings>();
            return s; // vaultMaxHeight=1.1, mantleMaxHeight=2.2 (defaults)
        }

        [Test]
        public void Decide_NoForwardHit_ReturnsNone()
        {
            Assert.AreEqual(LedgeActionType.None, LedgeDetector.Decide(false, 0f, MakeSettings()));
        }

        [Test]
        public void Decide_LowLedge_ReturnsVault()
        {
            Assert.AreEqual(LedgeActionType.Vault, LedgeDetector.Decide(true, 0.8f, MakeSettings()));
        }

        [Test]
        public void Decide_MidLedge_ReturnsMantle()
        {
            Assert.AreEqual(LedgeActionType.Mantle, LedgeDetector.Decide(true, 1.8f, MakeSettings()));
        }

        [Test]
        public void Decide_TooHigh_ReturnsNone()
        {
            Assert.AreEqual(LedgeActionType.None, LedgeDetector.Decide(true, 3f, MakeSettings()));
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter LedgeDetectorTests -logFile editmode_ledge.log`
Expected: FAIL — `LedgeDetector` does not exist.

- [ ] **Step 3: Implement `LedgeDetector`**

```csharp
namespace Veil.Movement.Actions
{
    /// <summary>Which traversal move a detected ledge should trigger, if any.</summary>
    public enum LedgeActionType
    {
        None,
        Vault,
        Mantle
    }

    /// <summary>
    /// Pure decision logic for vault-vs-mantle-vs-none, given ledge geometry already
    /// resolved by a caller's capsule casts. Kept free of Physics calls so it is
    /// directly unit-testable with synthetic inputs.
    /// </summary>
    public static class LedgeDetector
    {
        /// <summary>Decides which traversal action a detected obstacle should trigger.</summary>
        public static LedgeActionType Decide(bool forwardHit, float ledgeHeight, MovementSettings settings)
        {
            if (!forwardHit) return LedgeActionType.None;
            if (ledgeHeight <= settings.VaultMaxHeight) return LedgeActionType.Vault;
            if (ledgeHeight <= settings.MantleMaxHeight) return LedgeActionType.Mantle;
            return LedgeActionType.None;
        }
    }
}
```

- [ ] **Step 4: Run EditMode test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter LedgeDetectorTests -logFile editmode_ledge.log`
Expected: PASS, 4/4 tests green.

- [ ] **Step 5: Write the failing PlayMode test for `VaultAction`**

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Veil.Movement.States;

namespace Veil.Tests.PlayMode
{
    public class VaultMantlePlayModeTests
    {
        [UnityTest]
        public IEnumerator VaultAction_CanExecute_WhenLowObstacleAhead()
        {
            var obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.transform.localScale = new Vector3(2f, 0.8f, 0.5f);
            obstacle.transform.position = new Vector3(0f, 0.4f, 1.5f);

            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var playerGo = new GameObject("Player");
            var motor = playerGo.AddComponent<CharacterMotor>();
            motor.Settings = settings;
            playerGo.transform.position = new Vector3(0f, 1f, 0f);
            playerGo.transform.forward = Vector3.forward;

            yield return null; // let Awake run

            var action = new VaultAction();
            var ctx = new MovementContext(motor, ScriptableObject.CreateInstance<Veil.Input.InputReader>(), settings) { Velocity = Vector3.forward * 3f };

            bool canExecute = action.CanExecute(ctx, MovementStateId.Grounded);

            Assert.IsTrue(canExecute);

            Object.Destroy(obstacle);
            Object.Destroy(playerGo);
        }
    }
}
```

- [ ] **Step 6: Run PlayMode test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform PlayMode -testFilter VaultMantlePlayModeTests -logFile playmode_vault.log`
Expected: FAIL — `VaultAction` does not exist.

- [ ] **Step 7: Implement `VaultAction` and `MantleAction`**

```csharp
using UnityEngine;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>Vaults over a low obstacle: a short forward+up displacement over a fixed duration.</summary>
    public sealed class VaultAction : IMovementAction
    {
        private const float DurationSeconds = 0.35f;

        private float _elapsed;
        private Vector3 _startPos;
        private Vector3 _endPos;

        public int Priority => 30;
        public bool IsActive { get; private set; }

        public bool CanExecute(MovementContext ctx, MovementStateId currentState)
        {
            if (IsActive) return false;
            if (currentState == MovementStateId.Air) return false;

            Vector3 forward = ctx.Velocity.sqrMagnitude > 0.01f ? ctx.Velocity.normalized : Vector3.forward;
            if (!ctx.Motor.CapsuleCast(forward, ctx.Settings.LedgeDetectRange, out RaycastHit hit)) return false;

            float ledgeHeight = hit.point.y - (ctx.Motor.GroundNormal.y > 0 ? 0f : 0f); // height resolved by caller's cast geometry
            return LedgeDetector.Decide(true, ledgeHeight, ctx.Settings) == LedgeActionType.Vault;
        }

        public void Execute(MovementContext ctx)
        {
            IsActive = true;
            _elapsed = 0f;
            _startPos = ctx.Velocity;
            _endPos = ctx.Velocity.normalized * ctx.Settings.SprintSpeed + Vector3.up * 0.1f;
        }

        public void Tick(MovementContext ctx)
        {
            _elapsed += ctx.DeltaTime;
            float t = Mathf.Clamp01(_elapsed / DurationSeconds);
            ctx.Velocity = Vector3.Lerp(_startPos, _endPos, t);

            if (t >= 1f) IsActive = false;
        }

        public void Cancel(MovementContext ctx) => IsActive = false;
    }
}
```

```csharp
using UnityEngine;
using Veil.Movement.States;

namespace Veil.Movement.Actions
{
    /// <summary>Mantles up onto a higher ledge: forward+up displacement over a longer fixed duration than vault.</summary>
    public sealed class MantleAction : IMovementAction
    {
        private const float DurationSeconds = 0.6f;

        private float _elapsed;
        private Vector3 _startPos;
        private Vector3 _endPos;

        public int Priority => 30;
        public bool IsActive { get; private set; }

        public bool CanExecute(MovementContext ctx, MovementStateId currentState)
        {
            if (IsActive) return false;
            if (currentState == MovementStateId.Air) return false;

            Vector3 forward = ctx.Velocity.sqrMagnitude > 0.01f ? ctx.Velocity.normalized : Vector3.forward;
            if (!ctx.Motor.CapsuleCast(forward, ctx.Settings.LedgeDetectRange, out RaycastHit hit)) return false;

            float ledgeHeight = hit.point.y;
            return LedgeDetector.Decide(true, ledgeHeight, ctx.Settings) == LedgeActionType.Mantle;
        }

        public void Execute(MovementContext ctx)
        {
            IsActive = true;
            _elapsed = 0f;
            _startPos = ctx.Velocity;
            _endPos = ctx.Velocity.normalized * ctx.Settings.WalkSpeed + Vector3.up * 0.1f;
        }

        public void Tick(MovementContext ctx)
        {
            _elapsed += ctx.DeltaTime;
            float t = Mathf.Clamp01(_elapsed / DurationSeconds);
            ctx.Velocity = Vector3.Lerp(_startPos, _endPos, t);

            if (t >= 1f) IsActive = false;
        }

        public void Cancel(MovementContext ctx) => IsActive = false;
    }
}
```

- [ ] **Step 8: Run PlayMode test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform PlayMode -testFilter VaultMantlePlayModeTests -logFile playmode_vault.log`
Expected: PASS, 1/1 test green.

- [ ] **Step 9: Commit**

```bash
git add Assets/_Project/Scripts/Movement/Actions/LedgeDetector.cs Assets/_Project/Scripts/Movement/Actions/VaultAction.cs Assets/_Project/Scripts/Movement/Actions/MantleAction.cs Assets/_Project/Tests/EditMode/LedgeDetectorTests.cs Assets/_Project/Tests/PlayMode/VaultMantlePlayModeTests.cs
git commit -m "feat: add LedgeDetector decision logic plus VaultAction and MantleAction"
```

---

### Task 10: `CameraJuice` and `CameraController`

**Files:**
- Create: `Assets/_Project/Scripts/Camera/CameraJuice.cs`
- Create: `Assets/_Project/Scripts/Camera/CameraController.cs`
- Test: `Assets/_Project/Tests/EditMode/CameraJuiceTests.cs`

**Interfaces:**
- Consumes: `CameraSettings` (Task 3), `IMotor.Velocity` (Task 4), `MovementStateMachine.Current` / `SlideAction.IsActive` (Tasks 6, 8).
- Produces: `CameraJuice` static class: `float CalculateFovKick(...)`, `float CalculateTilt(...)`, `Vector3 CalculateBobOffset(...)` — pure, zero-GC. `CameraController : MonoBehaviour` wires these onto an actual `UnityEngine.Camera`, consumed by Task 14's Player prefab.

- [ ] **Step 1: Write the failing test**

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace Veil.Tests.EditMode
{
    public class CameraJuiceTests
    {
        [Test]
        public void CalculateFovKick_ZeroSpeed_ReturnsBaseFov()
        {
            float fov = CameraJuice.CalculateFovKick(baseFov: 90f, speed: 0f, maxSpeed: 8f, maxKick: 12f);
            Assert.AreEqual(90f, fov, 0.01f);
        }

        [Test]
        public void CalculateFovKick_MaxSpeed_ReturnsBasePlusMaxKick()
        {
            float fov = CameraJuice.CalculateFovKick(baseFov: 90f, speed: 8f, maxSpeed: 8f, maxKick: 12f);
            Assert.AreEqual(102f, fov, 0.01f);
        }

        [Test]
        public void CalculateTilt_Sliding_ReturnsSlideTilt()
        {
            float tilt = CameraJuice.CalculateTilt(horizontalInput: 0f, maxTiltDegrees: 4f, isSliding: true, slideTiltDegrees: 10f);
            Assert.AreEqual(10f, tilt, 0.01f);
        }

        [Test]
        public void CalculateBobOffset_IsAllocationFree()
        {
            Assert.That(() => CameraJuice.CalculateBobOffset(1f, 5f, 1.8f, 0.05f), Is.Not.AllocatingGCMemory());
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter CameraJuiceTests -logFile editmode_camerajuice.log`
Expected: FAIL — `CameraJuice` does not exist.

- [ ] **Step 3: Implement `CameraJuice`**

```csharp
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
```

- [ ] **Step 4: Implement `CameraController`**

```csharp
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
```

- [ ] **Step 5: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter CameraJuiceTests -logFile editmode_camerajuice.log`
Expected: PASS, 4/4 tests green.

- [ ] **Step 6: Commit**

```bash
git add Assets/_Project/Scripts/Camera/CameraJuice.cs Assets/_Project/Scripts/Camera/CameraController.cs Assets/_Project/Tests/EditMode/CameraJuiceTests.cs
git commit -m "feat: add CameraJuice math and CameraController"
```

---

### Task 11: Interaction — `IInteractable`, `InteractionCaster`, `Door`/`Lever`/`Pickup`

**Files:**
- Create: `Assets/_Project/Scripts/Interaction/IInteractable.cs`
- Create: `Assets/_Project/Scripts/Interaction/NearestInteractableSelector.cs`
- Create: `Assets/_Project/Scripts/Interaction/InteractionCaster.cs`
- Create: `Assets/_Project/Scripts/Interaction/Door.cs`
- Create: `Assets/_Project/Scripts/Interaction/Lever.cs`
- Create: `Assets/_Project/Scripts/Interaction/Pickup.cs`
- Test: `Assets/_Project/Tests/EditMode/NearestInteractableSelectorTests.cs`

**Interfaces:**
- Consumes: `InputReader.InteractPressed` (Task 2).
- Produces: `IInteractable { string GetPrompt(); void Interact(GameObject interactor); }`, fired via `InteractionCaster.event Action<IInteractable> FocusChanged` — consumed by Task 14's Player prefab wiring (and, later, a UI milestone).

- [ ] **Step 1: Write the failing test**

```csharp
using System.Collections.Generic;
using NUnit.Framework;

namespace Veil.Tests.EditMode
{
    public class NearestInteractableSelectorTests
    {
        private class FakeInteractable : IInteractable
        {
            public string GetPrompt() => "Fake";
            public void Interact(UnityEngine.GameObject interactor) { }
        }

        [Test]
        public void SelectNearest_ReturnsClosestByDistance()
        {
            var near = new FakeInteractable();
            var far = new FakeInteractable();
            var candidates = new List<(IInteractable, float)> { (far, 5f), (near, 1f) };

            var result = NearestInteractableSelector.SelectNearest(candidates);

            Assert.AreSame(near, result);
        }

        [Test]
        public void SelectNearest_EmptyList_ReturnsNull()
        {
            var candidates = new List<(IInteractable, float)>();
            Assert.IsNull(NearestInteractableSelector.SelectNearest(candidates));
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter NearestInteractableSelectorTests -logFile editmode_interact.log`
Expected: FAIL — types don't exist.

- [ ] **Step 3: Implement `IInteractable` and `NearestInteractableSelector`**

```csharp
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Anything the player can raycast-interact with (doors, levers, pickups).</summary>
    public interface IInteractable
    {
        /// <summary>Short prompt text, e.g. "Open Door" — a future UI binds to this.</summary>
        string GetPrompt();

        /// <summary>Performs the interaction.</summary>
        void Interact(GameObject interactor);
    }
}
```

```csharp
using System.Collections.Generic;

namespace Veil.Interaction
{
    /// <summary>Pure selection logic: picks the closest interactable from a candidate list.</summary>
    public static class NearestInteractableSelector
    {
        /// <summary>Returns the candidate with the smallest distance, or null if the list is empty.</summary>
        public static IInteractable SelectNearest(IReadOnlyList<(IInteractable interactable, float distance)> candidates)
        {
            if (candidates.Count == 0) return null;

            IInteractable best = candidates[0].interactable;
            float bestDistance = candidates[0].distance;
            for (int i = 1; i < candidates.Count; i++)
            {
                if (candidates[i].distance < bestDistance)
                {
                    bestDistance = candidates[i].distance;
                    best = candidates[i].interactable;
                }
            }
            return best;
        }
    }
}
```

- [ ] **Step 4: Implement `InteractionCaster`**

```csharp
using System;
using UnityEngine;
using Veil.Input;

namespace Veil.Interaction
{
    /// <summary>Spherecasts from the camera each frame to find the focused interactable, and fires interact on input.</summary>
    public sealed class InteractionCaster : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera eye;
        [SerializeField] private InputReader input;
        [SerializeField] private float range = 2.5f;
        [SerializeField] private float sphereRadius = 0.15f;
        [SerializeField] private LayerMask interactableMask = ~0;

        /// <summary>Fires when the focused interactable changes (including to/from null); a future UI binds its prompt here.</summary>
        public event Action<IInteractable> FocusChanged;

        private IInteractable _current;

        private void OnEnable()
        {
            if (input != null) input.InteractPressed += OnInteractPressed;
        }

        private void OnDisable()
        {
            if (input != null) input.InteractPressed -= OnInteractPressed;
        }

        private void Update()
        {
            IInteractable found = null;
            if (Physics.SphereCast(eye.transform.position, sphereRadius, eye.transform.forward, out RaycastHit hit, range, interactableMask, QueryTriggerInteraction.Collide))
            {
                found = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (!ReferenceEquals(found, _current))
            {
                _current = found;
                FocusChanged?.Invoke(_current);
            }
        }

        private void OnInteractPressed() => _current?.Interact(gameObject);
    }
}
```

- [ ] **Step 5: Implement `Door`, `Lever`, `Pickup`**

```csharp
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Swings open on interact; placeholder greybox behaviour for M1.</summary>
    public sealed class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private float openAngleDegrees = 90f;
        [SerializeField] private float openSpeedDegreesPerSecond = 120f;
        private bool _isOpen;
        private float _currentAngle;

        public string GetPrompt() => _isOpen ? "Close Door" : "Open Door";

        public void Interact(GameObject interactor) => _isOpen = !_isOpen;

        private void Update()
        {
            float target = _isOpen ? openAngleDegrees : 0f;
            _currentAngle = Mathf.MoveTowards(_currentAngle, target, openSpeedDegreesPerSecond * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0f, _currentAngle, 0f);
        }
    }
}
```

```csharp
using System;
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Toggleable lever; placeholder greybox behaviour, exposes an event other objects (e.g. a Door) can bind to for M1 puzzle wiring.</summary>
    public sealed class Lever : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool startsOn;
        private bool _isOn;

        /// <summary>Fires with the new on/off state each time the lever is used.</summary>
        public event Action<bool> StateChanged;

        private void Awake() => _isOn = startsOn;

        public string GetPrompt() => _isOn ? "Pull Lever (Off)" : "Pull Lever (On)";

        public void Interact(GameObject interactor)
        {
            _isOn = !_isOn;
            StateChanged?.Invoke(_isOn);
        }
    }
}
```

```csharp
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Simple pickup — destroys itself on interact; placeholder for M1.</summary>
    public sealed class Pickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Item";

        public string GetPrompt() => $"Pick Up {displayName}";

        public void Interact(GameObject interactor) => Destroy(gameObject);
    }
}
```

- [ ] **Step 6: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter NearestInteractableSelectorTests -logFile editmode_interact.log`
Expected: PASS, 2/2 tests green.

- [ ] **Step 7: Commit**

```bash
git add Assets/_Project/Scripts/Interaction/IInteractable.cs Assets/_Project/Scripts/Interaction/NearestInteractableSelector.cs Assets/_Project/Scripts/Interaction/InteractionCaster.cs Assets/_Project/Scripts/Interaction/Door.cs Assets/_Project/Scripts/Interaction/Lever.cs Assets/_Project/Scripts/Interaction/Pickup.cs Assets/_Project/Tests/EditMode/NearestInteractableSelectorTests.cs
git commit -m "feat: add prompt-based interaction system (Door, Lever, Pickup)"
```

---

### Task 12: Grab — `IGrabbable`, `GrabbableObject`, `GrabPhysicsMath`, `GrabController`

**Files:**
- Create: `Assets/_Project/Scripts/Interaction/IGrabbable.cs`
- Create: `Assets/_Project/Scripts/Interaction/GrabbableObject.cs`
- Create: `Assets/_Project/Scripts/Interaction/GrabPhysicsMath.cs`
- Create: `Assets/_Project/Scripts/Interaction/GrabController.cs`
- Test: `Assets/_Project/Tests/EditMode/GrabPhysicsMathTests.cs`
- Test: `Assets/_Project/Tests/PlayMode/GrabControllerPlayModeTests.cs`

**Interfaces:**
- Consumes: `InputReader.GrabPressed` (Task 2).
- Produces: `IGrabbable { Rigidbody Body { get; } }`, `GrabPhysicsMath.ComputeHoldVelocity(...)`, `GrabController` — kept separate from `InteractionCaster` (Task 11) per SRP. Consumed by Task 14's Player prefab wiring.

- [ ] **Step 1: Write the failing EditMode test**

```csharp
using NUnit.Framework;
using UnityEngine;

namespace Veil.Tests.EditMode
{
    public class GrabPhysicsMathTests
    {
        [Test]
        public void ComputeHoldVelocity_MovesTowardTarget()
        {
            Vector3 current = Vector3.zero;
            Vector3 target = new Vector3(1f, 0f, 0f);

            Vector3 velocity = GrabPhysicsMath.ComputeHoldVelocity(current, target, Vector3.zero, springStrength: 10f, damping: 1f, deltaTime: 0.1f);

            Assert.Greater(velocity.x, 0f);
        }

        [Test]
        public void ComputeHoldVelocity_AtTarget_WithZeroVelocity_ReturnsNearZero()
        {
            Vector3 velocity = GrabPhysicsMath.ComputeHoldVelocity(Vector3.zero, Vector3.zero, Vector3.zero, springStrength: 10f, damping: 1f, deltaTime: 0.1f);

            Assert.AreEqual(0f, velocity.magnitude, 0.001f);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter GrabPhysicsMathTests -logFile editmode_grab.log`
Expected: FAIL — `GrabPhysicsMath` does not exist.

- [ ] **Step 3: Implement `IGrabbable` and `GrabbableObject`**

```csharp
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Anything the player can physically grab, carry, and throw.</summary>
    public interface IGrabbable
    {
        /// <summary>The rigidbody being carried.</summary>
        Rigidbody Body { get; }

        /// <summary>Called when grabbed; used to e.g. disable other physics behaviour while held.</summary>
        void OnGrabbed();

        /// <summary>Called when released or thrown.</summary>
        void OnReleased();
    }
}
```

```csharp
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Marks a rigidbody object as grabbable by <see cref="GrabController"/>.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class GrabbableObject : MonoBehaviour, IGrabbable
    {
        private Rigidbody _body;

        public Rigidbody Body => _body != null ? _body : (_body = GetComponent<Rigidbody>());

        public void OnGrabbed() => Body.useGravity = false;

        public void OnReleased() => Body.useGravity = true;
    }
}
```

- [ ] **Step 4: Implement `GrabPhysicsMath`**

```csharp
using UnityEngine;

namespace Veil.Interaction
{
    /// <summary>Pure spring-damper math for held-object motion. Allocation-free.</summary>
    public static class GrabPhysicsMath
    {
        /// <summary>Returns the velocity that pulls <paramref name="current"/> toward <paramref name="target"/> like a critically-damped spring.</summary>
        public static Vector3 ComputeHoldVelocity(Vector3 current, Vector3 target, Vector3 currentVelocity, float springStrength, float damping, float deltaTime)
        {
            Vector3 displacement = target - current;
            Vector3 springForce = displacement * springStrength;
            Vector3 dampingForce = -currentVelocity * damping;
            return currentVelocity + (springForce + dampingForce) * deltaTime;
        }
    }
}
```

- [ ] **Step 5: Run EditMode test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter GrabPhysicsMathTests -logFile editmode_grab.log`
Expected: PASS, 2/2 tests green.

- [ ] **Step 6: Write the failing PlayMode test for `GrabController`**

```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Veil.Tests.PlayMode
{
    public class GrabControllerPlayModeTests
    {
        [UnityTest]
        public IEnumerator HeldObject_ConvergesTowardHoldPoint()
        {
            var holderGo = new GameObject("Holder");
            var controller = holderGo.AddComponent<GrabController>();

            var objGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var rb = objGo.AddComponent<Rigidbody>();
            var grabbable = objGo.AddComponent<GrabbableObject>();
            objGo.transform.position = new Vector3(5f, 0f, 0f);

            var holdPoint = new GameObject("HoldPoint").transform;
            holdPoint.position = Vector3.zero;
            controller.SetHoldPoint(holdPoint);

            controller.Grab(grabbable);

            for (int i = 0; i < 60; i++)
            {
                controller.TickHold(Time.fixedDeltaTime);
                yield return new WaitForFixedUpdate();
            }

            Assert.Less(Vector3.Distance(objGo.transform.position, holdPoint.position), 0.5f);

            Object.Destroy(holderGo);
            Object.Destroy(objGo);
        }
    }
}
```

- [ ] **Step 7: Run PlayMode test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform PlayMode -testFilter GrabControllerPlayModeTests -logFile playmode_grab.log`
Expected: FAIL — `GrabController` does not exist.

- [ ] **Step 8: Implement `GrabController`**

```csharp
using UnityEngine;
using Veil.Input;

namespace Veil.Interaction
{
    /// <summary>
    /// Physics grab/carry/throw for <see cref="IGrabbable"/> rigidbodies. Kept separate
    /// from <see cref="InteractionCaster"/> so static-trigger interaction and physics-carry
    /// interaction evolve independently.
    /// </summary>
    public sealed class GrabController : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [SerializeField] private Transform holdPoint;
        [SerializeField] private float grabRange = 2.5f;
        [SerializeField] private float springStrength = 200f;
        [SerializeField] private float damping = 12f;
        [SerializeField] private float throwForce = 8f;
        [SerializeField] private LayerMask grabbableMask = ~0;

        private IGrabbable _held;

        /// <summary>Overrides the hold point transform (used by tests and prefab wiring).</summary>
        public void SetHoldPoint(Transform point) => holdPoint = point;

        private void OnEnable()
        {
            if (input != null)
            {
                input.GrabPressed += OnGrabPressed;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.GrabPressed -= OnGrabPressed;
            }
        }

        private void FixedUpdate() => TickHold(Time.fixedDeltaTime);

        private void OnGrabPressed()
        {
            if (_held != null)
            {
                Release(throwing: true);
                return;
            }

            if (Physics.SphereCast(transform.position, 0.2f, transform.forward, out RaycastHit hit, grabRange, grabbableMask))
            {
                var grabbable = hit.collider.GetComponentInParent<IGrabbable>();
                if (grabbable != null) Grab(grabbable);
            }
        }

        /// <summary>Begins holding the given grabbable.</summary>
        public void Grab(IGrabbable grabbable)
        {
            _held = grabbable;
            _held.OnGrabbed();
        }

        /// <summary>Advances the spring-damper hold for one physics step.</summary>
        public void TickHold(float deltaTime)
        {
            if (_held == null || holdPoint == null) return;

            Vector3 newVelocity = GrabPhysicsMath.ComputeHoldVelocity(
                _held.Body.position, holdPoint.position, _held.Body.linearVelocity, springStrength, damping, deltaTime);
            _held.Body.linearVelocity = newVelocity;
        }

        /// <summary>Releases the held object, optionally applying a throw impulse.</summary>
        public void Release(bool throwing)
        {
            if (_held == null) return;

            if (throwing) _held.Body.linearVelocity += transform.forward * throwForce;
            _held.OnReleased();
            _held = null;
        }
    }
}
```

- [ ] **Step 9: Run PlayMode test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform PlayMode -testFilter GrabControllerPlayModeTests -logFile playmode_grab.log`
Expected: PASS, 1/1 test green.

- [ ] **Step 10: Commit**

```bash
git add Assets/_Project/Scripts/Interaction/IGrabbable.cs Assets/_Project/Scripts/Interaction/GrabbableObject.cs Assets/_Project/Scripts/Interaction/GrabPhysicsMath.cs Assets/_Project/Scripts/Interaction/GrabController.cs Assets/_Project/Tests/EditMode/GrabPhysicsMathTests.cs Assets/_Project/Tests/PlayMode/GrabControllerPlayModeTests.cs
git commit -m "feat: add physics grab/carry/throw system"
```

---

### Task 13: `LevelBuilder` — Procedural M1 Stealth Sandbox

**Files:**
- Create: `Assets/_Project/Scripts/LevelGen/Editor/LevelBuilder.cs`
- Test: `Assets/_Project/Tests/EditMode/LevelBuilderTests.cs`

**Interfaces:**
- Consumes: `Door`, `Lever`, `Pickup`, `GrabbableObject` (Tasks 11, 12).
- Produces: `LevelBuilder.Build(Scene scene)` — a repeatable, code-driven greybox generator (avoids hand-placing primitives in the Editor, keeps the level reproducible and diffable) plus a `[MenuItem("VEIL/Build M1 Test Level")]` entry point. Consumed by Task 14, which loads/saves the generated scene as `M1_StealthSandbox.unity`.

- [ ] **Step 1: Write the failing test**

```csharp
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Veil.Tests.EditMode
{
    public class LevelBuilderTests
    {
        [Test]
        public void Build_CreatesRequiredGreyboxElements()
        {
            var scene = SceneManager.CreateScene("LevelBuilderTestScene");
            LevelBuilder.Build(scene);

            var roots = scene.GetRootGameObjects();
            Assert.IsTrue(roots.Any(g => g.name == "VaultGap"), "Missing VaultGap");
            Assert.IsTrue(roots.Any(g => g.name == "MantleLedge"), "Missing MantleLedge");
            Assert.IsTrue(roots.Any(g => g.name == "SlideGap"), "Missing SlideGap");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Veil.Interaction.Door>() != null), "Missing Door");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Veil.Interaction.Lever>() != null), "Missing Lever");
            Assert.IsTrue(roots.Any(g => g.GetComponentInChildren<Veil.Interaction.GrabbableObject>() != null), "Missing GrabbableObject");

            SceneManager.UnloadSceneAsync(scene);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter LevelBuilderTests -logFile editmode_levelbuilder.log`
Expected: FAIL — `LevelBuilder` does not exist.

- [ ] **Step 3: Implement `LevelBuilder`**

```csharp
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Veil.Interaction;

namespace Veil.LevelGen.Editor
{
    /// <summary>
    /// Procedurally builds the M1 mini stealth sandbox: greybox verticality (vault gap,
    /// mantle ledge, slide gap) plus sightline-blocking cover and the three required
    /// interactables. Code-driven so the level is reproducible and reviewable as a diff
    /// rather than hand-placed in the Editor.
    /// </summary>
    public static class LevelBuilder
    {
        private const float ArenaSize = 40f;

        [MenuItem("VEIL/Build M1 Test Level")]
        public static void BuildActiveScene() => Build(SceneManager.GetActiveScene());

        /// <summary>Populates the given scene with the M1 greybox layout.</summary>
        public static void Build(Scene scene)
        {
            var floor = CreateBlock("Floor", new Vector3(ArenaSize, 1f, ArenaSize), new Vector3(0f, -0.5f, 0f));
            SceneManager.MoveGameObjectToScene(floor, scene);

            var vaultGap = CreateBlock("VaultGap", new Vector3(2f, 0.8f, 0.5f), new Vector3(-8f, 0.4f, 0f));
            SceneManager.MoveGameObjectToScene(vaultGap, scene);

            var mantleLedge = CreateBlock("MantleLedge", new Vector3(4f, 1.8f, 1f), new Vector3(-2f, 0.9f, 6f));
            SceneManager.MoveGameObjectToScene(mantleLedge, scene);

            var slideGapTop = CreateBlock("SlideGap", new Vector3(3f, 0.6f, 1.5f), new Vector3(4f, 1.3f, -6f));
            SceneManager.MoveGameObjectToScene(slideGapTop, scene);

            var cover1 = CreateBlock("CoverCrate_1", new Vector3(1f, 1f, 1f), new Vector3(3f, 0.5f, 3f));
            SceneManager.MoveGameObjectToScene(cover1, scene);
            var cover2 = CreateBlock("CoverPillar_1", new Vector3(0.8f, 3f, 0.8f), new Vector3(-5f, 1.5f, -4f));
            SceneManager.MoveGameObjectToScene(cover2, scene);

            var doorGo = CreateBlock("Door", new Vector3(1.2f, 2f, 0.1f), new Vector3(10f, 1f, 0f));
            doorGo.AddComponent<Door>();
            SceneManager.MoveGameObjectToScene(doorGo, scene);

            var leverGo = CreateBlock("Lever", new Vector3(0.2f, 0.5f, 0.2f), new Vector3(9f, 0.75f, 1.5f));
            leverGo.AddComponent<Lever>();
            SceneManager.MoveGameObjectToScene(leverGo, scene);

            var pickupGo = CreateBlock("Pickup", new Vector3(0.3f, 0.3f, 0.3f), new Vector3(0f, 0.15f, -2f));
            pickupGo.AddComponent<Pickup>();
            SceneManager.MoveGameObjectToScene(pickupGo, scene);

            var grabbableGo = CreateBlock("GrabbableCrate", new Vector3(0.5f, 0.5f, 0.5f), new Vector3(2f, 0.25f, 2f));
            var rb = grabbableGo.AddComponent<Rigidbody>();
            rb.mass = 5f;
            grabbableGo.AddComponent<GrabbableObject>();
            SceneManager.MoveGameObjectToScene(grabbableGo, scene);
        }

        private static GameObject CreateBlock(string name, Vector3 scale, Vector3 position)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.localScale = scale;
            go.transform.position = position;
            return go;
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter LevelBuilderTests -logFile editmode_levelbuilder.log`
Expected: PASS, 1/1 test green.

- [ ] **Step 5: Commit**

```bash
git add Assets/_Project/Scripts/LevelGen/Editor/LevelBuilder.cs Assets/_Project/Tests/EditMode/LevelBuilderTests.cs
git commit -m "feat: add procedural LevelBuilder for M1 stealth sandbox"
```

---

### Task 14: Integration — `PlayerController`, Player Prefab, Test Level Wiring

**Files:**
- Create: `Assets/_Project/Scripts/Movement/PlayerController.cs`
- Modify: `Assets/_Project/Scripts/Movement/Actions/ActionController.cs` — add `HasActiveAction`
- Create: `Assets/_Project/Scripts/LevelGen/Editor/PlayerPrefabBuilder.cs`
- Modify: `Assets/_Project/Scripts/LevelGen/Editor/LevelBuilder.cs` — spawn the player prefab into the generated scene
- Create: `docs/PLAYTEST_CHECKLIST.md`

**Interfaces:**
- Consumes: everything from Tasks 2–13.
- Produces: a playable `M1_StealthSandbox` scene with a wired `Player.prefab`. This is the terminal task — nothing downstream depends on it.

- [ ] **Step 1: Add `HasActiveAction` to `ActionController`**

```csharp
        /// <summary>True while an action is currently active and blocking the state machine from driving velocity.</summary>
        public bool HasActiveAction => _active != null;
```

Insert this property in `Assets/_Project/Scripts/Movement/Actions/ActionController.cs` directly above the existing `RegisterAction` method.

- [ ] **Step 2: Write the failing EditMode test**

```csharp
using NUnit.Framework;
using UnityEngine;
using Veil.Input;
using Veil.Movement.Actions;

namespace Veil.Tests.EditMode
{
    public class PlayerControllerTests
    {
        private class FakeMotor : IMotor
        {
            public bool IsGrounded { get; set; } = true;
            public Vector3 GroundNormal { get; set; } = Vector3.up;
            public Vector3 Velocity { get; set; }
            public void Move(Vector3 velocity, float deltaTime) => Velocity = velocity;
            public bool CapsuleCast(Vector3 direction, float maxDistance, out RaycastHit hit) { hit = default; return false; }
            public void SetHeight(float height) { }
        }

        [Test]
        public void Tick_WithNoActiveAction_LetsStateMachineDriveVelocity()
        {
            var motor = new FakeMotor();
            var input = ScriptableObject.CreateInstance<InputReader>();
            var settings = ScriptableObject.CreateInstance<MovementSettings>();
            var ctx = new MovementContext(motor, input, settings);
            var stateMachine = new Veil.Movement.States.MovementStateMachine(ctx);
            var actionController = new ActionController();

            PlayerController.TickMovement(ctx, stateMachine, actionController, 0.1f);

            Assert.IsFalse(actionController.HasActiveAction);
        }
    }
}
```

- [ ] **Step 3: Run test to verify it fails**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter PlayerControllerTests -logFile editmode_playercontroller.log`
Expected: FAIL — `PlayerController` does not exist.

- [ ] **Step 4: Implement `PlayerController`**

```csharp
using UnityEngine;
using Veil.Camera;
using Veil.Input;
using Veil.Interaction;
using Veil.Movement.Actions;
using Veil.Movement.States;

namespace Veil.Movement
{
    /// <summary>
    /// Owns and ticks one player's full movement stack: motor, state machine, and
    /// action controller. Also drives camera tilt from live slide state. This is the
    /// only class that wires Movement, Camera, and Actions together — everything else
    /// stays decoupled behind interfaces.
    /// </summary>
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private CharacterMotor motor;
        [SerializeField] private InputReader input;
        [SerializeField] private MovementSettings movementSettings;
        [SerializeField] private CameraController cameraController;

        private MovementContext _ctx;
        private MovementStateMachine _stateMachine;
        private ActionController _actionController;
        private SlideAction _slideAction;

        private void Awake()
        {
            _ctx = new MovementContext(motor, input, movementSettings);
            _stateMachine = new MovementStateMachine(_ctx);
            _actionController = new ActionController();

            _slideAction = new SlideAction();
            _actionController.RegisterAction(_slideAction);
            _actionController.RegisterAction(new VaultAction());
            _actionController.RegisterAction(new MantleAction());
        }

        private void OnEnable()
        {
            input.Enable();
            input.JumpVaultPressed += OnActionTriggerPressed;
            input.SlidePressed += OnActionTriggerPressed;
        }

        private void OnDisable()
        {
            input.JumpVaultPressed -= OnActionTriggerPressed;
            input.SlidePressed -= OnActionTriggerPressed;
            input.Disable();
        }

        private void OnActionTriggerPressed() => _actionController.TryTriggerBestAction(_ctx, _stateMachine.Current);

        private void Update()
        {
            _ctx.DeltaTime = Time.deltaTime;
            TickMovement(_ctx, _stateMachine, _actionController, Time.deltaTime);
            motor.Move(_ctx.Velocity, Time.deltaTime);

            if (cameraController != null)
            {
                cameraController.ApplyTilt(input.MoveInput.x, _slideAction.IsActive);
            }
        }

        /// <summary>
        /// Advances one movement tick: the state machine drives velocity unless an
        /// action is currently active, in which case the action owns velocity instead.
        /// Static and side-effect-visible-via-ctx so it's directly unit-testable
        /// without a live MonoBehaviour/scene.
        /// </summary>
        public static void TickMovement(MovementContext ctx, MovementStateMachine stateMachine, ActionController actionController, float deltaTime)
        {
            if (!actionController.HasActiveAction)
            {
                stateMachine.Tick(ctx);
            }
            actionController.Tick(ctx);
        }
    }
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `<UnityEditorPath>\Unity.exe -batchmode -runTests -projectPath "C:\Users\thats\OneDrive\Documents\game" -testPlatform EditMode -testFilter PlayerControllerTests -logFile editmode_playercontroller.log`
Expected: PASS, 1/1 test green.

- [ ] **Step 6: Implement `PlayerPrefabBuilder` editor script**

```csharp
using System.IO;
using UnityEditor;
using UnityEngine;
using Veil.Camera;
using Veil.Input;
using Veil.Interaction;
using Veil.Movement;

namespace Veil.LevelGen.Editor
{
    /// <summary>Code-driven builder for `Player.prefab` so the wiring between every M1 system is reproducible, not hand-clicked.</summary>
    public static class PlayerPrefabBuilder
    {
        private const string SettingsFolder = "Assets/_Project/Settings";
        private const string PrefabPath = "Assets/_Project/Prefabs/Player.prefab";

        [MenuItem("VEIL/Build Player Prefab")]
        public static void Build()
        {
            var movementSettings = LoadOrCreate<MovementSettings>($"{SettingsFolder}/DefaultMovementSettings.asset");
            var cameraSettings = LoadOrCreate<CameraSettings>($"{SettingsFolder}/DefaultCameraSettings.asset");
            var inputReader = LoadOrCreate<InputReader>($"{SettingsFolder}/DefaultInputReader.asset");

            var root = new GameObject("Player");
            root.AddComponent<CapsuleCollider>();
            var motor = root.AddComponent<CharacterMotor>();
            motor.Settings = movementSettings;

            var cameraRig = new GameObject("CameraRig");
            cameraRig.transform.SetParent(root.transform);
            cameraRig.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            var cam = cameraRig.AddComponent<UnityEngine.Camera>();

            var cameraController = cameraRig.AddComponent<CameraController>();
            var camControllerSo = new SerializedObject(cameraController);
            camControllerSo.FindProperty("targetCamera").objectReferenceValue = cam;
            camControllerSo.FindProperty("settings").objectReferenceValue = cameraSettings;
            camControllerSo.FindProperty("motor").objectReferenceValue = motor;
            camControllerSo.ApplyModifiedPropertiesWithoutUndo();

            var interactionCaster = cameraRig.AddComponent<InteractionCaster>();
            var casterSo = new SerializedObject(interactionCaster);
            casterSo.FindProperty("eye").objectReferenceValue = cam;
            casterSo.FindProperty("input").objectReferenceValue = inputReader;
            casterSo.ApplyModifiedPropertiesWithoutUndo();

            var holdPoint = new GameObject("HoldPoint").transform;
            holdPoint.SetParent(cameraRig.transform);
            holdPoint.localPosition = new Vector3(0f, 0f, 1.2f);

            var grabController = root.AddComponent<GrabController>();
            var grabSo = new SerializedObject(grabController);
            grabSo.FindProperty("input").objectReferenceValue = inputReader;
            grabSo.FindProperty("holdPoint").objectReferenceValue = holdPoint;
            grabSo.ApplyModifiedPropertiesWithoutUndo();

            var playerController = root.AddComponent<PlayerController>();
            var playerSo = new SerializedObject(playerController);
            playerSo.FindProperty("motor").objectReferenceValue = motor;
            playerSo.FindProperty("input").objectReferenceValue = inputReader;
            playerSo.FindProperty("movementSettings").objectReferenceValue = movementSettings;
            playerSo.FindProperty("cameraController").objectReferenceValue = cameraController;
            playerSo.ApplyModifiedPropertiesWithoutUndo();

            Directory.CreateDirectory("Assets/_Project/Prefabs");
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;

            Directory.CreateDirectory(SettingsFolder);
            var instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }
    }
}
```

- [ ] **Step 7: Wire the player into `LevelBuilder`**

Add to `Assets/_Project/Scripts/LevelGen/Editor/LevelBuilder.cs`, inside `Build`, after the existing content:

```csharp
            var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Player.prefab");
            if (playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.transform.position = new Vector3(0f, 1f, -10f);
                SceneManager.MoveGameObjectToScene(player, scene);
            }
```

(Requires `using UnityEngine.SceneManagement;` and `using UnityEditor;`, both already present or added at the top of the file.)

- [ ] **Step 8: Generate the prefab and the scene**

Run: `<UnityEditorPath>\Unity.exe -batchmode -quit -projectPath "C:\Users\thats\OneDrive\Documents\game" -executeMethod Veil.LevelGen.Editor.PlayerPrefabBuilder.Build -logFile build_prefab.log`
Expected: exit 0, `Assets/_Project/Prefabs/Player.prefab` now exists.

Then create and save the scene: open the Editor normally (or script scene creation via another `-executeMethod` that calls `EditorSceneManager.NewScene`, then `LevelBuilder.BuildActiveScene()`, then `EditorSceneManager.SaveScene(..., "Assets/_Project/Levels/M1_StealthSandbox.unity")`), confirming `M1_StealthSandbox.unity` exists in `Assets/_Project/Levels/`.

- [ ] **Step 9: Write the playtest checklist**

```markdown
# VEIL M1 Playtest Checklist

Run through this manually in the Editor (Play Mode) on `M1_StealthSandbox.unity`. Movement feel is validated here, not by automated tests — see the M1 design spec's Validation Approach.

- [ ] Sprint → crouch chains into a slide without a frame of dead input
- [ ] Slide → stand back up restores full capsule height and normal speed smoothly
- [ ] Vault triggers on the low ledge (`VaultGap`) and completes without snagging geometry
- [ ] Mantle triggers on the tall ledge (`MantleLedge`) and completes without snagging geometry
- [ ] Vault → sprint → mantle chains without a dropped or delayed input
- [ ] No stuck-in-air or stuck-in-crouch states after repeated vault/mantle/slide chaining
- [ ] Camera FOV kick and tilt are visible but never induce visible per-frame jitter
- [ ] `Door` opens/closes on interact prompt; `Lever` toggles and its prompt updates
- [ ] `Pickup` disappears cleanly on interact
- [ ] `GrabbableCrate` can be grabbed, carried without clipping through `CoverCrate_1`/`CoverPillar_1`, and thrown
- [ ] Frame rate holds at/above 144 FPS during continuous movement (check the Editor's Stats window or a Profiler capture)
- [ ] Profiler shows zero GC.Alloc spikes during a 30-second continuous sprint/vault/slide/grab sequence
```

- [ ] **Step 10: Batchmode build verification**

Run: `<UnityEditorPath>\Unity.exe -batchmode -quit -projectPath "C:\Users\thats\OneDrive\Documents\game" -buildWindows64Player "C:\Users\thats\OneDrive\Documents\game\Builds\VEIL_M1.exe" -logFile build_verify.log`
Expected: exit 0, `Builds/VEIL_M1.exe` produced, log contains `Build succeeded`. This is the closest automated stand-in for "milestone is shippable" — it does not replace the manual playtest checklist above.

- [ ] **Step 11: Commit**

```bash
git add Assets/_Project/Scripts/Movement/PlayerController.cs Assets/_Project/Scripts/Movement/Actions/ActionController.cs Assets/_Project/Scripts/LevelGen/Editor/PlayerPrefabBuilder.cs Assets/_Project/Scripts/LevelGen/Editor/LevelBuilder.cs Assets/_Project/Tests/EditMode/PlayerControllerTests.cs Assets/_Project/Prefabs/Player.prefab Assets/_Project/Settings Assets/_Project/Levels/M1_StealthSandbox.unity docs/PLAYTEST_CHECKLIST.md
git commit -m "feat: wire PlayerController, Player prefab, and M1 stealth sandbox scene"
```

---

## Self-Review

**Spec coverage:** Every M1 spec section maps to a task — movement controller (Tasks 4–9), interaction/grab (Tasks 11–12), camera juice (Task 10), test level (Task 13), Player integration + playtest validation (Task 14), input (Task 2), tunables (Task 3), project bootstrap (Task 1). Coding-standards/performance-target constraints are enforced inline (zero-GC assertions in Tasks 4/10, `MovementSettings`-only tunables throughout, `[SerializeField]` private fields everywhere, XML docs on every public member).

**Placeholder scan:** No TBD/TODO markers. The one open variable — the exact Unity Editor install path — is resolved by Task 1's own verification step, not left as an unresolved design gap.

**Type consistency:** Verified `IMotor`, `MovementContext`, `MovementStateId`, `IMovementState`, `IMovementAction`, `IInteractable`, `IGrabbable` signatures are used identically across every task that consumes them (cross-checked field/method names task-by-task while writing).

**Scope:** Single milestone, no decomposition needed — all 14 tasks build toward one playable slice.

---

**Plan complete and saved to `docs/superpowers/plans/2026-07-17-veil-milestone1.md`.** Two execution options:

1. **Subagent-Driven (recommended)** — fresh subagent per task, review between tasks, fast iteration.
2. **Inline Execution** — batch execution in this session with checkpoints.

Which approach?
