# Rebuild and inspect the Realta Spine rigs

Begin at the repository root with the [development environment](/README.md#새-pc에서-준비하기) and the [local rig-c toolchain](/docs/agents/rig-c-local.md). The source build uses PowerShell, the repository's locked `rig-c` and Bun profile, Python with NumPy, OpenCV and Pillow, the configured MegaDot executable (for resource import), and `<Sts2Path>/SlayTheSpire2.exe` (for `template_release` `libspine_godot` `SpineSprite` checks and frame capture). [Authoring files](/docs/characters/realta/animation-sources.md#inputs-and-ownership)

## Author and apply the rigs

```powershell
powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\dev.ps1 anim all -Variant both
```

Source mode performs these steps in order:

1. [build.ps1](rigc/build.ps1) unpacks the combat/merchant parts from [atlas.txt](rigc/atlas.txt) + [realta_skeleton.png](/Sts2WD/animations/realta/combat/realta_skeleton.png) (applying any explicit `-SeedParts <ignored-dir>` PNG overrides) and compiles the eight combat/merchant motions, then unpacks [select_atlas.txt](rigc/select_atlas.txt) + [realta_select_atlas.png](/Sts2WD/animations/realta/select/realta_select_atlas.png) and compiles the character-select overlay rig.
2. `dev.ps1 anim` gates the compiled `rig-c` output with `rigc render --folds` at 24 fps for every requested clip (including `select` when `-Clip all` is used, adding `--preroll 1` on looping clips when physics constraints exist), stopping before tracked runtime assets change if any triangle folds.
3. [import_game_rig.py](/scripts/realta/import_game_rig.py) invokes [export_spine42.py](/scripts/export_spine42.py) to emit Spine `4.2.43` `.spjson`, `.spatlas`, and 4px-aligned `.png` files into `Sts2WD/animations/realta/combat/` and `Sts2WD/animations/realta/select/`, syncing [atlas.txt](rigc/atlas.txt) and [select_atlas.txt](rigc/select_atlas.txt). MegaDot then imports the updated resources.
4. `anim` runs [check_rig.gd](/tests/rig-checks.md) and [realta_instance_isolation.gd](/tests/README.md) on `<Sts2Path>/SlayTheSpire2.exe` via `Invoke-WithLocalSpineExtension` (which writes a temporary git-ignored `<project>/local_spine.gdextension` pointing to `<Sts2Path>/libspine_godot.windows.template_release.x86_64.dll` and removes it on exit). Report any specification conflict with exact evidence under [test ownership](/docs/agents/test-ownership.md).
5. [render_clip.gd](tools/render_clip.gd) steps `SpineSprite` deterministically (`UpdateMode_Manual`) at 24 fps across the `640×420` viewport, followed by [review_frames.py](tools/review_frames.py) dark/light contact sheets and playback GIFs.

To seed new or replaced part PNGs into `realta_skeleton.png` while keeping tracked `rigc/` free of loose PNGs, pass `-SeedParts <ignored-dir>` to `dev.ps1 anim` or [build.ps1](rigc/build.ps1). When adding a brand-new atlas PNG for the first time, run `.\dev.ps1 import` → `python -X utf8 scripts/check_install_package.py --sync-params` → `.\dev.ps1 import` so its `.import` sidecar acquires Tier 1 BPTC (`path.bptc`) metadata.

## Compare native playback

```powershell
powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\dev.ps1 anim attack -Variant both -OutputDirectory artifacts/attack-before
powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\dev.ps1 anim attack -Variant both -OutputDirectory artifacts/attack-after -Baseline artifacts/attack-before/frames
```

`-Clip` accepts `idle_loop`, `blink_loop`, `attack`, `guard`, `hurt`, `cast`, `die`, `relaxed_loop`, the aliases `idle`, `relaxed`, `powerup`, and `all`. `-Variant` accepts `combat`, `merchant`, or `both`. Read each run's `capture.json` for viewport, origin, scale, duration, and frame count. [Motion review matrix](/docs/agents/cutout-motion-review.md)

## Publish and render the PCK

```powershell
powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\dev.ps1 publish
powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File .\dev.ps1 anim all -Variant both -PackagePath artifacts/mods/Sts2WD/Sts2WD.pck
```

Package mode mounts `artifacts/mods/Sts2WD/Sts2WD.pck` inside a clean host directory (`import_s3tc_bptc=true` + temporary `<host>/local_spine.gdextension`) and renders `combat.tscn` and `merchant.tscn` directly from the packed `.spjson`, `.spatlas`, and `.bptc.ctex` entries.

## Live transitions

Verify Track 0 action-to-idle mix transitions (`attack`, `guard`, `hurt`, `cast` → `idle_loop`), Track 1 `blink_loop` suspension during actions and automatic restoration on `idle_loop`, Spine events (`attack_hit` at `0.54 s`, `guard_brace` at `0.18 s`, `cast_release` at `0.25 s`, `weapon_release` at `0.12 s`), settled `die` hold (`1.66`–`2.20 s`), and `FlipX` mirroring against the [Spine dispatch contract](/docs/characters/realta/animation-api.md).
