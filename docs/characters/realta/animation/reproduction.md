# Rebuild and inspect the Realta combat and merchant rigs

Begin at the repository root with the [development environment](/README.md#새-pc에서-준비하기) and the [local rig-c toolchain](/docs/agents/rig-c-local.md). The source build uses PowerShell, the repository's locked rig-c and Bun profile, Python with NumPy, OpenCV and Pillow, and the configured MegaDot executable. These inputs drive both runtime variants. [Authoring files](/docs/characters/realta/animation-sources.md#inputs-and-ownership)

## Author and apply the rig

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 anim attack -Variant both -OutputDirectory artifacts/attack-before
```

Source mode performs these steps in order.

1. [build.ps1](rigc/build.ps1) unpacks the atlas parts (applying any explicit `-SeedParts <ignored-dir>` PNG overrides), compiles all seven motions into `realta_skeleton.png`, and bakes the Godot skeleton and animation library (see [inputs and ownership](/docs/characters/realta/animation-sources.md#inputs-and-ownership)). `dev.ps1 anim` immediately gates the compiled rig-c output with `rigc render --folds` at 24 fps for the requested clip(s) (adding `--preroll 1` on looping clips when physics constraints exist), stopping on any mesh fold before tracked runtime assets change.
2. [The game importer](/scripts/realta/import_game_rig.py) binds the combat and merchant variants to the shared skeleton and syncs [atlas.txt](rigc/atlas.txt), then Godot imports the updated tracked resources before checks run.
3. `anim` runs the existing [rig checks](/tests/rig-checks.md) and [instance isolation check](/tests/README.md). A failure in either the pre-import `rigc render --folds` mesh-fold gate or these post-import Godot checks stops source-mode capture. Read the failed condition alongside the current [clip and part contract](/docs/characters/realta/animation-sources.md). Report a specification conflict with the exact result through the [test-ownership procedure](/docs/agents/test-ownership.md).
4. The renderer captures the requested clip, followed by native-frame sheets and a playback GIF for each selected variant.

Edit the owning authoring file listed in [inputs and ownership](/docs/characters/realta/animation-sources.md#inputs-and-ownership). To seed new or replaced part PNGs into `realta_skeleton.png` while keeping the tracked `rigc/` tree free of loose PNGs, pass `-SeedParts <ignored-dir>` to `dev.ps1 anim` or [build.ps1](rigc/build.ps1); [build.ps1](rigc/build.ps1) fails fast if a stray `parts` folder containing PNGs exists under `rigc/`. For a compile that writes only to a separate directory, call [build.ps1](rigc/build.ps1) from the repository root with `-Output` pointing to a fresh output directory (and optional `-SeedParts <ignored-dir>`).

## Compare native playback

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 anim attack -Variant both -OutputDirectory artifacts/attack-after -Baseline artifacts/attack-before/frames
```

`-Clip` accepts the seven names in the [motion table](/docs/characters/realta/animation-sources.md#clips-and-dispatch), the three BaseLib aliases and `all`. `-Variant` accepts `combat`, `merchant` or `both`. An explicit output directory must be fresh. The optional baseline must use the same variant, clip, frame rate, camera and renderer for a meaningful comparison.

Read each run's `capture.json` for the actual viewport, positions, display scale and frame count. The GL compatibility renderer samples at 24 fps and retains the entire frame. The [review tool](tools/review_frames.py) produces dark and light sheets and optional registered before/after pairs. To check an exact event between display samples, compare the neighboring frames and inspect the native player at the cue time. [Motion review matrix](/docs/agents/cutout-motion-review.md)

## Publish and render the PCK

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 publish
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 anim all -Variant both -PackagePath artifacts/mods/Sts2WD/Sts2WD.pck
```

Package mode copies the tracked renderer into a fresh Godot host and mounts the specified PCK before resolving either rig. This directly exercises textures, animation resources and variant names present in the package. Source-mode rig checks, mounting the PCK, running BaseLib scene conversion, and observing a game encounter each provide different evidence. For a local installation use the [deployment procedure](/README.md) and record the verified scope in the [shared deployment history](/docs/deployment-status.md).

## Live transitions

Play clips through the `AnimationPlayer` under each game wrapper and let them advance past the endpoint. For combat, check returns to the standing loop. For the merchant, check the crouching loop through autoplay, `idle`, `relaxed` and return from action. Check attack contact at 0.54 s, cast release at 0.25 s and right-hand release at 0.12 s of `die`. Advance the full death animation, then trigger `revive` and observe both sides of its hidden pose change at 0.20 s. Test interruption by invoking the next game alias while the preceding action is still active. Use the native callbacks exercised by the [BaseLib integration](/docs/characters/realta/animation-api.md).
