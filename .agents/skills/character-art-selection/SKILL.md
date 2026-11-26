---
name: character-art-selection
description: Create normal and locked character-select tiles and full-screen selection illustrations.
---

Follow the [shared inputs, engine selection and delivery](/.agents/skills/character-asset-pipeline/SKILL.md#shared-production-contract) for the requested scope.

Use the approved base and same-use official tiles or selection scenes. A locked tile shares the approved normal tile silhouette. Choose the full-screen recipe for the selection background.

### 4-A. `char_select.png` (`132 × 195` Opaque Tile)

Inputs are the approved base and [actual base-game tiles](/docs/research/sts2-style-refs/character-select-tiles/README.md). The base owns identity; originals guide face occupancy, camera, and information density. Read the [tile study](/docs/research/sts2-character-select-tile-study.md) and [ARTS.md section 5.2.1](/ARTS.md#521-selection-tiles).

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

### 4-D. `char_select_bg.png` (`1920 × 1080` Selection Illustration)

1. Follow the [Branch B/C input rule](/.agents/skills/character-asset-pipeline/workflow.md#starting-inputs). Select only the approved base and actual originals from the [selection-composition study](/docs/research/sts2-character-select-composition-study.md), recording input roles. Exclude previous backgrounds and generation drafts.
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
