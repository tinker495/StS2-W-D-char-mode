# Tile conversion and UI observations

Use these commands for approved delivery conversions or review drafts. They do not imply approval to overwrite runtime files. Use Git-ignored draft outputs while reviewing candidates.

For official card extraction and packaged-texture inspection, use [card texture observations](card-observations.md). Those commands preserve the full image and record measurements without adding acceptance gates.

## Shared-alpha tile pair

`tile-pair` fits the whole transparent master once, composites normal RGB over the requested background, then composites black RGB with the exact same alpha over the locked background. Both results are opaque. Specify dimensions and colors from the target asset guide; use a transparent master rather than an opaque painted background.

```powershell
$pipeline = '.agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1'
& $pipeline tile-pair -Source $master -Target $normal -LockedTarget $locked -Width 132 -Height 195 -BackgroundColor '#24354B' -LockedBackgroundColor '#777777'
$info = & $pipeline inspect -Path $normal
$info.Width
```

`inspect` always returns a `PSCustomObject`, suitable for property access or `ConvertTo-Json -Depth 4`. `-AsObject` is retained as a compatibility no-op. Apply `Format-List` only when displaying the result, never before serialization.

`VisiblePixelRatio` counts pixels above `VisibleAlphaThreshold` (8); the bounding box and margins use that same visibility threshold. `OpaquePixelRatio` counts only alpha-255 pixels. Both percentages are rounded for display: use `FullyOpaque` (every pixel is alpha 255) and `MinAlpha` for exact opacity decisions. An opaque letterbox has no alpha margin; these measurements do not establish painted scene coverage.

## Card export and review

Use `export-card` for standard and Ancient card paintings. It selects the exact canvas from `-CardType Standard` (`1000 × 760`, default) or `-CardType Ancient` (`606 × 852`). Before writing, it requires an opaque master and a proportional Fit rectangle that fills both target dimensions after integer rounding. It rejects padding, Cover/Stretch, conflicting dimensions, and transparent pixels. Bicubic filtering preserves opaque source edges; no background fill is needed. A failed input leaves any existing target unchanged.

```powershell
# Set $reviewDirectory to a fresh Git-ignored working directory.
$candidate = Join-Path $reviewDirectory 'candidate.png'
$preview = Join-Path $reviewDirectory 'preview.png'
& $pipeline export-card -CardType Standard -Source $painting -Target $candidate
$info = & $pipeline inspect -Path $candidate
& $pipeline preview -Path $candidate -DisplayWidth 250 -DisplayHeight 190 -OutPath $preview
# Open the complete candidate and preview. After visual review, and within
# existing user authorization, promote the exact reviewed file:
Copy-Item -LiteralPath $candidate -Destination $runtimePath
& $pipeline check-cards
```

Use `250 × 350` for an Ancient preview. The whole candidate and preview must show continuous painted scene at every edge, correct subject staging, and readable action before promotion. These visual judgments are separate from mechanical size/alpha checks. Regenerate or outpaint an incompatible master; generic `export` is not a card padding fallback. Existing generic exports targeting the runtime card directory receive the same card checks.

`check-cards` checks every PNG under `Sts2WD/images/card_portraits/`: exact expected delivery dimensions, PNG encoding, and full opacity. Unlisted paths are Standard. Before delivering an Ancient portrait, register its relative PNG path as `Ancient` in [card-portrait-layouts.json](../../../../scripts/card-portrait-layouts.json); both export and validation use that mapping, never the image's actual dimensions, to select the expected layout. The existing root `card.png` fallback is explicitly `Thumbnail` (`250 × 190`). The local `dev.ps1 verify`, `build`, and `check` routes run the validator; the pre-commit hook uses `build`, and CI runs the same command on Windows. It needs neither the game nor the .NET SDK. Mechanical checks do not establish composition or painted coverage.

## Opaque Fit export for other asset types

```powershell
& $pipeline export -Source $painting -Target $output -Width 1920 -Height 1080 -CropMode Fit -BackgroundColor '#101321'
```

For non-card assets whose delivery contract permits padding, the optional background color composites behind the entire resized image, including partially transparent resampling edges and rounding margins. Omitting it preserves transparent output. Choose the color from the target asset guide. This preserves the complete painting instead of filling the canvas by cropping. Full-bleed cards follow the card export procedure above.

## UI layout preview

`ui-preview` fits the whole painting to a layout canvas and draws labeled translucent UI regions. It always labels the result as a composite, not a game capture. Without `-LayoutPath`, it uses the bundled [character-select-layout.json](character-select-layout.json): an approximate 1920×1080 layout based on the installed-game selection-screen observations in the [composition study](../../../../docs/research/sts2-character-select-composition-study.md). It covers information, ascension, tile-row and confirmation regions. This tool visualizes occupied areas, not actual font wrapping or live game behavior. For another resolution, UI scale or screen arrangement, supply an observed replacement layout and record its source in draft notes.

```powershell
& $pipeline ui-preview -Source $painting -LayoutPath $layout -OutPath $preview
# Default approximate character-selection layout:
& $pipeline ui-preview -Source $painting -OutPath $preview
```

Layout JSON has `width`, `height`, and `regions`, each with `x`, `y`, `width`, `height`, and `label`. Coordinates are pixels in the specified canvas. For example, this demonstrates the format only; it is not an authoritative game layout:

```json
{"width":1920,"height":1080,"regions":[{"x":220,"y":330,"width":660,"height":370,"label":"Character information"}]}
```

Open the output and inspect the face, active grip and action-defining contact against the regions. Follow Recipe 4-D's readability and anatomy priorities: supporting equipment can partly overlap the translucent ascension panel while essential expression/action stays readable. Record the actual tradeoff rather than treating every overlap as a failure. A successful command is not visual acceptance.
