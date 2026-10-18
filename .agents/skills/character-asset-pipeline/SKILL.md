---
name: character-asset-pipeline
description: Character asset pipeline (refs → base → assets). Use to establish an approved base from raw references, then generate, review, convert, and deliver UI, standing art, relics, powers, cards, and selection illustrations anchored to that base.
---

# Character Asset Pipeline

This skill owns shared execution procedures only. Keep character names, appearance, palettes, proportions, equipment, themes, counts, and version histories in character documents.

## 1. Establish the Target and Scope

Before working, read:

1. [ARTS.md](../../../ARTS.md): shared rendering, composition, and display-size rules.
2. `docs/characters/<char>/<char>-art.md`: the target's canonical identity, appearance, palette, reference roles, asset-specific proportions/staging, and prompt material.
3. The current status and requested asset sections of `docs/characters/<char>/<char>-assets.md`: delivery paths, dimensions, and model/scene wiring. Read `<char>-history.md` only when investigating previous choices, rejections, or deliveries.
4. For cards, relics, and powers, the relevant `<char>.md`, [GLOSSARY.md](../../../GLOSSARY.md), and specifications: the meaning of game effects and themes.

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

1. Open all raw references. Record adopted and excluded features in the target art specification. For revisions, separate features to preserve from the requested change axis.
2. For a new standing master or composition, combine Recipe 1-A, the common style, and character specification. For a selected image's local edit, use Recipe 1-B: exact source → changed axis → preserved features. Treat body direction, head angle, and eye direction separately.
3. Compare at the actual display size. Branch from a clean base/selected candidate instead of repeatedly editing a rejected result. Distinguish independent axes such as length, width, and thickness; keep small requests scoped.
4. If accumulated edits or surface noise are visible, consider Recipe 2's cleanup pass before promotion. If the user requests applying the final painting unchanged, preserve it without additional generation.
5. Promote the approved painting to the requested `base.png` and update the current-status table. Describe appearance changes, runtime coverage, and remaining differences. Record dimensions, procedure, and observed pixel changes for delivery conversions such as transparent padding. Export runtime standing art only if requested.

### Branch B: `base → UI, Standing Art, Selection Illustration`

1. Open the approved `base.png` as the primary identity/equipment input. Follow ARTS.md's common chibi scope; obtain proportion details, visible parts, expressions, and backgrounds from the target art specification.
2. Produce **two distinct full-body assets**. Ordinary standing art uses the complete approved `base.png` directly, with only proportional Fit resizing and necessary transparent padding. Combat art is newly generated from that base in a combat-appropriate pose; use Recipe 1-C and original-game combat references. Preserve identity, equipment, and rendering while designing a grounded stance, weight distribution, usable grip, and forward attention. A static PNG may depict a combat-ready idle pose; animation is not required. Do not deliver the base's relaxed pose as both assets. The target asset guide owns filenames and the separate combat versus rest/merchant scene mapping.
3. Design portrait and marker cameras/margins separately. For selection tiles, read the [normal/locked tile study](../../../docs/research/sts2-character-select-tile-study.md) and actual originals, then generate a new face-focused master.
4. Where possible, derive normal/locked tiles and portrait outlines from the same final alpha. Resize both energy sizes from one master. Do not copy another character's colors, outline thickness, or proportions.
5. A complete Branch B request includes UI, outline, **ordinary standing art, separate combat art**, and the selection illustration. Follow the shared input rule and [Recipe 4-D](PROMPT-RECIPES.md#4-d-char_select_bgpng-1920--1080-selection-illustration) to design a new scene from original-game research.
6. Follow the asset guide's model-property and scene contracts. Use an appropriate UI structure such as Control/TextureRect for selection backgrounds. Confirm all requested files and necessary references are connected. For a partial request, produce only that scope. Generating a new combat candidate does not approve its runtime promotion.

### Branch C: `base → Relics, Powers, Cards`

1. Read the target's themes and recurring objects, plus the asset guide's actual item lists and counts.
2. Input `base.png` when the character, body parts, or unique equipment appear. First generate and establish object-family masters for this production run, then use them to keep related objects consistent. Do not start from previous card, relic, or power deliveries.
3. Combine the relevant composition and safe zones from Recipes 5–8. Distinguish scenes from single symbols and deliver to the target file mapping.

### Branch D: Review, Cleanup, and Conversion

Use `inspect` and `preview` to examine the master and actual display size first. Use Recipe 1-B for selected-image edits, Recipe 2 for surface cleanup, and export/outline for delivery. For comparison-only requests, use `compare` to lay out complete paintings at one scale.

## 3. Observation and Conversion Tools

Immediately after generation and final output, run `inspect` and `preview` from the bundled [asset-pipeline.ps1](scripts/asset-pipeline.ps1), then open the previews. Replace the placeholders below with actual target values.

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

`compare` places two or more inputs in order, showing each label, original dimensions, and dark/light backgrounds. Labels default to filenames. It applies **one shared scale** that fits every full source canvas, without normalizing resolution, transparent padding, or silhouette size. Originals remain unchanged; output is written separately. Returned scale and display dimensions are observations.

- Are canonical identity, equipment, proportions, and the selected change axis preserved?
- Do major values, silhouette, expression, and action read at actual size?
- Are unintended halos, residue, or clipping absent on light/dark backgrounds?
- Are UI safe zones and text/frame exclusions respected?

Tool measurements and comparisons are observations, not formal tests, fixtures, or automatic acceptance thresholds.

## 4. Delivery and Completion Records

After applying a base or asset, update current appearance and adopted constraints in `<char>-art.md`; update technical mapping, dimensions, conversion procedures, delivery paths, and status in `<char>-assets.md`. Keep only decision rationale needed for future work in `<char>-history.md`. Explain adopted forms and forms to avoid directly. Full generation calls and version-by-version experiments are optional local records, not prerequisites for understanding the documents.

Before finishing, read the documentation as if from a fresh checkout without `drafts/`, personal caches, or execution logs. Check tracking and omissions for the base, runtime assets, and specifications using `git ls-files` and `git status`; do not substitute tracking every draft. Identify external games/packages by version, type/member, and summarized observations rather than personal cache paths or decompiler line numbers.

If runtime files changed, run `.\dev.ps1 -Task publish` to update Godot imports and PCK, then check loading. If local installation was requested, follow [build-install](../build-install/SKILL.md). Do not build/install for documentation-only or base-only work. Report file observations, package checks, actual game tests, and unperformed verification separately.
