# Realta Scene Connections and Standing Fallback

[Asset index](../../realta-assets.md).

As of 2026-10-20, the nine Branch B runtime PNGs, heated-iron card body, flame-heart combat energy counter, and dedicated power icons coexist with the four animated Realta scenes. `combat.png` remains the canonical standing still and fallback; the combat, rest, and merchant scenes use their animation resources. [runtime image inventory](../ui/assets.md#canonical-base-and-runtime-images) and [scene connections](../animation/assets.md#current-scene-and-property-connections) map the images and scenes; [card frame application](../cards/frames.md), [combat energy counter](../ui/assets.md#lower-left-combat-energy-counter) and [power presentation status](../powers/assets.md#power-display-titles-and-runtime-icon-status) describe the newer presentation resources.

The 2026-10-20 combat/merchant rig places the weapon and ribbon in front of all body and death cels. Only the gripping hand and VFX appear above them; the released hand moves behind the weapon during death. Existing PNGs, meshes, and bone paths are preserved. The tree order and death ribbon angle are recorded in [animation/README.md](../../animation/README.md).

Merchant `relaxed_loop` and its `idle`/`relaxed` aliases place the entire free arm behind the body. Clip keys switch the tree order of `upper_arm_r` and `forearm_r`; the wrist moves slightly outward so the glove edge remains visible beside the coat. Return-to-combat and death ordering changes occur when the arm and coat no longer overlap. Existing PNGs, meshes, and rest/selection resources are preserved. See Merchant free arm order in [animation/README.md](../../animation/README.md) for key timing and blend details.

Merchant refinement on 2026-10-19 replaces the native eight-second `relaxed_loop` with independently authored planted-foot IK, a lower fixed shaft grip, free glove by the belt and viewer-left leg support. The same resource serves merchant autoplay/idle and combat relaxed aliases. `author_clips.py` owns the pose; `build_combat_rig.gd` owns live-phase blends to idle/death; the runtime controller suppresses phase randomization when changing loops. Alpha-only cleanup of `combat/parts/weapon-rigid.png` and `foot_l.png` preserves their canvas/offset registration and all RGB. No new paintings or rest/selection edits are part of this refinement. Reproduction and verification scope are documented in [animation/README.md](../../animation/README.md).

As of 2026-10-19, **all four Realta scenes are animated** (combat/merchant cutout rig, seated rest mesh, animated selection painting). Their runtime files, ownership and reproduction live in [animation/README.md](../../animation/README.md); decisions and comparisons are in [the iteration log](../../realta-animation-iteration-log.md) and [the runtime dispatch research](../../../../research/sts2-cutout-runtime-contract.md). `combat.png` remains the canonical standing still (portrait source, fallback), but no scene references it any more. The repository build/PCK export and packaged-scene rendering were verified; in-game play was not.

The 2026-10-18 standing delivery reverted the separate combat pose and removed the extra `standing.png`. The base-derived `combat.png` retains the youthful appearance, open forehead, both pauldrons, continuous collar, and upright one-handed stance of the approved base. The selection illustration retains the blade-inspection scene. The scene table below and [delivery conversion](../shared/conversion.md) explain the current state without local generation masters or historical logs.

## Current Scene and Property Connections

`Realta.cs` returns these `.tscn` paths under `res://Sts2WD/scenes/realta/`. Animated content lives under `res://Sts2WD/animations/realta/` (see [animation/README.md](../../animation/README.md)).

| Property | Scene | Current structure / display placement |
|---|---|---|
| `CustomVisualPath` | `combat.tscn` | `Node2D` + `%Visuals` (empty `Sprite2D` at `(0,-155)`, scale 0.625) → `Rig` instance of `realta_combat_rig.tscn` at `(-192,-288)`; `Bounds`, dialogue/intent/orb anchors and `%FormVfx` unchanged. Displayed 240×360 |
| `CustomRestSiteAnimPath` | `rest_site.tscn` | `Node2D` → `ControlRoot` (flip/seat point) → `Visuals` (`realta_rest.gd`, position `(-258,-285)`, scale 0.4), `%Hitbox`, `%ThoughtBubbleLeft/Right` |
| `CustomMerchantAnimPath` | `merchant.tscn` | `Node2D` + `%Visuals` → `Rig` instance of `realta_merchant_rig.tscn` (autoplay and `idle` = `relaxed_loop`) |
| `CustomCharacterSelectBg` | `char_select_bg_realta.tscn` | `Control` (FullRect, clips, ignores input) → `Illustration` (`realta_select.gd`) covering the rect like `KEEP_ASPECT_COVERED`, using `char_select_bg.png` |

Combat `Bounds` span `(-120,-360)~(120,0)`; the rest `Hitbox` spans `(-195,-280)~(195,200)`. `CustomIconOutlineTexturePath` connects the portrait-outline PNG. The combat energy counter uses the [flame-heart treatment](../ui/assets.md#lower-left-combat-energy-counter). The four scenes use the animations listed above; multiplayer hands retain their existing defaults.

## Standing and Combat Split

The separate runtime combat pose was reverted at the user's request. The former base-derived `standing.png` now occupies `Sts2WD/images/charui/realta/combat.png`; `standing.png` and `standing.png.import` have been removed. At that stage, combat, rest, and merchant scenes all referenced the restored `combat.png`, with their previous anchors and scales unchanged. No C# property changes were needed for this rollback. [scene connections](../animation/assets.md#current-scene-and-property-connections) describes the subsequent animation integration.

The restored 384×576 RGBA image has SHA-256 `0372A0C097D3EF52CF90858889BAB0DAFE068D3B3171027902E6C0A9C1A7AE6B`, matching the former standing file exactly. This is the current character-specific delivery even though the generic Branch B workflow supports separate poses.

Rollback packaging completed through `dev.ps1 publish` with zero C# warnings/errors and successful Godot import/PCK export. Temporary resource observations loaded nine textures and instantiated four scenes. All three character scenes resolved `combat.png`, and `ResourceLoader.exists` returned false for `standing.png`. Subsequent local installations include this rollback; actual gameplay remains unverified.

## Attack Trail (2026-10-20)

The attack `Slash` retains `slash-trail.png` and uses a `Polygon2D` built from blade-tip/edge contours sampled at 240 Hz. `author_clips.py`, `build_combat_rig.gd` and `slash_reveal.gdshader` own the path, mesh and progressive reveal. This applies to both combat and merchant rigs; reproduction is in [animation/README.md](../../animation/README.md).
