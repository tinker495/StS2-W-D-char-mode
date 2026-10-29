# Realta Asset Model Inheritance

[Asset index](../../realta-assets.md).

## Base Model Class Status

- **Cards (`Sts2WDCode/Cards/Sts2WDCard.cs:11-26`, `Sts2WDCode/Characters/Realta/Cards/RealtaCard.cs:10-19`):**
  - `Sts2WDCard : CustomCardModel` overrides three properties:
    - `public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();`
    - `public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();`
    - `public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();`
  - `RealtaCard : Sts2WDCard` declares `[Pool(typeof(RealtaCardPool))]` and provides Duel/Valor helpers (`IsDuel`, `IsValor`).
  - **Token exception (`Sts2WDCode/Characters/Realta/Cards/RealtaGuard.cs:15-16`):** `RealtaGuard` (`강철의 수호자`) must belong to `[Pool(typeof(TokenCardPool))]`, so it inherits directly from `Sts2WDCard`, bypassing `RealtaCard`.
- **Relics (`Sts2WDCode/Relics/Sts2WDRelic.cs:10-15`, `Sts2WDCode/Characters/Realta/Relics/RealtaRelic.cs:6-7`):**
  - `Sts2WDRelic : CustomRelicModel` overrides three properties:
    - `public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();`
    - `protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();`
    - `protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();`
  - All five Realta relics inherit from `RealtaRelic` (`: Sts2WDRelic`).
  - **Outline fallback:** If `<name>_outline.png` is missing, `PackedIconOutlinePath` returns the `RelicImagePath()` fallback `res://Sts2WD/images/relics/relic.png`, rather than `relic_outline.png`.
- **Powers (`Sts2WDCode/Powers/Sts2WDPower.cs:14-31`, `Sts2WDCode/Characters/Realta/Powers/*.cs`):**
  - `Sts2WDPower : CustomPowerModel` provides overrides for `CustomPackedIconPath` (`PowerImagePath()`) and `CustomBigIconPath` (`BigPowerImagePath()`).
  - **Current status:** Twenty-one of the 22 custom powers inherit `Sts2WDPower` and use dedicated 64px/256px PNGs; only `RealtaForgeHeatPower` retains its native fallback. This follows the later application recorded in [power presentation status](../powers/assets.md#power-display-titles-and-runtime-icon-status), which also owns the three native presentation-only icons and Undying Loyalty's approved mirrored reuse.
- **Character and card/relic/potion pools (`Sts2WDCode/Characters/Realta/Realta.cs:11-61`, `RealtaCardPool.cs:7-34`, `RealtaRelicPool.cs:7-13`, `RealtaPotionPool.cs:7-14`):**
  - `Realta : PlaceholderCharacterModel` overrides the name and energy-label outline colors, portrait/outline, selection tiles, map marker, and the [combat/rest/merchant/selection scenes](../animation/assets.md#current-scene-and-property-connections). `RealtaSceneBridge.cs` forwards rest act/light-off and non-combat game-over death to the Godot scenes. `RealtaEnergyCounterAppearance` supplies the [combat energy counter](../ui/assets.md#lower-left-combat-energy-counter) treatment, and `RealtaCardAudio` reuses native upgrade and enchantment sounds. Other character visuals and sounds use `PlaceholderCharacterModel` defaults.
  - `RealtaCardPool : CustomCardPoolModel` defines `BigEnergyIconPath` (`"charui/realta/big_energy.png".ImagePath()`), `TextEnergyIconPath` (`"charui/realta/text_energy.png".ImagePath()`), a custom heated-iron frame with identity HSV parameters (`H = 1f, S = 1f, V = 1f`), and `DeckEntryCardColor = new("e76f51")`.
  - `RealtaRelicPool` and `RealtaPotionPool` use the same `BigEnergyIconPath`, `TextEnergyIconPath`, and `LabOutlineColor => Realta.Color`.
