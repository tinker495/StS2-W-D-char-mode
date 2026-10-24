# Character Two Art Direction

Character identity and appearance are not established. The domain draft is [althea-design.md](../../althea-design.md); do not infer a body, costume, or equipment from the energy symbol.

## Relic Candidates (2026-10-24)

The user supplied three object references and requested a simple original-game relic treatment. The working mapping is: the plain straight silver longsword supplies Wyrd and its Blood Frenzy upgraded variant; the steel/leather forearm bracer supplies Fighter's Bracer; the black plate-armor photograph supplies Crekin Mac Dubh. Ancestors' Recognition, the Rare relic without a photo, explicitly uses a blue version of the energy symbol's general shape. These object briefs do not establish the character's body, worn costume, or canonical personal weapon.

- Wyrd: whole straight double-edged pale steel longsword, simple straight crossguard, black wrapped grip and small round wheel pommel. No runes or magic. Diagonal full-object composition.
- Blood Frenzy: paired variant of the newly generated Wyrd master, preserving its construction; blackened burgundy blade, scarlet edge/vein and broad crimson wisps. No orange flames, dripping gore or background haze.
- Fighter's Bracer: one empty diagonal forearm bracer, broad cool steel borders, two reddish-brown leather panels, raised central ridge, one broad muted-gold chevron and restrained chunky brass rivets. No hand, arm, shield or engraved filigree.
- Crekin Mac Dubh: one empty compact blackened plate cuirass with attached shoulder plates, high open neck rim and short overlapping waist plates. Broad steel-blue/silver bevels separate dark surfaces. No helmet, head, arms, legs, stand or mannequin; replace the source's many narrow seams with a few major plate joints.
- Ancestors' Recognition: flat-topped faceted blue hexagon with six broad rim segments around a circular cyan-white inner light. Deep navy recesses and cobalt/icy-blue highlights; no digits, iris/pupil, ancestor figure or added external ring. Preserve the energy symbol's recognizable proportions without treating the relic as a functional energy counter.

All five candidates use bold near-black contours, two or three broad connected values and restrained painted edges, guided by the official relic/potion reference in ARTS.md. Generate each complete square transparent object directly; preserve full canvas during conversion, without extraction or crop. The current-run Wyrd master anchors Blood Frenzy; the existing energy master is an explicitly requested shape reference. No earlier delivered relic was an input. Candidate paths and application status belong to the asset guide.

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

## Wraith Token Portrait

The user requested the base-game Necrobinder Soul token portrait with only a dark crimson color treatment. Reference `Soul.PortraitPath`, currently `res://images/atlases/card_atlas.sprites/token/soul.tres`; preserve the complete skull-shaped spirit, supporting skeletal hand, background spirits, framing, and source alpha. Use blackened burgundy shadows, blood-red midtones, and coral-red highlights. Wraith uses this dedicated palette instead of the generic grayscale Token palette. This is a selected-source, code-native recolor under pipeline Branch D; no character body design or new painting is established.
