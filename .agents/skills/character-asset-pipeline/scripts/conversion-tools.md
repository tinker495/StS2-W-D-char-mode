# Tile conversion and UI observations

Use these commands for approved delivery conversions or review drafts. They do not imply approval to overwrite runtime files. Use Git-ignored draft outputs while reviewing candidates.

## Shared-alpha tile pair

`tile-pair` fits the whole transparent master once, composites normal RGB over the requested background, then composites black RGB with the exact same alpha over the locked background. Both results are opaque. Specify dimensions and colors from the target asset guide; use a transparent master rather than an opaque painted background.

```powershell
$pipeline = '.agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1'
& $pipeline tile-pair -Source $master -Target $normal -LockedTarget $locked -Width 132 -Height 195 -BackgroundColor '#24354B' -LockedBackgroundColor '#777777'
$info = & $pipeline inspect -Path $normal
$info.Width
```

`inspect` always returns a `PSCustomObject`, suitable for property access or `ConvertTo-Json -Depth 4`. `-AsObject` is retained as a compatibility no-op. Apply `Format-List` only when displaying the result, never before serialization.

## Opaque Fit export

```powershell
& $pipeline export -Source $painting -Target $output -Width 1920 -Height 1080 -CropMode Fit -BackgroundColor '#101321'
```

The optional background color composites behind the entire resized image, including partially transparent resampling edges and rounding margins. Omitting it preserves transparent output. Choose the color from the target asset guide. This preserves the complete painting instead of filling the canvas by cropping.

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
