# Shared Art Direction

Shared Slay the Spire 2 rendering and information-density rules for characters, cards, relics, powers, UI, backgrounds, and VFX. Character names, appearance, palettes, equipment, counts, decisions, and dedicated prompts belong in `docs/characters/<char>/`. Updated: 2026-10-28.

**Shared chibi scope:** apply chibi to `character_icon.png`, `character_icon_outline.png`, and `map_marker.png`. Selection tiles/locked variants, standing art, cards, and selection illustrations retain canonical proportions. Character specifications define faces, costumes, colors, and chibi details. Non-SD proportions do not imply an adult appearance.

## 1. Document Roles and Visual Inputs

### 1.1 `refs → base → assets`

Raw `refs/` may differ in costume, rendering, and camera. Resolve conflicts into an approved `base.png` first (`refs → base`), then combine up to four explicit image-input roles for downstream production (`base + asset refs/roughs + official anchors → assets`). Never withhold available visual references in favor of text-only prompts:

1. **Canonical character anchor (`base.png`):** owns facial anatomy, apparent age, sleek proportions, costume/weapon construction, and palette whenever the character, body parts (such as gloved hands and vambraces in card close-ups), or signature equipment appear.
2. **Rough sketch or composition guide (`refs/` or `drafts/<guide>.png`):** user-supplied rough sketch, symbol draft, silhouette, or agent-staged layout/pose guide. Owns primary camera framing, pose/gesture, focal placement, silhouette, and motif arrangement. Translate rough lines and blocks into finished 2D game art without discarding intentional staging or reinterpreting the subject from the asset title alone.
3. **Object, motif, or photo reference (`refs/` or current-run family master):** real-world photo, prop reference, or newly generated in-run object-family master. Owns structural construction, silhouette proportions, and functional identifiers while simplifying photorealistic grain, complex engraving, lighting mismatch, and backgrounds into 2–3 broad value families and crisp contours.
4. **Same-use-case official StS2 reference ([sts2-style-refs](docs/research/sts2-style-refs/README.md) or temporary local PCK extraction):** guides rendering economy, target display-size information density, contour hierarchy, and UI safe-zone readability, never character or object canon.

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
3. Preserve the complete painting with proportional `Fit` resizing by default. Transparent padding, background compositing, alpha outlines, and locked silhouettes are delivery conversions that preserve composition. **Full-bleed cards must already fill their target frame:** padding is not a correction for a card's wrong aspect ratio. Regenerate or extend the painted scene before delivery; follow section 5.5's geometry contract for standard and Ancient cards.
4. Crop only within an explicit user request for that image. Keep sources and full frames for research crops/enlargements and distinguish them from delivery artwork.

## 2. Shared Rendering and Density by Size

### 2.1 Target Direction

A game name alone can produce muddy colors, uniform black contours, or excessive brush texture. Translate Mega Crit's Playful, Crisp, Liveliness, Cinematic, and Colorfuler goals into the concrete rules below, combining official combat silhouette/readability observations ([study section 7](docs/research/sts2-combat-style-study.md#7-installed-build-spine-renders-2026-10-24)) with the crisp, clean 2D character rendering established in approved character bases (`docs/characters/althea/base.png`, `docs/characters/realta/base.png`). These are project rules, not quotations of internal production specifications. [Evidence](docs/research/sts2-art-direction-sources.md)

- **Crisp, clean 2D cel-plus-soft-form shading:** build each mass from a clear base color, one distinct shadow family, and selective light planes with smooth, clean transitions. Avoid muddy or chalky dry-brush blotches, heavy canvas grain, or dirty over-textured surfaces that make characters look clunky or weathered when their design calls for clean fabric and polished steel.
- **Readable material values:** keep light materials clean and distinct (crisp warm ivory/cream for white cloth, bright silver-gray for polished steel, luminous silver-white for hair) without blowing out into flat pure-white `#FFFFFF` fill or crushing everything into dull mid-gray/pewter. Contrast concentrates on the face, silhouette, action, and hand contacts.
- **Edge hierarchy:** use fine, crisp, dark material-colored linework to keep facial features, fingers, and overlapping plates sharp and legible, while letting internal cloth and armor planes turn cleanly by value. Avoid thick, uniform black comic outlines or rough, sketchy contour teeth.
- **Silhouette and connections:** long C/S curves, turning at structural joints. Explain body/tool masses, joints, grounding, and actual hand contact before ornaments.
- **Color-temperature contrast:** distinguish dominant, accent, and neutral linking roles; even neutrals carry a temperature. Specific colors belong to the character specification.
- **Clean surfaces and trim:** keep surface shading smooth and intentional so armor facets, gold trim, and emblems read crisply at both full and combat sizes without visual noise.

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

Reinvestigated on 2026-10-18; combat anchors updated 2026-10-24; downstream anchors expanded 2026-10-28. Do not put only a game name in the prompt. Every production branch follows a three-step official-reference discipline:

1. **Open same-use-case official pixels first:** before drafting a prompt, open the actual official StS2 reference image(s) for that exact asset category from [sts2-style-refs](docs/research/sts2-style-refs/README.md) (or render/extract from the installed `SlayTheSpire2.pck` into a temporary directory).
2. **Pass official references as style and density anchors:** supply the same-use-case official reference alongside `base.png` and any rough/object reference. Explicitly separate what to adopt (camera distance, silhouette separation, stroke economy, 2–3 value grouping, safe-zone framing) from what to exclude (the official asset's specific character/item identity, UI frame, composited digits, or GIF dithering).
3. **Compare side-by-side at actual display size:** run `asset-pipeline.ps1 compare` placing the candidate beside the same-use-case official reference at in-game display dimensions before recommending or promoting it.

Originals belong to Mega Crit and are for research/comparison, not mod runtime delivery. Public sources, frames, and crops are listed in the [image index](docs/research/sts2-style-refs/README.md) and [future-assets index](docs/research/sts2-style-refs/future-assets/README.md); detailed observations are in the [combat-style study](docs/research/sts2-combat-style-study.md) and [downstream-asset study](docs/research/sts2-future-asset-reference-study.md).

**Primary combat and merchant anchors** are installed-build Spine renders: alpha-clean, unoccluded and undithered. Generate fresh renders locally with `asset-pipeline.ps1 official-poses` ([pipeline section 3](.agents/skills/character-asset-pipeline/SKILL.md#3-observation-and-conversion-tools)) into a temporary output directory; they are never committed. Use the public crops only when the game is not installed, and do not read their dithering, overlapping figures or scene background as style.

| Priority/use | Image | Observe | Do not import |
|---|---|---|---|
| Primary combat stance, camera, grounding | Local renders `<c>-combat-idle_loop-00.png` for Ironclad, Silent, Defect, Necrobinder, Regent | Grounded combat stance, camera angle, silhouette separation, readable negative space around weapons/limbs | Each character's mask, skull, robot body, costume, weapons, companion, soft ground shadow, or rough/gritty monster-like surface texture and squat proportions when the character canon is sleek and clean |
| Merchant/rest relaxed poses | Local renders `<c>-merchant-relaxed_loop-00.png` | Relaxed weight, grounded posture | Character identity, as above |
| Public fallback: armor/materials | [Defect crop](docs/research/sts2-style-refs/defect-combat-crop.png), [Ironclad crop](docs/research/sts2-style-refs/ironclad-combat-crop.png) | Large armor plates, broad cloth flow | Overlapping arms/swords from other characters, scene background |
| Public fallback: cloth and pose | [Silent crop](docs/research/sts2-style-refs/silent-combat-crop.png), [official animation frame 40](docs/research/sts2-style-refs/official-silent-anim-frame40.png) | Long S/C curves, wedge-shaped hems, hand/weapon spacing | GIF dithering, green background, skull mask, horns, daggers |
| Standing-art simplification | [Necrobinder combat frame](docs/research/sts2-style-refs/official-necro-anim-frame0.png) | Small head, narrow connections, broad costume masses, minimal hands/bones | Lich face, levitation, scythe, companion hand |
| Character UI (`character_icon`, `map_marker`, `big_energy`, `text_energy`) | [Portrait crop](docs/research/sts2-style-refs/future-assets/ui-character-portrait-crop.png), [top bar](docs/research/sts2-style-refs/future-assets/ui-topbar.jpg), [map](docs/research/sts2-style-refs/future-assets/ui-map-qol.jpg), [combat energy crop](docs/research/sts2-style-refs/future-assets/ui-energy-in-combat-crop.png), plus installed `res://images/atlases/ui_atlas.sprites/card/energy_*.tres` | Small-scale contour clarity, broad 2–3 value separation, quiet central cost inset | Composited `2/3` cost digits, Silent mask/hood identity, top-bar background |
| Selection tiles (`char_select`, `char_select_locked`) | Ten 132×195 normal/locked originals in [character-select-tiles](docs/research/sts2-style-refs/character-select-tiles/README.md) ([tile study](docs/research/sts2-character-select-tile-study.md)) | Face-dominant close camera, head/horn frame exits, black-on-gray locked silhouette | Deselection darkening as locked state, painted button borders, other characters' masks |
| Selection/background illustration (`char_select_bg`) | Five skeleton renders and two background layers in [character-select](docs/research/sts2-style-refs/character-select/README.md), [Necrobinder screen](docs/research/sts2-style-refs/official-select.png) ([composition study](docs/research/sts2-character-select-composition-study.md)) | Gesture-driven focal flow, large color contrasts, secondary prop/ability mass, left UI safe-zone balance | Applying selection line weight, texture density, or violet rim to combat sprites; painting UI panels/text |
| Relic and potion icons (`relics`) | [Relic/potion still](docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png), [combat relic crop](docs/research/sts2-style-refs/future-assets/relics-in-combat-crop.png) ([downstream study section 2](docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density)) | Single isolated object, bold dark contour, 2–3 broad light planes, distinct outer silhouette at 48–64px | Source blue backing, numbered grid, `W` ticket lettering, forcing every relic into a coin/gem template |
| Power and status emblems (`powers`) | [Status icons crop](docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png) ([downstream study section 2](docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density)) | Single symbolic state emblem, broad interior/edge contrast, clear separation from lower-right stack numbers | Composited stack digits, health bar, shield frame where not part of the power motif |
| Standard Attack, Skill, and Power cards (`card_portraits`) | Attack: [Thrumming Hatchet](docs/research/sts2-style-refs/future-assets/card-attack-thrumming-hatchet.png), [Bodyguard / Bone Shards](docs/research/sts2-style-refs/future-assets/card-attack-bodyguard-bone-shards.png); Skill: [Scrawl](docs/research/sts2-style-refs/future-assets/card-skill-scrawl.png), [Soul / Grave Warden](docs/research/sts2-style-refs/future-assets/card-skill-soul-grave-warden.png); Power: [Rolling Boulder](docs/research/sts2-style-refs/future-assets/card-power-rolling-boulder.png), [Automation](docs/research/sts2-style-refs/future-assets/card-power-automation.png), [Blight Strike / Neurosurge](docs/research/sts2-style-refs/future-assets/card-attack-power-blight-strike-neurosurge.png) ([downstream study section 3](docs/research/sts2-future-asset-reference-study.md#3-attack-skill-and-power-card-illustrations)) | Close action camera around one weapon/hand/phenomenon, strong directional arcs, high-contrast foreground over 2–3 grouped background values | Card frame, title ribbon, cost orb, rules text, or shrinking full-body standing sprites into card windows |

Official URLs can serve replacement files. At investigation time, `steam_screenshot1_new.png` showed Defect combat rather than the Ironclad described in older notes. **Inspect the new file before reusing an old description.** Separate blog fan art, covers, and unused concepts from actual combat references. [Official press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/), [2024 art-direction article](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/)

### 2.4 Shape, Face, and Proportions

Read the silhouette, tool direction, and gaps between limbs first. Build clean volumes that turn at structural joints, ridges, and overlaps, with broad, well-defined armor and cloth planes between them.

Follow character canon and raw character references for body ratio, apparent age, facial beauty/sharpness, eye shape, hair flow, and expression. Preserve the character's sleek, balanced canonical proportions (typically 7–7.5 heads for full-body knight/adventurer characters such as Althea unless the character specification states otherwise); do not forcibly compress characters into squat, chunky, or thick-limbed 6-head proportions, which make elegant character designs look clumsy and coarse. Keep hands, boots, and armor rims well-proportioned to the character's frame while maintaining clear silhouette gaps so the figure reads cleanly at `240 × 360`. Simplify micro-noise (stray hair threads, tiny rivets, noisy scratches) without coarsening facial features or importing another character's mask, skeleton, or heavy build. Use the shared chibi scope and character-specific treatment within it. Non-SD does not mean adult.

### 2.5 Light, Edges, and Texture

Give the outer silhouette and crossing forms a clear value break, let clean cel-plus-soft-form transitions carry rounded volumes, and use crisp, refined contour lines where needed for facial and structural clarity. Focus contrast on face, action, and contact points rather than repeating noisy highlights along every edge. Keep surfaces clean and purposeful—avoid heavy gouache blotches, muddy midtones, or blurred edges.

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
| Cloth | Long force/gravity-driven flow, crisp base value (including clean warm ivory/cream for white cloth), clear shadow shapes, clean gold/colored trim | Equal-size triangular fragments, tiny folds, muddy gray/brown staining on white fabric |
| Metal | Clean faceted steel/gold planes with crisp light/shadow separation and a controlled edge highlight on primary ridges | Mirror chrome noise, dirty pewter muddiness, overly thick/clunky plate rims, random scratches |
| Fur, hair, fibers | Large connected flowing masses with clean tapered locks and readable volume (including bright silver-white hair when canonical) | Hundreds of fine single-pixel strands, or dull ash/muddy gray discoloration |
| Wood/leather | Clean material-appropriate values and sparse directional grain | Plastic gloss, crack webs, excessive stitching |
| Face | Crisp canonical facial structure, clean anime/illustrative eye and lash linework, nose shadow, and composed mouth | Over-rendered photorealistic pores/lips, or coarse blotchy gouache simplification that ruins facial appeal |

HEX values, alloys, fabrics, ornament combinations, and local exceptions belong in character specifications.

## 5. Asset Compositions and Safe Zones

These are production starting points, checked against actual UI/frames and character guides. Do not present them as official universal dimensions or automatic acceptance criteria.

### 5.1 Combat, Rest, and Merchant Standing Art

Branch B first inspects the approved base's actual pose. Use the complete `base.png` with proportional Fit conversion for ordinary standing. If it already depicts a suitable combat-ready stance, reuse that export for combat by default. Generate a separate combat pose only when the base is a noncombat/everyday pose or the user requests a distinct combat pose; use the base and original-game combat references for weapon readiness, grounding, weight and credible hand contacts. Preserve canonical identity and rendering. Character guides own filenames, dimensions and shared or separate scene mappings.

Include the entire character and tool, planning roughly 3–5% transparent margins at the extremities. Distinguish combat-forward from camera-facing. Preserve facial anatomy, apparent age, and identifying features while deriving rotation from original-game references for the requested use case. Facial identity does not require a frontal head or two fully visible eyes. Orientation prompt fields are defined in [Recipe 1](.agents/skills/character-asset-pipeline/PROMPT-RECIPES.md#recipe-1-new-canonical-art-and-local-edits). Character-specific face, pose, and direction belong in the target specification.

The character asset guide owns display size, ground line, sprite scale/position, dialogue/hit/health anchors. Do not fix those placements here.

### 5.2 Character UI

Generate portraits, markers, selection tiles, and energy icons for their respective purposes. Chibi for portrait/outline/map marker is shared. Read visible face/body scope, headwear, expression, and background color from the character specification. Derive outlines from the final small PNG's alpha for identical contours.

### 5.2.1 Selection Tiles

Read the [normal/locked tile study](docs/research/sts2-character-select-tile-study.md) and [actual originals](docs/research/sts2-style-refs/character-select-tiles/README.md). Design a close camera dominated by face/mask at generation time; compare at 132×195 beside originals. Do not pass a full-body equipment inventory as a requirement for the narrow tile.

Deriving normal/locked variants from the same resized full-image alpha prevents composition drift. Distinguish deselected UI tint from the locked source image. Follow the character specification for colors, silhouettes, and proportions; keep frames, text, and button-state UI out of the painting.

### 5.3 Relic Icons

Read the [relic/status density study](docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density) and open [relics-potions-frame0.png](docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png) and [relics-in-combat-crop.png](docs/research/sts2-style-refs/future-assets/relics-in-combat-crop.png). Show one isolated object without background, pedestal, or ground shadow. Start around 72–84% occupancy, prioritizing bold outer contour and 2–3 major value families that survive `48–64px` top-bar display. When a rough sketch or object photo is supplied, preserve its silhouette and functional identifiers while simplifying surface noise. Establish starter/base family masters before upgraded variants. Resize `256 × 256` large to `94 × 94` small (keeping 2–3px margin so the outline does not clip), then derive the `94 × 94` outline from the small alpha. Objects, family masters, counts, and filenames belong in target documents.

### 5.4 Power Icons

Open [status-icons-in-combat-crop.png](docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png) ([study section 2](docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density)). Read as a single symbolic emblem for a persistent state, never a miniature card scene. When user-supplied motif roughs or symbol references exist in `refs/powers/`, preserve their chosen symbol and arrangement rather than reinterpreting from the power name alone. Keep identifying details central/upper-left (`x/y:15–80%`) and clear of the bottom-right `x/y:60–100%` stack-number zone. Distinguish silhouettes and color roles within related families at `64 × 64` and `32 × 32`. Check actual number placement against the implementation.

### 5.5 Standard Card Illustrations

**Strict card geometry:** standard portraits are exactly `1000 × 760` (`25:19`); Ancient full-art cards are exactly `606 × 852` (`101:142`). Put the target dimensions, ratio, orientation, and edge-to-edge painted coverage in the generation prompt and any supported canvas controls. Inspect the returned file; the requested size is not evidence of the actual size. A larger master may be proportionally reduced only if the scaled dimensions round to both target dimensions, leaving zero padding pixels. This permits integer raster rounding only, not a percentage-based aspect-ratio tolerance. Otherwise regenerate or outpaint the missing scene with the image tool. Do not use letterboxing, transparent/solid-color bands, blurred or mirrored edge extensions, stretching, or unrequested cropping to make an incompatible canvas fit. The final PNG must have the exact target dimensions, full opacity, and continuous painted scene across all four edges; inspect both the complete image and its card-size preview before promotion.

Read the [40-card native portrait survey](docs/research/sts2-card-illustration-density.md) for scene families and density observations; the [earlier card study](docs/research/sts2-future-asset-reference-study.md#3-attack-skill-and-power-card-illustrations) supplies public sources. Inspect several native cards with comparable actors, targets, and actions before selecting a complementary reference set. Pass the applicable character, object, and rough references. Compose directly for the card window, preserving user-selected staging and keeping essential action, hand, and contact inside `x:15–85%, y:12–82%`.

**Design at thumbnail scale:** the `250 × 190` illustration owns the detail budget; the larger export adds edge quality, not more objects or surface marks. Choose one action and a small set of identifying silhouettes. Attacks emphasize a strike and its result; Skills emphasize a gesture or interaction; Powers emphasize one persistent state or space.

- **Focus:** concentrate the brightest light and sharpest value break at the action. Keep costume identifiers and target construction readable through their outer shapes and broad color patches.
- **Surfaces:** group light and shadow into connected masses, preserving selective shading and contours where they explain volume or action. Choose their density from several scene-matched cards; a fixed number of tones or uniform polygon facets is not a universal StS2 style. Group hair, fur, cloth, straw, and metal into connected shapes. Describe only visible identity cues in the prompt; image references carry construction details.
- **Effects:** use one coherent trail or impact shape. Add isolated debris only when it clarifies the cut, collision, or direction; leave clear space between fragments. Express glass and liquids through broad translucent planes and a continuous fill shape.
- **Setting:** use quiet background masses and a simple ground plane. Name architecture, paving, reflections, or atmospheric particles only when they are necessary to the card's action. A location can remain recognizable through silhouette and color alone.

**Compare illustration areas at the same displayed size.** Prefer native portrait textures. A complete official card includes UI and cannot be fitted into the same rectangle as a frameless candidate for a density comparison; either place both illustrations in matching card windows or explicitly report that comparison as limited. Inspect secondary edges and competing highlights as well as recognizable subjects. Readability alone does not resolve excessive detail; simplify competing shapes before recommending the candidate. These are visual judgments, not pixel-count acceptance thresholds.

### 5.6 Ancient and Vertical Full-Art Cards

Apply section 5.5's strict geometry and painted-coverage checks to the `606 × 852` canvas before export.

Current layout starting zones: title/cost at `y:0–14%`, focal window at `15–58%`, description at `62–100%`. Place the primary subject in the focal window and keep the lower text background quiet. Read character card lists, filenames, subjects, and actual frame sizes from the asset guide. [Limits of official references](docs/research/sts2-future-asset-reference-study.md)

### 5.7 Selection Illustrations

Use the [five-character composition study](docs/research/sts2-character-select-composition-study.md) and [original observations](docs/research/sts2-style-refs/character-select/README.md). Write one characteristic action sentence. Connect face, hands, equipment/ability, and a second major symbol. Design pose and background anew for selection use.

A safe zone avoids collisions between essential face/action and text; it is not an empty rectangle. Low-contrast ability/costume forms can continue behind UI. Inspect an overlay with actual description, tiles, and confirm controls. Scene sentence, palette, props, and runtime paths belong in the character document.

## 6. Translate Themes and Recurring Objects into Images

Read effect/theme names from domain documents and translate them into actions, contacts, objects, and causes of light in the character art specification. Do not paint effect numbers or HUD. Use object-family masters for recurring props. Keep specific theme counts, item names, companions, and colors out of this shared document.

## 7. Prompt Assembly and Iterative Editing

### 7.1 Iterative Revisions

Branch from a clean approved candidate and change one axis at a time. Separate length/width/thickness and gaze/head rotation. Do not accumulate excessive or rejected edits. Consider a form-preserving cleanup pass when surface noise appears. A user instruction to apply the final painting unchanged takes precedence over additional generation.

The [asset pipeline's bounded review loop](.agents/skills/character-asset-pipeline/SKILL.md#default-generatereviewcorrect-loop) owns generation, automatic visual review, in-scope corrections, and stopping. Review both the requested change and preservation of already-settled choices; file measurements alone do not establish completion.

### 7.2 Standard Prompt Blocks

#### [Block A] Shared Rendering

```text
Style/medium: crisp, clean hand-painted 2D game character art (clean cel-plus-soft-form shading), anchored to the approved character base and raw character references for facial appeal, proportions, and clean surface finish, while using official Slay the Spire 2 references for grounded combat silhouette and display-size readability. Build forms from large, well-organized masses with long C/S curves, clean base values, one clear shadow family, and smooth, controlled plane transitions. Keep light materials clean and true to palette (warm ivory/cream cloth, bright silver-gray steel, luminous silver-white hair) without muddy gray/brown dulling or flat pure-white blowouts. Use fine, crisp, dark material-colored contours for facial clarity, fingers, and armor overlaps. Preserve the character's sleek canonical proportions and sharp facial features; do not make limbs chunky, squat, or rough-textured. Match detail density to the actual asset display size; adapt scene depth to its purpose.
```

#### [Block B] Character Anchor Slot

Fill `<CHARACTER_ANCHOR>` with visible face, appearance, and equipment details from the target `<char>-art.md`. Do not store a particular character's English anchor in shared documents.

#### [Block C] Proportion Slot

`<PROPORTION_BRIEF>` follows the shared chibi scope and reads face, costume, and silhouette details from the target specification. Portrait/outline/map marker use chibi; other assets retain canonical apparent age and anatomy.

#### [Block D] Surface Cleanup

```text
Rendering cleanup only: preserve identity, equipment construction, pose, proportions, crisp facial features, and framing. Consolidate tiny fragmented marks or stray hair threads into clean connected value families while retaining smooth cel-plus-soft-form transitions and crisp edges appropriate to each material. Remove high-contrast flecks, scratches, muddy blotches, hatching, and unnecessary contour teeth. Preserve the existing clean palette and light direction. Do not blur the image, dull the colors, or change its design.
```

### 7.3 Comparison and Records

Compare the original, previous candidate, and new candidate at the same display size; record the largest difference and next change axis. Keep combat-sprite density distinct from large-illustration density. Separate official observations, project choices, and actual generation results; do not convert them into automated scores or test thresholds.

## 8. Production Records and Verification

Shared documents must explain production criteria without drafts, prompts, or comparison images. State current appearance and constraints in the art specification, dimensions/conversion/status in the asset guide, and decision rationale in history. Do not substitute version IDs or "same as the previous image." Share the base, runtime assets, and necessary specifications; optionally keep drafts and logs in Git-ignored `drafts/`. Tracking every intermediate is unnecessary.

Inspect actual-size previews on light `#E8E2D5` and dark `#1B1924` backgrounds for transparency edges, halos, and readability. Comparison boards preserve whole paintings; choose shared-scale preservation, individual-Fit, or equal-subject-height style comparisons through the [pipeline observation tools](.agents/skills/character-asset-pipeline/SKILL.md#3-observation-and-conversion-tools). Boards may remain local observations. State their conclusions and visual reasons directly in shared documents.

Distinguish file dimensions, conversions, and PCK-loading observations from actual game verification. Packaging/installation follow the [pipeline](.agents/skills/character-asset-pipeline/SKILL.md) scope; tests and validation assets follow [AGENTS.md](AGENTS.md).
