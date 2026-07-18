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
