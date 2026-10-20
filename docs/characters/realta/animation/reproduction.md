# Rebuild and inspect Realta animations

Use this workflow when changing combat/merchant authoring inputs or reviewing generated scenes. All executable tools are tracked: `dev.ps1`, the authoring/baking sources in this directory, `tools/render_clip.gd`, `tools/review_frames.py`, and the checks under `tests/`. Runtime PNGs and coordinate manifests are the production inputs. Every directory under `artifacts/` below is created by the command or explicitly selected as an optional comparison input.

## Setup

Complete the root [development setup](../../../../README.md#새-pc에서-준비하기): .NET 9, MegaDot 4.5.1.m.14, installed game assemblies and the locked BaseLib dependency. `dev.ps1 doctor` resolves project paths; no script contains a personal checkout path. Authoring and review sheets also require Python with NumPy and Pillow. The verified environment is Python 3.12.4, NumPy 2.3.1 and Pillow 11.3.0. For a matching Python environment, install `numpy==2.3.1` and `Pillow==11.3.0` with that interpreter's `python -m pip install`.

Run from the repository root in PowerShell. Command examples use a process-scoped execution-policy option, preserving machine settings.

## Iterate through one entry point

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 anim attack -Variant both -OutputDirectory artifacts/attack-before
```

The source workflow performs author → import → bake → mechanical and isolation checks → native render → GIF/review sheets. Authoring writes `rig.json` and the descriptive `authoring-report.json`; baking writes both runtime scenes. Inspect those diffs after an input change. Runtime PNGs, manifests, meshes and the frozen `rig-base.json` are inputs, not destinations of draft conversion.

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

`check` builds C#, imports resources and runs [the mechanical rig checks](../../../../tests/rig-checks.md) and [instance isolation test](../../../../tests/README.md). Require exit 0 and both PASS summaries. `publish` runs the same checks after import and before exporting the PCK; failures stop export. Existing pre-commit behavior remains C# build-only. Test scripts are excluded from game resource import/export by `tests/.gdignore` and remain explicitly executable with `--script`.

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
