# Realta Resource Paths, Packaging and Model IDs

[Asset index](realta-assets.md).

## Resource Root and Build/Packaging Pipeline

- **Resource root (`Sts2WDCode/MainFile.cs:12-13`):**
  - `MainFile.ModId = "Sts2WD"`
  - `MainFile.ResPath = "res://Sts2WD"`
  - All mod assets live under `Sts2WD/` on disk and correspond to runtime Godot paths under `res://Sts2WD/...`.
- **Godot import and PCK generation (`dev.ps1:62-68`, `Sts2WD.csproj:120-125`, `export_presets.cfg:9-11`):**
  - After adding a `.png` under `Sts2WD/images/`, running `.\dev.ps1 -Task publish` or `.\dev.ps1 -Task deploy` invokes MegaDot's headless import (`--headless --editor --path $projectRoot --import`). This creates an adjacent `<filename>.png.import` and `.godot/imported/<filename>-<hash>.ctex`.
  - `Godot.ResourceLoader.Exists(path)` and `ResourceLoader.Load<Texture2D>(path)` require the imported `CompressedTexture2D` in the PCK (`.ctex` plus `.import` remapping). **A standalone `dotnet build` does not package new images. Run Godot import and PCK export through publish or deploy.**
- **Default `.import` parameters (`Sts2WD/images/**/*.png.import`):**
  - Existing `.png.import` files use the same lossless 2D texture settings:
    - `importer="texture"`, `type="CompressedTexture2D"`
    - `compress/mode=0` (Lossless)
    - `mipmaps/generate=false`
    - `process/fix_alpha_border=true`, `process/premult_alpha=false`
    - `detect_3d/compress_to=1`

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
