# Local Rig-C Toolchain

## Windows Invocation

Run from the repository root. The local wrapper is off PATH.

```powershell
$rig = Join-Path '.tools' 'rig-c/setup/run.ps1'
& $rig rig-review <dir> --view-only
& $rig rig-review open <dir> --view-only --direct
```

Use the first form for a single foreground review and the second for a direct background worker. Either accepts `--animation <name>`. Managed `rig-review open` depends on POSIX owner-only mode bits, which Windows NTFS does not provide to `open-workspace-state.ts`.

- Quote comma-separated native arguments, including `--show "art,bones,names"` and `--seam "a:b,c:d"`.
- Pipe multiline Python through a single-quoted PowerShell here-string. Put the closing marker at the beginning of its line.

  ```powershell
  @'
  print("hello")
  '@ | python -
  ```

- Use `Out-File -Encoding utf8` for UTF-8 logs. Windows PowerShell 5.1 writes UTF-16LE with `>`.
- Prefer `rigc guide <RULE_CODE>`. For a section lookup in a PowerShell 5.1 script, form the argument with `([string][char]0x00A7 + '4.4')` so the section sign survives script decoding.

## Source Preflight and `@rig-c/parts` Parting

Before parting a flat PNG, run `python scripts/inspect_rigc_source.py <source.png> --opaque-alpha <cutoff>` with the same 8-bit-scale cutoff the planned source comparison uses. The tool reports the alpha distribution and the comparison pixel count, then exits nonzero if that count is zero. Use the actual distribution to choose the cutoff for that painting. Pillow and NumPy are required. For 16-bit PNGs also install OpenCV.

Preserve visible source pixels through deterministic layer decomposition and `@rig-c/parts` (`& $rig rig-parts ...`) rather than redrawing whole parts from scratch with an image generator; use image generation only to inpaint occluded joint ends, backings, and alternate hand/expression states. Per [ADR-0008](/docs/adr/0008-native-spine-rigs-and-artifact-refresh.md), `rig-c` (`4.3` assertions and 24 fps `--folds` verification) serves as the authoring gate for native `SpineSprite` (`4.2` `.spjson` + `.spatlas` + `SpineSkeletonDataResource`) targets across `combat`, `merchant`, and `char_select_bg`, and any update to the local `rig-c` packages requires syncing `.agents/skills/` via `rigc skills install --copy` and rebuilding tracked rig outputs in the repository.

## Loose Parts and First Motion

When parting is ready, set `$package` to the directory containing the loose `parts` directory and run this before compiling. This stage needs Python, Pillow, NumPy and SciPy.

```powershell
python scripts/rigc_package_checks.py $package --parts-only
```

Read the secondary 4-connected component warnings at alpha ≥ 128. Correct stray material at joint cuts and identify any intentional islands. Multiple components can make joint contact unavailable. The existing check still requires transparent black through alpha 8. Install `opencv-python-headless` when scanning 16-bit alpha-bearing PNGs.

Before authoring the full clip set, write a short `joint_probe` clip in a scratch motion spec. Start at setup pose, then raise an arm, bend a knee, and move the prop clear of the coat wherever the rig has those parts or overlaps. Use the intended motion range and choose times that land on the render's sample grid. Build and render into ignored directories.

```powershell
& $rig rigc build --rig $rigSpec --motion $probeMotion --images (Join-Path $package 'parts') --out $probeBuild --pack --profile rig-play
& $rig rigc render --candidate $probeBuild --animation joint_probe --fps 12 --max 1024 --folds --seam $jointPairs --out $probeRender
```

Use actual touching slot names in `$jointPairs`. Inspect the stressed frames and the affected slots for folds, joint gaps and foreign material exposed from the covered surfaces. Compare the same poses after a correction. See the [cutout motion review matrix](cutout-motion-review.md#review-matrix) for what to inspect. Use the full package check when the delivery clips are ready.

## Render and Verify

- Pass `--candidate <build-dir>` and one `--animation <name>` per `rigc render` invocation. Use a fresh `--out <dir>` for each clip to preserve its `frames.json`.
- Use `--preroll 1` for loops with physics. It wraps the track at `t == duration` to the pose at `t == 0`. Leave preroll off one-shots to retain their final pose.
- Run `./scripts/check-rigc-package.ps1 -PackageDir <dir>` with Python, Pillow, NumPy and SciPy available. The package needs `review.json`, a compiled `build` and loose PNGs in `parts`. The command repeats the loose-parts preflight on the delivery inputs.
- The script renders each clip at 12 fps with a 1024 px long edge. It applies preroll to `*_loop` clips when the compiled skeleton declares a physics constraint. It prints the total folded mesh count and the absolute path to `peak-frames.png` in the ignored output directory. The sheet uses full-size checkpoint and diagnostic frames. Without a timed checkpoint, it picks the frame that differs most from the first pose.
- Add `-Seam "slot_a:slot_b,slot_c:slot_d"` for pairs expected to remain joined. It fails on native `jointContact` separation. The final summary counts separated, joined and visual-inspection frames separately for each pair across all clips. Missing or unavailable contact samples count as visual inspection. The sheet includes the first such frame per pair and clip, along with visible gap and ghost peaks. Use the full render for any additional frames you need to inspect.
- Add `-MaterialLeak @('part.png=mask.png')` to reject visible part pixels in forbidden material regions. Use a same-size grayscale mask with black for allowed pixels and non-black for forbidden pixels. Part paths are relative to the loose-parts directory.

Report joined, separated and unavailable contact counts together. Identify any seam with zero automatic readings and the clip, time and result of the manual inspection performed for it. Report mesh folds separately from contact coverage.

### Official Spine Parser

Use the locked parser profile when a check needs a Spine runtime round trip.

```powershell
& $rig install-spine
& $rig rigc validate <build-dir> --profile rig-play
```

The tooling lock pins the official parser. Check for `A00_ROUNDTRIP_PARSE` in the result. The regular `install` profile uses rigc's built-in checks and omits the parser.

## Offline Preview

`rig-review check` validates the package files. Confirm playback in the browser before reporting a live review as available. The standalone HTML from `rigc preview` embeds the art and requires network access to fetch Spine Player on first open.

For offline handoff or a browser connection timeout, make a WebP from the verified render frames. Set `$renderRoot` to the `Rig diagnostics:` directory printed by the package check. Pass a single `frames.json` to export one render instead.

```powershell
python scripts/render_rigc_preview.py $renderRoot --out (Join-Path $renderRoot 'preview.webp')
```

The [WebP exporter](/scripts/render_rigc_preview.py) uses Pillow and the recorded frame rates. It preserves the full frame and scales the long edge to at most 720 pixels by default. If capture sizes differ, export each render separately. Keep the preview with the task artifacts.

## Authoring Reference

- **Bone inheritance**. With `inherit: "noRotationOrReflection"`, author the desired world-facing rotation directly. Use `rotation: 0.0` for a flat boot and omit parent-angle subtraction.
- **Segment weighting**. Repeat `{"bone": ..., "from": ..., "to": ...}` for one bone to cover several segments. Each vertex uses the nearest segment for that bone. Routing the front hat brim through several segments on the hat base keeps that brim attached to the back of the hat.
- **Alpha clipping**. Supply all six `alphaClip` fields (`skin`, `slot`, `attachment`, `alpha`, `tolerance`, `maxVertices`) and one alpha island without holes.
- **Draw order**. Offsets are evaluated in ascending setup-slot index order. Compute each offset from the setup order.
- **Review manifest**. Required keys are `schema: "rig-review/1"`, `id`, `title` and string `revision`. Set `model` to the compiled `.model.json` for playback. Declare IDs in `animations` and `references` before using them in `checkpoints`. File paths are literal package-relative paths with forward slashes and no `..`, encoding, query, or fragment. Omit unused `axes` and `producer`.
