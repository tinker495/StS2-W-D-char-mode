# Althea Card Portrait Integration

## Basic Strike Portrait

The basic Strike portrait is [Sts2WD/images/card_portraits/althea/althea_strike.png](/Sts2WD/images/card_portraits/althea/althea_strike.png), an exact 1000 x 760 fully opaque PNG (`25:19`). Input roles and the composition are in [the card art specification](cards-art.md#basic-strike-portrait). `AltheaStrike` resolves `CustomPortraitPath` and `PortraitPath` to this runtime texture through the inherited `Sts2WDCard` portrait resolver, and `AltheaCardAppearance.ApplyPortrait` bypasses the temporary palette shader for mod-provided portraits.

## Temporary Portrait Palettes

`Sts2WDCode/Characters/Althea/AltheaCardAppearance.cs` applies rarity palettes to the `%Portrait` texture of visible Althea cards through `AltheaCard.ApplyPortraitAppearance`. Shared `CardAppearance` invokes that model hook after restoring common node state and applying the visual pool's style. Portrait selection follows the actual Althea model independently of `VisualCardPool`. It uses `Sts2WD/materials/althea/portrait_common.tres`, `portrait_uncommon.tres`, and `portrait_rare.tres`, sharing `portrait_palette.gdshader`. No native raster or atlas is copied or edited. Existing Attack, Skill, Power, and Curse placeholder sources remain selected by `AltheaCard.Art`.

The shader preserves source alpha and vertex/canvas modulation so fades and native visual effects remain intact, and applies no runtime randomness, crop, scaling, or composition change. Rare holds darker shadows longer than Common and Uncommon so gold highlights sit over a black background. Exact luminance parameters and RGB values live in `portrait_palette.gdshader` and the three material files.

Native `NCard.Reload` assigns blur for undiscovered cards; the portrait treatment leaves non-null materials untouched. Native reload/pool reset clears the palette when a node displays another card. Ancient full-art cards are excluded. Native rarity banners, portrait borders, and frame layers remain independent.

The custom-portrait exclusion normalizes backslashes to forward slashes before comparing against `MainFile.ResPath`. On Windows, the shared portrait resolver can return a mixed-separator path such as `res://Sts2WD\images\card_portraits\althea\althea_strike.png`; the exclusion must still recognize the approved mod portrait and retain its original colors.

The palette wiring above describes the repository resources. See the [deployment record](/docs/deployment-status.md) for installed release scope.

## Wraith Token Portrait

`AltheaCard.Art` selects the native `Soul` model for `AltheaWraith`; all other placeholder source selections remain as above. `AltheaCardAppearance.ApplyPortrait` selects [portrait_wraith.tres](/Sts2WD/materials/althea/portrait_wraith.tres) (sharing `portrait_palette.gdshader`) before applying generic rarity palettes, preserving native undiscovered-card blur and pooled-node reset.

The native source is `res://images/atlases/card_atlas.sprites/token/soul.tres` in the Slay the Spire 2 PCK. The game displays the original 250×190 composition with the model's recolor material.
