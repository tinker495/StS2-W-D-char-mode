# Branch B: `base → UI, Standing Art, Selection Illustration`

Read the target's requested output brief/status and visible identity, then [the shared workflow](../workflow.md) and active [engine section](../engines.md). Read only the relevant row and recipe: standing/combat → ARTS.md 5.1 and 1-C; portrait/marker → 5.2 and Recipe 3; selection tiles → 5.2.1 and 4-A/B; energy → 5.2 and 4-C; selection scene → 5.7 and 4-D. A supplied rough uses [the landmark brief](../references.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng). Use [observation tools](../observation-tools.md) and [conversion commands](../scripts/conversion-tools.md) at the requested display size.

1. Open the approved `base.png` as the primary identity/equipment input, plus any UI/scene roughs in `refs/` and the matching official StS2 UI/tile/selection references from [ARTS.md 2.3](../../../../ARTS.md#23-actual-official-image-anchors-and-priority). Follow ARTS.md's common chibi scope; obtain proportion details, visible parts, expressions, and backgrounds from the target art specification.
2. Inspect the base's actual stance, grounding and hand/equipment contacts before choosing reuse or generation, following [ARTS.md 5.1](../../../../ARTS.md#51-combat-rest-and-merchant-standing-art). Export the complete base by proportional Fit for ordinary standing and reuse it for combat when already combat-ready. Use Recipe 1-C only for a noncombat/everyday base or a user-requested distinct combat pose. Record the decision and shared or separate scene mappings in the target asset guide. A static combat-ready PNG is sufficient; animation is not required.
3. Design portrait and marker cameras/margins separately, comparing at `48–85px` against [ui-character-portrait-crop.png](../../../../docs/research/sts2-style-refs/future-assets/ui-character-portrait-crop.png) and [ui-topbar.jpg](../../../../docs/research/sts2-style-refs/future-assets/ui-topbar.jpg). For selection tiles, read the [normal/locked tile study](../../../../docs/research/sts2-character-select-tile-study.md) and open the ten [actual originals](../../../../docs/research/sts2-style-refs/character-select-tiles/README.md), then generate a new face-focused master and compare at `132 × 195`.
4. Where possible, derive normal/locked tiles (`tile-pair`) and portrait outlines (`outline`) from the same final alpha. Resize both energy sizes (`74 × 74` and `24 × 24`) from one master, comparing against [ui-energy-in-combat-crop.png](../../../../docs/research/sts2-style-refs/future-assets/ui-energy-in-combat-crop.png). Do not copy another character's colors, outline thickness, or proportions.
5. A complete Branch B request includes UI, outline, standing/combat coverage according to step 2, and the selection illustration. Coverage can use a shared standing export; it does not require an extra generation. Follow the shared input rule and [Recipe 4-D](ui.md#4-d-char_select_bgpng-1920--1080-selection-illustration) to design a new scene from [original-game selection research](../../../../docs/research/sts2-character-select-composition-study.md) and inspect it with `ui-preview`.
6. Follow the asset guide's model-property and scene contracts. Use an appropriate UI structure such as Control/TextureRect for selection backgrounds. Confirm all requested files and necessary references are connected. For a partial request, produce only that scope. Generating a new combat candidate does not approve its runtime promotion.

### 1-C. `base → Combat Pose` for Branch B

First apply the reuse decision in [ARTS.md 5.1](../../../../ARTS.md#51-combat-rest-and-merchant-standing-art). A combat-ready base supplies both standing and combat through whole-image Fit, without generation. Use this recipe only for a noncombat/everyday base or a user-requested distinct combat pose. Then use the approved base for identity and original-game combat sprites for stance/readability. The new pose belongs to this production run; previous delivered combat sprites are not generation inputs.

```text
Use case: stylized-concept.
Asset type: isolated full-body combat-ready idle sprite for <CHARACTER>, distinct from ordinary standing art.
Input roles: approved base.png owns identity, apparent age, anatomy, equipment construction/proportions, palette and rendering; official combat references (installed-build Spine renders when available) guide camera, body/head orientation, gaze, grounded readiness and display-size clarity.
Subject: <CHARACTER_ANCHOR> with the approved base's canonical non-SD proportions.
Combat pose: <weapon-appropriate stance, planted feet, knee bend, weight distribution, torso engagement, exact hand contacts and grip spacing>. Keep the pose stable enough for a reusable combat idle PNG. Design it for combat use while preserving the base's identity; determine the base's existing pose from the actual image.
Body direction: <torso and pelvis orientation relative to the camera and combat target>.
Head angle: <visible facial planes and near/far eye visibility>. Preserve the approved facial identity.
Gaze: <pupil direction toward the combat target, independent of head rotation>.
Composition: complete figure AND complete equipment, separated from the face, with clear transparent margins. Generate directly at the intended aspect ratio; no foreshortening that disguises equipment length, clipping, or later cropping.
Style: <STYLE>. Materials and palette: <MATERIALS, PALETTE>. Match the approved rendering and actual display size.
Output: <OUTPUT_CONTRACT>, true transparent alpha. No environment, ground shadow, aura, motion trail, particles, UI, words, frame or watermark.
```

Compare the new combat pose against the ordinary standing export at the same display size. Inspect balance, both hands, equipment continuity, silhouette, margins, face identity, and gaze on light/dark backgrounds. Save candidates separately; runtime promotion follows the user's approval scope.

## Recipe 3: `base → Chibi UI`

### 3-A. `character_icon.png`

```text
Use case: identity-preserve / style-transfer.
Asset type: chibi character portrait for <OUTPUT_CONTRACT>.
Input: approved base.png for identity and equipment; official portrait reference (docs/research/sts2-style-refs/future-assets/ui-character-portrait-crop.png) for small-scale readability.
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

Inputs are the approved base and [actual base-game tiles](../../../../docs/research/sts2-style-refs/character-select-tiles/README.md). The base owns identity; originals guide face occupancy, camera, and information density. Read the [tile study](../../../../docs/research/sts2-character-select-tile-study.md) and [ARTS.md section 5.2.1](../../../../ARTS.md#521-selection-tiles).

When the camera is undecided, explore distinct camera/head-angle candidates before choosing one. Select contrasting directions or elevations from the native examples; the number follows the requested scope, not a fixed quota. Open a whole-image comparison at 132×195 with native tiles, then record the recommended candidate and its visual tradeoffs. An already selected composition needs no new variation round.

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
Input: <ROUGH_OR_REF_INPUT when supplied>; official energy reference (docs/research/sts2-style-refs/future-assets/ui-energy-in-combat-crop.png or installed energy_necrobinder.tres / energy_*.tres) for rim/inset readability.
Subject: <ASSET_SUBJECT, MATERIALS, PALETTE>.
Composition: <FRAMING>. Keep the actual central cost-number overlay area quiet and readable; use a few connected shapes that survive the smallest display size.
Style: <STYLE>. Transparent outside the icon. No painted cost number, text, frame or outer glow.
```

### 4-D. `char_select_bg.png` (`1920 × 1080` Selection Illustration)

1. Follow the [Branch B/C input rule](../workflow.md#starting-inputs). Select only the approved base and actual originals from the [selection-composition study](../../../../docs/research/sts2-character-select-composition-study.md), recording input roles. Exclude previous backgrounds and generation drafts.
2. Write a new scene sentence from the character's distinctive action and equipment/power relationship. Do not copy descriptions of the old background's pose or camera.
3. Connect face, active hand, and a second major shape from a new camera. Extend that movement or ability into the background.
4. Compare with the previous delivery only after the new painting exists. Output the whole painting with Fit and inspect an approximate UI overlay. Identify the action-defining contact (for example, fingers/thumb on a grip or a weapon meeting its sheath). Keep the face and active grip clear of the information panel and bottom tiles; judge whether the action remains readable, not whether every equipment pixel avoids every panel. Natural anatomy takes priority over completely clearing the translucent ascension panel: a supporting equipment tip, lower crossguard or lower arm may pass partly behind it when the action still reads. Preserve credible shoulder/elbow/wrist connections and natural sheath height rather than lifting the assembly to chest level just to clear the panel. Record these tradeoffs; regenerate when essential expression or action becomes unreadable, unless the user explicitly selects that composition.

The bundled 1920×1080 layout is an approximate observation: information x=220..880, y=330..700; ascension x=650..1260, y=740..850; tiles y=880..1016. Use it as a preview starting point, not a universal exclusion mask; replace it with the actual target-screen geometry when available.

```text
Use case: identity-preserve / stylized-concept.
Asset type: full-screen selection illustration using the approved non-chibi proportions and age appearance, <OUTPUT_CONTRACT>.
Input: approved base.png owns identity/materials; actual official selection references guide visual language. This is a newly composed scene from these inputs.
Scene: <SCENE_SENTENCE>. Subject: <CHARACTER_ANCHOR adapted to visible parts and action>.
Composition: connect face, active hand and equipment/power through a clear directional flow. Use a characteristic foreground object, cloth or ability shape as the second major mass. Paint the camera/perspective directly in the final canvas.
UI overlap: keep eyes and active grip/action readable and clear of information text, tiles and confirm controls. Preserve natural anatomy and equipment height; lower supporting forms may pass behind the translucent ascension panel if the action stays readable. Low-contrast character-specific shapes may extend behind UI; do not leave an arbitrary empty rectangle.
Style: <STYLE adapted to selection-scene depth>. Background: <PALETTE and character-specific motifs related to the action>.
Exclude: painted UI, words, numbers, frames, watermark and other characters' identity details.
```

Read scene, property, and runtime paths from the target asset guide.
