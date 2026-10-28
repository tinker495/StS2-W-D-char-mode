---
name: character-asset-pipeline
description: Character asset pipeline (refs/roughs + official StS2 anchors → base → UI, standing, selection, relics, powers, and cards). Use to establish an approved base, or run focused production routes for UI, relics, powers, and cards using Codex (GPT Image) or Antigravity (Nano Banana).
---

# Character Asset Pipeline

This skill owns shared execution procedures only. Keep character names, appearance, palettes, proportions, equipment, themes, counts, and version histories in character documents.

## 1. Establish the Target, Branch, and Official Anchors

Choose the branch first, then read the shared rules, the matching official StS2 reference pixels, and only the target-specific sections needed for it:

1. [ARTS.md](../../../ARTS.md): shared rendering, composition, official reference anchors, and display-size rules.
2. `docs/characters/<char>/<char>-art.md`: the target's canonical identity, appearance, palette, reference/rough roles, asset-specific proportions/staging, and prompt material.
3. The current status and requested asset sections of `docs/characters/<char>/<char>-assets.md`: delivery paths, dimensions, and model/scene wiring. Read `<char>-history.md` only when investigating previous choices, rejections, or deliveries.
4. For cards, relics, and powers, the relevant `<char>.md`, [GLOSSARY.md](../../../GLOSSARY.md), and specifications: the meaning of game effects and themes.

Apply the [repository reading procedure](../../../docs/agents/workflow.md#read-within-an-output-budget). In `ARTS.md`, read sections 1–4 and 7–8 plus the applicable composition subsection of section 5. In character documents, read identity/reference roles plus the requested asset's brief and current status.

| Branch | `ARTS.md` & official StS2 anchors to open | Recipe sections in `PROMPT-RECIPES.md` |
|---|---|---|
| A: new base (`refs → base`) | `ARTS.md` 1–4, 5.1; `official-poses` combat/merchant renders or [sts2-style-refs](../../../docs/research/sts2-style-refs/README.md) | Shared Input Slots, Recipe 1 intro, 1-A, 1-D (when roughs/refs guide staging); 1-B for local edits; 2 for cleanup |
| B: UI, standing, selection (`base → UI/standing/select`) | `ARTS.md` 5.1, 5.2, 5.2.1, 5.7; [character-select-tiles](../../../docs/research/sts2-style-refs/character-select-tiles/README.md), [character-select](../../../docs/research/sts2-style-refs/character-select/README.md), [future-assets UI crops](../../../docs/research/sts2-style-refs/future-assets/README.md) | Shared Input Slots and the requested recipes from 1-C, 1-D, 3, and 4 |
| C-1: focused relics (`base + refs → relics`) | `ARTS.md` 5.3; [relics-potions-frame0.png](../../../docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png), [relics-in-combat-crop.png](../../../docs/research/sts2-style-refs/future-assets/relics-in-combat-crop.png), [downstream study §2](../../../docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density) | Shared Input Slots, 1-D (for rough/photo refs), and Recipe 5 |
| C-2: focused powers (`base + refs → powers`) | `ARTS.md` 5.4; [status-icons-in-combat-crop.png](../../../docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png), [downstream study §2](../../../docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density) | Shared Input Slots, 1-D (for motif/symbol roughs), and Recipe 6 |
| C-3: focused cards (`base + refs → cards`) | `ARTS.md` 5.5, 5.6, 6; official Attack/Skill/Power cards in [future-assets](../../../docs/research/sts2-style-refs/future-assets/README.md), [downstream study §3–4](../../../docs/research/sts2-future-asset-reference-study.md#3-attack-skill-and-power-card-illustrations) | Shared Input Slots, 1-D (for scene roughs/guides), and Recipes 7–8 |
| D: review, edit, cleanup, conversion | `ARTS.md` 1.3, 2.2, 7.1, 8; same-use-case official anchor for `compare` | 1-B for local edits, 2 for cleanup; section 3 below for observation/conversion |

If no art specification exists, first record observed references and user decisions in the character directory. Do not copy another character's traits as defaults. Resolve actual filenames through the asset guide.

```text
docs/characters/<char>/
├── <char>-art.md        # Identity, appearance, palette, staging, rough/ref roles, adopted constraints
├── <char>-assets.md     # Technical mapping and application status
├── <char>-history.md    # Rationale for past choices and deliveries; read on demand
├── <char>.md            # Human-facing domain specification
├── refs/                # Raw character refs and optional asset-specific roughs/motifs (e.g. refs/relics/, refs/powers/, refs/cards/)
├── base.png             # Approved canonical base
└── drafts/              # Optional drafts, rough guides, prompts, comparisons, logs; Git-ignored
```

Keep drafts, actual prompts, input-role notes, reviews, and execution logs in Git-ignored `drafts/` when useful. Do not copy every intermediate into tracked paths merely to make documentation shareable. State the current appearance, constraints and rationale, output dimensions, conversion procedures, and application status directly in shared documents. Avoid explanations or links that require a local file or historical version ID to understand. The approved base, runtime assets, and necessary specifications are the collaboration contract.

**Scope and approval:** do not overwrite the base or runtime assets before approval. Promote only versions the user selected, approved, or instructed you to apply, within the requested scope. Do not ask again for existing approval. Updating only the base does not authorize runtime/UI regeneration or installation.

### Image Generation Engines: Codex (`GPT Image`) vs. Antigravity (`Nano Banana`)

Choose the generation path according to the active agent harness, or run both when comparing or resolving a bottleneck:

1. **Codex (`GPT Image` via `image_gen` / `image_gen__imagegen`):**
   - **Invocation:** inspect each actual input image before generation, reusing observations of unchanged inputs already opened in this session. Pass local files in `referenced_image_paths`; use `num_last_images_to_include` only when an input is an unsaved conversation attachment, never both parameters. Respect the active tool's input limit (five local image paths in this harness). Prioritize the edit target or rough, applicable identity anchors, and complementary same-use-case references; surveyed images need not all become generation inputs.
   - **Transparency:** set `transparent_background: true` for isolated RGBA assets (`base.png`, standing/combat, `character_icon`, `map_marker`, `char_select` transparent master, `big_energy`/`text_energy`, `relics`, `powers`). Omit it for opaque full-bleed paintings (`char_select_bg`, standard `1000 × 760` cards, Ancient `606 × 852` cards).
   - **Reference scope:** new character depictions use the applicable base and raw/rough inputs. A narrow edit of a user-selected painting uses that painting as its target, adding only references needed for the changed part. Keep identity and rendering consistent with `ARTS.md`; a local overlap fix does not require reloading the whole character inventory or introducing a new style set.
2. **Antigravity (`Nano Banana` via `image-generator` subagent / built-in image tool):**
   - **Invocation:** dispatch `invoke_subagent` with `TypeName: "image-generator"` (or call the built-in Nano Banana image tool), providing a self-contained brief that lists: (a) exact absolute paths to all input images (`base.png`, rough/motif `refs/`, and same-use-case official StS2 reference files) with each file's explicit axis ownership, (b) the complete assembled prompt from `PROMPT-RECIPES.md`, and (c) the output path under `docs/characters/<char>/drafts/`.
   - **Transparency:** Nano Banana may return opaque or pseudo-transparent backdrops even when asked for alpha. Immediately run `asset-pipeline.ps1 inspect` (`CornersTransparent`, `CornerAlpha`) and `preview` on dark/light boards; for RGBA assets, request clean isolation without background clutter and convert/clean alpha before downstream export.
   - **Strengths and compensation:** excels at sharp anime/illustrative facial appeal, sleek canonical proportions, luminous color separation, and dynamic full-bleed illustrations (`base.png`, `char_select_bg`, Branch C-3 cards). On small icons (relics/powers), guard against excessive micro-detail by anchoring to official StS2 icon references and enforcing bold 2–3 value plane budgets.
3. **Dual-Engine Comparison and Cross-Engine Workflow (`둘 다 시도 및 교차 활용`):**
   - When both environments are available or the user asks to compare engines—or when one engine repeatedly misses on an axis (e.g., `GPT Image` making character faces/proportions clunky, or `Nano Banana` struggling with clean isolated alpha/icon simplification)—**generate candidates with both `GPT Image (Codex)` and `Nano Banana (Antigravity)`** from the same reference set and place them side-by-side with the official StS2 anchor in a single `asset-pipeline.ps1 compare` board at target display size.
   - **Hybrid pass:** you may establish character appeal, pose, or scene composition in one engine and run a targeted local edit (Recipe 1-B), surface cleanup (Recipe 2-A), or isolated alpha pass in the other. Record the engine (`Codex / GPT Image` vs. `Antigravity / Nano Banana`) for each candidate in draft notes.

## 2. Production Branches

Every branch follows [ARTS.md section 1.3](../../../ARTS.md#13-composition-principles-for-all-assets) and [section 2.3](../../../ARTS.md#23-actual-official-image-anchors-and-priority). Generate new compositions directly with same-use-case official StS2 references alongside character/rough inputs. Narrow selected-image edits retain that source's composition/style and add only references needed for the changed part. Compare/resize the complete painting at actual display size. Regenerate incorrect compositions; do not fix them through post-generation cropping, extracted-region enlargement, or automatic Cover fitting. An explicit crop request applies only to that image and scope. Read [PROMPT-RECIPES.md](PROMPT-RECIPES.md) for templates.

### Default Generate–Review–Correct Loop

Run this loop for every newly generated or edited asset before the final recommendation. The agent performs the visual review automatically; the user need not request routine corrections. This is an execution procedure using the settled brief, not permission to invent new acceptance criteria or redesign approved choices.

1. **Lock the brief:** record the exact source/version, requested change, preserved features, output contract, and actual reference roles. When a rough supplies the composition, complete [Recipe 1-D's landmark brief](PROMPT-RECIPES.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng) before generation. For overlapping bodies or equipment, state camera-relative depth order and trace continuous objects through their contacts. Choose applicable review rows below before writing the prompt.
2. **Generate:** save a separate candidate and the prompt. Start new compositions from the required anchors; use the exact selected source for local edits. A tool-displayed image is an intermediate result until reviewed.
3. **Review the pixels:** use `inspect`, open the full candidate and an actual-display-size `preview`/`compare`, and review it against the locked brief and relevant references. For local edits, compare source/result at the same scale. For style or density, compare equivalent illustration areas. Record visible evidence and locations, not the generator's intent.

   | Review area | Required observation |
   |---|---|
   | Requested change and preservation | Is the requested difference visible? Did camera, identity, pose, palette, scale, or an unrelated approved feature drift? |
   | Rough fidelity and prop identity | Compare the rough and candidate directly against Recipe 1-D's landmarks. Check the requested object type, grip, tip direction, contact, and visible body extent before assessing polish. |
   | Structure and depth | Trace limbs to hands and hands to equipment. Trace a single shaft/tool through every visible and hidden segment; check alignment, continuity, grip, and camera-relative overlap. |
   | Composition and density | Does the action read at display size? Do secondary edges, highlights, or background marks compete with it relative to scene-matched references? |
   | Delivery | Check actual dimensions, alpha/opacity, margins, safe zones, and unintended text/UI. Tool measurements supplement visual review. |

4. **Classify findings:** mark each applicable row `clear`, `correction needed`, or `uncertain`, with a short observation. A missing requested change, broken connection, unintended occlusion, obvious artifact, output-contract mismatch, or regression is a correction when it contradicts the brief. A different aesthetic preference without such evidence is optional and does not trigger automatic redesign. A readable silhouette does not cancel a density or structure finding.
5. **Correct within scope:** fix the highest-impact finding and only the connected changes it requires. The correction prompt names the observed defect, intended visible result, and preserved features. Use deterministic delivery conversion for dimensions/alpha only when it preserves the painting; use the image tool for painting changes. Keep a sound current-run candidate as the next source; after identity/composition drift, branch again from the selected source or clean anchors rather than accumulating failures.
6. **Review again:** reopen the new pixels, check the fix, and repeat preservation and output checks. Record whether each prior finding was resolved, remained, or caused a regression. Do not replace a failed observation with a claim that the prompt requested the right thing.

**Default bound:** one initial generation plus up to two corrective generations per asset and active brief. Comparison variants and outputs from another engine consume the same three-candidate budget. These are execution limits, not product thresholds; explicit user limits or a preview-only/no-regeneration request take precedence. Stop early when applicable checks are clear. Stop at the bound, or when two corrections fail to improve the same defect, and report the best candidate with unresolved findings. Do not silently reset the budget, endlessly polish, or present an unresolved required correction as complete. Ask only when further work depends on an unsettled preference, new scope, or extending the exhausted loop; complete independent authorized work first.

**Completion record:** save a concise source → candidate → observation → correction/stop record in ignored drafts. The final response states the delivered candidate, actual review/correction count, remaining material differences, and runtime application status. Automated agent review is not user approval and does not authorize runtime promotion. Existing test/validator ownership remains unchanged; this loop does not add tests, CI stages, or automated aesthetic scores.

### Branch B and C: Start with New Artwork

For new or replacement UI, selection backgrounds, cards, relics, and powers, design and generate from **the approved `base.png`, required raw/rough references (`refs/`), and base-game references for the same use case**. Do not input previous deliveries or earlier production drafts as edit targets or composition anchors. Do not reuse prompts that reproduce an old composition. Open previous assets only for comparison after the new result exists. The approved base, user-supplied rough sketches/motif references, and current-run object-family masters are exempt from this restriction.

This starting-input rule governs the first new composition. The default loop may edit a sound candidate generated in the current run to correct an observed defect. That exception does not make an old delivery a starting anchor for a later new-composition request. Explicitly selected-image edits use Branch D.

Before generation, record each actual input's role and check that no previous asset is included:

- **Adding and using rough sketches or reference images (`refs/` and `drafts/`):**
  - Store user-supplied asset roughs, motif icons, or object photos under `docs/characters/<char>/refs/` (or category subfolders such as `refs/relics/`, `refs/powers/`, `refs/cards/`) and document their adopted/excluded traits in `<char>-art.md`. For complex multi-subject cards or scenes without a user rough, you may stage a structural layout/pose rough in `docs/characters/<char>/drafts/<asset>-rough.png` to lock camera and focal placement before final rendering.
  - **Pass roughs and reference images directly as image inputs** (`Recipe 1-D`). Never reduce a supplied rough sketch, symbol draft, or photo reference to text-only description.
  - **Assign non-overlapping axis ownership:**
    1. **Rough sketch / layout guide:** owns composition, camera angle, silhouette, gesture/pose, and motif arrangement. Translate rough lines into finished StS2 game art without discarding intentional staging or reinterpreting a user-chosen symbol from the asset name alone.
    2. **Object / photo / motif reference:** owns structural proportions and key functional identifiers, while photorealistic texture, complex engraving, lighting mismatch, and background are simplified into 2–3 broad value families.
    3. **Approved `base.png` & current-run family masters:** own character anatomy, face, hair, costume, gloved hands/vambraces, canonical weapon construction, and palette.
    4. **Official StS2 same-use-case reference:** owns display-size detail density, edge weight, value grouping, and safe-zone readability.

For selection backgrounds, proceed from original-game research to one characteristic action sentence, then a new camera, major shapes, and UI layout. Changing only a face or colors in an old background is not new scene production.

Separate invariant identity (face, anatomy, costume, equipment construction, palette and rendering) from asset staging (camera, body/head direction, gaze, hand contacts and equipment state). Read staging from the requested asset's brief. A selected base pose belongs to standing exports; it does not establish the pose or drawn/sheathed state of new UI and selection scenes.

A portrait or object-family master newly created in the same production run may anchor related assets. Size conversion, alpha outlines, and locked variants from the same master remain valid. When the user explicitly selects an existing image for a partial edit, specify the scope and use Branch D/Recipe 1-B.

### Branch A: `refs → base`

1. Open all raw references in `refs/` and the official combat/merchant references (`official-poses` or [sts2-style-refs](../../../docs/research/sts2-style-refs/README.md)). Record adopted and excluded features in the target art specification with [Recipe 1's source labels](PROMPT-RECIPES.md#recipe-1-new-canonical-art-and-local-edits). For revisions, separate preserved features from the requested change axis.
2. Generate with Recipe 1-A (and 1-D when combining a separate pose/layout rough with an identity reference), passing the raw character reference directly as an image input so facial appeal, sleek proportions, and costume construction stay faithful while translating surface micro-noise into clean 2D game art. Use Recipe 1-B for a local edit to a selected image.
3. Starting with the first candidate, run `inspect` and open a `compare` sheet of the complete candidate beside same-use-case official references at the intended display size (section 3; `SubjectFit` for style/silhouette, `SharedScale` for preservation). Review in this order: (a) camera, body direction, head angle and gaze; (b) action and weapon/equipment state; (c) silhouette, grounding, margins and hand/equipment connections; (d) clean 2D rendering, facial appeal, sleek proportions, and detail density. **Completion criterion:** the sheet is opened, the principal differences are recorded, every Recipe 1-A after-generation observation is filled, drift is recorded as unresolved, and `MinMarginRatio` is checked against ARTS.md 5.1. Resolve composition or use-case mismatches before surface cleanup or recommending a candidate. File measurements do not establish visual agreement. Branch from clean references or a selected source rather than repeatedly editing a rejected result.
4. If accumulated edits or surface noise are visible, consider Recipe 2-A's cleanup pass before promotion. If the user requests applying the final painting unchanged, preserve it without additional generation.
5. Promote the approved painting to the requested `base.png` and update the current-status table. Describe appearance changes, runtime coverage, and remaining differences. Record dimensions, procedure, and observed pixel changes for delivery conversions such as transparent padding. Export runtime standing art only if requested.

### Branch B: `base → UI, Standing Art, Selection Illustration`

1. Open the approved `base.png` as the primary identity/equipment input, plus any UI/scene roughs in `refs/` and the matching official StS2 UI/tile/selection references from [ARTS.md 2.3](../../../ARTS.md#23-actual-official-image-anchors-and-priority). Follow ARTS.md's common chibi scope; obtain proportion details, visible parts, expressions, and backgrounds from the target art specification.
2. Inspect the base's actual stance, grounding and hand/equipment contacts before choosing reuse or generation, following [ARTS.md 5.1](../../../ARTS.md#51-combat-rest-and-merchant-standing-art). Export the complete base by proportional Fit for ordinary standing and reuse it for combat when already combat-ready. Use Recipe 1-C only for a noncombat/everyday base or a user-requested distinct combat pose. Record the decision and shared or separate scene mappings in the target asset guide. A static combat-ready PNG is sufficient; animation is not required.
3. Design portrait and marker cameras/margins separately, comparing at `48–85px` against [ui-character-portrait-crop.png](../../../docs/research/sts2-style-refs/future-assets/ui-character-portrait-crop.png) and [ui-topbar.jpg](../../../docs/research/sts2-style-refs/future-assets/ui-topbar.jpg). For selection tiles, read the [normal/locked tile study](../../../docs/research/sts2-character-select-tile-study.md) and open the ten [actual originals](../../../docs/research/sts2-style-refs/character-select-tiles/README.md), then generate a new face-focused master and compare at `132 × 195`.
4. Where possible, derive normal/locked tiles (`tile-pair`) and portrait outlines (`outline`) from the same final alpha. Resize both energy sizes (`74 × 74` and `24 × 24`) from one master, comparing against [ui-energy-in-combat-crop.png](../../../docs/research/sts2-style-refs/future-assets/ui-energy-in-combat-crop.png). Do not copy another character's colors, outline thickness, or proportions.
5. A complete Branch B request includes UI, outline, standing/combat coverage according to step 2, and the selection illustration. Coverage can use a shared standing export; it does not require an extra generation. Follow the shared input rule and [Recipe 4-D](PROMPT-RECIPES.md#4-d-char_select_bgpng-1920--1080-selection-illustration) to design a new scene from [original-game selection research](../../../docs/research/sts2-character-select-composition-study.md) and inspect it with `ui-preview`.
6. Follow the asset guide's model-property and scene contracts. Use an appropriate UI structure such as Control/TextureRect for selection backgrounds. Confirm all requested files and necessary references are connected. For a partial request, produce only that scope. Generating a new combat candidate does not approve its runtime promotion.

### Branch C: Focused Relic, Power, and Card Production

Choose the dedicated sub-branch (`C-1`, `C-2`, or `C-3`) matching the requested asset type so each category uses its own official StS2 anchors, rough/reference rules, display-size comparisons, and delivery conversions.

#### Branch C-1: `base + refs → Relics` (Focused Relic Route)

1. **Establish scope, family masters, and references:** read the relic list, rarities, and object briefs in `<char>.md`, `<char>-art.md`, and `<char>-assets.md`. Open the official StS2 relic anchors ([relics-potions-frame0.png](../../../docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png) and [relics-in-combat-crop.png](../../../docs/research/sts2-style-refs/future-assets/relics-in-combat-crop.png); [study §2](../../../docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density)). For each relic, assemble:
   - Any user-supplied rough sketch, silhouette, or object photo in `refs/` (or an existing approved symbol explicitly designated by the user, such as an energy emblem for a matching crest relic).
   - `base.png` only when the relic depicts canonical character costume or signature equipment.
   - For paired/upgraded relics (such as starter → boss upgrade), generate and approve the starter/base object master in this run first, then pass it as the structural anchor for the upgraded variant.
2. **Generate isolated object masters (Recipe 5 & 1-D):** generate each relic directly as a `256 × 256` RGBA isolated object (roughly 72–84% occupancy, bold dark contour, 2–3 broad connected value families, no background, ground shadow, pedestal, or fine engraving).
3. **Inspect and compare against official relics:** run `inspect` (`CornersTransparent = true`, `BBoxWidthRatio`/`BBoxHeightRatio` around 72–84% with at least 2–3px outer margin at 94px scale so the outline will not clip) and run `compare` at `94 × 94` and `48 × 48` (top-bar scale) beside `relics-potions-frame0.png` / `relics-in-combat-crop.png` on dark `#1B1924` and light `#E8E2D5` backgrounds. When producing a set, build a combined overview sheet. **Completion criterion:** every relic silhouette reads distinctly at `48 × 48`, rough/reference identifiers are preserved without photo noise, and no corner/edge alpha clipping occurs.
4. **Export three files per relic:** upon approval, export `256 × 256` (`Sts2WD/images/relics/big/<relic>.png`), export `94 × 94` (`Sts2WD/images/relics/<relic>.png`, using a 90×90 Fit centered in 94×94 if needed to preserve a 2px outline margin), and derive the `94 × 94` outline (`Sts2WD/images/relics/<relic>_outline.png`) from the small alpha with `asset-pipeline.ps1 outline`.

#### Branch C-2: `base + refs → Powers` (Focused Power Emblem Route)

1. **Establish scope and motif references:** read power definitions and stack behavior in `<char>.md`, `<char>-art.md`, and `<char>-assets.md`. Open the official status anchor ([status-icons-in-combat-crop.png](../../../docs/research/sts2-style-refs/future-assets/status-icons-in-combat-crop.png); [study §2](../../../docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density)). When user-supplied symbol roughs or motif PNGs exist (such as `docs/characters/<char>/refs/powers/`), open each file directly—note that dark/black symbols on transparent backgrounds must be inspected on a light board—and preserve the user's chosen motif rather than reinterpreting from the power title alone. Apply any documented user overrides in `<char>-art.md`.
2. **Generate symbolic state emblems (Recipe 6 & 1-D):** pass the motif rough/reference and official status anchor directly to generate a `256 × 256` RGBA single persistent-state emblem (2–3 bold interlocking shapes, 2–3 primary colors, no miniature card scene or box frame). Place the primary identifying feature in `x/y:15–80%` (central to upper-left) and keep the bottom-right `x/y:60–100%` stack-number zone quiet.
3. **Inspect and compare at buff-bar scale:** run `inspect` (`CornersTransparent = true`) and `compare` at `64 × 64` (tooltip) and `32 × 32` (combat buff bar) beside `status-icons-in-combat-crop.png` and sibling powers in the character's set. **Completion criterion:** each power is immediately distinguishable by outer contour and dominant color at `32 × 32`, the rough/motif symbol is faithfully translated into StS2 rendering, and the lower-right stack-number zone is unobstructed.
4. **Export two files per power:** upon approval, export `256 × 256` (`Sts2WD/images/powers/big/<power>.png`) and `64 × 64` (`Sts2WD/images/powers/<power>.png`) via proportional `Fit`.

#### Branch C-3: `base + refs → Cards` (Focused Card Illustration Route)

1. **Group by theme/object family and establish masters first:** read card effects, types (`Attack`, `Skill`, `Power`, `Ancient`), themes, and recurring objects in `<char>.md`, `<char>-art.md`, and `<char>-assets.md`. Use the [native portrait survey](../../../docs/research/sts2-card-illustration-density.md) to inspect a broad set, then select complementary anchors for the requested actors/targets, action, materials, and background hierarchy. A weapon-only close-up does not by itself establish a multi-subject scene's composition or density. The older public examples below are starting points, not a complete or mandatory input set ([public study](../../../docs/research/sts2-future-asset-reference-study.md#3-attack-skill-and-power-card-illustrations)):
   - **Attack anchors:** [card-attack-thrumming-hatchet.png](../../../docs/research/sts2-style-refs/future-assets/card-attack-thrumming-hatchet.png), [card-attack-bodyguard-bone-shards.png](../../../docs/research/sts2-style-refs/future-assets/card-attack-bodyguard-bone-shards.png).
   - **Skill anchors:** [card-skill-scrawl.png](../../../docs/research/sts2-style-refs/future-assets/card-skill-scrawl.png), [card-skill-soul-grave-warden.png](../../../docs/research/sts2-style-refs/future-assets/card-skill-soul-grave-warden.png).
   - **Power anchors:** [card-power-rolling-boulder.png](../../../docs/research/sts2-style-refs/future-assets/card-power-rolling-boulder.png), [card-power-automation.png](../../../docs/research/sts2-style-refs/future-assets/card-power-automation.png), [card-attack-power-blight-strike-neurosurge.png](../../../docs/research/sts2-style-refs/future-assets/card-attack-power-blight-strike-neurosurge.png).
   Organize card production in focused batches by **recurring object family or theme** (e.g., signature weapon strikes, close-up artisan/knight hands and vambraces, guardian/companion actions, spell/curse phenomena). Before generating batch cards that share a recurring non-base prop or summon, generate and lock a current-run **object-family master** in `drafts/` to pass as an image input across that batch.
2. **Assemble per-card image inputs (Recipes 7, 8 & 1-D):**
   - Pass `base.png` whenever the character, gloved hands/vambraces, or canonical weapon appear. Assign identity and prop ownership separately using Recipe 1-D; a card-specific object named by the user takes precedence over the base's equipment.
   - Pass the relevant current-run object-family master for recurring props/summons.
   - Pass any user-supplied card rough, pose sketch, or motif reference (`refs/cards/` or chat attachment). For complex two-subject duels, perspective strikes, or strict Ancient card windows without a user rough, optionally create a quick composition/layout rough in `drafts/<card>-rough.png` first and pass it as the composition owner.
   - Pass the matching official StS2 Attack, Skill, or Power card reference for rendering density and value grouping. Use it for camera distance only when the user has not supplied staging through a rough or selected image.
3. **Generate directly for the card type and frame geometry:**
   - **Lock and enforce the canvas:** use the strict card geometry contract in [ARTS.md 5.5](../../../ARTS.md#55-standard-card-illustrations). Record target width/height, reduced ratio, orientation, and fully painted opaque edges before generation. Include these in every initial and corrective prompt, and set the tool's canvas controls when available. A wide reference owns its subject staging, not the output ratio: extend its setting into the target frame instead of importing its letterboxing.
   - **Standard cards (`1000 × 760` opaque, `25:19`, Recipe 7):** follow [ARTS.md 5.5](../../../ARTS.md#55-standard-card-illustrations) for thumbnail composition and detail density. Write one action sentence, the visible identity cues, and the required large shapes before the compact recipe. Retain user-selected staging. Let image anchors supply construction details instead of expanding the prompt into an inventory of ornaments, materials, debris, and scenery.
   - **Ancient full-art cards (`606 × 852` opaque vertical, `101:142`, Recipe 8):** stage the primary subject and action inside the upper-middle focal window (`y:15–58%`), clear of the top title/cost banner (`y:0–14%`), and keep the lower description area (`y:62–100%`) dark, low-contrast, and quiet.
4. **Check actual geometry before visual approval:** run `inspect` on the generated file, then `export-card -CardType Standard` (or `Ancient`) to a temporary candidate in an ignored working directory using the [card export procedure](scripts/conversion-tools.md#card-export-and-review). The command rejects a source requiring even one padding pixel after proportional Fit rounding and requires fully opaque input/output. Record source and final dimensions; use `FullyOpaque`, not rounded percentages, for opacity. Also open the image: a mechanically valid PNG can still contain painted letterbox bands. If geometry or painted coverage fails, regenerate/outpaint under the default correction loop, preserving the accepted subjects and composition. Do not promote or recommend the candidate as complete while the mismatch remains.
5. **Compare at in-game card scale:** open a `250 × 190` standard-card preview (`250 × 350` for Ancient cards). Follow [ARTS.md 5.5](../../../ARTS.md#55-standard-card-illustrations) to compare equal-sized illustration areas; `compare -CompareMode IndividualFit` is suitable for native portrait textures, not a frameless candidate beside a complete card fitted into the same rectangle. **Completion criterion:** record both action readability and secondary-detail density, along with identity, safe zones, painted coverage, and absence of painted UI. If background edges, material marks, or effects compete with the action relative to the reference, revise the painting before recommending it. Opening a preview or passing dimension checks does not resolve a visual mismatch.
6. **Deliver full-bleed card portraits:** after the temporary candidate passes mechanical and visual review, copy that exact file to the runtime mapping in `<char>-assets.md` within the user's existing application authorization. Run `check-cards` to verify the delivered PNGs. Record generation, export, and visual observations separately. If the retry limit is reached with a mismatch, report it as unresolved rather than applying a padded substitute.

### Branch D: Review, Cleanup, and Conversion

Use `inspect` and `preview` to examine the master and actual display size first. Use Recipe 1-B for selected-image edits, Recipe 1-D when applying a user-supplied rough/reference correction, Recipe 2-A for surface cleanup, and export/outline for delivery. For comparison-only requests, choose the `compare` mode in section 3 for the question being investigated.

## 3. Observation and Conversion Tools

Immediately after generation and final output, run `inspect` and `preview` from the bundled [asset-pipeline.ps1](scripts/asset-pipeline.ps1), then open the previews. `inspect` reports the alpha bounding box, `MarginsPx` (`Left`, `Top`, `Right`, `Bottom`) and `MinMarginRatio`, the smallest margin as a percentage of its axis. Compare it with the margins in ARTS.md 5.1 right after generation. Replace the placeholders below with actual target values.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 inspect -Path "docs/characters/<char>/drafts/<asset>.png"
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 preview -Path "docs/characters/<char>/drafts/<asset>.png" -DisplayWidth 94 -DisplayHeight 94 -OutPath "docs/characters/<char>/drafts/<asset>-preview.png"
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 export -Source "docs/characters/<char>/drafts/<asset>.png" -Target "<runtime-path>.png" -Width 94 -Height 94 -CropMode Fit
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 outline -Source "<runtime-path>.png" -Target "<runtime-path>_outline.png" -Radius 2 -Color "#FFFFFF"
```

Pass array arguments to the comparison command with `&` inside a PowerShell session:

```powershell
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 compare -Paths @('docs/characters/<char>/drafts/<selected>.png', 'docs/characters/<char>/drafts/<edited>.png') -Labels @('선택본', '수정안') -DisplayWidth 240 -DisplayHeight 360 -OutPath 'docs/characters/<char>/drafts/<asset>-comparison.png'
```

`compare` places two or more inputs in order, showing each label, original dimensions, scale, and dark/light backgrounds. Labels default to filenames. Originals remain unchanged; output is written separately.

- `-CompareMode SharedScale` (default): one shared scale fits all full source canvases. Use for preservation comparisons where changes in canvas size or padding must stay visible. It does not normalize resolution, padding, or silhouette size.
- `-CompareMode IndividualFit`: each full source canvas fits the display area independently. Use original files directly for mixed-resolution comparisons, without intermediate resized copies. Each image reports its original dimensions and applied scale; the result's shared `Scale` is null and each `Images` entry has its own `Scale`. It preserves all pixels in the composition but does not equalize subject height or transparent padding. Judge those differences explicitly; this mode is not evidence of unchanged character scale.
- `-CompareMode SubjectFit`: scales each image so its subject bounding box (alpha > 128, which excludes the official renders' soft ground shadow) is 90% of `DisplayHeight`, centers it horizontally and aligns its lowest visible pixel to a common baseline. Cells widen for wide poses rather than shrinking them. Use it for style and density comparisons between transparent sprites. Each entry reports `SubjectBox`, `SubjectDrawHeight` and `SubjectIsFullCanvas`. When that flag is true the source has no transparent surround, as with the public screenshot crops, so its "subject" is the whole canvas. Raised weapons or wide cloth count toward subject height.

Official comparison inputs come from the installed game. `official-poses` renders time 0 of `idle_loop` (combat) and `relaxed_loop` (merchant) for Ironclad, Silent, Defect, Necrobinder and Regent through the game's own Spine extension, framed to their alpha with 5% margins. It resolves `GodotPath` and `Sts2Path` through the same MSBuild settings as `dev.ps1`, including `local.props` overrides and Steam discovery. Override either value with `-GodotPath` or `-GamePath`; passing both avoids SDK discovery. Engine, stdout and stderr logs are written beside the renders. Keep these game assets out of version control. The example creates a fresh temporary output directory and compares files produced by that invocation:

```powershell
$poseOutput = Join-Path ([IO.Path]::GetTempPath()) ('sts2-official-poses-' + [guid]::NewGuid().ToString('N'))
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 official-poses -OutPath $poseOutput
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 compare -Paths @('docs/characters/<char>/drafts/<candidate>.png', (Join-Path $poseOutput 'ironclad-combat-idle_loop-00.png'), (Join-Path $poseOutput 'silent-combat-idle_loop-00.png')) -Labels @('Candidate', 'Ironclad', 'Silent') -CompareMode SubjectFit -DisplayWidth 240 -DisplayHeight 360 -OutPath 'docs/characters/<char>/drafts/<asset>-style-comparison.png'
```

Returned scale and display dimensions are observations, not quality scores.

`inspect -Path <png>` always returns structured measurements; `-AsObject` remains a compatibility no-op. `VisiblePixelRatio` describes occupied alpha above 8, `OpaquePixelRatio` describes alpha-255 pixels, and `FullyOpaque` is the exact all-pixels predicate. Pipe to `Format-List` only at the display boundary. Cards use `export-card` into a temporary candidate, then review and promotion; `check-cards` checks runtime dimensions/alpha. Use `tile-pair`, `export -BackgroundColor <hex>` only for non-card contracts permitting Fit margins, and `ui-preview` for layout observations. See [conversion-tools.md](scripts/conversion-tools.md) for commands and the temporary-output-first sequence.

- Are canonical identity, equipment, proportions, and the selected change axis preserved?
- Do major values, silhouette, expression, and action read at actual size?
- Are unintended halos, residue, or clipping absent on light/dark backgrounds?
- Are UI safe zones and text/frame exclusions respected?

Tool measurements and comparisons are observations, not formal tests, fixtures, or automatic acceptance thresholds.

## 4. Delivery and Completion Records

After applying a base or asset, update current appearance and adopted constraints in `<char>-art.md`; update technical mapping, dimensions, conversion procedures, delivery paths, and status in `<char>-assets.md`. Keep only decision rationale needed for future work in `<char>-history.md`. Explain adopted forms and forms to avoid directly. Full generation calls and version-by-version experiments are optional local records, not prerequisites for understanding the documents.

Before editing shared guidance after long-running generation or packaging, reread the current target sections and apply scoped patches. Preserve intervening edits; an earlier in-memory copy is not a safe replacement for the current file.

Keep one current status per asset family, including the latest candidate, unresolved differences, approval and runtime coverage. Replace superseded status paragraphs rather than accumulating multiple "current" candidates. Summarize historical decisions and their reasons in `<char>-history.md`; leave per-attempt measurements and execution logs in optional drafts. Link history from current status without requiring history to understand today's specification.

Before finishing, read the documentation as if from a fresh checkout without `drafts/`, personal caches, or execution logs. Check tracking and omissions for the base, runtime assets, and specifications using `git ls-files` and `git status`; do not substitute tracking every draft. Identify external games/packages by version, type/member, and summarized observations rather than personal cache paths or decompiler line numbers.

If runtime files changed, run `.\dev.ps1 -Task publish` to update Godot imports and PCK, then check loading. If local installation was requested, follow [build-install](../build-install/SKILL.md). Do not build/install for documentation-only or base-only work. Report file observations, package checks, actual game tests, and unperformed verification separately.
