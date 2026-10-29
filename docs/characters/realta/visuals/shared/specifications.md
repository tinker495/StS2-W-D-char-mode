# Realta Asset Specification Table

[Asset index](../../realta-assets.md).

## Complete Asset Specification Table

This table covers both current deliveries and future production targets. [current application status](../../realta-assets.md#0-current-application-status) owns current application status and production basis.

| Asset category | Recommended target path (`Sts2WD/...`) | Exact pixel dimensions (`W x H`) | Format / Godot import rules | C# class and property/method to override |
|---|---|---|---|---|
| **Card top-left energy icon** | `Sts2WD/images/charui/realta/big_energy.png` | `74 x 74` | RGBA PNG (`CompressedTexture2D`, Lossless, Mipmaps Off) | `RealtaCardPool.BigEnergyIconPath`<br>`RealtaRelicPool.BigEnergyIconPath`<br>`RealtaPotionPool.BigEnergyIconPath` |
| **Inline-text energy icon** | `Sts2WD/images/charui/realta/text_energy.png` | `24 x 24` | RGBA PNG (`CompressedTexture2D`, Lossless) | `RealtaCardPool.TextEnergyIconPath`<br>`RealtaRelicPool.TextEnergyIconPath`<br>`RealtaPotionPool.TextEnergyIconPath` |
| **Top-bar / statistics / run-history character icon** | `Sts2WD/images/charui/realta/character_icon.png` | `128 x 128` *(vanilla `85 x 85`)* | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomIconTexturePath` and `Realta.CustomIcon` |
| **Ancient-dialogue / multiplayer-vote character-icon outline** | `Sts2WD/images/charui/realta/character_icon_outline.png` | `128 x 128` *(vanilla `85 x 85`)* | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomIconOutlineTexturePath` |
| **Character-selection button icon** | `Sts2WD/images/charui/realta/char_select.png` | `132 x 195` | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomCharacterSelectIconPath` |
| **Locked character-selection icon** | `Sts2WD/images/charui/realta/char_select_locked.png` | `132 x 195` | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomCharacterSelectLockedIconPath` |
| **Map-marker icon** | `Sts2WD/images/charui/realta/map_marker.png` | `128 x 128` *(vanilla `49 x 64`)* | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomMapMarkerPath` |
| **Combat energy counter** | `Sts2WD/images/charui/realta/combat_energy.png` | `256 x 256` (UI display size `128 x 128`) | RGBA PNG (`CompressedTexture2D`, Lossless) | `RealtaEnergyCounterAppearance` replaces the native counter's first texture layer; native counter logic and VFX remain active |
| **Standard card portraits (79)** | `Sts2WD/images/card_portraits/realta/<card_name>.png` *(current default: `card_portraits/big/<card_name>.png`; newer-card aliases in [additional card sources](../cards/inventory-additional.md))* | **`1000 x 760`** *(smaller output: `500 x 380` / `250 x 190`, ratio `25:19`)* | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDCard.CustomPortraitPath` (and `PortraitPath`) |
| **Ancient full-art card portraits (two)** | `Sts2WD/images/card_portraits/realta/<card_name>.png` (`realta_iordina_mac_bethlehem.png`, `realta_genius_inspiration.png`) | **`606 x 852`** *(smaller output: `250 x 350`)* | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDCard.CustomPortraitPath` (and `PortraitPath`) |
| **Applied shared card body** | `Sts2WD/images/cards/realta/frame.png` | `598 x 844` | RGBA PNG (`CompressedTexture2D`, Lossless) | `RealtaCardPool.CustomFrame(CustomCardModel)` or `CustomCardModel.CustomFrame` |
| **Small relic icons (five)** | `Sts2WD/images/relics/<relic_name>.png` | **`94 x 94`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDRelic.PackedIconPath` |
| **Relic-outline icons (five)** | `Sts2WD/images/relics/<relic_name>_outline.png` | **`94 x 94`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDRelic.PackedIconOutlinePath` |
| **Large relic icons (five)** | `Sts2WD/images/relics/big/<relic_name>.png` | **`256 x 256`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDRelic.BigIconPath` |
| **Small power icons (22)** | `Sts2WD/images/powers/<power_name>.png` | **`64 x 64`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `CustomPowerModel.CustomPackedIconPath` (`Sts2WDPower`) |
| **Large power icons (22)** | `Sts2WD/images/powers/big/<power_name>.png` | **`256 x 256`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `CustomPowerModel.CustomBigIconPath` (`Sts2WDPower`) |
| **Character-selection background scene** | `Sts2WD/scenes/realta/char_select_bg_realta.tscn` (+ background PNG) | `1920 x 1080` (`Control` root scene) | Godot `.tscn` + RGBA PNG | `Realta.CustomCharacterSelectBg` |
| **Combat character visuals** | `Sts2WD/scenes/realta/combat.tscn` + `Sts2WD/animations/realta/combat/` | rig in 384×576 source px, displayed at `240 x 360`, Bounds `240 x 360` | Godot `.tscn` (`Node2D` root) with baked rig | `Realta.CustomVisualPath` |
| **Rest-site character scene** | `Sts2WD/scenes/realta/rest_site.tscn` + `Sts2WD/animations/realta/rest/` | 1254² painting at scale 0.4 (≈ 448 x 467 figure), Hitbox `390 x 480` | Godot `.tscn` (`Node2D` + `ControlRoot`) | `Realta.CustomRestSiteAnimPath` |
| **Merchant character scene** | `Sts2WD/scenes/realta/merchant.tscn` | same rig, displayed at `240 x 360` (`Node2D` root) | Godot `.tscn` (`Node2D` root) | `Realta.CustomMerchantAnimPath` |
| **Multiplayer-hand textures (four, optional)** | `Sts2WD/images/charui/realta/hand_{point,rock,paper,scissors}.png` | `422 x 1200` | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomArm{Pointing,Rock,Paper,Scissors}TexturePath` |
