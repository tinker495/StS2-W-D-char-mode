# Observation and Conversion Tools

Immediately after generation and final output, run `inspect` and `preview` from the bundled [asset-pipeline.ps1](scripts/asset-pipeline.ps1), then open the previews. `inspect` reports the alpha bounding box, `MarginsPx` (`Left`, `Top`, `Right`, `Bottom`) and `MinMarginRatio`, the smallest margin as a percentage of its axis. For standing art, compare it with the margins in ARTS.md 5.1; other routes supply their own canvas and margin requirements. Replace the placeholders below with actual target values.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 inspect -Path "docs/characters/<char>/drafts/<asset>.png"
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 preview -Path "docs/characters/<char>/drafts/<asset>.png" -DisplayWidth 94 -DisplayHeight 94 -OutPath "docs/characters/<char>/drafts/<asset>-preview.png"
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 export -Source "docs/characters/<char>/drafts/<asset>.png" -Target "<runtime-path>.png" -Width 94 -Height 94 -CropMode Fit
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 outline -Source "<runtime-path>.png" -Target "<runtime-path>_outline.png" -Radius 2 -Color "#FFFFFF"
```

Pass array arguments to the comparison command with `&` inside a PowerShell session:

```powershell
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 compare -Paths @('docs/characters/<char>/drafts/<selected>.png', 'docs/characters/<char>/drafts/<edited>.png') -Labels @('선택본', '수정안') -DisplayWidth 240 -DisplayHeight 360 -OutPath 'docs/characters/<char>/drafts/<asset>-comparison.png'
```

`compare` places two or more inputs in order, showing each label, original dimensions, scale, and dark/light backgrounds. Labels default to filenames. Originals remain unchanged; output is written separately.

- `-CompareMode SharedScale` (default): one shared scale fits all full source canvases. Use for preservation comparisons where changes in canvas size or padding must stay visible. It does not normalize resolution, padding, or silhouette size.
- `-CompareMode IndividualFit`: each full source canvas fits the display area independently. Use original files directly for mixed-resolution comparisons, without intermediate resized copies. Each image reports its original dimensions and applied scale; the result's shared `Scale` is null and each `Images` entry has its own `Scale`. It preserves all pixels in the composition but does not equalize subject height or transparent padding. Judge those differences explicitly; this mode is not evidence of unchanged character scale.
- `-CompareMode SubjectFit`: scales each image so its subject bounding box (alpha > 128, which excludes the official renders' soft ground shadow) is 90% of `DisplayHeight`, centers it horizontally and aligns its lowest visible pixel to a common baseline. Cells widen for wide poses rather than shrinking them. Use it for style and density comparisons between transparent sprites. Each entry reports `SubjectBox`, `SubjectDrawHeight` and `SubjectIsFullCanvas`. When that flag is true the source has no transparent surround, as with the public screenshot crops, so its "subject" is the whole canvas. Raised weapons or wide cloth count toward subject height.

For native card textures or a packaged portrait, use [card observations](scripts/card-observations.md): `official-cards` extracts original portraits, and `observe-texture` exports a resource with dimensions, alpha and optional pixel-comparison observations. Keep reference extracts in ignored working storage. These observations do not introduce acceptance gates.

## Official Standing Poses

Official standing comparison inputs come from the installed game. `official-poses` renders time 0 of `idle_loop` (combat) and `relaxed_loop` (merchant) for Ironclad, Silent, Defect, Necrobinder and Regent through the game's own Spine extension, framed to their alpha with 5% margins. It resolves `GodotPath` and `Sts2Path` through the same MSBuild settings as `dev.ps1`, including `local.props` overrides and Steam discovery. Override either value with `-GodotPath` or `-GamePath`; passing both avoids SDK discovery. Engine, stdout and stderr logs are written beside the renders. Keep these game assets out of version control. The example creates a fresh temporary output directory and compares files produced by that invocation:

```powershell
$poseOutput = Join-Path ([IO.Path]::GetTempPath()) ('sts2-official-poses-' + [guid]::NewGuid().ToString('N'))
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 official-poses -OutPath $poseOutput
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 compare -Paths @('docs/characters/<char>/drafts/<candidate>.png', (Join-Path $poseOutput 'ironclad-combat-idle_loop-00.png'), (Join-Path $poseOutput 'silent-combat-idle_loop-00.png')) -Labels @('Candidate', 'Ironclad', 'Silent') -CompareMode SubjectFit -DisplayWidth 240 -DisplayHeight 360 -OutPath 'docs/characters/<char>/drafts/<asset>-style-comparison.png'
```

Returned scale and display dimensions are observations, not quality scores.

`inspect -Path <png>` always returns structured measurements; `-AsObject` remains a compatibility no-op. `VisiblePixelRatio` describes occupied alpha above 8, `OpaquePixelRatio` describes alpha-255 pixels, and `FullyOpaque` is the exact all-pixels predicate. Pipe to `Format-List` only at the display boundary. Cards use `export-card` into a temporary candidate, then review and promotion; `check-cards` checks runtime dimensions/alpha. Use `tile-pair`, `export -BackgroundColor <hex>` only for non-card contracts permitting Fit margins, and `ui-preview` for layout observations. See [conversion-tools.md](scripts/conversion-tools.md) for commands and the temporary-output-first sequence.

- Are canonical identity, equipment, proportions, and the selected change axis preserved?
- Do major values, silhouette, expression, and action read at actual size?
- Are unintended halos, residue, or clipping absent on light/dark backgrounds?
- Are UI safe zones and text/frame exclusions respected?

Tool measurements and comparisons are observations, not formal tests, fixtures, or automatic acceptance thresholds.
