# Shared Asset Prompt Recipes

Generic templates for branches in the [pipeline](SKILL.md). Read [ARTS.md](../../../ARTS.md) for rendering/composition, `docs/characters/<char>/<char>-art.md` for character values, and `<char>-assets.md` for exact delivery paths and dimensions.

**Chibi scope is shared.** Apply it to `character_icon`, its outline, and `map_marker`; keep canonical proportions for selection tiles, standing art, cards, and selection illustrations. Character documents supply face, costume, ornament, palette, and staging within that scope.

Generate new assets directly in the target composition; use Recipe 1-B for local edits to selected images. In both cases compare the whole painting and deliver through full-image Fit resizing. Do not solve composition through cropping, extracted-region enlargement, or automatic Cover fitting. Shapes outside the frame below are part of the generation camera design.

## Shared Input Slots

| Slot | Source |
|---|---|
| `<CHARACTER_ANCHOR>` | Visible parts of the canonical appearance, face, apparent age, and equipment in the character art specification |
| `<IDENTITY_CUES>` | Face, hair, and costume identifiers retained in the portrait |
| `<CHIBI_DETAILS>` | Character-specific proportions, expression, and silhouette within the common chibi scope |
| `<PALETTE>`,`<MATERIALS>` | Color roles and materials in the character art specification |
| `<ASSET_SUBJECT>`,`<SCENE_SENTENCE>` | Character themes, recurring objects, and chosen action |
| `<FRAMING>`,`<OUTPUT_CONTRACT>` | Target composition, UI overlap, and asset-guide dimensions/paths |
| `<STYLE>` | Shared Block A in ARTS.md section 7.2, adapted to the use case |

Do not fill slots with another character's example values. Write reusable descriptions of appearance, composition, palette, and preserved constraints in the character document. Full generation calls may be kept in optional Git-ignored drafts; assembling a prompt must not require those records.

## Recipe 1: New Canonical Art and Local Edits

Use 1-A for new standing art/compositions and 1-B for modifying a specific part of a user-selected image. Recipe 1-B also applies to selected UI, card, and selection-scene artwork.

### 1-A. `refs → base` New Standing Master

Read each reference's adopted/excluded roles in the art specification. If a base exists, distinguish preserved identity from the newly designed composition.

```text
Use case: stylized-concept / identity-preserve.
Asset type: full-body standing master for <CHARACTER>.
Input roles: <each reference's adopted and excluded features; existing base owns approved identity/rendering when present>.
Subject: <CHARACTER_ANCHOR>. Preserve the character-specific apparent age and natural non-SD proportions.
Composition: <FRAMING, battle direction, grounded stance, tool placement and clear margins>. Design the entire final canvas directly, with all required extremities inside it.
Head and gaze: <battle direction, head angle and eye direction, specified separately>. Preserve the approved facial anatomy and apparent age.
Proportions: <character and equipment proportions supported by the references and user choices>.
Style: <STYLE>. Materials and palette: <MATERIALS, PALETTE>.
Output: <OUTPUT_CONTRACT>; transparent background where required. No floor shadow, scenery, glow, UI, text, frame or watermark.
```

### 1-B. Local Edits to a Selected Image

Specify inputs in this order: **exact user-selected source → changed axis → preserved features**. Open the actual filename, attachment, or version to confirm it, and use it as the sole edit target. Limit any additional reference to the relevant part.

- Separate axes such as eye direction/head rotation and length/width/thickness. Send only the requested axis; do not turn a small adjustment into a new composition or body redesign.
- Describe change relative to the selected source. If a numeric amount was not supplied by the user, state its observational basis and provisional status. Do not add unrequested absolute ratios or body-size targets.
- List neighboring axes that could be confused with the edit, plus preserved face, pose, character scale, framing, and style. Do not paste the full equipment specification or new-generation recipe again.

```text
Use case: precise-object-edit / identity-preserve.
Edit target: <exact user-selected image and version>. This image owns the current design and rendering.
Change only: <requested part and dimension, with a restrained change relative to this input>.
Preserve: <neighboring dimensions and identity/pose/scale/framing/materials that must stay fixed>.
Output: the complete edited image in the same canvas and framing, preserving existing transparency.
```

After generation, open the selected source and new result side by side with `compare`. Check both the requested difference and preserved features. If another axis changed, record it as unresolved and branch again from the selected source. Do not accumulate failed edits as subsequent inputs. Present changes, remaining differences, and the comparison; promotion follows the skill's existing approval scope.

### 1-C. `base → Combat Pose` for Branch B

Ordinary standing art is a whole-image Fit export of the approved base and needs no generation call. Generate combat art separately, using the approved base for identity and original-game combat sprites for stance/readability. The new pose belongs to this production run; previous delivered combat sprites are not generation inputs.

```text
Use case: stylized-concept.
Asset type: isolated full-body combat-ready idle sprite for <CHARACTER>, distinct from ordinary standing art.
Input roles: approved base.png owns identity, apparent age, anatomy, equipment construction/proportions, palette and rendering; official combat references guide grounded readiness and display-size clarity only.
Subject: <CHARACTER_ANCHOR> with natural canonical non-SD proportions.
Combat pose: <weapon-appropriate stance, planted feet, knee bend, weight distribution, torso engagement, exact hand contacts and grip spacing>. Keep the pose stable enough for a reusable combat idle PNG. Design it anew instead of repeating the base's relaxed stance.
Direction: <body direction, restrained head angle, and pupil direction separately>. Preserve the approved face and forward attention.
Composition: complete figure AND complete equipment, separated from the face, with clear transparent margins. Generate directly at the intended aspect ratio; no foreshortening that disguises equipment length, clipping, or later cropping.
Style: <STYLE>. Materials and palette: <MATERIALS, PALETTE>. Match the approved rendering and actual display size.
Output: <OUTPUT_CONTRACT>, true transparent alpha. No environment, ground shadow, aura, motion trail, particles, UI, words, frame or watermark.
```

Compare the new combat pose against the ordinary standing export at the same display size. Inspect balance, both hands, equipment continuity, silhouette, margins, face identity, and gaze on light/dark backgrounds. Save candidates separately; runtime promotion follows the user's approval scope.

## Recipe 2: Surface Cleanup

Use when surface noise is visible at actual size. Do not generate again when the user requests applying the selected painting unchanged.

```text
Use case: identity-preserve / rendering-cleanup.
Edit target: <ASSET>. Preserve identity, equipment construction, pose, proportions, framing and transparency.
Rendering cleanup only: <ARTS.md7.2 Block D>. Consolidate unnecessary fragments while preserving material-specific light/shadow transitions, palette and roughness. Do not redesign equipment, change facial anatomy, blur or sharpen the image.
```

## Recipe 3: `base → Chibi UI`

### 3-A. `character_icon.png`

```text
Use case: identity-preserve / style-transfer.
Asset type: chibi character portrait for <OUTPUT_CONTRACT>.
Input: approved base.png for identity and equipment.
Subject: <IDENTITY_CUES>. Apply the common chibi portrait treatment with <CHIBI_DETAILS>.
Composition: directly paint a head-dominant portrait with only the specified small shoulder/costume cues at the bottom. Keep the face readable at the actual small display size. <FRAMING>.
Style: <STYLE>. Palette: <PALETTE>. True transparent background.
Exclude: weapon, hands, scenery, badge/frame, text and UI unless the actual asset contract requires them.
```

Generate the outline from the final small PNG's alpha. Read radius and color from the target asset guide.

### 3-B. `map_marker.png`

```text
Use case: identity-preserve / stylized-concept.
Asset type: chibi head-only map marker for <OUTPUT_CONTRACT>.
Input: approved base.png and the new chibi portrait created in this production run as an identity/proportion reference.
Subject: <IDENTITY_CUES, CHIBI_DETAILS>. Head and identifying headwear only.
Composition: design the required complete silhouette and transparent margin directly in the square. <FRAMING>. Use the whole painting.
Style: <STYLE>. Palette: <PALETTE>. True transparent background.
Exclude: shoulders, torso, hands, weapons, pin/badge borders, text and drop shadow.
```

## Recipe 4: `base → Standard UI`

### 4-A. `char_select.png` (`132 × 195` Opaque Tile)

Inputs are the approved base and [actual base-game tiles](../../../docs/research/sts2-style-refs/character-select-tiles/README.md). The base owns identity; originals guide face occupancy, camera, and information density. Read the [tile study](../../../docs/research/sts2-character-select-tile-study.md) and [ARTS.md section 5.2.1](../../../ARTS.md#521-selection-tiles).

```text
Use case: identity-preserve / stylized-concept.
Asset type: newly painted character-select FACE portrait for <OUTPUT_CONTRACT>.
Subject: <CHARACTER_ANCHOR adapted to visible head/neck cues>. Preserve canonical non-chibi anatomy and age appearance.
Composition: <FRAMING>. Let face/mask and expression dominate the narrow panel as in the supplied actual game tiles. Keep torso and nonessential accessories outside this close camera. Paint this final layout directly, not a distant figure for later extraction.
Style: <STYLE>. Background master: genuine transparent alpha for a shared normal/locked silhouette.
Exclude: scenery, state UI, painted borders, text, particles and watermark.
```

Preview the whole painting at actual size beside base-game tiles. Regenerate small faces or unsuitable margins. Finalize the full-image Fit alpha and share it between normal/locked outputs. Read background colors from the character art specification and deliver opaque composited PNGs.

### 4-B. `char_select_locked.png`

Share the resized alpha of the complete approved normal master. Follow the original locked tiles and target specification for silhouette treatment and background/figure colors, removing internal facial information. Distinguish deselected UI tint from a locked texture. Do not generate a different head direction or contour with a separate AI call.

If only an opaque master exists, do not mix its background into the figure using an arbitrary brightness threshold. Obtain a transparent master and output normal/locked variants from the same alpha. Inspect matching composition, dimensions, and outline in actual previews.

### 4-C. `big_energy.png` & `text_energy.png`

```text
Use case: stylized-concept.
Asset type: one energy-icon master exported to the two sizes in <OUTPUT_CONTRACT>.
Subject: <ASSET_SUBJECT, MATERIALS, PALETTE>.
Composition: <FRAMING>. Keep the actual central cost-number overlay area quiet and readable; use a few connected shapes that survive the smallest display size.
Style: <STYLE>. Transparent outside the icon. No painted cost number, text, frame or outer glow.
```

### 4-D. `char_select_bg.png` (`1920 × 1080` Selection Illustration)

1. Follow the [Branch B/C input rule](SKILL.md#branch-b-and-c-start-with-new-artwork). Select only the approved base and actual originals from the [selection-composition study](../../../docs/research/sts2-character-select-composition-study.md), recording input roles. Exclude previous backgrounds and generation drafts.
2. Write a new scene sentence from the character's distinctive action and equipment/power relationship. Do not copy descriptions of the old background's pose or camera.
3. Connect face, active hand, and a second major shape from a new camera. Extend that movement or ability into the background.
4. Compare with the previous delivery only after the new painting exists. Output the whole painting with Fit and inspect an approximate UI overlay. Resolve conflicts by changing the generated composition.

```text
Use case: identity-preserve / stylized-concept.
Asset type: full-screen selection illustration using the approved non-chibi proportions and age appearance, <OUTPUT_CONTRACT>.
Input: approved base.png owns identity/materials; actual official selection references guide visual language. This is a newly composed scene from these inputs.
Scene: <SCENE_SENTENCE>. Subject: <CHARACTER_ANCHOR adapted to visible parts and action>.
Composition: connect face, active hand and equipment/power through a clear directional flow. Use a characteristic foreground object, cloth or ability shape as the second major mass. Paint the camera/perspective directly in the final canvas.
UI overlap: keep eyes, fingers and bright action contact clear of text/tile/confirm controls. Low-contrast character-specific shapes may extend behind UI; do not leave an arbitrary empty rectangle.
Style: <STYLE adapted to selection-scene depth>. Background: <PALETTE and character-specific motifs related to the action>.
Exclude: painted UI, words, numbers, frames, watermark and other characters' identity details.
```

Read scene, property, and runtime paths from the target asset guide.

## Recipe 5: `base → Relic Icon`

Input the base for character, costume, or equipment-related objects; also input an object-family master newly generated and established in this run. New/replacement production follows the [Branch B/C input rule](SKILL.md#branch-b-and-c-start-with-new-artwork).

```text
Use case: stylized-concept.
Asset type: isolated relic icon for <OUTPUT_CONTRACT>.
Subject: <ASSET_SUBJECT, canonical object construction, MATERIALS, PALETTE>.
Composition: a single centered object, approximately72-84% occupancy as a starting point. Prioritize a clear silhouette and a few large readable value families at the actual small size.
Style: <STYLE>. True transparent background. No floor shadow, pedestal, scenery, text or UI.
```

Resize large to small, then derive the outline from the small alpha.

## Recipe 6: `base → Power Emblem`

```text
Use case: stylized-concept.
Asset type: single symbolic status emblem for <OUTPUT_CONTRACT>.
Subject: <ASSET_SUBJECT derived from the actual power meaning and character's visual theme>.
Composition: a few bold interlocking shapes, distinct silhouette and <PALETTE>. Keep the primary feature central/upper-left and the actual bottom-right stack-number area quiet.
Style: <STYLE>. True transparent background. No painted numbers, letters, box/frame or scene background.
```

## Recipe 7: `base → Standard Card Portrait`

Input the base for the character, body parts, or unique equipment. For recurring objects, also use family masters newly generated and established in this run. Read card names, types, themes, and counts from target documents. New/replacement production follows the [Branch B/C input rule](SKILL.md#branch-b-and-c-start-with-new-artwork).

```text
Use case: identity-preserve / stylized-concept.
Asset type: standard landscape card illustration, <OUTPUT_CONTRACT>.
Input: <required character/object anchors>. Subject: <CHARACTER_ANCHOR and ASSET_SUBJECT adapted to this scene>.
Card action: <one concrete action derived from the actual card effect/type/theme>.
Composition: <FRAMING and actual UI-safe zones>. High-contrast focal foreground, supportive midground, and a quiet low-contrast background. Preserve readable face/hand/tool/contact inside the art window.
Style: <STYLE>. Palette/materials: <PALETTE, MATERIALS>.
Backdrop: full bleed opaque scene. No painted card frame, cost, words, numbers, UI icons or watermark.
```

## Recipe 8: `base → Ancient Full-Art Card`

Read the target card list and actual frame dimensions from the character asset guide.

```text
Use case: identity-preserve / stylized-concept.
Asset type: vertical full-art card illustration, <OUTPUT_CONTRACT>.
Input: <required character/object anchors>. Subject and scene: <CHARACTER_ANCHOR, ASSET_SUBJECT, SCENE_SENTENCE>.
Layout: use the actual title/cost overlay zone, upper-middle focal window and lower description zone from the asset contract. Put face/hands/main action in the focal window and keep the text background quiet. Do not move the primary action by post-generation cropping.
Style: <STYLE>. Palette/materials: <PALETTE, MATERIALS>.
Backdrop: full bleed opaque painting. No painted frame, banner, text box, numbers, letters or watermark.
```
