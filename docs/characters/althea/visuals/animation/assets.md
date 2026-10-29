# Althea Standing Delivery and Scene Connections

`base.png` stays 1024×1536, SHA-256 `51DB2A49CD46AC74BF97F7BF692BE6A00300B8BD1D9FA956C5973B5D3F144B22`. `standing.png` is a complete-image Fit to 384×576; no added padding or crop. Do not ship a separately generated combat painting. Combat and merchant show this texture at scale 0.625 (240×360 canvas); rest uses 0.85 (326.4×489.6). Canvas scale is not opaque subject height. The base's narrow sword-tip margin remains intentional preservation.

`Althea.CustomVisualPath`, `CustomMerchantAnimPath`, `CustomRestSiteAnimPath` and `CustomCharacterSelectBg` point to `Sts2WD/scenes/althea/combat.tscn`, `merchant.tscn`, `rest_site.tscn` and `char_select_bg_althea.tscn`. Combat exposes `Visuals`, `Bounds`, `CenterPos`, `OrbPos`, `TalkPos` and `FormVfx`; merchant exposes `Visuals`; rest exposes `ControlRoot/Visuals`, `Hitbox` and thought-bubble anchors. These are static sprites, with no new animation or event bridge. Character selection uses a full-rect Control/TextureRect.

[UI inventory](../ui/assets.md) lists the applied files; [selection delivery](../selection/assets.md) owns background conversion and selection observations. These are approved runtime uses of the draft base, not a final character-design lock.
