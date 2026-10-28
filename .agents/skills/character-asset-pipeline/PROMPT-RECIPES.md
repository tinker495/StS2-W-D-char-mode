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
| `<ROUGH_OR_REF_INPUT>` | User-supplied rough sketch, symbol draft, object photo (`refs/`), or agent-staged layout/pose rough (`drafts/`), with adopted vs. excluded traits |
| `<OFFICIAL_ANCHOR>` | Same-use-case official StS2 reference image(s) from [sts2-style-refs](../../../docs/research/sts2-style-refs/README.md) (or temporary local PCK extraction), guiding display-size density and value grouping; framing only when no user rough or selected composition owns it |
| `<ENGINE_MODE>` | Engine-specific invocation (`Codex / GPT Image` via `image_gen` with `referenced_image_paths` and `transparent_background`, `Antigravity / Nano Banana` via `image-generator` subagent brief, or dual-engine comparison; see [SKILL.md](SKILL.md#image-generation-engines-codex-gpt-image-vs-antigravity-nano-banana)) |
| `<PALETTE>`,`<MATERIALS>` | Color roles and materials in the character art specification |
| `<ASSET_SUBJECT>`,`<SCENE_SENTENCE>` | Character themes, recurring objects, and chosen action |
| `<FRAMING>`,`<OUTPUT_CONTRACT>` | Target composition, UI overlap, and asset-guide dimensions/paths |
| `<STYLE>` | Shared Block A in ARTS.md section 7.2, adapted to the use case |

Do not fill slots with another character's example values. Write reusable descriptions of appearance, composition, palette, and preserved constraints in the character document. Full generation calls may be kept in optional Git-ignored drafts; assembling a prompt must not require those records.

## Recipe 1: New Canonical Art and Local Edits

Use 1-A for new standing art/compositions, 1-B for modifying a specific part of a user-selected image, 1-C for a separate combat pose, and 1-D whenever a rough sketch, layout guide, or object/motif reference image drives the composition or subject across any asset family. Recipe 1-B and 1-D also apply to UI, relic, power, card, and selection-scene artwork.

For character orientation, write body direction, head angle, and gaze as separate prompt fields. Describe head angle through the visible facial planes and the extent of the near/far eyes, using the target specification and references. A screen direction alone does not determine head rotation.

**Source labels:** record each adopted feature as *user-confirmed* (a user decision), *reference-observed* (seen in a raw reference; evidence, not approval) or *provisional* (an agent choice absent from confirmed references). Preserve by these labels. A provisional choice stays provisional even when the output looks successful, and is reassessed when the use case changes.

### 1-A. `refs → base` New Standing Master

Read each reference's adopted/excluded roles in the art specification. If a base exists, distinguish preserved identity from the newly designed composition.

Before drafting the prompt, assign an input owner for each axis: identity/equipment, camera, pose/direction, and rendering. Pass the raw character reference directly as an image input for identity, facial appeal, proportions, and costume construction; use approved character bases (such as `docs/characters/realta/base.png` or `docs/characters/althea/base.png`) or official sprites as needed for clean 2D game-sprite readability and grounded combat framing. Do not withhold the character reference (`style-pure` text-only generation) or force a rough gouache/chunky-proportion conversion that degrades the character's facial appeal, sleek build, or clean palette. Resolve conflicts between inputs in this assignment (for example, expanding a bust reference into a full-body combat stance), then make the prompt follow it. A bust reference supplies appearance, not a default full-body camera. Branch A establishes a canonical base; its name does not prescribe a frontal or relaxed pose.

Record the actual image-input list in the before/after notes.

Keep a short before/after record in optional draft notes. Define concrete items before generation; after generation inspect those same items in the comparison sheet and fill every observation. List equipment dimensions and hand contacts as separate items.

| Treatment | Before generation | After generation |
|---|---|---|
| Change | Requested axis, intended difference, and source of the request | Observed change and remaining mismatch |
| Preserve | Visible features and their user-confirmed or reference-observed basis | Retained features and any unintended drift |
| Provisional | Agent-selected details absent from confirmed references | Resulting interpretation, still unapproved |

Rewrite the prompt around the current objective instead of appending another priority block to an old prompt. Lead with concrete rendering observations from the relevant originals, then preserved identity, composition/contacts, and output. Prefer positive descriptions of the intended shapes and materials; retain only exclusions needed to prevent specific confusion or scope violations. Input order can express intent but is not proof of causal influence on the result. Recipe 1-B retains its selected-source-first order for local edits.

```text
Use case: stylized-concept / identity-preserve.
Asset type: full-body standing master for <CHARACTER>.
Input owners: identity/equipment/proportions = <raw character reference and CHARACTER_ANCHOR>; camera/pose/direction = <requested stance and same-use-case reference>; rendering = <clean 2D cel-plus-soft-form sprite style from ARTS.md Block A>. Image inputs: <actual list>. <Adopted/excluded features; preserve approved identity/rendering when present>.
Rendering target: <clean 2D cel-plus-soft-form shading, crisp dark material-colored contours, smooth faceted metal, and clean fabric/hair values from ARTS.md Block A for the target display size>.
Subject: <CHARACTER_ANCHOR>. Preserve the character-specific apparent age, sharp facial appeal, and sleek canonical proportions.
Materials and palette: <MATERIALS, PALETTE>. Preserve: <confirmed visible features>. Provisional: <unconfirmed design choices>.
Composition: <FRAMING, battle direction, grounded stance, tool placement and clear margins>. Design the entire final canvas directly, with all required extremities inside it.
Body direction: <torso and pelvis orientation relative to the camera and target>.
Head angle: <visible facial planes and near/far eye visibility>. Preserve the approved facial anatomy and apparent age.
Gaze: <pupil direction and attention target, independent of head rotation>.
Proportions: <sleek canonical body proportions from the character specification and ARTS.md 2.4; equipment proportions supported by the references and user choices>.
Output: <OUTPUT_CONTRACT>; transparent background where required. No floor shadow, scenery, glow, UI, text, frame or watermark.
```

### 1-B. Local Edits to a Selected Image

Specify inputs in this order: **exact user-selected source → changed axis → preserved features**. Open the actual filename, attachment, or version to confirm it, and use it as the sole edit target. Limit any additional reference to the relevant part.

- Separate axes such as eye direction/head rotation and length/width/thickness. Name the requested change and the minimum connected parts needed to make it coherent; preserve unrelated axes.
- Describe change relative to the selected source. If a numeric amount was not supplied by the user, state its observational basis and provisional status. Do not add unrequested absolute ratios or body-size targets.
- List neighboring axes that could be confused with the edit, plus preserved face, pose, character scale, framing, and style. Do not paste the full equipment specification or new-generation recipe again.
- For hand/equipment edits, identify each affected arm by its shoulder and near/far position, then trace shoulder → elbow → wrist → hand → contact target. Specify which joints must move and which remain fixed. Check that each required hand and its contact remain readable after the edit; screen-left/right hand labels alone do not establish the arm connection.
- For overlap edits, distinguish anatomical front/back from camera depth. In a rear view, equipment behind the character's back may be nearer the camera and visible over the coat. State which surface occludes which, where the object enters/exits the silhouette, and how its centerline continues through the grip. Preserve unrelated pose and equipment design.
- For requests to reduce detail, keep the selected image's staging and use Recipe 2-A. Identify the surfaces and background marks to merge; simplifying the prompt alone does not establish that the painting became simpler.

```text
Use case: precise-object-edit / identity-preserve.
Edit target: <exact user-selected image and version>. This image owns the current design and rendering.
Change only: <requested part and dimension, with a restrained change relative to this input; necessary connected joints/contacts when applicable>.
Depth and continuity: <camera-near to camera-far order at the affected crossing, visible/hidden segments, and hand/tool connection; for overlap edits>.
Body direction: <preserved or changed torso/pelvis orientation; for character orientation edits>.
Head angle: <preserved or changed facial planes and near/far eye visibility; for character orientation edits>.
Gaze: <preserved or changed pupil direction and target; for character orientation edits>.
Preserve: <neighboring dimensions and identity/pose/scale/framing/materials that must stay fixed, with their source labels>.
Output: the complete edited image in the same canvas and framing, preserving existing transparency.
```

After generation, use the [default review loop](SKILL.md#default-generatereviewcorrect-loop). Compare the selected source and result, checking the requested difference, depth/continuity, and preserved features. Correct clear defects within the bounded loop before the final recommendation; after unrelated drift, branch again from the selected source. Promotion follows the skill's existing approval scope.

### 1-C. `base → Combat Pose` for Branch B

First apply the reuse decision in [ARTS.md 5.1](../../../ARTS.md#51-combat-rest-and-merchant-standing-art). A combat-ready base supplies both standing and combat through whole-image Fit, without generation. Use this recipe only for a noncombat/everyday base or a user-requested distinct combat pose. Then use the approved base for identity and original-game combat sprites for stance/readability. The new pose belongs to this production run; previous delivered combat sprites are not generation inputs.

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

### 1-D. Rough Sketch or Reference Image → Finished Asset (`refs/` or `drafts/<guide>.png`)

Use whenever the user supplies a rough sketch, symbol draft, silhouette, or object/photo reference (`refs/<category>/` or chat attachment), or when you construct a layout/pose rough in `drafts/<asset>-rough.png` to lock complex staging before final rendering. Combine this block with the target asset's recipe (1-A, 3, 4, 5, 6, 7, or 8).

- **Always pass the rough or reference file directly as an image input.** Never discard it in favor of text-only description.
- **Write a landmark brief before generation:** record camera/body direction, visible body extent, head/gaze, each hand's position and role, and the object's grip-to-tip direction and contact point. Use rough-relative locations and visible relationships; label any necessary interpretation as an assumption. Preserve these relationships when adapting the canvas ratio by extending the setting around them. Keep the rough first among composition inputs and label each reference's role in the prompt.
- **Assign explicit, non-overlapping owners:**
  - **Rough sketch / layout guide:** owns primary camera angle, silhouette, pose/gesture, focal placement, and core symbol arrangement. Translate rough lines and flats into finished StS2 2D game art without reinterpreting a user-chosen motif from the asset title alone.
  - **Object / photo / motif reference:** owns structural construction, proportions, and functional identifiers. Simplify photorealistic grain, complex engraving, background clutter, lighting mismatch, and watermarks into 2–3 broad value families and crisp contours.
  - **Approved `base.png`:** owns character face, apparent age, proportions, costume, and palette. It supplies weapon construction only when that weapon belongs in the requested scene. The user's card-specific prop description and its rough/object reference own a different weapon's identity and handling; carry the latest correction into subsequent edits.
  - **Current-run object-family masters:** own the recurring prop's construction within the requested object family.
  - **`<OFFICIAL_ANCHOR>`:** owns target display-size information density, contour hierarchy, and safe-zone readability.

After generation, compare the rough and candidate directly: mark each landmark preserved, changed, or uncertain with visible evidence. A matching theme or plausible pose does not resolve a reversed object direction, substituted weapon, changed hand action, or lost body extent. Correct those findings before surface polish; return to the rough when the candidate has drifted. Record remaining differences rather than claiming exact rough fidelity.

```text
Use case: reference-guided-generation / identity-preserve.
Asset type: <target asset family and OUTPUT_CONTRACT>.
Engine mode: <ENGINE_MODE>.
Input owners:
- Rough / motif / object reference (<ROUGH_OR_REF_INPUT>): owns <composition / silhouette / pose / object construction / primary symbol arrangement>. Preserve <adopted rough/reference traits>; exclude <sketch construction lines, photorealistic noise, source background, border, or superseded details>.
- Character & prop anchors (<approved base.png and/or current-run object-family master, when applicable>): own <visible character identity and requested prop construction, separately assigned; omit unrelated base equipment>.
- Official StS2 anchor (<OFFICIAL_ANCHOR>): guides <display-size readability, 2-3 value plane grouping, contour weight, and UI safe-zone framing>; exclude <official character/item identity, UI frame, numbers, and dithering>.
Subject & staging: <translate the rough/reference directly into the finished asset subject and action without reinterpreting the chosen motif>.
Landmarks: <camera/body direction and visible extent; head/gaze; hand positions and roles; requested object type, grip-to-tip direction, and contact point>.
Style: <STYLE>.
Output: <OUTPUT_CONTRACT>.
```

## Recipe 2: Surface Passes

Do not generate again when the user requests applying the selected painting unchanged.

### 2-A. Surface Cleanup

Use when surface noise is visible at actual size or the user asks for less detail. Name the noisy regions, then specify the larger connected shapes that replace them: hair and fur clumps, broad cloth/metal planes, a few continuous liquid/effect shapes, and quiet background masses. Preserve the focal contact and silhouette. Compare source/result beside the official anchor at the same display size; record which competing marks disappeared and whether the action and pose stayed intact.

```text
Use case: identity-preserve / rendering-cleanup.
Edit target: <ASSET>. Preserve identity, equipment construction, pose, proportions, crisp facial features, framing and transparency.
Rendering cleanup only: <ARTS.md 7.2 Block D>. Merge <observed noisy regions> into <larger connected shapes>, keeping the strongest contrast at <focal action/contact>. Preserve clean cel-plus-soft-form transitions, crisp contours, and palette clarity. Do not redesign equipment, coarsen facial anatomy, dull colors, blur, or over-sharpen the image.
```

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

Inputs are the approved base and [actual base-game tiles](../../../docs/research/sts2-style-refs/character-select-tiles/README.md). The base owns identity; originals guide face occupancy, camera, and information density. Read the [tile study](../../../docs/research/sts2-character-select-tile-study.md) and [ARTS.md section 5.2.1](../../../ARTS.md#521-selection-tiles).

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

1. Follow the [Branch B/C input rule](SKILL.md#branch-b-and-c-start-with-new-artwork). Select only the approved base and actual originals from the [selection-composition study](../../../docs/research/sts2-character-select-composition-study.md), recording input roles. Exclude previous backgrounds and generation drafts.
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

## Recipe 5: `base + refs → Relic Icon` (Branch C-1)

Follow [Branch C-1](SKILL.md#branch-c-1-base--refs--relics-focused-relic-route) and the [Branch B/C input rule](SKILL.md#branch-b-and-c-start-with-new-artwork). Open and pass the official StS2 relic anchors ([relics-potions-frame0.png](../../../docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png) and [relics-in-combat-crop.png](../../../docs/research/sts2-style-refs/future-assets/relics-in-combat-crop.png)). Pass `<ROUGH_OR_REF_INPUT>` (object photo, rough sketch, or designated symbol) when supplied, `base.png` only when the relic depicts canonical character costume/weaponry, and the newly generated current-run starter/base relic master when producing an upgraded/paired relic variant.

```text
Use case: stylized-concept / reference-guided-generation.
Asset type: isolated relic icon for <OUTPUT_CONTRACT> (256x256 master exported to 94x94 + 94x94 outline).
Engine mode: <ENGINE_MODE>.
Input owners:
- Object / rough reference (<ROUGH_OR_REF_INPUT or current-run family master, when present>): owns silhouette proportions, structural construction, and key functional identifiers; simplify photo grain, engraving noise, and source backing.
- Character anchor (<approved base.png, only when canonical character gear is depicted>): owns canonical material and equipment construction.
- Official StS2 relic anchor (docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png, relics-in-combat-crop.png): guides bold near-black outer contour, 2-3 broad matte value planes, and 48-64px top-bar readability; exclude source blue backing, numbers, and ticket lettering.
Subject: <ASSET_SUBJECT, canonical object construction, MATERIALS, PALETTE>.
Composition: a single centered isolated object, approximately 72-84% occupancy (preserving at least 2-3px transparent margin at 94x94 so the alpha outline never clips). Prioritize a distinct outer silhouette and 2-3 large connected value families at 48-64px display size.
Style: <STYLE>. True transparent background. No floor shadow, pedestal, mannequin, scenery, fine filigree, text or UI frame.
```

Compare at `94 × 94` and `48 × 48` beside `relics-potions-frame0.png` and `relics-in-combat-crop.png` on dark/light boards. Resize `256 × 256` large to `94 × 94` small, then derive the `94 × 94` outline from the small alpha.

## Recipe 6: `base + refs → Power Emblem` (Branch C-2)

Follow [Branch C-2](SKILL.md#branch-c-2-base--refs--powers-focused-power-emblem-route). Open and pass the official StS2 status icon anchor ([status-icons-in-combat-crop.png](../../../docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png)). When a user-supplied motif rough or symbol PNG exists in `docs/characters/<char>/refs/powers/`, pass it directly as `<ROUGH_OR_REF_INPUT>` and preserve its chosen symbol rather than reinterpreting from the power title alone.

```text
Use case: stylized-concept / reference-guided-generation.
Asset type: single symbolic status emblem for <OUTPUT_CONTRACT> (256x256 master exported to 64x64).
Engine mode: <ENGINE_MODE>.
Input owners:
- Motif / symbol rough (<ROUGH_OR_REF_INPUT in refs/powers/, when present>): owns the primary symbol choice, silhouette, and motif arrangement, incorporating any documented user overrides in <char>-art.md.
- Official StS2 status anchor (docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png): guides compact symbolic abstraction, bold interior/edge value separation, and 32-64px buff-bar readability; exclude composited stack digits, health bar, and unrelated symbols.
Subject: <ASSET_SUBJECT derived from the motif rough and character's visual theme>.
Composition: 2-3 bold interlocking shapes, a distinct outer silhouette, and <PALETTE>. Place the primary identifying feature in x/y:15-80% (central/upper-left) and keep the bottom-right x/y:60-100% stack-number zone quiet.
Style: <STYLE>. True transparent background. No miniature card scene, full figures, painted numbers, letters, box/frame or scene background.
```

Compare at `64 × 64` and `32 × 32` beside `status-icons-in-combat-crop.png` and sibling powers in the character's set.

## Recipe 7: `base + refs → Standard Card Portrait` (Branch C-3)

Follow [Branch C-3](SKILL.md#branch-c-3-base--refs--cards-focused-card-illustration-route) and [ARTS.md 5.5](../../../ARTS.md#55-standard-card-illustrations). Pass the applicable base, object, rough, and same-type official card anchors. Use this compact card-specific block instead of appending the full character inventory and general rendering block. Include only identity cues visible in this camera; the references carry the remaining construction. For a selected-image simplification, name the preserved composition and repaint its secondary forms under the same density rules.

```text
Create one opaque landscape card illustration on an exact 1000x760 canvas (25:19), designed to read at 250x190. Compose directly for this ratio, even when a reference has a different ratio.
Action: <one sentence linking the subject's gesture to its result>.
References: <rough or selected image> owns composition; <base/object anchors> own identity; <complementary official illustrations selected from a broader survey> guide <the specific action, hierarchy, or material role assigned to each>.
Preserve: <camera, pose, visible identity cues, weapon connection, and target silhouette>.
Large shapes: <primary subject, action/target, and quiet setting>. Keep essential action inside x15-85%, y12-82%.
Rendering: crisp connected painted shapes with selective form shading, guided by the reference set. Concentrate the sharpest contrast and useful detail at <action>. Group secondary surfaces into broad color areas.
Effects and setting: <one coherent effect and only the setting cues necessary to the action>. Leave clear space around the focal silhouettes.
Output: scene painted continuously to all four canvas edges. No letterboxing, blank/solid-color bands, transparent margins, blurred/mirrored padding, card frame, text, cost orb, UI, or watermark. If adapting a wider reference, paint the missing setting above and below while preserving its subjects; do not crop, stretch, or add padding.
```

Follow Branch C-3's actual-file geometry check, painted-edge inspection, and preview/density review before recommending or promoting the result. Prompt dimensions alone do not establish compliance. For edits, repeat the exact canvas contract in the correction prompt.

## Recipe 8: `base + refs → Ancient Full-Art Card` (Branch C-3)

Read the target card list and actual frame dimensions from the character asset guide. Pass `base.png`, any current-run object-family master, any vertical layout rough (`<ROUGH_OR_REF_INPUT>`), and official StS2 card/illustration references for value hierarchy.

```text
Use case: identity-preserve / stylized-concept / reference-guided-generation.
Asset type: vertical full-art card illustration on an exact 606x852 opaque canvas (101:142), <OUTPUT_CONTRACT>. Compose directly for this ratio.
Engine mode: <ENGINE_MODE>.
Input owners:
- Character & prop anchors (<approved base.png and/or current-run object-family master>): own canonical identity, weapon/prop construction, and <PALETTE, MATERIALS>.
- Layout rough / reference (<ROUGH_OR_REF_INPUT, when present>): owns vertical staging and focal-window placement.
- Official StS2 anchors (<official card/illustration references in docs/research/sts2-style-refs/>): guide 2-3 value plane economy and 250x350 display readability.
Subject and scene: <CHARACTER_ANCHOR, ASSET_SUBJECT, SCENE_SENTENCE>.
Layout: stage face, hands, and primary action inside the upper-middle focal window (y:15-58%), clear of the top title/cost zone (y:0-14%), and keep the lower description zone (y:62-100%) dark, low-contrast, and quiet. Do not move the primary action by post-generation cropping.
Style: <STYLE>. Palette/materials: <PALETTE, MATERIALS>.
Backdrop: continuous opaque scene painted to all four edges. No letterboxing, blank/solid-color bands, transparent margins, blurred/mirrored padding, painted frame, banner, text box, numbers, letters or watermark. Extend the setting instead of padding a mismatched reference.
```

Run Branch C-3's actual-file geometry and painted-edge checks, then compare at `250 × 350` and verify that the upper-middle focal window (`y:15–58%`) and quiet lower text zone (`y:62–100%`) hold without post-generation cropping.
