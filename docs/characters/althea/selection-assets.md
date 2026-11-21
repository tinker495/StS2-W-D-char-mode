# Althea Selection Delivery

## Background Conversion

The selected illustration master is 1672×941 RGB. Whole-image Fit into 1920×1080 preserves the complete painting, with the approximately 1px Fit margin and resampling edge backed by opaque navy `#101321`; no image pixels are cropped. The runtime selection `TextureRect` uses `KEEP_ASPECT_CENTERED` (`stretch_mode = 5`), not Cover. The sheathed sword extends toward the lower-right illustration boundary; no post-generation crop is performed.

## Tile Export and Runtime Scope

The normal and locked tiles share the same whole-image Fit alpha and export as opaque 132×195 textures. See [selection art](selection-art.md) for the tile camera and background composition, the [UI inventory](ui-assets.md) for the three runtime paths, and the [deployment record](/docs/deployment-status.md) for local installation scope.
