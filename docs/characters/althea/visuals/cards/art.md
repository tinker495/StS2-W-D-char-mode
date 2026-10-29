# Althea Card Portrait Art

## Basic Strike candidate (2026-10-25)

The user requested a weapon-focused Strike based on the character image. Production brief: the straight silver double-edged sword, angular gold guard, navy-black grip and faceted gold pommel sweeping diagonally across a crimson/burgundy background with a pale blue/ivory crescent. Preserve the complete blade, tip and hilt; use broad clean cel-shaded planes, no person, text, frame or UI. [base.png](../../base.png) supplied weapon construction; native `StrikeIronclad`/`StrikeSilent` portraits supplied staging and density. The built-in image generator produced a new complete landscape composition. At 250 x 190 the long straight blade and gold hilt remain distinct, with a brighter and sharper slash than native Ironclad's orange arc. Pending user review.

## Temporary Card Portrait Palettes

On 2026-10-23, the user confirmed that Karmic Madness was present in the deck; its reused Defend artwork had made identification difficult. The acquisition report was withdrawn, so do not change its card or curse acquisition behavior on that basis.

For temporary Althea portraits, preserve the full native image and apply a code-native luminance palette at render time. Common portraits use black, gray, and white; Uncommon portraits use deep blue, clear blue, and icy white; Rare portraits use near-black shadows and gold midtones/highlights. Basic and Token portraits use the Common palette. Ancient full-art portraits keep their native special presentation. This is a temporary distinction by rarity, not new character artwork or unique artwork for every card.

Keep card bodies, rarity banners, portrait borders, text, cost icons, source alpha, and portrait composition unchanged. Keep undiscovered-card blur intact and clear palette materials through the native pooled-card reset. Preserve canvas modulation so fades and other native visual effects still work.

## Wraith Token Portrait

The user requested the base-game Necrobinder Soul token portrait with only a dark crimson color treatment. Reference `Soul.PortraitPath`, currently `res://images/atlases/card_atlas.sprites/token/soul.tres`; preserve the complete skull-shaped spirit, supporting skeletal hand, background spirits, framing, and source alpha. Use blackened burgundy shadows, blood-red midtones, and coral-red highlights. Wraith uses this dedicated palette instead of the generic grayscale Token palette. This is a selected-source, code-native recolor under pipeline Branch D; no character body design or new painting is established.

[Portrait integration](assets.md) owns the applied materials, source selection and observations.
