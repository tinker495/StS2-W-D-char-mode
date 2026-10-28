# Althea Art Direction

## Basic Strike candidate (2026-10-25)

The user requested a weapon-focused Strike based on the character image. Production brief: the straight silver double-edged sword, angular gold guard, navy-black grip and faceted gold pommel sweeping diagonally across a crimson/burgundy background with a pale blue/ivory crescent. Preserve the complete blade, tip and hilt; use broad clean cel-shaded planes, no person, text, frame or UI. `base.png` supplied weapon construction; native `StrikeIronclad`/`StrikeSilent` portraits supplied staging and density. The built-in image generator produced a new complete landscape composition. At 250 x 190 the long straight blade and gold hilt remain distinct, with a brighter and sharper slash than native Ironclad's orange arc. Pending user review.

The user selected `base.png` as the draft visual base on 2026-10-24. It owns the current appearance and equipment for further work; this is draft adoption, not final design lock. Branch B runtime scope is recorded below. The domain draft is [althea.md](althea.md). Do not infer body, costume, or equipment from the energy symbol's shape or color. Current candidate status is in [althea-assets.md](althea-assets.md); the reasons behind past choices are in [althea-history.md](althea-history.md).

Labels *user-confirmed*, *reference-observed* and *provisional* follow [Recipe 1's source labels](../../../.agents/skills/character-asset-pipeline/references.md#source-labels-and-character-orientation).

## Adopted Draft Base — 2026-10-24

The user selected `base.png` as the preferred visual base and explicitly confirmed preference for its clean 2D rendering, bright silver-white/white/navy palette, facial appeal, and graceful proportions over the subsequent `style-pure` (compact-proportion / compressed-pewter) experiment, which was rejected as too crude and clunky (`투박함`). Preserve the complete `base.png` painting unchanged: bright silver-white half-tied hair, pale young adult face and blue eyes, navy collar and lining, white split tabard with red cross, clean silver plates with gold accents, grounded right-facing three-quarter stance, drawn straight sword up-right, and the opposite hand supporting an empty sheath. The current draft includes its lower costume and equipment interpretation; weapon naming and final construction remain undecided.

Use `base.png` directly as the image input anchor for identity, facial appeal, proportions, palette, and surface rendering in downstream work. The narrow right sword-tip margin (`12px`) remains a known characteristic of the unedited `base.png` master; do not silently crop or repaint `base.png`.

## 1. Raw References and Input Roles

| Input | Adopted | Excluded or unconfirmed |
|---|---|---|
| `base.png` (user-selected draft base, generated with `refs/ref1.jpg` as direct image input) | User-confirmed visual anchor: graceful young-adult proportions, facial appeal, calm resolute blue eyes, bright silver-white half-tied hair in clean flowing locks, dark cross earring, navy collar with gold clasp, clean silver plate armor, white split tabard with red cross and gold trim | Do not replace with text-only (`style-pure`) transcription, compact `6–6.5`-head proportions, or muted ash/pewter recoloring |
| `refs/ref1.jpg` (user-supplied semi-realistic painted bust) | Supporting reference for upper-body identity cues (face, blue eyes, diagonal fringe, half-tied silver-white hair, dark cross earring, high navy collar, gold clasp, silver pauldrons, white tabard with red cross) | Photorealistic individual hair strands, noisy micro-speculars, dark vignette background, and lower-left signature |
| Official combat references ([ARTS.md 2.3](../../../ARTS.md#23-actual-official-image-anchors-and-priority)) | Three-quarter combat camera, grounded stance, and grouping high-frequency surface noise into clean 2D planes | Each official character's robot/warrior build, stocky proportions, dark/muted palette, masks, skulls, horns, weapons, and ground shadow |

Always pass `base.png` (and `refs/ref1.jpg` when referencing raw bust details) **directly as image inputs** so the generator preserves Althea's facial appeal, hair flow, clean palette brightness, and natural proportions. Withholding character image inputs in favor of text-only transcription (`style-pure`) degrades facial appeal and makes the figure look crude and clunky.

## 2. Full-Body Base Specification

- **Camera and body (user-confirmed):** weak side-on three-quarter combat camera. Torso and pelvis face the enemy at screen right; knees slightly bent with weight across front and back feet.
- **Head angle (user-confirmed):** right-facing three-quarter, between frontal and profile. The head turns slightly toward the camera so the far eye and part of the far cheek show, the far eye narrower than the near one.
- **Gaze (user-confirmed):** pupils toward the enemy at screen right, independent of head rotation. Changing the head angle does not change body direction, the drawn sword or either hand.
- **Weapon state (user-confirmed):** one straight sword, fully drawn. The screen-right hand grips the hilt with the tip pointing up-right and clear space between face and blade. The other hand supports the empty scabbard at the waist; the scabbard holds no second hilt or blade. No extra weapons, shield or magic effects.
- **Sword form and name (provisional):** follows the design's sword/scabbard theme; unique shape and name are undecided.
- **Lower body (adopted in draft base, provisional canon):** navy inner garment, white split skirts with gold trim, clean silver knee and shin plates, silver sabatons.
- **Construction (production rule):** tip, blade, guard center, grip and pommel share one straight axis. Gauntlet and vambrace, fingers and grip, belt and scabbard ring each read separately. No floating metal pieces or duplicate guards. Hems and gold trim follow continuous cloth edges. Armor resolves into clean plates with clear overlaps.
- **Canvas:** generate a vertical 2:3 transparent canvas containing the whole figure and equipment, with ARTS.md 5.1 margins at every extremity; do not crop or extract regions. Check whole-image Fit previews at `240 × 360` and `326 × 490`.

## 3. Rendering and Palette

**User-confirmed style (2026-10-24):** the user explicitly preferred the clean, appealing 2D rendering and natural proportions of `base.png` and rejected the subsequent `style-pure` candidate (which applied rough gouache brushwork, compressed pewter/bone values, and compact `6–6.5`-head proportions) as too crude and clunky (`투박함`).

- **Hair:** bright, clean silver-white hair grouped into flowing connected locks with soft cool-gray shadows; do not darken into muddy ash/pewter or fragment into photorealistic individual strands.
- **Body cloth:** clean warm-ivory / white split tabard with smooth-turning shadow planes, navy collar and inner lining, crisp gold trim bands, and a dark red cross emblem on the left chest.
- **Armor:** clean silver steel plates in 2–3 smooth-turning value planes with crisp structural bevels; muted gold cross-shaped collar clasp and gold sword/scabbard fittings.
- **Face:** attractive, sharp young-adult facial features faithful to `base.png` and `ref1.jpg`, with calm resolute blue eyes and clean skin shading.
- **Proportions:** graceful, slender-yet-armored young-adult knight proportions as established in `base.png`; avoid stocky body ratios or oversized chunky hands and boots.

## 4. Prompt Material

`<CHARACTER_ANCHOR>`: Althea, a graceful young adult woman knight with fair skin and calm, resolute blue eyes. Long, bright silver-white hair in clean flowing locks swept back and half-tied at the crown, a fringe falling diagonally across her forehead, and a small dark cross-shaped earring. High navy collar with a gold cross-shaped clasp, clean silver plate armor over a white split tabard with gold trim and a dark red cross emblem on the left chest, navy inner lining and trousers. One straight sword with a gold crossguard and a dark waist scabbard with gold fittings.

`<PALETTE>`, `<MATERIALS>`: section 3. For standing exports only, preserve section 2 staging. For newly generated assets, read camera, body/head direction, gaze, hand contacts and weapon state from the asset-specific brief below; the common identity anchor supplies equipment construction, not its current use.

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


## Relic application approval (2026-10-24)

The user approved applying all five relic illustrations and installing them locally. The described candidate forms are now the selected relic artwork; earlier pending-selection wording records only the previous review stage. Runtime uses the existing model filenames; gameplay behavior was not changed by the image promotion.

## Branch B Direction — 2026-10-24

**User-confirmed standing reuse:** combat, merchant and rest use the complete adopted `base.png` painting. Its existing combat-ready stance supports the shared Branch B reuse rule, and the user explicitly selected this reuse. Preserve the source pose, equipment, original margins and pixels; only whole-image proportional Fit resizing is authorized. The runtime uses one `standing.png` for all three scenes. The base remains unchanged.

**Selection revision approved and applied:** Selection cameras are independent of the standing/combat facing direction. Explore four face-tile compositions: left-facing eye-level, right-facing eye-level, high camera looking down, and low camera looking up. Preserve adult facial proportions and a face-dominant frame. Original Silent and Defect tiles demonstrate distinct head angles and gaze directions; do not enforce right-facing UI. The user selected candidate C: the high-angle face tile with a concentrated expression. Its near-frontal face still turns slightly right, while the downward camera is clearly distinct. The candidate retains more fine hair/face detail than the native stylized tiles.

The requested background action is the held breath immediately before drawing: one hand grips the sword hilt, the other braces its scabbard, with no exposed steel. Black-crimson curse currents gather around the closed scabbard throat. Stage a nearly frontal coiled torso on the right, head lowered toward screen left, flowing hair and cloth, and a quiet left information area. The new candidate follows this action; its face and both hands remain visible in an approximate UI overlay, but the ascension panel reaches the lower crossguard. After this overlap was reported, the user explicitly requested applying C and this background. Preserve the selected composition, including this known UI tradeoff. Base owns identity and rendering; original Silent and Regent selection illustrations guide varied orientation and action staging. The fully sheathed pre-draw illustration replaces the earlier drawn-sword selection painting. This request supersedes its preservation-only direction for new background production; standing art remains unchanged.

**User-approved UI, applied:** head-dominant chibi portrait with small silver shoulder/navy collar cues; separately composed chibi head-only map marker; non-chibi close-face selection tile. All preserve silver-white half-tied hair, blue eyes and dark cross earring. Normal tile background is navy `#24354B`; locked tile is a black silhouette over gray `#777777`, derived from the same resized alpha. Portrait outline is white with radius 2px at 128×128. The user approved the complete UI set for application. These UI adaptations do not modify the base. Existing approved crimson energy textures remain unchanged.

Input roles: base owns identity/rendering for every new image. Official topbar/map screenshots guide small-size readability; native Silent/Defect tiles guide the revised tile; Silent/Regent selection art guides the revised background, with the complete Necrobinder screen informing approximate UI placement. No prior mod asset or rejected candidate was a generation input. Rejected combat candidates are excluded by the user's standing override.
