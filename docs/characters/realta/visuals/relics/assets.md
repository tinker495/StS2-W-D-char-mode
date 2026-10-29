# Realta Relic Deliveries and Icon API

[Asset index](../../realta-assets.md).

On 2026-10-24 the user approved applying all five relic refresh illustrations. Their 94 px small icons, 94 px outlines and 256 px large icons were copied unchanged to the model-ID filenames under Sts2WD/images/relics/ (15 PNGs). The overview retains the reviewed complete compositions and display sizes.


**Applied relic artwork, 2026-10-24:** five approved illustrations are preserved in [approved deliveries](../../relic-candidates/2026-10-24/). The folder mirrors the five existing model filenames in [relic file mapping](../relics/assets.md#filename-mapping-for-five-realta-relics): 94 x 94 small PNGs, matching white 2 px alpha-derived outlines, and 256 x 256 large PNGs in `big/` (15 assets total). `overview.png` shows the full compositions and 94/48 px readability on light/dark backgrounds. All images were generated separately using the built-in image tool and proportionally Fit-resized without cropping. Small icons fit the entire source canvas into 90 x 90 with 2 px transparent padding on all sides before outline generation, preserving clearance around the sword tip. All 15 delivered PNGs have the intended dimensions and transparent corners. Original-game relic/potion pixels supplied rendering density; the user's five subject references supplied the updated forms. The canine starter correction replaces the initial weapon proposal. The 15 selected PNGs are now copied to the matching runtime paths. Their conversion and transparency checks passed; package and installation verification are in [deployment status](../../../../deployment-status.md).

## Relic Icons (`RelicModel`, `CustomRelicModel`, `Sts2WDRelic`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.RelicModel`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomRelicModel`, `BaseLib.Patches.UI.RelicImageOverridePatch`, `BaseLib.Patches.UI.RelicIconData`)
  - `Sts2WDCode/Relics/Sts2WDRelic.cs` (lines `10-15`)
  - `Sts2WDCode/Characters/Realta/Relics/RealtaRelic.cs` (lines `6-7`)

## Relic Icon Properties and Resolutions

`RelicModel` declares three path properties as `virtual`; `Sts2WDRelic` overrides them directly:

1. **`public virtual string PackedIconPath`** (`RelicModel.cs:130`, `Sts2WDRelic.cs:12`):
   - Loaded by `RelicModel.Icon` (`ResourceLoader.Load<Texture2D>(PackedIconPath)`).
   - Small icon for the top relic bar, shop, rewards, and character-selection screen.
   - **Resolution:** **`94 x 94`** (`Format32bppArgb`, RGBA PNG).
   - Current path: `Sts2WD/images/relics/<relic_name>.png` (fallback: `Sts2WD/images/relics/relic.png`).
2. **`protected virtual string PackedIconOutlinePath`** (`RelicModel.cs:132`, `Sts2WDRelic.cs:13`):
   - Loaded by `RelicModel.IconOutline` (`ResourceLoader.Load<Texture2D>(PackedIconOutlinePath)`).
   - Outline for relic hover/highlight and compendium display.
   - **Resolution:** **`94 x 94`** (`Format32bppArgb`, RGBA PNG).
   - Current path: `Sts2WD/images/relics/<relic_name>_outline.png`.
   - **Proposed code improvement (`StringExtensions.cs:49-56`):** `Sts2WDRelic.PackedIconOutlinePath` currently calls `RelicImagePath()`, so a missing `<relic_name>_outline.png` falls back to `relic.png` instead of `relic_outline.png`. A separate `RelicOutlineImagePath` in `StringExtensions.cs` would supply the appropriate `relic_outline.png` fallback.
3. **`protected virtual string BigIconPath`** (`RelicModel.cs:134, 148-170`, `Sts2WDRelic.cs:14`):
   - Loaded by `RelicModel.BigIcon` (`PreloadManager.Cache.GetTexture2D(ResolvedBigIconPath)`).
   - High-resolution icon for relic inspection popups and detail views.
   - **Resolution:** **`256 x 256`** (`Format32bppArgb`, RGBA PNG).
   - Current path: `Sts2WD/images/relics/big/<relic_name>.png` (fallback: `Sts2WD/images/relics/big/relic.png`).

## Filename Mapping for Five Realta Relics

| # | C# class (`Sts2WDCode/Characters/Realta/Relics/`) | Korean relic name | Rarity | `ModelId.Entry` | Small icon (`94 x 94`) | Outline icon (`94 x 94`) | Large icon (`256 x 256`) |
|---|---|---|---|---|---|---|---|
| 1 | `RealtaBronzeWatchdog` | 청동의 번견 | Starter | `STS2WD-REALTA_BRONZE_WATCHDOG` | `Sts2WD/images/relics/realta_bronze_watchdog.png` | `Sts2WD/images/relics/realta_bronze_watchdog_outline.png` | `Sts2WD/images/relics/big/realta_bronze_watchdog.png` |
| 2 | `RealtaKuu` | 쿠우 | Starter (upgraded) | `STS2WD-REALTA_KUU` | `Sts2WD/images/relics/realta_kuu.png` | `Sts2WD/images/relics/realta_kuu_outline.png` | `Sts2WD/images/relics/big/realta_kuu.png` |
| 3 | `RealtaWitchPointedHat` | 마녀 고깔모자 | Common | `STS2WD-REALTA_WITCH_POINTED_HAT` | `Sts2WD/images/relics/realta_witch_pointed_hat.png` | `Sts2WD/images/relics/realta_witch_pointed_hat_outline.png` | `Sts2WD/images/relics/big/realta_witch_pointed_hat.png` |
| 4 | `RealtaDragonCloak` | 용의 망토 | Uncommon | `STS2WD-REALTA_DRAGON_CLOAK` | `Sts2WD/images/relics/realta_dragon_cloak.png` | `Sts2WD/images/relics/realta_dragon_cloak_outline.png` | `Sts2WD/images/relics/big/realta_dragon_cloak.png` |
| 5 | `RealtaEldirGramr` | 엘디르 그라므르 | Rare | `STS2WD-REALTA_ELDIR_GRAMR` | `Sts2WD/images/relics/realta_eldir_gramr.png` | `Sts2WD/images/relics/realta_eldir_gramr_outline.png` | `Sts2WD/images/relics/big/realta_eldir_gramr.png` |
