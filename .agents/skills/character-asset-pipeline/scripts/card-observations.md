# Card texture observations

Use these commands to obtain original card anchors or inspect a packaged mod texture. Both run the installed Godot engine in a fresh minimal host, export complete PNGs without cropping/resizing, and retain provenance plus process logs. They do not modify a game pack, runtime PNG, test, fixture, or acceptance setting.

## Official card anchors

Select complementary card IDs from the [native portrait survey](../../../../docs/research/sts2-card-illustration-density.md). IDs use `pool/card_id`, without an extension. The command maps them to `res://images/packed/card_portraits/<pool>/<card_id>.png` inside the installed `SlayTheSpire2.pck`; it does not depend on a previous task's draft folder.

Run from the repository root in PowerShell. Pass arrays with `&`, rather than a comma-containing string through `powershell -File`.

```powershell
$pipeline = '.agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1'
$runDirectory = Join-Path 'artifacts' ('card-observation-' + [guid]::NewGuid().ToString('N'))
$anchors = Join-Path $runDirectory 'official'
& $pipeline official-cards -CardIds @('ironclad/infernal_blade', 'ironclad/fiend_fire') -OutPath $anchors
& $pipeline compare -Paths @((Join-Path $anchors 'ironclad-infernal_blade.png'), (Join-Path $anchors 'ironclad-fiend_fire.png')) -CompareMode IndividualFit -DisplayWidth 250 -DisplayHeight 190 -OutPath (Join-Path $runDirectory 'anchors.png')
```

Open the original pixels before assembling generation inputs, then open the comparison at the intended display size. Standard native illustrations are normally `1000 × 760`; use the actual observations when choosing a comparison. Vertical Ancient examples need their own `250 × 350` display area. These images supply style/density evidence and remain Mega Crit assets, never mod delivery artwork.

## Packaged texture

After packaging, inspect the resource by its native `res://` path. Pass `-Source` to record pixel equality with a local PNG; omit it to extract and measure only. The command supports any loadable `Texture2D` in the specified PCK, including imported PNG resource paths.

```powershell
& $pipeline observe-texture -PackPath 'artifacts/mods/Sts2WD/Sts2WD.pck' -ResourcePath 'res://Sts2WD/images/card_portraits/realta/realta_great_flame.png' -Source 'Sts2WD/images/card_portraits/realta/realta_great_flame.png' -OutPath (Join-Path $runDirectory 'packaged')
```

The complete extracted image is `texture.png`. Open it or use `preview`/`compare` at the card's display size. A loadable matching texture establishes pack contents; it does not establish the model's portrait wiring, game UI appearance, or gameplay behavior.

## Paths, outputs and failures

- Use a **fresh Git-ignored output directory** (`artifacts/` or the character's `drafts/`), or OS temporary storage. An existing output directory is rejected so earlier evidence is preserved. Keep extracted official art, hosts, manifests and comparisons out of Git.
- `official-cards` resolves `GodotPath` and `Sts2Path` through the repository's existing development settings. Supply **both** `-GodotPath <console executable>` and `-GamePath <game directory>` to skip that discovery. `observe-texture` needs only `-GodotPath` to skip discovery because its pack is explicit. This command does not change the repository's Steam-registry discovery behavior.
- `request.json` records the exact pack path, size, modification time, resource selections and optional source PNG. `manifest.json` records the installed game assembly ProductVersion when available, Godot version, UTC observation time, and each exported image's resource path, filename, width, height and `fully_opaque` value. Optional comparison adds `source_width`, `source_height` and `pixels_equal_source`; both images are compared as RGBA8 pixels, not encoded PNG bytes.
- `complete: true` means every requested texture was loaded and exported and its observations were written. It is **not** a visual approval or product acceptance result. `pixels_equal_source: false` is a successful observation of a difference, not a command failure. These commands impose no expected dimensions, alpha tolerance or equality gate; delivery still uses the existing card export/check commands and visual review.
- Missing resources, unreadable packs/PNGs, non-texture resources and export failures terminate the command. Read `textures.log`, `textures.json` and `engine.log` in the output directory. `textures.json` records arguments and exit status. The retained `_host/` is disposable diagnostic input; `.gdignore` excludes the output from Godot's project scan. After a failed or partial run, use a fresh directory for the next attempt.

The PowerShell command also returns the manifest as an object; format it only at the display boundary. Keep only the public/versioned resource evidence and conclusions needed for future production in shared documentation, rather than links to local observation directories.
