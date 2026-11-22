# Realta Resource Paths, Packaging and Model IDs

[Asset index](realta-assets.md).

## Resource Root and Build/Packaging Pipeline

- **Resource root (`Sts2WDCode/MainFile.cs:12-13`):**
  - `MainFile.ModId = "Sts2WD"`
  - `MainFile.ResPath = "res://Sts2WD"`
  - All mod assets live under `Sts2WD/` on disk and correspond to runtime Godot paths under `res://Sts2WD/...`.
- **Godot import and PCK generation (`dev.ps1`, `Sts2WD.csproj`, `export_presets.cfg`):**
  - After adding a `.png` under `Sts2WD/`, run `.\dev.ps1 import` (invokes MegaDot `--headless --editor --path $projectRoot --import --quit` without Stage 1 upfront) to create `<filename>.png.import`, sync tier parameters via `python -X utf8 scripts/check_install_package.py --sync-params`, and run `.\dev.ps1 import` again so `.godot/imported/<filename>-<hash>.ctex` and `[remap]` are refreshed before staging and running `.\dev.ps1 publish` or `.\dev.ps1 deploy`.
  - `Godot.ResourceLoader.Exists(path)` and `ResourceLoader.Load<Texture2D>(path)` require the imported `CompressedTexture2D` in the PCK (`.ctex` plus `.import` remapping). **A standalone `dotnet build` does not package new images. Run Godot import and PCK export through `import` and `publish`/`deploy`.**
- **`.import` compression tiers (`Sts2WD/**/*.png.import`):**
  - Texture import settings follow the repository-wide three-tier policy in [Install Package and PCK Minimization](/docs/conventions/packaging/install-package.md):
    - `importer="texture"`, `type="CompressedTexture2D"`, `mipmaps/generate=false`, `process/size_limit=0`
    - Tier 1 (`>=2048px` combat atlases): `compress/mode=2`, `compress/high_quality=true` (VRAM BPTC/BC7)
    - Tier 2 (large painterly card portraits, selection backgrounds, ancient scenes, rest masters, standing stills): `compress/mode=1`, `compress/lossy_quality=0.85` (Lossy WebP)
    - Tier 3 (`<=256px` icons, outlines, blink patches, card frames, shader materials, `mod_image.png`): `compress/mode=0` (Lossless WebP)

## Path Helpers

[StringExtensions.cs](/Sts2WDCode/Extensions/StringExtensions.cs) resolves mod asset paths relative to `MainFile.ResPath` (`res://Sts2WD`). Checked category helpers (`CardImagePath`, `PowerImagePath`, `RelicImagePath`, `PotionImagePath`, and their `Big*`/`Outline*` variants) test `ResourceLoader.Exists` and log before falling back to a shared placeholder PNG, while `ImagePath` and `CharacterUiPath` return paths without a fallback.

## ID Generation (`ModelDb` + `BaseLib`)

1. `ModelDb.GetEntry(Type type)` (`sts2.dll: MegaCrit.Sts2.Core.Models.ModelDb`) calls `StringHelper.Slugify(type.Name)` (`StringHelper.cs:90-95`) to convert the C# class name to `UPPER_SNAKE_CASE`.
   - Replace regex `([A-Za-z0-9]|\G(?!^))([A-Z])` with `$1_$2`, then uppercase.
   - Example: `RealtaFlyingGlaive` -> `REALTA_FLYING_GLAIVE`.
2. `BaseLib.Patches.Content.PrefixIdPatch` (`BaseLib.dll`, `[HarmonyPatch(typeof(ModelDb), "GetEntry")]`) adds the root-namespace prefix to types implementing `ICustomModel` (`TypePrefix.GetPrefix(type)` -> `"STS2WD-"`).
   - Resulting `Id.Entry`: `"STS2WD-REALTA_FLYING_GLAIVE"`.
3. `BaseLib.Extensions.StringExtensions.RemovePrefix(this string id)` (`BaseLib.dll`) takes the substring after the first `'-'`; `.ToLowerInvariant()` then produces the filename stem.
   - Result: `"realta_flying_glaive"`.
