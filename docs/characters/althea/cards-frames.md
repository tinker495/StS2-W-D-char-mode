# Althea Card Frame Art and Integration

## Appearance

On 2026-10-22 the user selected a subtle red upper section transitioning toward increasingly strong black-blue at the bottom. The follow-up revision increases the upper section to a clearer crimson; use vivid dark red above and inky navy below, preserving the native card body's silhouette, transparent portrait aperture, bevels and restrained texture. Keep the native rarity-dependent title ribbon and portrait border visible, and show the accepted crimson energy symbol with a readable centered cost. The description area should be dark and quiet with warm-white text. This request does not establish character artwork or card effects.

Use the original game's card-frame texture and a vertical shader gradient for the preview. This code-native color treatment preserves the original geometry and is preferable to redrawing the frame. Preserve original alpha and export the complete 598x844 canvas without cropping. Show Common/Uncommon/Rare examples to check that the palette does not hide rarity.

## Delivery and Integration

A 598x844 RGBA body and a 1120x620 three-rarity comparison were rendered on 2026-10-22. The shader prototype and comparison preview are optional local working material; the shader procedure is recorded below. The accepted body was copied to `Sts2WD/images/cards/althea/frame.png` on 2026-10-23 and connected through `AltheaCardPool.CustomFrame`. The pool accent color is `#a91f30`. This frame was installed locally on 2026-10-23.

The source body is the native `res://images/atlases/ui_atlas.sprites/card/card_frame_skill_s.tres`, recovered from its decompressed atlas with the declared region and margin. Retain its full canvas and alpha. The draft uses a canvas-item shader with upper RGB `(0.67, 0.12, 0.16)` and lower RGB `(0.065, 0.13, 0.22)`, interpolated by `smoothstep(0.05, 0.9, UV.y)`. Multiply by `clamp(dot(source.rgb, vec3(0.2126, 0.7152, 0.0722)) / 0.29, 0.52, 1.6)` to preserve the native bevels and quiet surface texture. Apply the shader to a standalone texture so UV coordinates cover the whole frame rather than its atlas.

The comparison uses the actual game's `card_banner` and `card_portrait_border_skill_s` textures with the original Common/Uncommon/Rare banner materials and Korean font, plus the crimson energy variant displayed at 64px. The illustration area and effect text are placeholders. This comparison is a resource-based SubViewport preview, not a live `NCard` check. Runtime `RealtaCardAppearance` now also handles Althea: native rarity layers remain visible, body tinting is disabled for the precolored texture, and the type plaque uses `Sts2WD/materials/althea/type_plaque.tres` with `dark_metal.gdshader`. Cost autosizing uses 18–26px with a 3px outline and native positioning. Ancient cards retain their native special layers. Final in-game UI review remains necessary.
