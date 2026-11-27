# Althea Standing Art and Delivery

## Appearance

**User-confirmed standing reuse:** combat and merchant use the complete adopted `base.png` painting. Its existing combat-ready stance supports the shared Branch B reuse rule, and the user explicitly selected this reuse. Preserve the source pose, equipment, original margins and pixels; only whole-image proportional Fit resizing is authorized. The runtime uses one `standing.png` for both standing scenes. The base remains unchanged.

Use [base appearance](base-appearance.md) for identity.

## Delivery and Integration

`base.png` stays 1024×1536, SHA-256 `51DB2A49CD46AC74BF97F7BF692BE6A00300B8BD1D9FA956C5973B5D3F144B22`. `standing.png` is a complete-image Fit to 640×960 (Tier 2 Lossy WebP, `mipmaps/generate=false`); no added padding or crop. Do not ship a separately generated combat painting. Per-room `Visuals` scales, ground-line positions, and hitbox/anchor offsets are owned directly by [combat.tscn](/Sts2WD/scenes/althea/combat.tscn), [merchant.tscn](/Sts2WD/scenes/althea/merchant.tscn), and [rest_site.tscn](/Sts2WD/scenes/althea/rest_site.tscn), following the standard-build baselines in [the runtime scene contract](/docs/research/sts2-cutout-runtime-contract.md#room-scale-and-ground-line-contract). The base's narrow sword-tip margin remains intentional preservation.

`Althea.CustomVisualPath`, `CustomMerchantAnimPath`, `CustomRestSiteAnimPath` and `CustomCharacterSelectBg` point to `Sts2WD/scenes/althea/combat.tscn`, `merchant.tscn`, `rest_site.tscn` and `char_select_bg_althea.tscn`. Combat exposes `Visuals` (with the shared [ground_shadow.tscn](/Sts2WD/scenes/shared/ground_shadow.tscn) contact shadow behind the sprite), `Bounds`, `CenterPos`, `OrbPos`, `TalkPos` and `FormVfx`; merchant exposes `Visuals` and `GroundShadow`; rest exposes `ControlRoot/Visuals`, `Hitbox` and thought-bubble anchors. At rest, Althea rests her gauntlets on her knees. The [seated painting](/Sts2WD/animations/althea/rest/rest_master.png) and two registered masks preserve the separation of tabard and steel under each Act's ambient lighting and synchronized campfire flicker ([seated_illustration.gd](/Sts2WD/scenes/rest_site/seated_illustration.gd)). Character selection uses a full-rect Control/TextureRect.

[UI inventory](ui-assets.md) lists the applied files. [Selection delivery](selection-assets.md) describes the applied background and its conversion. The [base appearance guide](base-appearance.md) records the remaining design decisions.
