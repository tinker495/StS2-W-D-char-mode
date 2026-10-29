# Realta Card Frame and Cost-Label Integration

[Asset index](../../realta-assets.md).

## Heated-Iron Frame and Flame-Heart Energy Application

The 2026-10-20 user-selected frame concept was adapted into a separate body layer, using the game's `card_frame_attack_s` layout as a composition reference. The body has no baked cost icon, title banner, or type plaque. The original complete generated body is proportionally Fit resized to 598×844; the selected flame-heart master is Fit resized to 74×74 and 24×24. No generation region is extracted or enlarged. The applied PNGs and the descriptions here are sufficient for runtime use; optional drafts are not build inputs.

| Resource | Runtime path | Contract |
|---|---|---|
| Card body | `Sts2WD/images/cards/realta/frame.png` | 598×844 RGBA, transparent outer corners and illustration aperture, dark description panel |
| Cost icon | `Sts2WD/images/charui/realta/big_energy.png` | 74×74 RGBA, flame outline and empty dark heart |
| Inline icon | `Sts2WD/images/charui/realta/text_energy.png` | 24×24 RGBA, same whole-image conversion |
| Title / portrait-border materials | Native `CardModel.BannerMaterial` | Retain rarity-dependent gray/cyan/gold materials and visible border |
| Type material | `Sts2WD/materials/realta/card_type_plaque.tres` | Warm iron shading over the native nine-patch plaque |
| Shared shader | `Sts2WD/materials/realta/heated_metal.gdshader` | Retains each original layer's alpha and broad luminance variation |

`RealtaCardPool.CustomFrame` supplies the body; H/S/V are all 1 to preserve the painted palette. `RealtaGuard` keeps TokenCardPool ownership, overrides `VisualCardPool` for the Realta cost icon, and explicitly supplies the same custom frame because BaseLib's pool frame hook reads `Pool`, not `VisualCardPool`.

`RealtaCardAppearance` runs after `NCard.Reload`. Standard Realta cards clear the body HSV material and replace only the type-plaque material; the native title banner and portrait border retain `CardModel.BannerMaterial` and visible opacity so rarity remains readable. The type label becomes ivory with a dark outline for readability on the dark plaque. Ancient cards keep the native full-art layers. `%EnergyLabel` uses offsets left/right −17/+17 and top/bottom −8/+28 (34×36px), 14–22px live autosizing, 3px outline/shadow outline, and 1px shadow offsets. The existing Korean/locale font, cost colors, and cost values are retained. The hook explicitly refreshes `MegaLabel.AdjustFontSize` because `SetTextAutoSize` skips unchanged text. For another character on a pooled node, it restores native ±23 / −26,+30 offsets, 22–32px autosizing, 16px outlines, and 2px shadows. Portrait-border opacity and type-label overrides are also restored. Existing portrait geometry, title/description positions, upgrade coloring, and rarity-dependent title outlines remain native.

The 128×128 combat energy counter uses its native scene with the [dedicated counter treatment](../ui/assets.md#lower-left-combat-energy-counter); its texture is separate from card and inline energy icons. A temporary Godot renderer loaded the actual game/mod PCK resources and rendered Common/Uncommon/Rare layers with native banner/border materials, heated-iron body, and the game's Korean `gyeonggi_cheonnyeon_batang_bold_shared.tres` font. The rarity colors remain distinct and 0–3, X, 10, and ? fit the 64px emblem with the capped font/outline layout. This resource preview does not execute NCard or replace an in-game screenshot. Mod Harmony patches registered successfully and the temporary audio-path observations still passed. Packaging/installation verification is separate from actual gameplay, which remains unperformed.

The final `dev.ps1 publish` completed successfully with zero C# warnings/errors. The local game installation was updated after backing up its previous four mod files. SHA256 comparisons matched for the installed DLL, PCK, JSON manifest, and PDB. This installation includes the working-tree visual changes on top of gameplay commit `5c64ff8`; it does not imply those visual changes have been committed or published to GitHub.
---

## Card Frames and Energy Icons (`CustomCardPoolModel`, `RealtaCardPool.cs`)

- **Card-frame color / custom textures (`RealtaCardPool.cs:19-28`, `CustomCardPoolModel.cs`, `CustomCardFrame.cs`):**
  - Default approach: when `CardFrameMaterialPath` is `"card_frame_red"`, `CustomCardPoolMaterialPatch` generates a `ShaderUtils.GenerateHsv(H, S, V)` material (`res://shaders/hsv.gdshader`) and applies it to the vanilla attack/skill/power frames (`598 x 844`). Realta now supplies its own painted body texture with H/S/V all 1 and clears its frame material after reload; the default HSV route remains an alternative for other pools.
  - Custom frame images:
    - For the whole pool, override `public override Texture2D? CustomFrame(CustomCardModel card)` in `RealtaCardPool`, returning a `598 x 844` `Texture2D` for each `card.Type` (`Attack`, `Skill`, `Power`). Set `H = 1f, S = 1f, V = 1f` to avoid shader recoloring.
    - For one card, override `CustomCardModel.CustomFrame` (`Texture2D?`) or `CustomCardModel.CreateCustomFrameMaterial` (`Material?`).
    - **Exception (`CardModel.cs:179-183, 1235-1250`):** `CardRarity.Ancient` cards (`RealtaIordinaMacBethlehem`, `RealtaGeniusInspiration`) hide the regular `_frame` and use `AncientBorder` and `AncientTextBg`, so `CustomFrame` does not affect them.
  - **Token frame color: `RealtaGuard` (`CardModel.cs:252, 286, 324`):**
    - Because `RealtaGuard` belongs to `[Pool(typeof(TokenCardPool))]`, its default `VisualCardPool` is `TokenCardPool`: gray `card_frame_colorless` frame and default colorless energy icon.
    - The applied `RealtaGuard.VisualCardPool` override selects Realta's frame material and energy icon (`big_energy.png`):
      ```csharp
      public override CardPoolModel VisualCardPool => ModelDb.CardPool<RealtaCardPool>();
      ```
