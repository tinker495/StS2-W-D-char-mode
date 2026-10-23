# Character Two Art Direction

Character identity and appearance are not established. The domain draft is [character-2-design.md](../../character-2-design.md); do not infer a body, costume, or equipment from the energy symbol.

## Energy Icon

The user selected the base-game Necrobinder energy icon as the exact shape reference and requested only a dark crimson color change on 2026-10-22. Preserve its faceted hexagonal metal rim, circular inner light, bevel arrangement, orientation, proportions, transparent margins, and number inset. Shift the pink/magenta body to blackened burgundy and blood red, retaining enough separation between rim, center, and highlights for white cost digits.

The selected source is the installed Slay the Spire 2 resource `res://images/atlases/ui_atlas.sprites/card/energy_necrobinder.tres`; the inline reference is `res://images/packed/sprite_fonts/necrobinder_energy_icon.png`. These are local game references, not a newly established character base. This is a selected-image recolor under pipeline Branch D, not new character artwork.

The user accepted the energy direction before requesting a card-frame preview. The accepted direction was connected to runtime card-cost and inline textures during the 2026-10-23 card implementation. See the asset mapping for the exact scope.

## Card Frame

On 2026-10-22 the user selected a subtle red upper section transitioning toward increasingly strong black-blue at the bottom. The follow-up revision increases the upper section to a clearer crimson; use vivid dark red above and inky navy below, preserving the native card body's silhouette, transparent portrait aperture, bevels and restrained texture. Keep the native rarity-dependent title ribbon and portrait border visible, and show the accepted crimson energy symbol with a readable centered cost. The description area should be dark and quiet with warm-white text. This request does not establish character artwork or card effects.

Use the original game's card-frame texture and a vertical shader gradient for the preview. This code-native color treatment preserves the original geometry and is preferable to redrawing the frame. Preserve original alpha and export the complete 598x844 canvas without cropping. Show Common/Uncommon/Rare examples to check that the palette does not hide rarity.

## Temporary Card Portrait Palettes

On 2026-10-23, the user confirmed that Karmic Madness was present in the deck; its reused Defend artwork had made identification difficult. The acquisition report was withdrawn, so do not change its card or curse acquisition behavior on that basis.

For temporary Althea portraits, preserve the full native image and apply a code-native luminance palette at render time. Common portraits use black, gray, and white; Uncommon portraits use deep blue, clear blue, and icy white; Rare portraits use near-black shadows and gold midtones/highlights. Basic and Token portraits use the Common palette. Ancient full-art portraits keep their native special presentation. This is a temporary distinction by rarity, not new character artwork or unique artwork for every card.

Keep card bodies, rarity banners, portrait borders, text, cost icons, source alpha, and portrait composition unchanged. Keep undiscovered-card blur intact and clear palette materials through the native pooled-card reset. Preserve canvas modulation so fades and other native visual effects still work.
