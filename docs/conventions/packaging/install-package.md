# Install Package and PCK Minimization

Single source of truth for mod install-bundle contents, Godot `.import` texture compression tiers, unconditional source PNG resolution ceilings, PCK export boundaries, and mechanical verification gates. Rationale for the hybrid compression and boundary trade-offs lives in [ADR-0007](/docs/adr/0007-tiered-texture-compression-and-pck-boundary.md).

## 1. Three-Tier `.import` Texture Compression Policy

Every tracked PNG under `Sts2WD/` must have a tracked `.import` sidecar matching one deterministic compression tier, evaluated against lowercase POSIX paths in strict precedence order (**Tier 1 → Tier 3 → Tier 2**):

| Precedence | Tier | Match Predicate | Required `[params]` | Required `[remap]` |
|---|---|---|---|---|
| 1 | **Tier 1: VRAM BPTC (BC7)** | Combat atlas under `Sts2WD/animations/*/combat/*.png` (or `*_skeleton.png` / `*_atlas.png` under `Sts2WD/animations/`) with `max(w, h) >= 2048` | `compress/mode=2`, `compress/high_quality=true`, `mipmaps/generate=false`, `process/size_limit=0` | `"vram_texture": true`; `path.bptc` points to `.godot/imported/<file>.bptc.ctex`; no `path.s3tc` or plain `path` |
| 2 | **Tier 3: Lossless WebP** | Any of: (a) `max(w, h) <= 256`; (b) `*_outline.png` or `*/outline/*`; (c) under `Sts2WD/images/cards/`; (d) `*_blink_patch.png`; (e) under `Sts2WD/materials/`; (f) `Sts2WD/mod_image.png` | `compress/mode=0`, `mipmaps/generate=false`, `process/size_limit=0` | `"vram_texture": false` |
| 3 | **Tier 2: Lossy WebP (`0.85`)** | All remaining PNGs under `Sts2WD/` (`max(w, h) > 256`, non-Tier-1, non-Tier-3) | `compress/mode=1`, `compress/lossy_quality=0.85` (tolerance `1e-4`), `mipmaps/generate=false`, `process/size_limit=0` | `"vram_texture": false` |

- **No import-time rescaling (`process/size_limit=0`):** Pixel-space `Polygon2D.uv` coordinates (`realta_body.tscn`) and hardcoded pixel offsets (`realta_rest.gd`, `realta_select.gd`) break if Godot rescales textures at import time.
- **Engine BPTC configuration:** [project.godot](/project.godot) must set `[rendering]` `textures/vram_compression/import_s3tc_bptc=true`, and [export_presets.cfg](/export_presets.cfg) must keep `texture_format/s3tc_bptc=true` and `texture_format/etc2_astc=false`.
- **Non-PNG raster ban:** Do not track `.jpg`, `.jpeg`, `.webp`, `.svg`, `.bmp`, or `.tga` source rasters under `Sts2WD/`; convert runtime textures to PNG so all assets pass through PNG alpha and compression-tier checks.
- **Packaged texture inspection caveat:** Running `asset-pipeline.ps1 observe-texture -Source` against a packaged Tier 2 Lossy WebP portrait is expected to report `pixels_equal_source: false` (non-fatal; see [card observations](/.agents/skills/character-asset-pipeline/scripts/card-observations.md)).

## 2. Unconditional Source Resolution Ceilings

Source PNG `IHDR` dimensions are checked independently of compression tier (most-specific path rule first; `<= WxH` means `width <= W and height <= H`):

| Path Pattern (under `Sts2WD/`) | Ceiling | Notes |
|---|---|---|
| `animations/*/combat/*.png`, `animations/**/*_skeleton.png`, `animations/**/*_atlas.png` | `<= 2048x2048` (`w % 4 == 0`, `h % 4 == 0` when Tier 1) | BC7 4x4 block alignment prevents import padding from shifting `Polygon2D.uv` |
| `animations/*/rest/*_master.png` | `<= 1536x1536` | Rest scene masters |
| `**/*_blink_patch.png` | `<= 512x512` | Facial blink overlays |
| `images/card_portraits/**` | `<= 1024x1024` | Exact layouts (`1000x760`, `606x852`, `250x190`) owned by [check_card_portraits.py](/scripts/check_card_portraits.py) |
| `images/cards/**` | `<= 1024x1024` | Card frames |
| `images/charui/*/char_select_bg.png` | `<= 1920x1080` | Selection backgrounds |
| `images/charui/*/{combat,standing,merchant,rest}.png` | `<= 1024x1024` | Non-rig standing portraits |
| `images/charui/*/hands/**` | `<= 1024x1536` | Portrait multiplayer hand sprites |
| `images/ancients/*_icon.png`, `images/ancients/*_icon_outline.png` | `<= 256x256` | Ancient map and run-history icons |
| `images/ancients/**` | `<= 1920x1080` | Ancient event illustrations |
| `images/relics/**`, `images/powers/**`, `images/potions/**`, `images/charui/**` | `<= 256x256` | Small UI icons, outlines, buttons, energy, map markers |
| `materials/**` | `<= 1024x1024` | Shader LUT/mask textures |
| `mod_image.png` | `<= 512x512` | Application icon |
| Any other path under `Sts2WD/` | `<= 1920x1080` or `<= 1080x1920` | Fallback ceiling |

## 3. Dual PCK Inclusion Boundary and Unreferenced-Asset Hygiene

1. **Non-runtime `.gdignore` markers:** Every tracked non-dot top-level directory except `Sts2WD` and `Sts2WDCode` must contain a tracked `<dir>/.gdignore` file (`docs/.gdignore`, `tests/.gdignore`, `scripts/.gdignore`). Never place `.gdignore` anywhere inside `Sts2WD/` or `Sts2WDCode/` (Godot C# scans `.cs` and `.uid` files under `Sts2WDCode/`).
2. **Hardened `export_presets.cfg`:** Keep `export_filter="all_resources"` with `exclude_filter` covering `Sts2WDCode/*`, `scripts/*`, `docs/*`, `tests/*`, `artifacts/*`, `bin/*`, `obj/*`, `publish/*`, `packages/*`, `**/drafts/*`, dot-directories (`.agents/*`, `.github/*`, `.githooks/*`, `.codex/*`, plus `.tools`), tooling extensions (`*.csproj`, `*.sln`, `*.props`, `*.targets`, `*.ps1`, `*.py`, `*.sh`, `*.md`, `*.txt`), and root manifests/config, with `dotnet/include_debug_symbols=false`.
3. **Zero unreferenced PNGs under `Sts2WD/`:** Delete superseded stills and unused template PNGs together with their `.import` sidecars (preserve active runtime fallbacks referenced by [StringExtensions.cs](/Sts2WDCode/Extensions/StringExtensions.cs) and [Sts2WDCard.cs](/Sts2WDCode/Cards/Sts2WDCard.cs)). For future dynamic C# lookups not inferred from `res://`, `MainFile.ResPath`, extension helpers, transitive model inheritance, or file-scoped interpolated templates, declare an audited `// sts2wd-asset: Sts2WD/<rel_path>` comment in the owning `.cs` file.

## 4. Adding or Re-Tiering Runtime PNGs (`dev.ps1 import`)

Because `dev.ps1 verify` and `dev.ps1 publish` run static validation before MegaDot import, use this deadlock-free sequence whenever adding a new PNG or changing a texture's compression tier:

1. Place the final PNG under `Sts2WD/`.
2. Run `.\dev.ps1 import` (runs MegaDot headless import without Stage 1 upfront to generate initial `.import` files).
3. Run `python -X utf8 scripts/check_install_package.py --sync-params` to apply the required Tier 1/2/3 `[params]` keys.
4. Run `.\dev.ps1 import` again so MegaDot refreshes `.godot/imported/<file>.ctex` caches and `[remap]`/`[deps]` entries (required when transitioning to Tier 1 `path.bptc`).
5. Stage the PNG and `.import` with `git add` and run `.\dev.ps1 verify` or `.\dev.ps1 publish`.

## 5. Install Bundle Contract (`Release` Default vs `Debug` Override)

- **`dev.ps1 publish` and `dev.ps1 deploy` default to `-Configuration Release`:** Produces and installs a **3-file** bundle (DLL, PCK, JSON manifest) under `artifacts/mods/Sts2WD/` and `<Sts2Path>/mods/Sts2WD/`. Stale PDB files are actively deleted from staging by [Sts2WD.csproj](/Sts2WD.csproj) (`CopyToModsFolderOnBuild`, guarded with `IsInnerGodotExport != 'true'`) and from the game mod folder by [dev-workflow.ps1](/scripts/dev-workflow.ps1) `Install-ModPackage` (after backing up any prior PDB).
- **`dev.ps1 build`, `dev.ps1 check`, and `dev.ps1 card-tests` default to `-Configuration Debug`:** Keeps fast local builds and crash stack-trace symbols during development. Pass `-Configuration Debug` explicitly to `publish`/`deploy` when a 4-file Debug bundle with PDB is needed.
- **Shared staging caveat:** `dev.ps1 build` (including `.githooks/pre-commit`) and default `dev.ps1 card-tests` write Debug outputs into the shared `artifacts/mods/Sts2WD/` directory. `dev.ps1 deploy` always runs a fresh `Release` publish before installing.

## 6. Two-Stage Pure-Stdlib Python Verification

1. **Stage 1 — Static hard gate ([check_install_package.py](/scripts/check_install_package.py)):** Runs in `dev.ps1 verify`, `build`, `check`, `publish`, `deploy`, and GitHub Actions (`verify` job, before third-party package installation). Enforces compression tiers, `[remap]` parity, source resolution ceilings, non-PNG raster bans, `.gdignore`/`export_presets.cfg`/`project.godot` boundaries, and static `Sts2WD/` PNG reachability.
2. **Stage 2 — Post-export PCK gate and size reporter ([check_pck_package.py](/scripts/check_pck_package.py)):** Runs in `dev.ps1 publish` and `deploy` (`-Step 'pck-check'`) before installation. Hard-fails on malformed/encrypted PCKs, paths outside `Sts2WD/` / `.godot/*` / `project.binary`, disallowed `Sts2WD/` extensions or misplaced `.json`, orphaned `.godot/imported/<file>` cache entries not referenced by a packed `Sts2WD/**/*.import`, or missing/unexpected sibling bundle files (rejecting PDB in `Release`). Prints a non-blocking category and `.ctex` format breakdown and writes `artifacts/runtime/packaging-.../pck-report.json`.
