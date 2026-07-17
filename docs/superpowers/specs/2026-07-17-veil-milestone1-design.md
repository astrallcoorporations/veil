# VEIL — Milestone 1 Design: Movement & Interaction Foundation

**Date:** 2026-07-17
**Status:** Approved for planning
**Scope:** First playable milestone only. Full-game architecture (AI, missions, UI shell, save system, etc.) is deferred to later milestones and is explicitly out of scope for this document.

## Context

VEIL is a commercial-quality Unity 6 (URP) first-person stealth + parkour game targeting Steam, drawing on Hitman (sandbox assassination), Mirror's Edge (fluid movement), and Dishonored (multiple approaches). Core pillars: player freedom, replayability, highly polished movement, intelligent AI, minimalist premium UI, AAA game juice.

This is a greenfield project — no existing code. This spec covers only the first milestone from the roadmap: a first-person controller with sprint, crouch, slide, vault, mantle, basic interaction (including physics grab/carry), and one small test level using placeholder assets.

## Decisions Locked In

- **Movement feel:** Mirror's Edge–pure. Fast, momentum-heavy, speed is the reward. Vault/mantle/slide chain with minimal friction. Stealth sections slow the player by design choice (crouch/lean), not by controller weight.
- **Camera juice:** Full dynamic — FOV kick on sprint, camera tilt on slide/lean, procedural speed-driven bob. Sells speed at the cost of more tuning work now.
- **Interaction scope:** Includes physics grab/carry/throw for rigidbody objects in addition to prompt-based interaction (doors, levers, pickups), not deferred to a later milestone.
- **Test level:** Mini stealth sandbox — blends verticality with sightline-blocking cover, previewing the future stealth space, rather than a pure movement-only gym.
- **Architecture:** Hybrid FSM + Actions (see below), chosen over pure FSM or pure ability-component system.

## Architecture

### Movement controller: Hybrid FSM + Actions

Two cooperating layers, kept deliberately separate:

1. **MovementStateMachine** — owns coarse **macro-state** only: `Grounded`, `Air`, `Crouch`. Ticks the active state each frame and exposes transition-validity checks.
2. **ActionController** — owns transient **one-shot moves**: Vault, Mantle, Slide. Each is a self-contained action object implementing a shared `IMovementAction` contract (`CanExecute(context)`, `Execute`, `Cancel`). Actions are priority-resolved and interruptible, and may only trigger from valid macro-states (e.g. Vault only from Grounded/Air, not mid-Slide).

Both layers act on a **CharacterMotor** that is intentionally dumb: a kinematic, capsule-cast-based motor (not Unity's built-in `CharacterController`, which is too limited for slope handling and slide physics) that just applies velocity and reports ground/slope state. Neither the state machine nor the action layer touches physics directly — they only tell the motor what to do.

**Why this over the alternatives considered:**
- *Pure FSM* was rejected: with vault/mantle/slide plus pillars calling for future traversal abilities (wall-run, lean, ziplines), a single state machine's state count would combinatorially explode.
- *Pure ability/component system* (every move fully decoupled, priority-resolved from the ground up) was rejected for M1: maximum long-term scalability, but too much upfront conflict-resolution plumbing for a first milestone.
- *Hybrid* balances both: macro-state stays small and never needs to grow much, while new one-shot moves can be added later as new Action classes without touching the state machine.

Supporting pieces:
- **InputReader** — a ScriptableObject wrapping Unity's Input System, decoupling input reads from every consumer (motor, states, actions, UI later).
- **CameraController / CameraJuice** — a camera rig decoupled from the capsule root, driving procedural FOV kick, tilt, and bob from current state + speed.
- **MovementSettings (ScriptableObject)** — all tunables (speeds, acceleration, vault detection distance, slide friction curve, etc.) live in data, not code, so movement feel can be iterated without touching scripts.

### Interaction system

Two separate classes, kept apart deliberately (single-responsibility — avoids one god-Interactor class):

- **IInteractable + InteractionCaster** — a raycast/spherecast from the camera each frame surfaces the nearest valid `IInteractable` (doors, levers, static pickups) and fires a C# event carrying the current prompt. No UI is built in M1; the event exists so a future UI system can bind to it without changes here.
- **GrabController** — handles physics grab/carry/throw for `IGrabbable` rigidbodies: pickup, camera-relative held offset, mass/collision-aware release, throw impulse. Operates independently of `InteractionCaster` so static-trigger interaction and physics-carry interaction can each evolve without coupling to the other.

### Folder structure

```
Assets/_Project/
  Scripts/
    Core/           — interfaces, bootstrap, GameManager
    Movement/
      Motor/          — CharacterMotor (kinematic capsule-cast)
      States/         — IMovementState, Grounded/Air/Crouch states, MovementStateMachine
      Actions/        — IMovementAction, VaultAction/MantleAction/SlideAction, ActionController
      Input/          — InputReader (Input System wrapper, ScriptableObject)
    Camera/         — CameraController, CameraJuice (FOV kick/tilt/bob)
    Interaction/    — IInteractable, InteractionCaster, GrabController, Interactables/*
    Settings/       — tunable ScriptableObjects (MovementSettings, CameraSettings)
  Levels/           — test level scene + level-specific prefabs
  Editor/           — future custom tooling
```

## Test Level — Mini Stealth Sandbox

A small enclosed greybox space (~40×40m) combining verticality (rooftop ledges, vent shafts, low walls) with sightline-blocking cover (crates, pillars). No AI is present in M1 — the geometry exists to preview the future stealth space and validate that movement and cover read well together.

Must contain, at minimum:
- One vault gap
- One mantle ledge
- One slide-under gap
- 2–3 interactables (at least one door, one lever, one grabbable prop)
- Open sightline lanes suitable for future patrol-AI testing

Placeholder greybox materials/prefabs only (ProBuilder or primitives) — no final art in M1.

## Validation Approach

Movement feel is playtest-driven, not automated, for M1. Playtest checklist:
- Vault → mantle → slide chains without dropped input or stuck capsule states
- No visible camera jitter from juice systems at frame boundaries
- Grab/carry/throw does not clip held objects through geometry
- No stuck states transitioning between macro-states mid-action

Where feasible, state and action logic is kept free of `MonoBehaviour` dependencies (plain C# classes driven by the motor/state machine), so Unity Test Framework unit tests can be added later without requiring a refactor. No automated tests are required to ship M1.

## Explicitly Out of Scope for M1

- AI (patrols, detection, alert states)
- Any UI (HUD, menus, prompts rendering — only the underlying event hooks exist)
- Save system
- Mission/objective structure
- Final art, audio, animation — placeholder only throughout
