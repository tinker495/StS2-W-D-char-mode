# Codex Image Tool → Godot Cutout Rig

Use this one-shot instruction when converting an approved standing image into a layered, articulated character. Read repository `AGENTS.md`, `ARTS.md`, the character art/asset specifications, and the `character-asset-pipeline` and `imagegen` skills first. Include this language rule in each task's shared preamble: agent instructions and implementation notes (including draft `NOTES.md`) are English; human-facing reports are Korean.

## Inputs and output scope

- `IDENTITY_IMAGE`: `docs/characters/<char>/base.png`.
- `POSE_IMAGE`: the target character's approved full-resolution standing/combat reference (`docs/characters/<char>/base.png` or the standing pose defined in `<char>-assets.md`). Existing combat/merchant edits start from the character's tracked production layout and part images (see `<char>`'s `animation-sources.md`, such as Realta's [production layout and part images](/docs/characters/realta/animation-sources.md#inputs-and-ownership)).
- `OUTPUT_DIRECTORY`: a fresh Git-ignored directory under `docs/characters/<char>/drafts/`.
- Goal: transparent parts, explicit ownership and rig manifests, native Godot playback, and an evidence-backed comparison with official combat and scene variants. Resolve actual clip names from the runtime; the observed official set is `idle_loop`, `attack`, `hurt`, `cast`, `die`, and `relaxed_loop`.
- Start with a reviewable standalone candidate. Promote assets or replace the runtime scene only when the user's requested scope authorizes application. Installation is a separate action.

## 1. Inspect the actual integration contract

Read the domain documentation before exploring code. Locate the character's visual scene property, the scene's required node names/types, layout markers, and the installed framework's animation interface. Record exact repository paths and members; distinguish observed contracts from assumptions.

Read the character's scene path, required `Node2D` layout markers (`Bounds`, `CenterPos`, `IntentPos`, `OrbPos`, `TalkPos`, `FormVfx`), `Visuals` container transform, and scene contract from the target's `animation-assets.md` (for example, Realta's [runtime asset guide](/docs/characters/realta/animation-assets.md)).

Find actual attack, damage, death, and return-to-idle dispatch before wiring clips. A playable standalone `AnimationPlayer` does not establish that game events invoke it. If the framework dispatch is unresolved, deliver the standalone candidate and state that limit explicitly.

Confirm the installed framework's [non-Spine dispatch contract](/docs/research/sts2-cutout-runtime-contract.md) before promotion: non-Spine rigs must supply BaseLib clip aliases (`idle`/`relaxed`/`powerup`, with merchant `idle` mapped to the merchant pose), explicit completion transitions back to idle, a `revive` clip (because custom player discovery suppresses the 0.2 s default fade fallback even when `revive` is missing), an intentional death wait, and custom bridging for rest Act/light-off and non-combat game-over. Because combat/rest factories replace the original scene root, place persistent controllers below a retained child such as `Visuals`.

## 2. Preserve visible pixels; generate only missing artwork

Use the exposed `image_gen` tool (currently `image_gen__imagegen`), not a terminal SDK runner. This mode needs neither `GEMINI_API_KEY` nor `OPENAI_API_KEY`. Do not install `google-genai`, invoke Gemini-dependent `split_character.py`, clone an unrelated pipeline by default, or silently switch to an API fallback.

Open both local inputs with `view_image` before generation. Pass their real paths through `referenced_image_paths`, with `transparent_background: true`. Use `num_last_images_to_include` only for conversation images without local paths; never combine the two input mechanisms.

Use the base as the identity and rendering anchor, and the selected combat image as the parting target. Preserve the setup pose, face, equipment construction, proportions, and palette. For an authorized clip that needs a distinct pose, such as seated rest or a grounded death pose, author a pose-specific drawing while preserving character identity.

First assign the existing image's visible pixels to named ownership masks. Keep all parts on the same source canvas and store masks, pivots, parents, and draw order in one manifest. For a defective joint, regenerate the affected component and compare it with the visible source pixels in the assembled pose.

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

Use a manifest in native Godot coordinates: pixels, +X right, +Y down, positive clockwise rotation in radians. Record each part's PNG, parent joint, source-pixel pivot, attachment scale, local position, and absolute draw order. Specify canvas size and ground anchor.

Generated parts are redrawn approximations, not lossless segmentation. SIFT/RANSAC may suggest placements for unchanged textured regions, but cannot guarantee exact transforms, infer hidden joints, or establish occlusion reliably. Manual reviewed anchors and draw order are the default. Record any drift from the original as unresolved rather than calling it an exact reconstruction.

Check semantic ownership at every boundary: coat fur on a leg creates a moving white shard; straw on a coat leaves an orange streak when the weapon moves. A pixel-perfect setup reconstruction alone cannot detect either error. Alpha/color-assisted masks may help a reviewed source-specific extraction, but are not a general semantic segmenter.

Draw by tree order, not `z_index`. Godot draws canvas items in tree order within a canvas layer, and any positive `z_index` (relative or absolute) lifts parts above sibling game UI such as health bars. Keep every part at z 0 and order the children; a weapon parented under bones draws at its `Skeleton2D`'s slot. Do not use `top_level` for a released prop: it leaves the parent's modulate/visibility chain, so game fades miss it; express the target in the bone's local space instead. Parent the weapon to a grip joint under the gripping forearm; confirm the grip through the complete animation range.

## 4. Build a native Godot rig

Create a `Node2D` scene root and a `Sprite2D` compatibility child named `Visuals` where the real integration needs it. Keep `Visuals.visible = true` and its modulation opaque: hiding/modulating the parent also affects descendants. An empty texture is sufficient for the container. Attach an ordinary `Node2D` joint hierarchy and one `Sprite2D` per part. Use `Skeleton2D`/`Bone2D` only when their actual features are needed.

Build hip → torso → head/arms, hip → legs/coat, forearm → grip → weapon. Put source-pixel pivot offsets on the sprite, separate from the joint's parent-local transform. Use unit animation scale; rotate/translate individual joints instead of squashing the complete standing image.

Use a planted foot target and two-bone solve for supported poses, then bake its result. Release the foot for intentional stepping/falling. For continuous seams use local `Polygon2D`/`Skeleton2D` weights only across anatomically connected parts. Do not infer bone connectivity from 2D proximity: a casting forearm next to the coat must not pull that coat. Restrict triangles to each texture's occupied region; a full-canvas grid duplicated for every layer wastes work. Retain topology and weights between timing edits.

Separate rigid metal/weapon shapes, flexible cloth, and pose-specific attachments. A prone drawing may solve a final silhouette but does not solve the transition into it; inspect the frames immediately before and after an attachment change. Avoid hiding a pop with a long crossfade or declaring a good final still a finished death animation.

Write an `AnimationLibrary` and `AnimationPlayer`. Author setup/reset values for every animated property; include those properties in each clip so switching from hit/death cannot leave stale transforms. `idle` loops with restrained upper-body breathing and planted feet. `attack` uses anticipation, a readable swing, and recovery. `hit` recoils and returns. `die` settles into a nonlooping final pose; revival/reset is explicit. Use Godot interpolation deliberately, and distinguish a linear first draft from a finished eased animation.

Let the native player own clip state and time. A script that calls `seek()` every frame can display correct previews while overriding external `Play()` calls. Bake joint motion and key any attachment-controller state/time on the same player; reserve `seek()` for scrubbing. Observe automatic return with `advance()` and the real engine clock, since seeking to a clip's end does not exercise completion. Repeated `play()` of the already-playing clip does not establish a fresh time-zero sample: stop/reset or observe accumulated time explicitly.

Resolve adjacent resources relative to the script directory and reimport after relocation. Follow the [runtime contract](/docs/research/sts2-cutout-runtime-contract.md) for retained-child controller placement, alias mapping, return-to-idle transitions, and explicit `revive` / mid-fade interruption checks.

Check prop clearance numerically: transform the rigid weapon's alpha pixels by FK at every key and at interpolation quarter-points and compare with the source floor. When it fails, redesign the grip path first (lift during wind-up and recovery); a per-key angle correction that spreads into the anticipation or the event pose is a regression. Put expressions and VFX (texture swaps, trail/sigil alpha and scale) on the same player as the joints, and key their defaults at t=0 of every clip so an interrupted clip never leaves a stale face or effect.

Resolve effect time separately from clip length. Observed official characters default to `AttackAnimDelay = 0.15`, while custom attacks and casts override delays to match their actual contact or release frame; check the character's `animation-api.md` (such as Realta's [model and clip timings](/docs/characters/realta/animation-api.md)) whenever a motion changes. Offset secondary attachments (headwear, ribbons, coats) to settle after the torso. Keep combat, merchant, rest (including Act lighting variants), and selection compositions distinct.

Native Godot data requires no Spine conversion. Only if importing a verified Spine schema, convert Y and rotation signs once, respecting setup-relative translation/rotation, multiplicative scale, attachment transforms, draw-order timelines and curve semantics. Record the schema/version and supported features. Negating all numbers is not a complete Spine importer; unsupported meshes, constraints or timelines must be reported.

## 5. Verify and report

Use the [motion review matrix](cutout-motion-review.md) for each affected action and record evidence for every applicable row. For Realta, `dev.ps1 anim` compiles the registered rig-c inputs, applies the native resources, imports and checks the game scene, then captures both variants when requested. Its package mode reads the PCK independently. [Animation procedure](/docs/characters/realta/animation/reproduction.md)

Serialize scalar rotation key values as floating-point literals, including `0.0`. Sample actual joint properties during playback; integer-valued keys can round small rotations to zero even when clip names and durations load correctly.

Load and instantiate the candidate with the repository's Godot version. Exercise every clip and inspect setup, overlap, grip, planted feet, clipping and silhouette on light/dark backgrounds at 240×360. Save an animated preview and the original comparison. Treat exploratory scripts and rendered observations as informal verification, not new acceptance tests; preserve all protected tests and validators.

Use actual Godot renders for final comparisons. The Python preview is useful during extraction but is not evidence of engine playback. Compare with official resources for the same scene, using a fixed camera throughout each clip; include normal-speed sequences, event-time poses and attachment-transition neighbors. Record the active installation version and renderer for each capture.

At every verification iteration, open a relevant external primary source and connect its technique to the current defect. Keep the source, tested approach, edits, renders, timing, and remaining defects in the task's working records. Prefer one shared source-coordinate manifest, local regeneration, and topology reuse. Use the [shared documentation rules](/AGENTS.md#shared-documentation) when recording an applied decision.

For runtime promotion, preserve markers/bounds and the verified framework dispatch, run the repository build/import/PCK workflow, and verify resources instantiate. Run gameplay checks if available. A successful C# build alone verifies neither imported textures nor event playback. For standalone draft-only work, compile/package the candidate only as necessary; installation is not implied.

Prefer baking the authored rig into a `.tscn` (bones, skinned polygons, AnimationPlayer with aliases, `next` and `autoplay`) and keeping only a small controller script for state a value track cannot express. Trim fully transparent margins from part textures and record their source-canvas offsets in one manifest; downscale VFX painted far larger than displayed. Realta's implementation is the reference: [animation-sources.md](/docs/characters/realta/animation-sources.md).

Raw JSON loaded by `FileAccess` needs explicit non-resource export inclusion. Dynamically constructed texture/script paths also need inclusion when exporting selected resources. Build an isolated candidate PCK and mount it in a fresh host project without the source project's import cache; render the loaded candidate and exercise the actual clip names. This demonstrates packaged resource loading, not BaseLib factory conversion or gameplay. Do not promote unused experiments, captures, or official research artwork into the package.

Report saved artifacts, prompt location, rig hierarchy, parts/Z order, clips, observed visual defects, actual checks run, and unverified game integration. Human approval of visual quality remains separate from successful loading.

## Primary engine references

- [Godot 4.5 cutout animation](https://docs.godotengine.org/en/4.5/tutorials/animation/cutout_animation.html): hierarchy and pivots.
- [Godot 4.5 AnimationPlayer](https://docs.godotengine.org/en/4.5/classes/class_animationplayer.html): native clip playback.
- [Godot 4.5 CanvasItem](https://docs.godotengine.org/en/4.5/classes/class_canvasitem.html): inherited visibility/modulation and Z ordering.
- [Esoteric asset preparation](https://eu.esotericsoftware.com/blog/How-to-cut-your-assets-for-animation): hidden surfaces, joint overlap and motion-driven decomposition.
- [Spine mesh attachments](https://esotericsoftware.com/spine-meshes): topology, weights, vertex economy and destructive Trace/Generate caveats.
- [Godot 4.5 2D skeletons](https://docs.godotengine.org/en/4.5/tutorials/animation/2d_skeletons.html): weighted geometry and internal vertices; verify the version-marked tutorial in the actual engine.
- [Official animation comparison](/docs/research/sts2-official-animation-comparison.md): local resource evidence, scene differences and external workflow research.
