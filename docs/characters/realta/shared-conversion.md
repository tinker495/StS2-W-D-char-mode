# Realta Image Delivery Conversion

[Asset index](realta-assets.md).

## New Production and Delivery Conversion

The tracked runtime PNGs supply the game textures. For new images, use the approved base, the [art specification](realta-art.md), and original-game references for the target scene. Use the procedures below to export new approved masters.

- **Canonical standing figure:** Fit the whole `base.png` to 384×576, including transparent margins, without cropping. The command below reproduces the `combat.png` still; animated resources are listed in [scene connections](animation-assets.md#current-scene-and-property-connections).
- **Portrait and marker:** Generate each for a square composition. Export the whole portrait to 128×128 and dilate its final alpha by a 2 px radius for a white outline. Fit the whole marker master within 110×110, then place it at `(9,9)` on a 128×128 transparent canvas.
- **Two selection tiles:** Fit the whole transparent face-close-up master to 132×195. Reuse its downscaled RGB and alpha `a` (0–1). Composite the normal tile as `a × figure RGB + (1−a) × #2476A6`, and the locked tile as `(1−a) × #3B3B3B` to produce a black figure silhouette. Both final outputs have alpha 255.
- **Two energy icons:** Proportionally resize the complete flame-heart master, with an empty dark inset for the number, to 74×74 and 24×24. Keep the outside background transparent.
- **Selection background:** Generate a 16:9 scene from the start and Fit the whole image to 1920×1080. Fill edge margins caused by aspect-ratio rounding with the background's dark blue `#0C273C`; set final alpha to 255. Do not use post-generation cropping or Cover extraction.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 export -Source docs/characters/realta/base.png -Target Sts2WD/images/charui/realta/combat.png -Width 384 -Height 576 -CropMode Fit
```

After export, inspect the whole image at actual display size on light and dark backgrounds. Check output dimensions and scene connections, then refresh imports and the PCK with `powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 -Task publish`. Installation requires a separate request.
