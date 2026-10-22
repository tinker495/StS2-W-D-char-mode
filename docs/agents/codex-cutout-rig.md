# Character parts and native Godot animation contract

Use this contract when preparing parts, editing native Godot scenes and inspecting integration. Godot `.tscn` is the sole combat/merchant animation source; see the [editing guide](../characters/realta/animation/godot-editing.md). Save edits directly; inspection and packaging never regenerate motion. Read repository `AGENTS.md`, `ARTS.md`, the character art/asset specifications, and the `character-asset-pipeline` and `imagegen` skills first. Include this language rule in each task's shared preamble: agent instructions and implementation notes (including draft `NOTES.md`) are English; human-facing reports are Korean.

## Inputs and output scope

- `IDENTITY_IMAGE`: `docs/characters/realta/base.png`.
- `POSE_IMAGE`: `Sts2WD/images/charui/realta/combat.png`.
- `OUTPUT_DIRECTORY`: a fresh version under `docs/characters/realta/drafts/`.
- Goal: transparent parts, native Godot scenes, explicit ownership, and a separate evidence-backed account of game runtime compatibility and official combat/scene comparisons. Resolve actual clip names from the runtime; the observed official set is `idle_loop`, `attack`, `hurt`, `cast`, `die`, and `relaxed_loop`.
- Start with a reviewable standalone candidate. Promote assets or replace the runtime scene only when the user's requested scope authorizes application. Installation is a separate action.

## 1. Inspect the actual integration contract

Read the domain documentation before exploring code. Locate the character's visual scene property, the scene's required node names/types, layout markers, and the installed framework's animation interface. Record exact repository paths and members; distinguish observed contracts from assumptions.

For the initial Realta trial, `Realta.CustomVisualPath` points to `Sts2WD/scenes/realta/combat.tscn`. Its root is `Node2D`; the unique `Visuals` child is `Sprite2D`, at `(0, -155)` with scale `0.625`. The scene also provides `Bounds`, `CenterPos`, `IntentPos`, `OrbPos`, `TalkPos`, and `FormVfx`. Preserve these contracts on promotion. Do not infer an `InvalidCastException` merely from the presence of a sprite.

Find actual attack, damage, death, and return-to-idle dispatch before wiring clips. A playable standalone `AnimationPlayer` does not establish that game events invoke it. If the framework dispatch is unresolved, deliver the standalone candidate and state that limit explicitly.

The subsequent [runtime investigation](../research/sts2-cutout-runtime-contract.md) found a separate BaseLib 3.4.7 non-Spine dispatch. Combat needs `idle`/`relaxed`/`powerup` aliases for official-style names, explicit completion transitions, and an intentional death wait. Merchant searches `idle` before `relaxed_loop`, so its idle alias must represent the merchant pose. Rest Act/light-off and rest game-over do not automatically reach non-Spine players. Combat/rest factories replace the original scene root; place persistent controllers below a retained child. Confirm these version-specific findings before promotion.

## 2. Preserve visible pixels; generate only missing artwork

Use the exposed `image_gen` tool (currently `image_gen__imagegen`), not a terminal SDK runner. This mode needs neither `GEMINI_API_KEY` nor `OPENAI_API_KEY`. Do not install `google-genai`, invoke Gemini-dependent `split_character.py`, clone an unrelated pipeline by default, or silently switch to an API fallback.

Open both local inputs with `view_image` before generation. Pass their real paths through `referenced_image_paths`, with `transparent_background: true`. Use `num_last_images_to_include` only for conversation images without local paths; never combine the two input mechanisms.

Label the base as the identity/rendering anchor and combat as the user-selected deconstruction target. Preserve its setup pose, face, equipment construction, proportions, and palette. The animation-improvement task authorizes pose-specific candidates where the motion requires them, including seated rest and a grounded death pose. Do not silently replace the approved setup image or treat candidate art as approved canon.

First assign the existing image's visible pixels to named ownership masks. Keep all parts on the same source canvas and store masks, pivots, parents and draw order in one manifest. Extraction is authorized by this task. Do not repaint pixels in a conversion script. A generative atlas redrew the first trial's face/proportions and duplicated armor; do not repeat a whole-atlas generation to fix one defective joint.

Begin with head+hat, torso, rear coat, both upper arms, both forearms, gripping hand, thighs, shins, feet, and one rigid weapon. Split the ribbon only if its motion needs independence. Use viewer-left/right names. A small rest motion can instead use a continuous UV mesh around a few controls, preserving hand-to-face contact without cutting that contact apart.

Inspect both setup and extreme poses. Use the image tool to paint missing joint ends, the coat behind legs/weapon, or a pose-specific cel that cannot be made by rotating the original surfaces. Preserve original visible pixels over separate underpaint where practical. If a whole component must be redrawn for continuity, state which component changed and compare the new silhouette. Generated canvas position, scale, alpha and exact identity are not guaranteed; inspect and register the returned component explicitly.

Prompt template:

```text
Use case: identity-preserve / precise-object-edit.
Asset type: one transparent cutout animation component / occlusion fill.
Input 1: approved base; identity, materials, proportions and rendering.
Input 2: selected standing/combat image; exact assembled pose.
Return only [named component], in its specified source-canvas placement.
Repair [observed missing surface] needed by [specific extreme pose].
Paint complete hidden joint ends with rounded overlapping material.
Assign each shoulder plate, cuff, collar, belt and skirt edge to exactly
one component; no duplicated armor or seams across adjacent parts.
Keep the whole polearm as one rigid connected component without a hand.
Preserve the approved face, apparent age, pose, palette and anatomy.
Actual transparent alpha; no labels, grid, scenery or ground shadow.
```

The tool reframes its canvas (different size, scale and placement) even when told not to. Register every result before use: search scale and translation against regions the edit should not change (for a face edit, the hair/brim ring around the face; for a blink, the face outside the eyes), then composite only the intended feathered mask. When a pose change cannot be made by rotating existing parts (a long rigid coat cannot kneel), pose the real parts by FK into a guide, render several target poses as panels of one canvas, and ask for all cels in one call; the panels stay mutually consistent and map back to source pixels by the guide's known region and scale.

After generation, copy the output from the tool-reported location into the project. Save the exact prompt and input roles with the draft. Inspect alpha, component completeness, ownership and actual-size appearance with the asset pipeline's `inspect` and `preview` commands, then open the previews.

Part extraction is expressly part of this workflow: extract atlas components into separate PNGs without repainting, scaling individual regions to fix composition, or clipping component silhouettes. Preserve the original atlas and extraction rectangles. If components overlap, regenerate them instead of cutting through their artwork. Automated conversion may split transparent gutters and resize whole components; image changes and hidden-surface painting belong to the image tool.

## 3. Record placement and joint ownership

Author directly in native Godot coordinates: pixels, +X right, +Y down, positive clockwise rotation in radians. Record each part's PNG, parent joint, source-pixel pivot, attachment scale, local position, and absolute draw order. Specify canvas size and ground anchor.

Generated parts are redrawn approximations, not lossless segmentation. SIFT/RANSAC may suggest placements for unchanged textured regions, but cannot guarantee exact transforms, infer hidden joints, or establish occlusion reliably. Manual reviewed anchors and draw order are the default. Record any drift from the original as unresolved rather than calling it an exact reconstruction.

Check semantic ownership at every boundary: coat fur on a leg creates a moving white shard; straw on a coat leaves an orange streak when the weapon moves. A pixel-perfect setup reconstruction alone cannot detect either error. Alpha/color-assisted masks may help a reviewed source-specific extraction, but are not a general semantic segmenter.

Draw by tree order, not `z_index`. Godot draws canvas items in tree order within a canvas layer, and any positive `z_index` (relative or absolute) lifts parts above sibling game UI such as health bars. Keep every part at z 0 and order the children; a weapon parented under bones draws at its `Skeleton2D`'s slot. Do not use `top_level` for a released prop: it leaves the parent's modulate/visibility chain, so game fades miss it; express the target in the bone's local space instead. Parent the weapon to a grip joint under the gripping forearm; confirm the grip through the complete animation range.

## 4. Preserve the Godot runtime rig contract

The game runtime requires a `Node2D` scene root and a `Sprite2D` compatibility child named `Visuals` where the real integration needs it. Keep `Visuals.visible = true` and its modulation opaque: hiding/modulating the parent also affects descendants. An empty texture is sufficient for the container. Retain the existing game joint hierarchy, skinned `Polygon2D` parts and sprite attachments when editing native scenes.

Maintain hip → torso → head/arms, hip → legs/coat, forearm → grip → weapon in the native Skeleton2D hierarchy. Preserve source-pixel pivot registration separately from the joint's parent-local transform at the runtime boundary. Use unit animation scale; rotate/translate individual joints instead of squashing the complete standing image.

For supported poses, author planted-foot targets and two-bone IK in Godot and store the resulting motion in AnimationPlayer tracks. Release the foot for intentional stepping/falling. For continuous seams use local `Polygon2D`/`Skeleton2D` weights only across anatomically connected parts. Do not infer bone connectivity from 2D proximity: a casting forearm next to the coat must not pull that coat. Restrict triangles to each texture's occupied region; a full-canvas grid duplicated for every layer wastes work. Retain topology and weights between timing edits.

Separate rigid metal/weapon shapes, flexible cloth, and pose-specific attachments. A prone drawing may solve a final silhouette but does not solve the transition into it; inspect the frames immediately before and after an attachment change. Avoid hiding a pop with a long crossfade or declaring a good final still a finished death animation.

Edit native `AnimationLibrary` and `AnimationPlayer` resources in the `.tscn` scene. Author setup/reset values for every animated property; include those properties in each clip so switching from hit/death cannot leave stale transforms. `idle` loops with restrained upper-body breathing and planted feet. `attack` uses anticipation, a readable swing, and recovery. `hit` recoils and returns. `die` settles into a nonlooping final pose; revival/reset is explicit. Use Godot track interpolation deliberately; distinguish a linear first draft from a finished eased animation.

Keep static polygon/UV/triangle/weight data on Polygon2D nodes. A property that is identical across all keys of every canonical clip should not be copied into every animation; keep only actual changes and their reset keys. Derive composition from one set of semantic state tracks rather than parallel resolved visibility/order arrays. Preserve dense adopted motion during ownership cleanup. See the [Godot native workflow comparison](../research/godot-native-animation-workflow.md). Use the editor-only `RESET` with `reset_on_save=true` for animated setup defaults; keep static meshes out of it. The user explicitly authorized the original ten game dispatch names plus RESET on 2026-10-22. Maintain RESET when intentionally changing animated neutral values; never route gameplay autoplay/next to RESET.

Common combat/merchant hierarchy and mesh ownership belongs to `realta_rig_base.tscn`. The existing game entry paths inherit it; do not copy common node data into variants. Six common clips are external Animation resources in `clips/`, referenced by both native libraries; preserve resource identity across variants. Keep only variant RESET/revive, initial pose overrides and dispatch settings separate. Edit/save native `.tres` resources as well as scenes. Saved game AnimationPlayers must use automatic idle processing, and resources must pass the configured MegaDot version's import rather than relying on a newer ordinary Godot serializer.

Let the native player own clip state and time. A script that calls `seek()` every frame can display correct previews while overriding external `Play()` calls. Bake joint motion and key any attachment-controller state/time on the same player; reserve `seek()` for scrubbing. Observe automatic return with `advance()` and the real engine clock, since seeking to a clip's end does not exercise completion. Repeated `play()` of the already-playing clip does not establish a fresh time-zero sample: stop/reset or observe accumulated time explicitly.

Keep the controller under the retained `Visuals` child when the framework replaces scene roots. Resolve adjacent resources relative to the script directory, and reimport after relocation. Combat `idle` and merchant `idle` need different underlying poses. Define return-to-idle explicitly; a name alias alone does not create a transition. The inspected game's default revive is a 0.2-second fade-out, idle reset, and 0.2-second fade-in, not reversed death. Custom player discovery can suppress this fallback even when the candidate lacks a `revive` clip. Include and observe that dispatch name and interrupted fade recovery. See the [packaging/lifecycle review](../research/sts2-cutout-v8-packaging-review.md) for the evidence and version limits.

Check prop clearance numerically: transform the rigid weapon's alpha pixels by FK at every key and at interpolation quarter-points and compare with the source floor. When it fails, redesign the grip path first (lift during wind-up and recovery); a per-key angle correction that spreads into the anticipation or the event pose is a regression. Put expressions and VFX (texture swaps, trail/sigil alpha and scale) on the same player as the joints, and key their defaults at t=0 of every clip so an interrupted clip never leaves a stale face or effect.

Resolve effect time separately from clip length. The observed official characters use `AttackAnimDelay = 0.15`, while their full attack clips are much longer. Confirm Realta's actual call path, then place the strike accordingly. Offset the head, hat tip, ribbon and coat settling instead of giving every part identical key times. Merchant needs a visibly relaxed pose, rest needs its seated pose/three Act lighting variants and light-off behavior, and selection needs its own composition. Do not count renamed identical loops as completed variants.

## 5. Verify and report

Use the [motion review matrix](cutout-motion-review.md) for each affected action and record evidence for every applicable row. Animation edits belong to the native `.tscn` bones, Polygon2D mesh/weights and AnimationPlayer tracks. For retained game-resource inspection, follow the tracked [animation procedure](../characters/realta/animation/reproduction.md): `dev.ps1 anim` imports, checks, captures and creates review sheets from saved game scenes. It reads saved scenes without generating motion. Run unchanged checks before packaging.

Serialize scalar rotation key values as floating-point literals, including `0.0`. Sample actual joint properties during playback; integer-valued keys can round small rotations to zero even when clip names and durations load correctly.

Load and instantiate the candidate with the repository's Godot version. Exercise every clip and inspect setup, overlap, grip, planted feet, clipping and silhouette on light/dark backgrounds at 240×360. Save an animated preview and the original comparison. Treat exploratory scripts and rendered observations as informal verification, not new acceptance tests; preserve all protected tests and validators.

Use actual Godot renders for final comparisons. The Python preview is useful during extraction but is not evidence of engine playback. Compare with official resources for the same scene, using a fixed camera throughout each clip; include normal-speed sequences, event-time poses and attachment-transition neighbors. Record which version and renderer produced each image. The observed installation is v0.107.1 / commit 59260271; re-check this for another installation.

At every verification iteration, open a relevant external primary source and connect its technique to the current defect. Log the URL/section, adopted or rejected approach, edited masks/parts/rig/clips, image-tool calls, rendered frames, measured elapsed time (or unmeasured), improvement and remaining defects. Reusing a relevant source is allowed; copying the previous rationale without a new observation is not research. Prefer one shared source-coordinate manifest, local regeneration and topology reuse over repeated full-atlas alignment. Do not turn informal observations into new acceptance thresholds.

For runtime promotion, preserve markers/bounds and the verified framework dispatch, run the repository build/import/PCK workflow, and verify resources instantiate. Run gameplay checks if available. A successful C# build alone verifies neither imported textures nor event playback. For standalone draft-only work, compile/package the candidate only as necessary; installation is not implied.

The editable `.tscn` contains bones, skinned polygons and AnimationPlayer with aliases, `next` and `autoplay`, with a small controller script only for state a value track cannot express. Trim fully transparent margins from part textures and record their source-canvas offsets in one manifest; downscale VFX painted far larger than displayed. Realta's implementation is the reference: [animation/README.md](../characters/realta/animation/README.md).

Raw JSON loaded by `FileAccess` needs explicit non-resource export inclusion. Dynamically constructed texture/script paths also need inclusion when exporting selected resources. Build an isolated candidate PCK and mount it in a fresh host project without the source project's import cache; render the loaded candidate and exercise the actual clip names. This demonstrates packaged resource loading, not BaseLib factory conversion or gameplay. Do not promote unused experiments, captures, or official research artwork into the package.

Report saved artifacts, prompt location, rig hierarchy, parts/Z order, clips, observed visual defects, actual checks run, and unverified game integration. Human approval of visual quality remains separate from successful loading.

## Primary engine references

- [Godot 4.5 cutout animation](https://docs.godotengine.org/en/4.5/tutorials/animation/cutout_animation.html): hierarchy and pivots.
- [Godot 4.5 AnimationPlayer](https://docs.godotengine.org/en/4.5/classes/class_animationplayer.html): native clip playback.
- [Godot 4.5 CanvasItem](https://docs.godotengine.org/en/4.5/classes/class_canvasitem.html): inherited visibility/modulation and Z ordering.
- [Esoteric asset preparation](https://eu.esotericsoftware.com/blog/How-to-cut-your-assets-for-animation): hidden surfaces, joint overlap and motion-driven decomposition.
- [Spine mesh attachments](https://esotericsoftware.com/spine-meshes): topology, weights, vertex economy and destructive Trace/Generate caveats.
- [Godot 4.5 2D skeletons](https://docs.godotengine.org/en/4.5/tutorials/animation/2d_skeletons.html): weighted geometry and internal vertices; verify the version-marked tutorial in the actual engine.
- [Official animation comparison](../research/sts2-official-animation-comparison.md): local resource evidence, scene differences and external workflow research.
