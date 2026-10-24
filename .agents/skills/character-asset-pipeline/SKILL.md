---
name: character-asset-pipeline
description: Character asset pipeline (refs → base → assets). Use to establish an approved base from raw references, then generate, review, convert, and deliver UI, standing art, relics, powers, cards, and selection illustrations anchored to that base.
---

# Character Asset Pipeline

This skill owns shared execution procedures only. Keep character names, appearance, palettes, proportions, equipment, themes, counts, and version histories in character documents.

## 1. Establish the Target and Scope

Choose the branch first, then read the shared rules and only the target-specific sections needed for it:

1. [ARTS.md](../../../ARTS.md): shared rendering, composition, and display-size rules.
2. `docs/characters/<char>/<char>-art.md`: the target's canonical identity, appearance, palette, reference roles, asset-specific proportions/staging, and prompt material.
3. The current status and requested asset sections of `docs/characters/<char>/<char>-assets.md`: delivery paths, dimensions, and model/scene wiring. Read `<char>-history.md` only when investigating previous choices, rejections, or deliveries.
4. For cards, relics, and powers, the relevant `<char>.md`, [GLOSSARY.md](../../../GLOSSARY.md), and specifications: the meaning of game effects and themes.

Use headings to select bounded reads; if output is truncated, retrieve the missing selected section before proceeding. In `ARTS.md`, read sections 1–4 and 7–8 plus the applicable composition subsection of section 5. In character documents, read identity/reference roles plus the requested asset's brief and current status. Expand only through relevant links.

| Branch | Recipe sections to read in `PROMPT-RECIPES.md` |
|---|---|
| A: new base | Shared Input Slots, Recipe 1 introduction, 1-A; 1-B only for selected-image edits; 2 only for cleanup |
| B: UI/standing/selection | Shared Input Slots and the requested recipes from 1-C, 3 and 4 |
| C: relics/powers/cards | Shared Input Slots and the requested recipes from 5–8 |
| D: review/edit/conversion | 1-B for edits, 2 for cleanup; section 3 below for observation/conversion |

If no art specification exists, first record observed references and user decisions in the character directory. Do not copy another character's traits as defaults. Resolve actual filenames through the asset guide.

```text
docs/characters/<char>/
├── <char>-art.md        # Identity, appearance, palette, staging, adopted constraints
├── <char>-assets.md     # Technical mapping and application status
├── <char>-history.md    # Rationale for past choices and deliveries; read on demand
├── <char>.md            # Human-facing domain specification
├── refs/                # Raw visual references
├── base.png             # Approved canonical base
└── drafts/              # Optional drafts, prompts, comparisons, logs; Git-ignored
```

Keep drafts, actual prompts, input-role notes, reviews, and execution logs in Git-ignored `drafts/` when useful. Do not copy every intermediate into tracked paths merely to make documentation shareable. State the current appearance, constraints and rationale, output dimensions, conversion procedures, and application status directly in shared documents. Avoid explanations or links that require a local file or historical version ID to understand. The approved base, runtime assets, and necessary specifications are the collaboration contract.

**Scope and approval:** do not overwrite the base or runtime assets before approval. Promote only versions the user selected, approved, or instructed you to apply, within the requested scope. Do not ask again for existing approval. Updating only the base does not authorize runtime/UI regeneration or installation.

## 2. Production Branches

Every branch follows [ARTS.md section 1.3](../../../ARTS.md#13-composition-principles-for-all-assets). Generate the target composition directly and compare/resize the complete painting. Regenerate incorrect compositions; do not fix them through post-generation cropping, extracted-region enlargement, or automatic Cover fitting. An explicit crop request applies only to that image and scope. Read [PROMPT-RECIPES.md](PROMPT-RECIPES.md) for templates.

### Branch B and C: Start with New Artwork

For new or replacement UI, selection backgrounds, cards, relics, and powers, design and generate from **the approved `base.png`, required raw references, and base-game references for the same use case**. Do not input previous deliveries or earlier production drafts as edit targets or composition anchors. Do not reuse prompts that reproduce an old composition. Open previous assets only for comparison after the new result exists. The approved base itself is exempt from this restriction.

Before generation, record each actual input's role and check that no previous asset is included. For selection backgrounds, proceed from original-game research to one characteristic action sentence, then a new camera, major shapes, and UI layout. Changing only a face or colors in an old background is not new scene production.

A portrait or object-family master newly created in the same production run may anchor related assets. Size conversion, alpha outlines, and locked variants from the same master remain valid. When the user explicitly selects an existing image for a partial edit, specify the scope and use Branch D/Recipe 1-B.

### Branch A: `refs → base`

1. Open all raw references. Record adopted and excluded features in the target art specification with [Recipe 1's source labels](PROMPT-RECIPES.md#recipe-1-new-canonical-art-and-local-edits). For revisions, separate preserved features from the requested change axis.
2. Generate with Recipe 1-A for a new standing master or composition, passing the character reference (`refs/`) directly as an image input alongside the official use-case images. Use Recipe 1-B for a local edit to a selected image.
3. Starting with the first candidate, run `inspect` and open a `compare` sheet of the complete candidate beside same-use-case official renders at the intended display size (section 3; `SubjectFit` for style, `SharedScale` for preservation). Review in this order: (a) camera, body direction, head angle and gaze; (b) action and weapon/equipment state; (c) silhouette, grounding, margins and hand/equipment connections; (d) materials, value range and detail density. **Completion criterion:** the sheet is opened, the principal differences are recorded, every Recipe 1-A after-generation observation is filled, drift is recorded as unresolved, and `MinMarginRatio` is checked against ARTS.md 5.1. Resolve composition or use-case mismatches before surface cleanup or recommending a candidate. File measurements do not establish visual agreement. Branch from clean references or a selected source rather than repeatedly editing a rejected result.
4. If accumulated edits or surface noise are visible, consider Recipe 2's cleanup pass before promotion. If the user requests applying the final painting unchanged, preserve it without additional generation.
5. Promote the approved painting to the requested `base.png` and update the current-status table. Describe appearance changes, runtime coverage, and remaining differences. Record dimensions, procedure, and observed pixel changes for delivery conversions such as transparent padding. Export runtime standing art only if requested.

### Branch B: `base → UI, Standing Art, Selection Illustration`

1. Open the approved `base.png` as the primary identity/equipment input. Follow ARTS.md's common chibi scope; obtain proportion details, visible parts, expressions, and backgrounds from the target art specification.
2. Produce **two distinct full-body assets**. Ordinary standing art uses the complete approved `base.png` directly, with only proportional Fit resizing and necessary transparent padding. Combat art is newly generated from that base in a combat-appropriate pose; use Recipe 1-C and original-game combat references. Preserve identity, equipment, and rendering while designing a grounded stance, weight distribution, usable grip, and forward attention. A static PNG may depict a combat-ready idle pose; animation is not required. Inspect the base's actual pose and design the separate combat asset for its use case. The target asset guide owns filenames and the separate combat versus rest/merchant scene mapping.
3. Design portrait and marker cameras/margins separately. For selection tiles, read the [normal/locked tile study](../../../docs/research/sts2-character-select-tile-study.md) and actual originals, then generate a new face-focused master.
4. Where possible, derive normal/locked tiles and portrait outlines from the same final alpha. Resize both energy sizes from one master. Do not copy another character's colors, outline thickness, or proportions.
5. A complete Branch B request includes UI, outline, **ordinary standing art, separate combat art**, and the selection illustration. Follow the shared input rule and [Recipe 4-D](PROMPT-RECIPES.md#4-d-char_select_bgpng-1920--1080-selection-illustration) to design a new scene from original-game research.
6. Follow the asset guide's model-property and scene contracts. Use an appropriate UI structure such as Control/TextureRect for selection backgrounds. Confirm all requested files and necessary references are connected. For a partial request, produce only that scope. Generating a new combat candidate does not approve its runtime promotion.

### Branch C: `base → Relics, Powers, Cards`

1. Read the target's themes and recurring objects, plus the asset guide's actual item lists and counts.
2. Input `base.png` when the character, body parts, or unique equipment appear. First generate and establish object-family masters for this production run, then use them to keep related objects consistent. Do not start from previous card, relic, or power deliveries.
3. Combine the relevant composition and safe zones from Recipes 5–8. Distinguish scenes from single symbols and deliver to the target file mapping.

### Branch D: Review, Cleanup, and Conversion

Use `inspect` and `preview` to examine the master and actual display size first. Use Recipe 1-B for selected-image edits, Recipe 2 for surface cleanup, and export/outline for delivery. For comparison-only requests, choose the `compare` mode in section 3 for the question being investigated.

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

Official comparison inputs come from the installed game. `official-poses` renders time 0 of `idle_loop` (combat) and `relaxed_loop` (merchant) for Ironclad, Silent, Defect, Necrobinder and Regent through the game's own Spine extension, framed to their alpha with 5% margins. It defaults to the MegaDot console path that `Directory.Build.props` assigns to `GodotPath` and to the default Steam install path; override with `-GodotPath` and `-GamePath`. Engine, stdout and stderr logs are written beside the renders. Output must stay in a Git-ignored path because the renders are game assets:

```powershell
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 official-poses -OutPath 'docs/research/sts2-style-refs/drafts/spine-poses'
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 compare -Paths @('docs/characters/<char>/drafts/<candidate>.png', 'docs/research/sts2-style-refs/drafts/spine-poses/ironclad-combat-idle_loop-00.png', 'docs/research/sts2-style-refs/drafts/spine-poses/silent-combat-idle_loop-00.png') -Labels @('Candidate', 'Ironclad', 'Silent') -CompareMode SubjectFit -DisplayWidth 240 -DisplayHeight 360 -OutPath 'docs/characters/<char>/drafts/<asset>-style-comparison.png'
```

Returned scale and display dimensions are observations, not quality scores.

- Are canonical identity, equipment, proportions, and the selected change axis preserved?
- Do major values, silhouette, expression, and action read at actual size?
- Are unintended halos, residue, or clipping absent on light/dark backgrounds?
- Are UI safe zones and text/frame exclusions respected?

Tool measurements and comparisons are observations, not formal tests, fixtures, or automatic acceptance thresholds.

## 4. Delivery and Completion Records

After applying a base or asset, update current appearance and adopted constraints in `<char>-art.md`; update technical mapping, dimensions, conversion procedures, delivery paths, and status in `<char>-assets.md`. Keep only decision rationale needed for future work in `<char>-history.md`. Explain adopted forms and forms to avoid directly. Full generation calls and version-by-version experiments are optional local records, not prerequisites for understanding the documents.

Keep one current status per asset family, including the latest candidate, unresolved differences, approval and runtime coverage. Replace superseded status paragraphs rather than accumulating multiple "current" candidates. Summarize historical decisions and their reasons in `<char>-history.md`; leave per-attempt measurements and execution logs in optional drafts. Link history from current status without requiring history to understand today's specification.

Before finishing, read the documentation as if from a fresh checkout without `drafts/`, personal caches, or execution logs. Check tracking and omissions for the base, runtime assets, and specifications using `git ls-files` and `git status`; do not substitute tracking every draft. Identify external games/packages by version, type/member, and summarized observations rather than personal cache paths or decompiler line numbers.

If runtime files changed, run `.\dev.ps1 -Task publish` to update Godot imports and PCK, then check loading. If local installation was requested, follow [build-install](../build-install/SKILL.md). Do not build/install for documentation-only or base-only work. Report file observations, package checks, actual game tests, and unperformed verification separately.
