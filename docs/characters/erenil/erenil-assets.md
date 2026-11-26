# Erenil Asset Status

This index records the current tracked runtime assets and model bindings for **Erenil** (`STS2WD-ERENIL`). See [art direction](erenil-art.md) for visual approval status, [Erenil's design](erenil.md) for identifiers and save compatibility, and [deployment status](/docs/deployment-status.md) for recorded local installations.

## Model and Scene Status

- The [standing master](base-art.md#delivery-and-integration) is available as a source for future in-game art.
- [Erenil.cs](/Sts2WDCode/Characters/Erenil/Erenil.cs) subclasses `PlaceholderCharacterModel` and binds `CustomRestSiteAnimPath` to the [seated scene](/Sts2WD/scenes/erenil/rest_site.tscn). Its [seated painting](/Sts2WD/animations/erenil/rest/rest_master.png) shares a canvas with the cast-shadow and fire-rim masks. The other scenes and outline use the template bindings.
- `ErenilCardPool`, `ErenilRelicPool`, and `ErenilPotionPool` currently contain no exclusive cards, relics, potions, or powers, and no custom card frame or portrait textures exist yet.

## Placeholder UI Textures

All six tracked UI textures under `Sts2WD/images/charui/erenil/` are template placeholders using Tier 3 lossless WebP `.import` settings (`max(w, h) <= 256`).

| File | Dimensions | Runtime binding |
|---|---|---|
| [character_icon.png](/Sts2WD/images/charui/erenil/character_icon.png) | `128 × 128` | `Erenil.CustomIconTexturePath` |
| [char_select.png](/Sts2WD/images/charui/erenil/char_select.png) | `132 × 195` | `Erenil.CustomCharacterSelectIconPath` |
| [char_select_locked.png](/Sts2WD/images/charui/erenil/char_select_locked.png) | `132 × 195` | `Erenil.CustomCharacterSelectLockedIconPath` |
| [map_marker.png](/Sts2WD/images/charui/erenil/map_marker.png) | `128 × 128` | `Erenil.CustomMapMarkerPath` |
| [big_energy.png](/Sts2WD/images/charui/erenil/big_energy.png) | `74 × 74` | `ErenilCardPool`, `ErenilRelicPool`, `ErenilPotionPool` `BigEnergyIconPath` |
| [text_energy.png](/Sts2WD/images/charui/erenil/text_energy.png) | `24 × 24` | `ErenilCardPool`, `ErenilRelicPool`, `ErenilPotionPool` `TextEnergyIconPath` |

## Text and Localization Resources

- Character text preset: [erenil.json](/Sts2WD/character_text/erenil.json) (`characterId`: `STS2WD-ERENIL`)
- Base localization tables: [kor/characters.json](/Sts2WD/localization/kor/characters.json), [eng/characters.json](/Sts2WD/localization/eng/characters.json), [kor/ancients.json](/Sts2WD/localization/kor/ancients.json), [eng/ancients.json](/Sts2WD/localization/eng/ancients.json)

## Pending Assets

Custom standing and selection scenes, character icon outline, card frame, card portraits, and relic/power icons are not yet created.
