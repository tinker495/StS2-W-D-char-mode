# Realta Power Icon API and Presentation Status

[Asset index](realta-assets.md).

## Power Icons (`PowerModel`, `CustomPowerModel`, `Sts2WDPower`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.PowerModel`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomPowerModel`, `BaseLib.Abstracts.ICustomPower`, `BaseLib.Abstracts.PackedIconPath`, `BaseLib.Abstracts.BigIconPath`, `BaseLib.Abstracts.BigBetaIconPath`)
  - `Sts2WDCode/Powers/Sts2WDPower.cs` (lines `14-31`)
  - `Sts2WDCode/Characters/Realta/Powers/*.cs` (22 classes)

## Power Icon Properties and Resolutions

`PowerModel.PackedIconPath`, `BigIconPath`, and `BigBetaIconPath` are non-virtual. BaseLib's Harmony Prefix patches (`BaseLib.Abstracts.PackedIconPath`, `BigIconPath`, `BigBetaIconPath`) replace them with the following virtual properties of `ICustomPower` (`CustomPowerModel`):

1. **`public virtual string? CustomPackedIconPath => null;`** (`CustomPowerModel.cs`, `Sts2WDPower.cs:17`):
   - Loaded by `PowerModel.Icon` (`ResourceLoader.Load<Texture2D>(PackedIconPath)`).
   - Small icon rendered in the power strip below the character/enemy health bar during combat.
   - **Resolution:** **`64 x 64`** (`Format32bppArgb`, RGBA PNG).
   - `Sts2WDPower` path: `Sts2WD/images/powers/<power_name>.png` (fallback: `Sts2WD/images/powers/power.png`).
2. **`public virtual string? CustomBigIconPath => null;`** (`CustomPowerModel.cs`, `Sts2WDPower.cs:18`):
   - Loaded by `PowerModel.BigIcon` (`PreloadManager.Cache.GetTexture2D(ResolvedBigIconPath)`). `ResolvedBigIconPath` is also cached during `ModelDb.Preload()` (`ModelDb.cs:462`).
   - High-resolution icon for hover tips and power-application VFX.
   - **Resolution:** **`256 x 256`** (`Format32bppArgb`, RGBA PNG).
   - `Sts2WDPower` path: `Sts2WD/images/powers/big/<power_name>.png` (fallback: `Sts2WD/images/powers/big/power.png`).
3. **`public virtual string? CustomBigBetaIconPath => null;`** (`CustomPowerModel.cs`):
   - Secondary beta-icon path checked by `PowerModel.ResolvedBigIconPath` (`PowerModel.cs:118-140`) when `ResourceLoader.Exists` cannot find the resource specified by `CustomBigIconPath`.

## PowerModel Count and Resource Ownership

- **Reason for the count difference:**
  - Realta has **22 cards of `CardType.Power`**.
  - **`RealtaAgdridsAnvil` (`아그드리드의 모루`)** immediately upgrades/improves the draw pile, and **`RealtaAlmenelsBulwark` (`알메넬의 철벽`)** immediately grants existing Plating. Neither needs a corresponding custom `PowerModel` (`22 - 2 = 20`).
  - The Skill cards **`RealtaUndyingLoyalty` (`불멸의 충성`)** and **`RealtaWellPrepared` (`준비만전`)** add two models for persistent/delayed effects: **`RealtaUndyingLoyaltyPower`** and **`RealtaVigorNextTurnPower` (displayed as `준비만전`)**, respectively (`20 + 2 = 22`).
  - `Sts2WDCode/Characters/Realta/Powers/` therefore contains **22 `PowerModel` classes**, all visible buffs (`IsVisibleInternal == true`).
- **Current resource ownership:** Twenty-one delivered custom powers inherit the existing `Sts2WDPower` base and resolve model-ID PNG filenames through the repository's image-path helpers. Only ForgeHeat retains its explicit native icon overrides. No extra fallback abstraction is needed.

## Power Display Titles and Runtime Icon Status

`RealtaPowerPresentation` changes presentation while retaining native power instances, stacking, audio, and turn hooks. On Realta, applied `BlurPower` uses the canonical `RealtaRinnalBodbDearg` card title and `ReflectPower` uses `RealtaSpatialWarp`. The native power model does not preserve the granting card, so these aliases apply to those powers on Realta regardless of source; other characters keep native Blur/Reflect titles. The next-turn Vigor timer takes `RealtaWellPrepared.TitleLocString`. Its independent `DrawCardsNextTurnPower` keeps its shared functional label because other effects also use it.

`FlameBarrierPower.Title` uses the mod-localized 화염 / Flame label for both applied powers and canonical hover tips, including on other characters. The original Flame Barrier card title is unchanged. Other dedicated buff titles already agree with their granting cards; Vigor, Plating, and other shared generic powers keep their existing names.

Four non-obvious runtime icon mappings override or supplement the default model-ID rule (all icons are Fit-resized to 256×256 and 64×64 RGBA PNGs): native `FlameBarrierPower` uses `realta_flame_power.png` globally (including canonical hover tips), applied `BlurPower` on Realta uses `realta_rinnal_bodb_dearg_power.png`, applied `ReflectPower` on Realta uses `realta_spatial_warp_power.png`, and `RealtaVigorNextTurnPower` (`준비만전`) uses `realta_vigor_next_turn_power.png`. Other characters keep native Blur/Reflect titles and art. Native power icon routing intercepts `ResolvedBigIconPath` before its inherited canonical cache so the owning character's tooltip and application VFX match the small icon without forking power behavior or stacking.

Motif references live in [refs/powers](refs/powers/README.md), production overrides and mirrored reuse rules live in [power art](powers-art.md), and the 22-model filename table (with `RealtaForgeHeatPower` as the sole native-art power) lives in [power inventory](powers-inventory.md).
