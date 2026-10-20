# Cutout v8 packaging and lifecycle review

Audience: agents preparing a later authorized runtime candidate. Reviewed 2026-10-19. This is a read-only review of the candidate, game/library source, and export configuration. No candidate, runtime, tests, package, or installation was changed. This note describes the inspected v8 state; later drafts may already address individual findings.

## Evidence baseline

- Game: Slay the Spire 2 `v0.107.1`, commit `59260271`, release metadata timestamp `2026-06-18T15:43:56-07:00`.
- BaseLib dependency: `Alchyr.Sts2.BaseLib 3.4.7`; inspected source commit `c070755d11ec062f6c97c4dda1a205d975ed066a`. [Primary source tree](https://github.com/Alchyr/BaseLib-StS2/tree/c070755d11ec062f6c97c4dda1a205d975ed066a).
- Engine: MegaDot `4.5.1.m.14.mono.custom_build`; project SDK `Godot.NET.Sdk/4.5.1`.
- Candidate: `docs/characters/realta/drafts/cutout-v8/rig.gd`, `death_body.gd`, `combat_candidate.tscn`, `merchant_candidate.tscn`, three JSON files, and texture imports. The body uses fourteen textured meshes plus a rigid weapon and ribbon; the alternate death body loads `fall.png`.
- The candidate's six source clips are `idle_loop` 2.6 s, `attack` 1.05 s, `hurt` 0.8 s, `cast` 1.3 s, `die` 2.2 s, and `relaxed_loop` 8 s. Existing logs exercise direct AnimationPlayer calls and explicit idle reset. They do not establish BaseLib factory conversion, packaged resource loading, or actual revive dispatch.

## Required before packaging

### Include the raw JSON, not only textures and scripts

`rig.gd` reads `rig.json` and `meshes.json` with `FileAccess.get_file_as_string` and `JSON.parse_string`. `death_body.gd` reads `death-mesh.json` the same way. The main project's [export preset](../../export_presets.cfg) uses `export_filter="all_resources"`, but its only non-resource include pattern is `Sts2WD/localization/**/*.json`. Arbitrary JSON is not automatically a Godot resource. Merely relocating the draft under `Sts2WD/` does not solve that inclusion gap. Godot explicitly requires a Keep File import mode or a non-resource export filter for files read this way. [Godot 4.5 FileAccess](https://docs.godotengine.org/en/4.5/classes/class_fileaccess.html), [Godot export resource filters](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_projects.html)

For a later approved application, choose one concrete option:

1. Preserve the current JSON format and add a narrow include pattern covering its chosen runtime folder, while retaining the localization pattern. For example, if the files are placed in `Sts2WD/scenes/realta/cutout/`, the combined value can be `Sts2WD/localization/**/*.json,Sts2WD/scenes/realta/cutout/*.json`. Keep all three files together with the scripts, because the loaders resolve adjacent paths.
2. Convert this data to explicit Godot resources and use resource loading. This is a larger implementation change and is not necessary merely to fix the filter.

The raw readers currently assume successful reads and correct dictionary shapes. Missing JSON will fail while building the rig, before successful playback. Add contextual load/parse diagnostics in the implementation if promoting it; do not hide the error by showing an empty rig.

### Reimport relocated textures in the actual project

The main project has both `docs/.gdignore` and an export exclusion for `docs/*`. Standalone draft import success therefore does not mean those assets exist in the main PCK.

The candidate already uses `load(asset_path(...))` for textures and the death-body GDScript, which is appropriate for Godot's imported-resource remapping. Keep that resource-loading approach. Do not replace it with raw PNG reads merely because the source files are visible in the editor. Preserve the folder layout, including `parts/`, when relocating. `asset_path` derives its base from `get_script().resource_path`; it is independent of the wrapper's reparenting.

The existing standalone import descriptors identify sources such as `res://fall.png` and `res://parts/torso.png`. Those paths will be wrong after a runtime relocation. Run the main project's import/publish workflow for the new location; do not treat a copied standalone `.godot/` cache as the runtime import result. The unused standalone `project.godot`, preview, render scripts, logs, and capture images are not runtime dependencies.

The current `all_resources` policy makes dynamically selected imported textures and GDScript eligible once they are in an included runtime folder. If a future export changes to selected-scene dependencies, dynamically constructed string paths require separate inclusion evidence. This review did not produce a PCK inventory or a clean packaged load.

## Factory compatibility: what is already improved

The v8 combat wrapper has a plain `Node2D` root and a child `%Visuals` `Sprite2D`, with the scripted `Rig` below it. Its fixed-path `Bounds` and `IntentPos`, and unique `CenterPos`, `OrbPos`, `TalkPos`, `FormVfx` markers match the BaseLib factory's expected names/types. A textureless `Sprite2D` is still a `Node2D`; `NCreatureVisuals._Ready` uses it as that visual container and does not require its own texture.

`BaseLib.Utils.NodeFactories.NodeFactory.TransferAndCreateNodes` replaces the combat root because this factory has non-unique fixed paths. The child `Visuals/Rig` survives that operation; the rig no longer depends on a script attached to the discarded root. `NMerchantCharacterFactory` has no named-node constraints, so its flexible branch retains the original wrapper as a child. Relative rig/bone AnimationPlayer paths remain inside the retained rig in both cases. Source: `NodeFactory.cs`, `NCreatureVisualsFactory.cs`, `NMerchantCharacterFactory.cs`; game `MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals._Ready`.

The wrapper's `IntentPos` does not declare `unique_name_in_owner`, but the factory's fixed-path `NodeInfo<Marker2D>("IntentPos")` sets the unique flag during conversion. Do not report that alone as a defect. Verify the converted instance rather than only the unconverted draft.

The dynamic AnimationPlayer is created synchronously in the child rig's `_ready`. Normal parent-ready ordering should make it available when BaseLib handles the parent. However, `CustomAnimation` caches a null discovery as well as a positive one; its rediscovery branch handles an invalid previously found player, not an initially null result. Avoid querying custom-animation availability before the rig has created its player. If construction later becomes deferred/asynchronous, build an explicit readiness contract rather than relying on the current ordering. Source: `Utils/CustomAnimation.cs`, `_animHandler`, `PlayCustomAnimation`.

One primary AnimationPlayer currently exists in each wrapper; the death body creates bones and meshes but no competing player. Preserve that clear discovery target. BaseLib prefers any AnimationTree it finds before AnimationPlayer and can select a different player if future VFX nodes add one earlier in traversal.

## Remaining lifecycle gaps

### Actual revive is not equivalent to a direct idle observation

V8 aliases `idle` to the variant's default loop, `relaxed` to `relaxed_loop`, and `powerup` to `cast`. It queues attack/hurt/cast/powerup back to the default. Those changes address the earlier trigger-name mismatch. It does not expose `revive` or `Revive`.

`BaseLib.Patches.UI.CustomAnimationPatch.UseCustomReviveAnim` intercepts the game's `NCreature.AnimTempRevive`. For a non-Spine creature it searches `revive`, then `Revive`. `CustomAnimation.PlayCustomAnimation` returns whether a custom animation source exists, even when the requested names were not found. Consequently, the existence of v8's AnimationPlayer suppresses the game's fallback while leaving the death pose unchanged if no revive mapping is provided. `Play("idle")` in a draft observation is useful reset evidence but does not exercise that missing dispatch.

The inspected official behavior does **not** require a reverse-death or rising motion:

- `CharacterModel.GenerateAnimator` and the overrides for Ironclad, Silent, Defect, Necrobinder, and Regent register no `Revive` trigger.
- `NCreature.StartReviveAnim` uses a registered Spine `Revive` trigger if one exists; otherwise players enter `AnimTempRevive`.
- `AnimTempRevive` fades `Visuals.modulate:a` to zero over 0.2 s, calls `ImmediatelySetIdle`, then fades alpha back to one over 0.2 s. `ImmediatelySetIdle` sends `Idle` to the Spine animator and clears mixing/sets the current track to its animation end.
- The directly enumerated Ironclad and Silent combat/merchant resource clip lists contain no revive clip. This resource-list observation covers those two characters; the five-character conclusion above comes from their animator registration code.

Thus a `revive` alias to the default idle animation is a valid **minimal visual-state restoration**, but it omits the official 0.4 s fade transition. A more faithful candidate can implement that fade-out/reset/fade-in pattern. Do not author a reversed fall on the assumption that it is official behavior.

For either implementation, restore `pose_kind`, `pose_time`, live body visibility, death-body visibility, and weapon attachment/transform. The current `_sync_overlay` already restores most of these when the active pose becomes idle. If adding a distinct `revive` clip, remember that `_sync_overlay` indexes `data.clips[pose_kind]`; setting an unknown pose_kind string without adding matching data will fail. An alias to the existing idle resource keeps its original idle pose_kind track and avoids that mismatch.

### Death duration is still a runtime model decision

The selected v8 clip is 2.2 s, while Realta currently inherits `CustomCharacterModel.DeathAnimTime == 1.5f`. BaseLib's `AdjustTime` and `WaitCustomAnim` use that model property with a maximum of 5 s. Align the model delay with the selected animation's meaningful settling interval during authorized integration; do not infer it from an empty `NCreature.GetCurrentAnimationLength` for non-Spine visuals. This discrepancy does not affect the independent preview, which waits its own duration.

### Draw order and released-weapon composition remain unverified

V8 gives live body polygons and the weapon `z_as_relative=false`, while the alternate corpse uses relative `z_index=20`. The released weapon additionally switches to `top_level=true`. These choices work in a single candidate viewport, but they bypass some parent ordering/transform behavior. A parent's game-layer Z change can affect the corpse differently from the live polygons; top-level also changes the weapon's draw ordering. Inspect multiple actors, room overlays, and the merchant game-over reparent rather than assuming the single-actor result covers them. Godot documents both top-level and relative-Z behavior. [Godot 4.5 CanvasItem](https://docs.godotengine.org/en/4.5/classes/class_canvasitem.html)

If matching the official revive fade, specifically observe whether the released weapon fades with the visual container. Do not assume inherited modulation behaves identically after making a node top-level. A dedicated release container under the visual rig or explicit weapon modulation may be needed, depending on the selected implementation. This is a targeted integration observation, not a claim that the present single-scene animation is broken.

## Next authorized observations, in order

1. Add the missing revive mapping/transition to the next selected standalone candidate, retaining the six authored motion clips. Exercise `die → revive`, not just `die → idle`; confirm live-body visibility and weapon reattachment after the real requested name.
2. Prepare a narrow runtime resource layout and JSON inclusion rule. Reimport resources from that location. Do not broaden the filter to include every draft or every repository JSON.
3. Export the candidate and inspect only the resulting PCK from a clean probe project that cannot see the source draft. Confirm all three JSON reads, sixteen used part textures, `fall.png`, both GDScripts, and both wrappers resolve from the package. Merely loading a PCK over the source project can let missing package files fall back to local files.
4. Instantiate through the actual BaseLib factory conversion and inspect `%Visuals`, root `Bounds`, unique `IntentPos`, selected AnimationPlayer, and variant-specific startup. Log that the resolved scene is a `NCreatureVisuals`/`NMerchantCharacter`, not only a generic Node2D wrapper.
5. Exercise real combat aliases, repeated attack, interruption by hurt/death, actual revive, merchant startup/death/reparent, and death waiting. Verify draw order/modulation with surrounding content. Rest-site act/light-off and rest-site game-over's direct Spine call remain separate contracts described in [runtime contract research](sts2-cutout-runtime-contract.md).

No tests or acceptance assets were added or changed. No candidate files were altered during this review. Current logs were read as observations, not adopted as expected fixtures. This iteration consulted the official Godot FileAccess/export/CanvasItem documentation and local first-party framework/game sources; it used zero image-generation calls, zero native rerenders, and zero package exports. The useful efficiency change is to close dispatch and packaging gaps with small targeted probes before spending another art-generation/render iteration on an otherwise unreachable animation.
