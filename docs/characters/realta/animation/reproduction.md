# Inspect saved Realta Godot animations

Edit the shared base scene, common Animation resources and variant-specific settings using [the Godot editing guide](godot-editing.md), then save before running these commands. Saved native `.tscn` and `.tres` files are the only editing source and packaging input. The existing combat/merchant paths remain inherited game entry scenes. All executable tools are tracked: `dev.ps1`, `tools/render_clip.gd`, `tools/review_frames.py`, and checks under `tests/`. Every directory under `artifacts/` below is created by the command or explicitly selected as an optional comparison input.

## Migration observations (2026-10-22)

MegaDot 4.5.1.m.14 with .NET SDK 9.0.318 loaded both native rigs. `dev.ps1 check` passed the C# build, resource import, 42,193 native mechanical samples and four instance-isolation cases. Existing tests and acceptance data were neither added nor changed. Normalizing only the presentation-property renames and removed external-authoring flag made both scene texts identical to their pre-migration versions.

At 24 fps, 600 of 792 before/after frames matched directly. Merchant relaxed-loop capture can retain an already deferred random autoplay seek even after disconnecting the start signal. A temporary observation disabled phase initialization before entering the tree on both versions; all 192 remaining RGBA frames then matched exactly. For a deterministic comparison, suppress random loop entry before tree insertion, not after `_ready`. This observation did not change the tracked renderer or product motion.

A temporary headless editor observation confirmed `Engine.is_editor_hint() == true`, loaded both rigs and scrubbed all seven canonical clips at 0, .15, .3 and .7 seconds (clamped to duration), without initializing random phase. Both scene paths also opened through the native headless editor command. This is script-level editor evidence; interactive mouse editing, save/reopen and actual game playback were not exercised. The sandbox reported certificate-store warnings; the temporary editor process also reported shutdown RID leaks. These are distinct from script parse/load failures.

The first `publish` failed with the sandbox certificate-store error. Following [the build diagnostic procedure](../../../agents/build-troubleshooting.md), one escalated retry passed and produced the package. `dev.ps1 anim attack -Variant both -PackagePath artifacts/mods/Sts2WD/Sts2WD.pck` then mounted that package in a fresh host and rendered both variants successfully. This confirms packaged resource loading; it does not establish actual in-game dispatch. NuGet vulnerability metadata remained unavailable (NU1900 warning); restore/build and the unchanged checks completed. No game installation was performed.

## Native ownership cleanup observations (2026-10-22)

Following the [official-workflow comparison](../../../research/godot-native-animation-workflow.md), static polygon/UV/triangle/weight data and invariant textures were moved to node defaults; redundant animation copies and resolved visibility/order arrays were removed (749 tracks per scene). The five existing semantic composition properties now own body/cel visibility, free-arm order, weapon-sleeve order and held/released glove order. No motion keys were resampled or simplified.

Both variants passed temporary save/reload observations after editing a head polygon point, UV and bone weight. All seven canonical clips retained those edited values while seeking forward/backward at 0, .7, .15 and 0 seconds, in runtime and headless editor modes. Separate semantic-state changes kept the released glove behind the weapon and the final death cel visible. No interactive mouse operation or actual game playback was exercised.

All 792 before/after RGBA frames at 24 fps matched after aligning merchant loop phase. Temporary side-by-side playback observations compared visibility, modulation, transforms and root draw order across 28 transitions (two variants, two outgoing loops, seven incoming clips), each sampled for 121 steps at 240 Hz: 3,388 snapshots matched. These observations supplement the unchanged mechanical/isolation checks; they do not add acceptance cases or replace visual review.

The tracked renderer now clears autoplay and selects manual processing on the observation instance before tree insertion, then disconnects the runtime phase callback. Before the requested clip, it explicitly establishes the variant's idle at time zero so incoming arm holds start from the normal outgoing pose. This prevents random autoplay seeks and startup scheduling from contaminating capture time without modifying saved scenes or reaching into private controller state. Two repeated merchant-loop captures matched all 192 frames and the fixed-phase reference. Earlier migration observations above describe the defect before this capture fix.

The user subsequently explicitly requested RESET and the corresponding exact-name check change on 2026-10-22. Each variant now contains a 0.001-second nonlooping RESET with time-zero defaults for its 256 animated properties, plus `reset_on_save=true`. Static polygon/UV/triangle/weight values are excluded so mesh edits persist. The name assertion permits exactly the original ten dispatch names plus RESET, with the requester/context recorded in the test. Existing library/default/alias/transition checks still run; no other acceptance conditions were relaxed.

A temporary observation waited for editor initialization, opened fresh copied scenes, edited a head polygon point, previewed die at 1.3 seconds and called the native `EditorInterface.save_scene()`. Both saved scenes reloaded with RESET's hip/default body mode while retaining the polygon edit. Initial exploratory runs raced editor tab restoration; the corrected observation verified the active scene and avoided yielding between preview and save. Headless thumbnail generation reported a dummy-texture warning and editor shutdown reported RID leaks; scene saves returned OK and their serialized values were verified. Mouse interaction and actual gameplay remain unverified.

Final `check`/`publish` passed with RESET included: 42,193 mechanical samples and four instance-isolation cases, followed by PCK export. The final PCK was mounted in a fresh host and all seven clips of both variants were captured. All 792 RGBA frames matched the pre-cleanup motion at aligned loop phase. NuGet vulnerability metadata was unavailable (NU1900); compilation and package export completed. No game installation was performed.

## Shared rig ownership observations (2026-10-22)

The two game entry scenes now inherit `realta_rig_base.tscn`. It owns the common hierarchy, static geometry, bindings, textures and local-to-scene Slash material. Six common clips are single external Animation resources referenced by both `combat_library.tres` and `merchant_library.tres`. Only RESET/revive resources, initial pose differences and dispatch settings remain variant-specific. Combat is a five-line inherited entry; merchant contains property overrides rather than copied meshes or Animation data. `dev.ps1 editor -Variant both` opens the common base; individual variants open their inherited preview scene. The native editor command accepted only its first scene argument, so the launcher passes exactly one path rather than claiming to open several tabs.

At the start of this correction, the combat file used Godot 4.7's `libraries/` serialization, which the configured MegaDot 4.5 could not instantiate as a library. It also stored manual processing. Before splitting ownership, the same saved data was read with the compatible `libraries` dictionary and normal idle processing. These compatibility adjustments changed neither keys nor mesh data; this normalized scene supplied the comparison baseline. Do not introduce an automatic converter into `check` or rewrite acceptance criteria to mask incompatible resources.

Temporary observations confirmed that editing one base head point flowed into both inherited scenes and that both players referenced the same attack Animation object and observed the same key edit. RESET and revive remained different objects across variants. Both variants advanced under normal process frames with idle callback mode. Native editor save/reload of fresh temporary copies retained inheritance, the external common clip path and geometry. Thumbnail/dummy-renderer and editor shutdown warnings were logged separately from successful save results.

The unchanged mechanical/isolation checks passed (42,193 samples and four isolation cases), followed by successful PCK export. The 792 source renders, all 792 renders of the final PCK mounted in a fresh host, and 3,388 transition snapshots matched the normalized pre-split scenes. Texture documentation now identifies all four head/expression paintings as 196×140 at `(134,93.99999)`, using `parts.json` and the common base's UVs as current registration evidence. No artwork or test acceptance data was changed in this correction. Actual gameplay and mouse-driven editing were not exercised; no game installation was performed.

## Setup

Complete the root [development setup](../../../../README.md#새-pc에서-준비하기): .NET 9, MegaDot 4.5.1.m.14, installed game assemblies and the locked BaseLib dependency. `dev.ps1 doctor` resolves project paths; no script contains a personal checkout path. Review sheets also require Python with Pillow. The verified environment is Python 3.12.4 and Pillow 11.3.0; install `Pillow==11.3.0` with that interpreter's `python -m pip install` if needed.

Run from the repository root in PowerShell. Command examples use a process-scoped execution-policy option, preserving machine settings.

## Iterate through one entry point

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 anim attack -Variant both -OutputDirectory artifacts/attack-before
```

The source inspection workflow performs import → mechanical and isolation checks → native game render → GIF/review sheets. It never writes editable scenes or PNG parts. Edit bones, meshes and keys with the native Godot 2D and Animation panels.

`-Clip` accepts all ten dispatch names and `all` (seven distinct clips). `-Variant` accepts `combat` (default), `merchant`, or `both`. The second positional argument is the clip, so `anim attack` and `anim -Clip attack` are equivalent. Omitting `-OutputDirectory` creates a unique `artifacts/animation-*` directory. An explicit output directory must be new; the command never clears a previous run.

After an edit, compare at identical sample times:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 anim attack -Variant both -OutputDirectory artifacts/attack-after -Baseline artifacts/attack-before/frames
```

The baseline is optional and is produced by the first command; there is no dependency on a private capture. Baseline/current metadata must agree on variant, clip, FPS, camera, viewport and renderer. Changed clip lengths show missing frame positions explicitly.

Each run contains:

- Per-stage `.stdout.log`, `.stderr.log`, `.exitcode` and engine logs. Native process results are recorded before decoding output as UTF-8; Python runs with `-X utf8 -B`.
- `frames/<variant>/<clip>/0000.png` onward, with `capture.json` describing the engine, camera, timing and frame count.
- `review/<variant>/<clip>/playback.gif` and numbered `dark-*.png` / `light-*.png` sheets. Sheets include every captured frame at native resolution, with before/after pairs when requested. Whole frames are retained without cropping.

The fixed viewport is 640×420, with source origin at (120,32), scale .625, transparent background, and GL compatibility rendering. The helper window runs hidden/offscreen. Frame n is n/24 seconds, ending before the clip endpoint. The seven current clip lengths produce 396 frames per variant; this describes the sampling recipe, not an acceptance threshold. Frame capture requires a renderer; the numerical tests use headless mode.

Review at normal speed and by frame using [the motion review matrix](../../../agents/cutout-motion-review.md). The isolated capture stops autoplay, drains its deferred random seek, disconnects only phase selection and plays with zero blend; the controller's mixer handling remains active. For revive it starts from the final death pose. This isolates authored frames and does not demonstrate live transitions.

## Mechanical checks and publishing

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 check
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 publish
```

`check` builds C#, imports resources and runs [the mechanical rig checks](../../../../tests/rig-checks.md) and [instance isolation test](../../../../tests/README.md). Require exit 0 and both PASS summaries. `publish` runs the same checks after import and before exporting the PCK; failures stop export. No inspection or packaging command reauthors saved scenes. Existing pre-commit behavior remains C# build-only. Test scripts are excluded from game resource import/export by `tests/.gdignore` and remain explicitly executable with `--script`.

For native startup crashes, certificate-store errors, access denial, or separate log-display failures, follow [Build execution and diagnostics](../../../agents/build-troubleshooting.md). It owns the shared handling procedure; certificate settings stay unchanged.

## Inspect only the published package

After a successful publish:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 anim all -Variant both -PackagePath artifacts/mods/Sts2WD/Sts2WD.pck
```

Package mode creates a fresh host with only `project.godot` (`config_version=5`) and a copy of the tracked renderer. The renderer mounts the specified PCK before resolving scenes. No source `Sts2WD/` directory or source import cache exists in that host. This mode renders and creates review sheets without reauthoring or rebaking source inputs. It accepts the same optional baseline and fresh output-directory arguments. It demonstrates packaged resource loading, not BaseLib's root conversion, combat dispatch or multiplayer playback.

## Observe live transitions separately

For a temporary live observation, instantiate the runtime rig, retain `animation_started` and `mixer_applied` connections, and set the AnimationPlayer processing mode to manual. Use `play(target)` with no blend override. After advancing, allow a process frame so the controller records the displayed loop phase.

- Start `relaxed_loop` (also `relaxed`, and merchant `idle`), allow initial deferred phase selection, then seek to source phase 0, 3.375 or 6.25. Play `idle_loop`, `attack`, `hurt`, `cast`/`powerup` or `die`; advance in 1/60 s steps through the transition. Read actual `upper_arm_r`, `forearm_r`, `coat`, `coat-front`, `hand_l` and `Skeleton2D` sibling indices alongside native overlap images.
- Let actions and revive complete through `advance` and observe the variant's default loop. Let die pass its endpoint and observe the held pose. Seeking alone does not exercise completion.
- Observe die → revive, including the invisible reset at .20 s. Read `lower_mode`, `weapon_gripped`, `free_arm_back`, body/cel visibility and rig alpha alongside the image.
- While a loop plays, allow a process frame, record `current_animation_position`, call `stop()` then `play()` with the same name, and allow the deferred callback. Repeat with aliases sharing the same resource. Observe saved phase restoration separately from ordinary resource changes.

These transition observations supplement the explicitly requested automated checks. Record inputs, actual behavior and remaining uncertainty. Keep dated verification reports in [iteration notes](iteration-notes.md), and current ownership/timing rules in [README.md](README.md). For official comparisons, identify the installation/version/resources using [the animation study](../../../research/sts2-official-animation-comparison.md); prior local captures are optional evidence.
