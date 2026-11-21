# Althea Card Portrait Art

## Basic Strike Portrait

The applied portrait shows the straight silver double-edged sword, angular gold guard, navy-black grip, and faceted gold pommel sweeping diagonally through a pale blue and ivory arc over crimson. Keep the blade, tip, and hilt distinct with broad cel-shaded planes. [base.png](base.png) supplies the sword design, and native `StrikeIronclad` and `StrikeSilent` portraits guide the framing and visual density. See [portrait integration](cards-assets.md#basic-strike-portrait) for the tracked texture.

## Temporary Card Portrait Palettes

Temporary Althea portraits apply a code-native luminance palette at render time to distinguish reused base-game placeholder artwork by rarity while preserving the full native image. Common portraits use black, gray, and white; Uncommon portraits use deep blue, clear blue, and icy white; Rare portraits use near-black shadows and gold midtones/highlights. Basic and Token portraits use the Common palette. Ancient full-art portraits keep their native special presentation. This is a temporary distinction by rarity, not new character artwork or unique artwork for every card.

Keep card bodies, rarity banners, portrait borders, text, cost icons, source alpha, and portrait composition unchanged. Keep undiscovered-card blur intact and clear palette materials through the native pooled-card reset. Preserve canvas modulation so fades and other native visual effects still work.

## Wraith Token Portrait

The user requested the base-game Necrobinder Soul token portrait with only a dark crimson color treatment. Reference `Soul.PortraitPath`, currently `res://images/atlases/card_atlas.sprites/token/soul.tres`; preserve the complete skull-shaped spirit, supporting skeletal hand, background spirits, framing, and source alpha. Use blackened burgundy shadows, blood-red midtones, and coral-red highlights. Wraith uses this dedicated palette instead of the generic grayscale Token palette. This is a selected-source, code-native recolor under pipeline Branch D; no character body design or new painting is established.

[Portrait integration](cards-assets.md) owns the applied materials and source mapping.
