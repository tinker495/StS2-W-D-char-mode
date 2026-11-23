# Branch B: `base → UI, Standing Art, Selection Illustration`

For an Ancient's dialogue/history/map miniature, use [event miniatures](event.md#dialogue-history-and-map-miniatures). Its native dimensions and outline contract come from the owner's UI topic rather than the playable-character output set below.

Read the target's requested output brief/status and visible identity, then [the shared workflow](workflow.md) and active [engine section](engines.md). Read only the relevant row and recipe: standing/combat → ARTS.md 5.1 and 1-C; rest-site seated illustration → ARTS.md 5.1.1 and 1-E; portrait/marker → 5.2 and Recipe 3; selection tiles → 5.2.1 and 4-A/B; energy → 5.2 and 4-C; selection scene → 5.7 and 4-D. A supplied rough uses [the landmark brief](references.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng). Use [observation tools](observation-tools.md) and [conversion commands](scripts/conversion-tools.md) at the requested display size.

1. Open the approved `base.png` as the primary identity/equipment input, plus any UI/scene roughs in `refs/` and the matching official StS2 UI/tile/selection references from [ARTS.md 2.3](/ARTS.md#23-actual-official-image-anchors-and-priority). Follow ARTS.md's common chibi scope; obtain proportion details, visible parts, expressions, and backgrounds from the target art specification.
2. Inspect the base's actual stance, grounding and hand/equipment contacts before choosing reuse or generation, following [ARTS.md 5.1](/ARTS.md#51-combat-rest-and-merchant-standing-art). Export the complete base by proportional Fit for ordinary standing and reuse it for combat when already combat-ready. Use Recipe 1-C only for a noncombat/everyday base or a user-requested distinct combat pose. Record the decision and shared or separate scene mappings in the target asset guide. A static combat-ready PNG is sufficient; animation is not required.
3. Design portrait and marker cameras/margins separately, comparing at `48–85px` against [ui-character-portrait-crop.png](/docs/research/sts2-style-refs/future-assets/ui-character-portrait-crop.png) and [ui-topbar.jpg](/docs/research/sts2-style-refs/future-assets/ui-topbar.jpg). For selection tiles, read the [normal/locked tile study](/docs/research/sts2-character-select-tile-study.md) and open the ten [actual originals](/docs/research/sts2-style-refs/character-select-tiles/README.md), then generate a new face-focused master and compare at `132 × 195`.
4. Where possible, derive normal/locked tiles (`tile-pair`) and portrait outlines (`outline`) from the same final alpha. Resize both energy sizes (`74 × 74` and `24 × 24`) from one master, comparing against [ui-energy-in-combat-crop.png](/docs/research/sts2-style-refs/future-assets/ui-energy-in-combat-crop.png). Do not copy another character's colors, outline thickness, or proportions.
5. A complete Branch B request includes UI, outline, standing/combat coverage according to step 2, and the selection illustration. Coverage can use a shared standing export; it does not require an extra generation. Follow the shared input rule and [Recipe 4-D](ui.md#4-d-char_select_bgpng-1920--1080-selection-illustration) to design a new scene from [original-game selection research](/docs/research/sts2-character-select-composition-study.md) and inspect it with `ui-preview`.
6. Follow the asset guide's model-property and scene contracts. Use an appropriate UI structure such as Control/TextureRect for selection backgrounds. Confirm all requested files and necessary references are connected. For a partial request, produce only that scope. Generating a new combat candidate does not approve its runtime promotion.

### 1-C. `base → Combat Pose` for Branch B

First apply the reuse decision in [ARTS.md 5.1](/ARTS.md#51-combat-rest-and-merchant-standing-art). A combat-ready base supplies both standing and combat through whole-image Fit, without generation. Use this recipe only for a noncombat/everyday base or a user-requested distinct combat pose. Then use the approved base for identity and original-game combat sprites for stance/readability. The new pose belongs to this production run; previous delivered combat sprites are not generation inputs.

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

### 1-E. Rest-Site Seated Illustration (Stage-Inpaint → Cutout → Post-Process)

Use this official three-stage pipeline for dedicated seated rest-site illustrations (read [ARTS.md 5.1.1](/ARTS.md#511-rest-site-seated-illustrations) and [the base-game rest-site study](/docs/research/sts2-rest-site-illustration-study.md)). Do not generate a seated figure blindly on blank alpha—staging directly on the native log locks camera elevation, `~24.5°` log seating, ground contact, co-op spacing, and campfire gaze before extraction.

#### Stage 0: Prepare Stature-Zoomed Native Log Stage and Pure `base.png` Style Anchors
1. **Measure canonical `base.png` proportions and stature tier first:** check the character's actual head-to-body ratio in `base.png` and stature tier in `<char>-art.md` / [runtime scene contract](/docs/research/sts2-cutout-runtime-contract.md#room-scale-and-ground-line-contract) (`standard/adult` `~225–235 px` seated body height vs. `petite/youthful` `~190–205 px` seated body height, plus headwear clearance).
2. **Render the installed `overgrowth_rest_site.tscn` left-log stage at stature-matched native `4:3` (`1200 × 896`) zoom** into an ignored `drafts/` directory so the seated character always fills **`~75–82%` of canvas height** (high pixel budget preventing small-sprite thick-outline drift) with zero resize or aspect stretch:
   - **Standard / adult tier (`2.5×` zoom):** screen window `(491, 507.6)` to `(971, 866)`, `Character_1 = (400.0, 521.0)`.
   - **Petite / youthful tier (`2.85×` zoom):** screen window `(538, 530)` to `(959, 844.4)`, `Character_1 = (322.05, 530.10)`. The thicker log in frame naturally anchors shorter compact legs without forcing the generator to draw a tiny low-res face.
3. Build the inpainting stage from the unlit/gamma-lifted log (`RestSiteLighting` and vignette hidden so bark contours stay clear) plus the campfire flame hue-shifted to a warm neutral amber-gold on the right edge (real fire cue for gaze without Overgrowth's neon green cast on base pixels).
4. **Crop a native-resolution `base_style_closeup.png` (upper body & face of `base.png`, RGBA)** to use as the third generation input. **Do NOT pass vanilla StS2 character sprites (`Ironclad`, `Silent`, etc.) into Stage 1**—their heavy dark ink outlines contaminate `base.png`'s clean value-edge painterly finish. Reserve vanilla renders strictly for Stage 0 measurements and Stage 3 four-player co-op verification.

#### Stage 1: Inpaint Seated Character onto the Log Stage
Supply ONLY these three pure inputs: (1) the stature-zoomed `1200 × 896` native warm-fire log stage (exact background geometry lock), (2) full canonical `base.png` (exact proportions, head-to-body ratio, and complete costume), and (3) `base_style_closeup.png` (1:1 rendering finish, lineless/value-edge shading, facial age, lashes, and expression lock). Use positive visual language anchored to `base_style_closeup.png`; avoid dark-outline or comic/cartoon negative phrasing that can activate outline priors.

```text
Use case: image-edit / identity-preserve.
Asset type: seated rest-site character inpainted directly onto the supplied native StS2 wooden-log stage.
Input roles:
1. Target stage canvas (1200x896, 4:3): edit directly onto this image; preserve the exact elevated (~35-40 degree) top-down three-quarter camera, wooden log angle/bark, grass, stones, and glowing warm campfire flame on the right edge pixel-for-pixel outside the character and subtle contact shadow.
2. Full canonical base.png: owns exact canonical proportions (<MEASURED_HEAD_RATIO_AND_STATURE_TIER>) and complete costume construction; omit combat weapons.
3. Native-resolution base_style_closeup.png: ABSOLUTE art-style, linework, material finish, and facial-appeal master (painted by the exact same artist). Match its lineless / value-edge painterly cel-plus-soft-form rendering—edges on hat, cloth, fur, hair, armor, and boots formed by clean value contrast and soft colored occlusion shadows, reserving delicate fine linework strictly for eyelashes, brows, subtle mouth, and thin jaw edge. Preserve exact canonical face proportions, eye size/luminosity, vibrant clean palette, and warm relaxed expression.
Subject: <CHARACTER_ANCHOR adapted to unarmed seated rest; omit combat weapons>.
Seating & log sync: hips sit perched on the rounded upper ridge of the bare diagonal wooden log in the lower-left front half (leaving the upper-right half of the log completely empty for co-op Character_3; ZERO rock, boulder, cushion, or bench). Legs bend naturally forward-down over the log's front face with feet/boots grounded cleanly at the <FLOOR_OR_LOG_BASE_CONTACT matching stature tier>; coat/cloak drapes compactly beside the hips and front of the log without sprawling onto the grass or upper-right seat.
Gaze (mandatory): head turned three-quarter right and angled gently toward the glowing campfire flame on the right; pupils look squarely toward the fire—strictly zero eye contact with the camera.
Cutout-ready silhouette: paint NO grass blades, twigs, moss, or log bark overlapping in front of boots, ankles, legs, or hems; keep outer contours crisp and unobstructed, with only a subtle soft dark contact shadow underneath hips, resting hand (if on log), and boots.
```

**Mandatory Stage 1 style & registration gate:** inspect the candidate at 200% zoom directly beside `base.png` before running Stage 2. Reject immediately if dark ink outlines appeared around hat/fur/armor/boots, if apparent age or head-to-body ratio drifted, if gaze looks at the camera, or if border-masked `ECC` correlation drops below **`0.99`** (indicating the generator repainted/shifted the log background).

#### Stage 2: Register (`ECC >= 0.99`) and Analytically Cut Out from the Stage
1. Align the Stage 1 image back onto the exact canonical `1200 × 896` stage grid via border-masked `cv2.findTransformECC` (excluding the character and any far-corner artifacts) and confirm **`ECC >= 0.99`** before fitting per-channel linear color calibration (`a * stg + b`) on the border pixels.
2. Separate background cast shadow from foreground pixels before segmentation: pixels that only darkened relative to `stg_matched` while keeping background hue/cosine similarity (`L_inp < L_stg` with true olive-green/bark hue `G > R + 1.5, H ∈ [33..76]` or `cos_sim > 0.988`) are **cast shadow**, never opaque foreground. Never use broad `H >= 28` filters that eat warm-brown interior armor/leather contours.
3. Run GrabCut initialized from the shadow-excluded diff trimap, fill enclosed interior contour holes (`< 900 px`) while preserving real border-connected gaps (such as between legs), solve analytical background-difference alpha ONLY on the exterior boundary band (`dist <= 2.2 px` from border-connected transparent exterior) with boundary color decontamination (`F_clean = (I − (1 − α)B) / max(α, 0.25)`), and force all deeper interior pixels (`dist > 2.2 px`) to solid `alpha = 255`.
4. Inspect the raw cutout on light (`#E8E2D5`), dark (`#1B1924`), and high-contrast magenta (`#FF00FF`) boards beside `base.png` to verify zero interior pinholes, zero green/bark fringe, zero enclosed arm/waist background triangles, and exact `base.png` art-style parity.

#### Stage 3: Post-Process, Lighting Masks, and Four-Act / Co-op Verification
1. Strip any tiny residual boundary specks, calibrate warm-shifted cloth/accent hues smoothly back toward `base.png` medians, and enforce `alpha <= 8/255 → (0, 0, 0, 0)` ([ARTS.md section 8](/ARTS.md#8-production-records-and-verification)).
2. Extract a clean, feathered pure-black `(0, 0, 0, α)` soft contact shadow from the lower seated zone (`hips/coat` on log + `boots` on floor) and composite it behind the character cutout (or keep it as a dedicated shadow layer).
3. Prepare dynamic lighting masks mirroring vanilla Spine slots: a persistent back-left directional `cast_shadow` mask (preserving face readability while grounding the character in the dark clearing, active even during `light_off`) and a fire-facing front-right `rim_highlight` mask tinted per Act (`Overgrowth` `#EBFF52`, `Hive` `#FFD62B`, `Glory` `#00DEF5`, `Underdocks` `#33F5E0`) and disabled by `light_off`.
4. Compute exact hip-anchored `(position, scale)` in `Character_1` local space from the registered stage coordinates (keeping `scale = 0.8` on a `2.5×` stage when the painted stature already hits the target tier; only use minor hip-anchored adjustments, never large downscales to disguise mature/elongated proportions), keeping total source dimensions within `<= 1536 × 1536` ([install package policy](/docs/conventions/packaging/install-package.md)).
5. Render and visually inspect headless MegaDot composites across all four Act rest rooms (`Overgrowth`, `Hive`, `Glory`, `Underdocks`), `light_off`, and four-player co-op (`Character_1..4`, testing both `FlipX()` right-side seats and back-seat `Character_3` directly beside vanilla `Ironclad`, `Silent`, and `Defect`) to verify both log sync and relative body stature against vanilla characters before recommending promotion or optional Stage 4 animation/blink rigging.

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

1. Follow the [Branch B/C input rule](workflow.md#starting-inputs). Select only the approved base and actual originals from the [selection-composition study](/docs/research/sts2-character-select-composition-study.md), recording input roles. Exclude previous backgrounds and generation drafts.
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
