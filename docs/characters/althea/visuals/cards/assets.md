# Althea Card Portrait Integration

## Basic Strike candidate (2026-10-25)

[card-candidates/2026-10-25/althea_strike.png](../../card-candidates/2026-10-25/althea_strike.png) is a 1000 x 760 opaque PNG. The complete 1439 x 1093 generated landscape was proportionally Fit-resized without cropping. A 250 x 190 preview was opened beside the other Strike candidates and native `StrikeIronclad` at the same scale. Input roles and the production brief are in [the card art specification](art.md#basic-strike-candidate-2026-10-25). The runtime placeholders remain unchanged; review precedes promotion.

## Temporary Portrait Palettes

`Sts2WDCode/Characters/Althea/AltheaPortraitAppearance.cs` applies rarity palettes to the `%Portrait` texture of visible Althea cards after native `NCard.Reload`. It uses `Sts2WD/materials/althea/portrait_common.tres`, `portrait_uncommon.tres`, and `portrait_rare.tres`, sharing `portrait_palette.gdshader`. No native raster or atlas is copied or edited. Existing Attack, Skill, Power, and Curse placeholder sources remain selected by `AltheaCard.Art`.

The shader preserves source alpha and vertex/canvas modulation. It computes luminance with weights `(0.2126, 0.7152, 0.0722)`, raises it to `0.85`, and uses two smooth transitions between shadow, middle, and light colors. The standard transition reaches its middle at `0.55`; Rare holds shadows through `0.22` and reaches gold at `0.65` to retain a black background. Palette RGB values are explicit in the three material files. No randomness, crop, scaling, or composition change is applied at runtime.

Native `NCard.Reload` assigns blur for undiscovered cards; the palette patch leaves non-null materials untouched. Native reload/pool reset clears the palette when a node displays another card. Ancient full-art cards are excluded. Native rarity banners, portrait borders, and frame layers remain independent.

A 840×520 comparison was rendered using complete native Ironclad Strike and Defend portraits at their actual 250×190 size, arranged as two rows and three rarity columns. The comparison checks the same shader/materials used at runtime; it is not a live card-node or gameplay check. Runtime source wiring and packaging are updated, but this portrait palette change has not been installed into the game folder. Earlier card/energy assets were installed on 2026-10-23.

## Wraith Token Portrait

`AltheaCard.Art` selects the native `Soul` model for `AltheaWraith`; all other placeholder source selections remain as above. `AltheaPortraitAppearance` selects `portrait_wraith.tres` before applying generic rarity palettes. This material shares `portrait_palette.gdshader` and maps luminance to shadow RGB `(0.025, 0.004, 0.012)`, middle `(0.48, 0.025, 0.065)`, and light `(0.98, 0.35, 0.32)`, with shadow hold `0.02` and middle point `0.55`. Native undiscovered-card blur and pooled-node reset are preserved.

The native source is `res://images/atlases/card_atlas.sprites/token/soul.tres` in the installed Slay the Spire 2 PCK, observed at 250×190. Runtime references the original resource and applies the shader; no extracted native image is shipped. A 560×245 side-by-side preview rendered both complete portraits at 250×190 confirmed preserved composition with dark burgundy background and readable red spirit/hand silhouettes. The source and preview are optional local observations. `dev.ps1 publish -ExecutionContextLabel escalated` completed with exit code 0 and both existing rig checks passing. A separate engine loaded all four portrait materials, including Wraith, from the published PCK; a temporary native-model observation confirmed Wraith's `PortraitPath` points to Soul. Local game installation and live card-node visual review remain unperformed.
