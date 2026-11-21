# Althea Card Frame Art and Integration

## Appearance

Use a vertical gradient from vivid dark crimson at the top to inky navy at the bottom, preserving the native card body's silhouette, transparent portrait aperture, bevels and restrained texture. Keep the native rarity-dependent title ribbon and portrait border visible, and show the accepted crimson energy symbol with a readable centered cost. The description area should be dark and quiet with warm-white text. This specification does not establish character artwork or card effects.

Use the original game's card-frame texture and a vertical shader gradient to preserve the original geometry. Preserve the original alpha and export the complete 598x844 canvas. Check legibility against all three native rarity banners.

## Delivery and Integration

The 598x844 RGBA card body is `Sts2WD/images/cards/althea/frame.png`, wired through `AltheaCardPool.CustomFrame` with pool accent color `#a91f30`. See [deployment status](/docs/deployment-status.md) for recorded local installations.

The source body is the native `res://images/atlases/ui_atlas.sprites/card/card_frame_skill_s.tres`. Recover its declared region and margin from the decompressed atlas and retain the full canvas and alpha. Interpolate the canvas-item shader between upper RGB `(0.67, 0.12, 0.16)` and lower RGB `(0.065, 0.13, 0.22)` using `smoothstep(0.05, 0.9, UV.y)`. Multiply by `clamp(dot(source.rgb, vec3(0.2126, 0.7152, 0.0722)) / 0.29, 0.52, 1.6)` to preserve the native bevels and surface texture. Apply the shader to the extracted frame so UV coordinates cover its entire canvas.

`AltheaCardPool` selects `AltheaCardAppearance` through `ICardPoolAppearance` after shared `CardAppearance` restores pooled-node state. Native rarity layers remain visible, body tinting is disabled for the precolored texture, and the type plaque uses `Sts2WD/materials/althea/type_plaque.tres` with `dark_metal.gdshader`. Cost labels retain native positioning and autosizing, Ancient cards retain their native special layers, and portrait palettes remain model-owned even if a card selects a different visual pool.
