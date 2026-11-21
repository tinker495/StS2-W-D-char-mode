# Realta Card Frame and Cost-Label Integration

[Asset index](realta-assets.md).

## Heated-Iron Frame and Flame-Heart Energy Application

The heated-iron card body is authored as a separate layer using the game's `card_frame_attack_s` layout as a composition reference, with no baked cost icon, title banner, or type plaque. The complete body image is proportionally Fit-resized to 598×844, and the flame-heart emblem master is Fit-resized to 74×74 and 24×24 without extracting or enlarging sub-regions.

| Resource | Runtime path | Contract |
|---|---|---|
| Card body | `Sts2WD/images/cards/realta/frame.png` | 598×844 RGBA, transparent outer corners and illustration aperture, dark description panel |
| Cost icon | `Sts2WD/images/charui/realta/big_energy.png` | 74×74 RGBA, flame outline and empty dark heart |
| Inline icon | `Sts2WD/images/charui/realta/text_energy.png` | 24×24 RGBA, same whole-image conversion |
| Title / portrait-border materials | Native `CardModel.BannerMaterial` | Retain rarity-dependent gray/cyan/gold materials and visible border |
| Type material | `Sts2WD/materials/realta/card_type_plaque.tres` | Warm iron shading over the native nine-patch plaque |
| Shared shader | `Sts2WD/materials/realta/heated_metal.gdshader` | Retains each original layer's alpha and broad luminance variation |

`RealtaCardPool.CustomFrame` supplies the 598×844 body texture. Because BaseLib's `CustomCardPoolMaterialPatch` applies `ShaderUtils.GenerateHsv(H, S, V)` (`res://shaders/hsv.gdshader`) to pool frames, `RealtaCardPool` sets `H = 1f, S = 1f, V = 1f` and clears `%Frame.Material` (`null`) after reload so the shader does not recolor the painted body. `RealtaGuard` stays in `TokenCardPool`, overrides `VisualCardPool` to `RealtaCardPool` for Realta's energy icon and styling, and explicitly supplies `CustomFrame` because BaseLib's pool frame hook reads `Pool` rather than `VisualCardPool`. `CardRarity.Ancient` cards (`RealtaIordinaMacBethlehem`, `RealtaGeniusInspiration`) hide `_frame` in favor of native `AncientBorder` and `AncientTextBg` (`sts2.dll: CardModel.cs:179-183, 1235-1250`), so `CustomFrame` does not affect them.

`RealtaCardPool` selects [`RealtaCardAppearance`](/Sts2WDCode/Characters/Realta/RealtaCardAppearance.cs) through `ICardPoolAppearance`, invoked after native `NCard.Reload` by [`CardAppearance`](/Sts2WDCode/Cards/CardAppearance.cs). Standard Realta cards replace only the type-plaque material while retaining native `CardModel.BannerMaterial` and portrait-border opacity so Common/Uncommon/Rare rarity colors stay readable; the type label shifts to ivory with a dark outline for legibility against the dark plaque. Because the flame-heart inset sits lower and narrower than the native orb, `RealtaCardAppearance` adjusts `%EnergyLabel` bounds, autosizing, and outline/shadow metrics while preserving native Korean/locale fonts, cost colors, and cost values. Shared `CardAppearance` calls `MegaLabel.AdjustFontSize` after styling because `SetTextAutoSize` skips unchanged text, and resets native label geometry, outline/shadow, portrait-border opacity, and type-label overrides before applying a visual pool's style so recycled card nodes do not leak overrides across pools. Existing portrait geometry, title/description positions, upgrade coloring, and rarity-dependent title outlines remain native.

The 128×128 combat energy counter uses its native scene and a dedicated texture. See the [counter treatment](ui-assets.md#lower-left-combat-energy-counter) for its resource and label styling.
