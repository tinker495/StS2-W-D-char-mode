# Shared Art Direction

Shared Slay the Spire 2 rendering and information-density rules for characters, cards, relics, powers, UI, backgrounds, and VFX. Character names, appearance, palettes, equipment, counts, decisions, and dedicated prompts belong in `docs/characters/<char>/`. Updated: 2026-10-18.

**Shared chibi scope:** apply chibi to `character_icon.png`, `character_icon_outline.png`, and `map_marker.png`. Selection tiles/locked variants, standing art, cards, and selection illustrations retain canonical proportions. Character specifications define faces, costumes, colors, and chibi details. Non-SD proportions do not imply an adult appearance.

## 1. Document Roles and Visual Inputs

### 1.1 `refs → base → assets`

Raw `refs/` may differ in costume, rendering, and camera. Resolve conflicts into an approved `base.png` first, then use it as the identity/equipment anchor for downstream assets. Official game images guide rendering and composition, not character canon.

### 1.2 Document Ownership

| Document | Owns |
|---|---|
| This document | Shared rendering, information density, materials, asset compositions, and UI safe zones |
| `<char>-art.md` in the [character directory](docs/characters/) | Appearance, proportions, colors, equipment, prompt material, reference roles, and adopted constraints |
| `<char>-assets.md` in the same directory | Delivery paths, dimensions, model/scene wiring, counts, and current status |
| `<char>-history.md` in the same directory | Past exploration, choices, rejections, deliveries, and observations |
| `base.png` and `refs/` in the same directory | Approved visual canon and raw references |
| Optional `drafts/` in the same directory | Git-ignored drafts, prompts, comparisons, and other local records; never required to understand shared documents |
| [character-asset-pipeline](.agents/skills/character-asset-pipeline/SKILL.md) | Production branches, records, previews, conversion, promotion, and packaging |
| [Official-source research](docs/research/sts2-art-direction-sources.md) and related studies | Official statements, source images, observations, project judgments, and limitations |

Read the target art specification and asset guide before creating or revising a character. Update character decisions there rather than duplicating them in shared documents or generic skills.

### 1.3 Composition Principles for All Assets

**Do not use post-generation cropping as the default solution.** Study originals for the same use case and generate directly for the target aspect ratio, subject occupancy, camera, and UI overlap.

1. Headwear extending outside a close portrait is a generation-camera decision. Do not extract part of a finished full-body painting to make another asset.
2. Compare the whole generated image at actual size. If face, action, or margins are wrong, revise the composition and regenerate. Do not use automatic Cover clipping or extracted-region enlargement.
3. Preserve the complete painting with proportional `Fit` resizing by default. Transparent padding, background compositing, alpha outlines, and locked silhouettes are delivery conversions that preserve composition.
4. Crop only within an explicit user request for that image. Keep sources and full frames for research crops/enlargements and distinguish them from delivery artwork.

## 2. Shared Rendering and Density by Size

### 2.1 Target Direction

A game name alone can produce muddy colors, uniform black contours, or excessive brush texture. Translate Mega Crit's Playful, Crisp, Liveliness, Cinematic, and Colorfuler goals, together with restrained painterly treatment, into the following rules. These are project interpretations, not quotations of internal production specifications. [Evidence](docs/research/sts2-art-direction-sources.md)

- **Major forms and value families:** establish volume through base color, main shadow, and selective light. Two or three dominant families do not require exactly three flat colors.
- **Edge hierarchy:** separate forms through color, value, and overlap. Use dark, material-colored lines at deep joints and selected contours, not equally weighted lines around every fold.
- **Silhouette and connections:** combine long C/S curves with selected angles. Explain body/tool masses, joints, grounding, and actual hand contact before ornaments.
- **Color-temperature contrast:** distinguish dominant, accent, and neutral linking roles. Specific colors belong to the character specification.
- **Low surface noise:** allow low-contrast brush variation and short soft transitions inside broad planes. Avoid high-contrast white flecks, dense hatching, needless polygon fragmentation, and compression dithering.

### 2.2 Color, Line, and Detail Budgets by Display Size

These values are project starting points. Check actual paths, dimensions, scales, and UI in the character asset guide. Judge density at **actual display size**, not through automatic acceptance thresholds.

| Asset | Delivery size (`W × H`) | In-game display | Value families per material | Smallest identifying mass | Remove |
|---|---|---|---|---|---|
| Inline energy (`text_energy`) | `24 × 24` | `24 × 24` | 2: rim + center | At least `4px` in output | Interior motifs, scratches, multiple highlights, intricate angular grooves |
| Power icons (`powers`) | `256 × 256` large; `64 × 64` small | Buff bar `32–64px`; tooltip `64–128px` | 2–3 | `16px` at 256px | Scenery, full figures, four or more separate props, detail under the lower-right number |
| Card energy (`big_energy`) | `74 × 74` | `36–74px` | 2–3 | `6px` at 74px | High-contrast motifs or runes crossing the central number area (`r < 45%`) |
| Relic icons (`relics`) | `256 × 256` large; `94 × 94` small/outline | Top bar `48–64px`; popup `256px` | 2–3 | `12px` at 256px | Background, ground shadow, fine engraving, individual hairs/feathers |
| Portrait/marker (`character_icon`, `map_marker`) | `128 × 128` | Top bar/map `48–85px` | 2–3 | `8px` at 128px | Hands/weapons, hair strands, tiny armor rivets, complex eye reflections |
| Selection tile (`char_select`) | `132 × 195` | `132 × 195` | 3 | `6px` at 132px | Full/lower body, complex background objects, rough breastplate blotches |
| Combat/rest/merchant standing art (`combat`) | `384 × 576`; master `base.png` | Combat/merchant `240 × 360`; rest `326 × 490` | 3 | `6px` at 384px | Fragmented armor facets, dense wood cracks, repeated thin straw lines |
| Standard card portraits | `1000 × 760` (`25:19`) | Standard `250 × 190`; enlarged `500 × 380` | 3; background 2 | `16px` at 1000px / `4px` at 250px | Obscuring mist/particles, high-contrast background ornament, painted text/frame |
| Ancient cards | `606 × 852` full art | Standard `250 × 350`; enlarged `500 × 700` | 3; low contrast in lower 38% | `12px` at 606px | Essential faces/hands under lower text (`y:62–100%`) or top title (`y:0–14%`) |

### 2.3 Actual Official Image Anchors and Priority

Reinvestigated on 2026-10-18. Do not put only a game name in the prompt. Open the **actual downloaded pixels** and use images for the same asset purpose. Originals belong to Mega Crit and are for research/comparison, not mod runtime delivery. Sources, frames, and crops are listed in the [image index](docs/research/sts2-style-refs/README.md); detailed observations are in the [combat-style study](docs/research/sts2-combat-style-study.md).

| Priority/use | Actual local image | Observe | Do not import |
|---|---|---|---|
| Primary combat materials/values | [Defect crop](docs/research/sts2-style-refs/defect-combat-crop.png), [official full frame](docs/research/sts2-style-refs/official-combat.png) | Large armor plates, subtle metal-plane changes, broad cloth flow, joints joined through planes | Robot body, golden costume, claws, orbs |
| Humanoid armor support | [Ironclad crop](docs/research/sts2-style-refs/ironclad-combat-crop.png), [co-op full frame](docs/research/sts2-style-refs/official-combat-2.png) | Simple shoulder/forearm/shin plates, large connected masses, selective bright ridges | Mask, head ornament, bulky physique, overlapping arms/swords from other characters |
| Cloth and pose support | [Silent crop](docs/research/sts2-style-refs/silent-combat-crop.png), [official animation frame 40](docs/research/sts2-style-refs/official-silent-anim-frame40.png) | Long S/C curves, wedge-shaped hems, broad folds, hand/weapon spacing | Skull mask, horns, two daggers, concealed head |
| Standing-art simplification | [Necrobinder combat frame](docs/research/sts2-style-refs/official-necro-anim-frame0.png) | Small head, narrow connections, broad costume masses, minimal hands/bones | Lich face, levitation, scythe, companion hand |
| Selection/background illustration only | [Necrobinder selection screen](docs/research/sts2-style-refs/official-select.png) | Large color contrasts, stronger contours, paper/brush texture, theatrical rim light | Applying the same line weight, texture density, or violet rim to combat sprites |

![Official Defect combat materials](docs/research/sts2-style-refs/defect-combat-crop.png)
![Official Silent combat forms](docs/research/sts2-style-refs/silent-combat-crop.png)

Official URLs can serve replacement files. At investigation time, `steam_screenshot1_new.png` showed Defect combat rather than the Ironclad described in older notes. **Inspect the new file before reusing an old description.** Separate blog fan art, covers, and unused concepts from actual combat references. [Official press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/), [2024 art-direction article](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/)

### 2.4 Shape, Face, and Proportions

Read the silhouette, tool direction, and gaps between limbs first. Use selected angles within rounded volume; do not inflate or triangulate every surface. Keep armor planes broad and turn values at structural ridges/overlaps.

Follow character canon for body ratio, apparent age, eye size, hair part, ears, and expression. Simplify eyelids, nose shadow, and mouth without importing another character's mask, skeleton, or proportions. Use the shared chibi scope and character-specific treatment within it. Non-SD does not mean adult.

### 2.5 Light, Edges, and Texture

Keep silhouettes/overlaps firm, use limited soft transitions on rounded volumes, and allow nearby light planes to meet without outlines. Focus contrast on face, action, and contact points rather than repeating reflections along every edge. "Less painterly" does not mean pure vector, textureless, or blurred.

## 3. Character Art Specification Contract

Check `docs/characters/<char>/<char>-art.md` for:

- Approved canonical appearance, runtime coverage, and remaining differences.
- Adopted/excluded features of each raw reference.
- Apparent age, face/hair, body, pose, gaze, and equipment identifiers.
- Dominant/accent/material palette and chibi proportions, backgrounds, and visible parts within the common scope.
- Recurring objects, themes, family masters, and character prompt material.
- Constraints established through selection/rejection and their reasons, understandable without opening historical drafts.

Do not change identity while applying the shared style. If a new character has no specification, record observed references and user decisions in its directory rather than copying another character's defaults.

## 4. Shared Material Treatment

| Material | Preserve | Reduce |
|---|---|---|
| Cloth | Long force/gravity-driven flow, major shadows, restrained transitions | Equal-size triangular fragments, tiny folds, noise |
| Metal | Broad base/reflection/shadow planes and selected structural ridges | Gem-like facets, bright borders on every plate |
| Fur, hair, fibers | Large connected masses with selected separated tips | Uniform teeth, high-contrast individual strands |
| Wood/leather | Dull material-appropriate values and sparse directional grain | Plastic gloss, crack webs, excessive stitching |
| Face | Canonical shape/age with simple eyes, nose, mouth | Excess iris sparkle, skin gloss, identity-changing omission |

HEX values, alloys, fabrics, ornament combinations, and local exceptions belong in character specifications.

## 5. Asset Compositions and Safe Zones

These are production starting points, checked against actual UI/frames and character guides. Do not present them as official universal dimensions or automatic acceptance criteria.

### 5.1 Combat, Rest, and Merchant Standing Art

Branch B delivers ordinary standing art and separate combat art. Use the complete approved `base.png` directly for ordinary standing, with proportional Fit conversion. Generate the combat pose anew from the base and original-game combat references: weapon-appropriate readiness, grounded feet, intentional weight, and credible hand contact. Preserve canonical identity and rendering in both. Character guides own filenames, dimensions, and separate scene mappings; do not assume combat and rest/merchant share one PNG.

Include the entire character and tool, planning roughly 3–5% transparent margins at the extremities. Distinguish combat-forward from camera-facing. Preserve the approved face instead of turning it into a strong profile to achieve forward gaze. Specify head rotation and eye direction separately. Character-specific face, pose, and direction belong in the target specification.

The character asset guide owns display size, ground line, sprite scale/position, dialogue/hit/health anchors. Do not fix those placements here.

### 5.2 Character UI

Generate portraits, markers, selection tiles, and energy icons for their respective purposes. Chibi for portrait/outline/map marker is shared. Read visible face/body scope, headwear, expression, and background color from the character specification. Derive outlines from the final small PNG's alpha for identical contours.

### 5.2.1 Selection Tiles

Read the [normal/locked tile study](docs/research/sts2-character-select-tile-study.md) and [actual originals](docs/research/sts2-style-refs/character-select-tiles/README.md). Design a close camera dominated by face/mask at generation time; compare at 132×195 beside originals. Do not pass a full-body equipment inventory as a requirement for the narrow tile.

Deriving normal/locked variants from the same resized full-image alpha prevents composition drift. Distinguish deselected UI tint from the locked source image. Follow the character specification for colors, silhouettes, and proportions; keep frames, text, and button-state UI out of the painting.

### 5.3 Relic Icons

Show one isolated object without background or ground shadow. Start around 72–84% occupancy, prioritizing contour and major values. Resize large to small, then derive the outline from the small alpha. Objects, family masters, counts, and filenames belong in target documents.

### 5.4 Power Icons

Read as a single symbol for a persistent state. Keep identifying details central/upper-left and clear of the bottom-right `x/y:60–100%` number zone. Distinguish silhouettes and color roles within related families. Check actual number placement against the implementation.

### 5.5 Standard Card Illustrations

Start with essential action, face, hand, and contact inside `x:15–85%, y:12–82%`. Use a high-contrast, clear-edged foreground, supportive midground, and large low-contrast background masses. Relate card function to action without making one character's craft, companion, or weapon a universal theme.

### 5.6 Ancient and Vertical Full-Art Cards

Current layout starting zones: title/cost at `y:0–14%`, focal window at `15–58%`, description at `62–100%`. Place the primary subject in the focal window and keep the lower text background quiet. Read character card lists, filenames, subjects, and actual frame sizes from the asset guide. [Limits of official references](docs/research/sts2-future-asset-reference-study.md)

### 5.7 Selection Illustrations

Use the [five-character composition study](docs/research/sts2-character-select-composition-study.md) and [original observations](docs/research/sts2-style-refs/character-select/README.md). Write one characteristic action sentence. Connect face, hands, equipment/ability, and a second major symbol. Design pose and background anew for selection use.

A safe zone avoids collisions between essential face/action and text; it is not an empty rectangle. Low-contrast ability/costume forms can continue behind UI. Inspect an overlay with actual description, tiles, and confirm controls. Scene sentence, palette, props, and runtime paths belong in the character document.

## 6. Translate Themes and Recurring Objects into Images

Read effect/theme names from domain documents and translate them into actions, contacts, objects, and causes of light in the character art specification. Do not paint effect numbers or HUD. Use object-family masters for recurring props. Keep specific theme counts, item names, companions, and colors out of this shared document.

## 7. Prompt Assembly and Iterative Editing

### 7.1 Iterative Revisions

Branch from a clean approved candidate and change one axis at a time. Separate length/width/thickness and gaze/head rotation. Do not accumulate excessive or rejected edits. Consider a form-preserving cleanup pass when surface noise appears. A user instruction to apply the final painting unchanged takes precedence over additional generation.

### 7.2 Standard Prompt Blocks

#### [Block A] Shared Rendering

```text
Style/medium: use the target-appropriate OFFICIAL Slay the Spire 2 references for rendering and composition, and the approved character base for identity. Broad connected silhouettes, flowing C/S curves with selected angular folds, quiet interiors and a few dominant value families rather than a strict color count. Combine firm overlaps with restrained soft transitions on rounded forms. Separate shapes through color, value and overlap; use selective dark material-colored contours. Retain subtle low-contrast directional brush variation while suppressing high-contrast flecks, fine hatching and unnecessary surface fragmentation. Preserve the approved facial anatomy, age appearance, proportions and material hierarchy. Do not copy another character's identity, costume or compression dithering. Match detail density to the actual asset display size; adapt scene depth to its purpose.
```

#### [Block B] Character Anchor Slot

Fill `<CHARACTER_ANCHOR>` with visible face, appearance, and equipment details from the target `<char>-art.md`. Do not store a particular character's English anchor in shared documents.

#### [Block C] Proportion Slot

`<PROPORTION_BRIEF>` follows the shared chibi scope and reads face, costume, and silhouette details from the target specification. Portrait/outline/map marker use chibi; other assets retain canonical apparent age and anatomy.

#### [Block D] Surface Cleanup

```text
Rendering cleanup only: preserve identity, equipment construction, pose, proportions and framing. Consolidate tiny fragmented marks into connected value families while retaining restrained soft transitions and low-contrast brush variation appropriate to each material. Remove high-contrast flecks, scratches, hatching and unnecessary contour teeth. Preserve the existing palette, material roughness and light direction. Do not blur the image, change its design or turn it into glossy cel shading, pure vector art or uniformly faceted shapes.
```

### 7.3 Comparison and Records

Compare the original, previous candidate, and new candidate at the same display size; record the largest difference and next change axis. Keep combat-sprite density distinct from large-illustration density. Separate official observations, project choices, and actual generation results; do not convert them into automated scores or test thresholds.

## 8. Production Records and Verification

Shared documents must explain production criteria without drafts, prompts, or comparison images. State current appearance and constraints in the art specification, dimensions/conversion/status in the asset guide, and decision rationale in history. Do not substitute version IDs or "same as the previous image." Share the base, runtime assets, and necessary specifications; optionally keep drafts and logs in Git-ignored `drafts/`. Tracking every intermediate is unnecessary.

Inspect actual-size previews on light `#E8E2D5` and dark `#1B1924` backgrounds for transparency edges, halos, and readability. Comparison boards use whole paintings at a common scale and may remain local observations. State their conclusions and visual reasons directly in shared documents.

Distinguish file dimensions, conversions, and PCK-loading observations from actual game verification. Packaging/installation follow the [pipeline](.agents/skills/character-asset-pipeline/SKILL.md) scope; tests and validation assets follow [AGENTS.md](AGENTS.md).
