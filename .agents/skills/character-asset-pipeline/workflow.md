# Shared Asset Workflow

All routes use this workflow. For new generation, read Starting Inputs and the review loop. For a selected-image edit, read the review loop and the edit route. Read Delivery and Completion Records when applying or finishing an asset. Route files provide asset-specific references, geometry, preview sizes and prompt recipes.

## Starting Inputs

Generate new or replacement UI, event/selection scenes, cards, relics and powers from the approved base when applicable, supplied rough/object references, and same-use-case official StS2 images. Previous deliveries and earlier production drafts are comparison material after the new result exists, not initial edit targets, composition anchors or prompts to reproduce. The approved base, user-supplied rough/motif references and current-run object-family masters are exceptions. Explicitly selected-image edits use [the review route](routes/review.md).

This rule governs the initial composition. The bounded loop may edit a sound candidate from the current run to correct an observed defect; that does not make it a starting anchor for a later production request.

Open each actual input and record its role before generation. Use [reference roles and rough landmarks](references.md) to separate composition, identity, object construction and rendering. Separate invariant identity from staging: a base pose governs standing exports, not the camera, gaze, contacts or drawn/sheathed state of a new scene. A current-run portrait or object-family master may anchor related outputs; shared-master size conversions, outlines and locked variants remain valid.

Read [ARTS.md 1.3](../../../ARTS.md#13-composition-principles-for-all-assets), [2.1–2.3](../../../ARTS.md#2-shared-rendering-and-density-by-size), and the selected route's composition section. Generate directly for the intended camera, occupancy and canvas. Compare and resize the complete painting. Regenerate wrong compositions; cropping, extracted-region enlargement and automatic Cover fitting are not corrections. An explicit crop request applies only to its specified image and scope. Cards also use the strict geometry contract in ARTS.md 5.5.

## Default Generate–Review–Correct Loop

Run this loop for every newly generated or edited asset before the final recommendation. The agent performs the visual review automatically; the user need not request routine corrections. This is an execution procedure using the settled brief, not permission to invent new acceptance criteria or redesign approved choices.

1. **Lock the brief:** record the exact source/version, requested change, preserved features, output contract, and actual reference roles. When a rough supplies the composition, complete [Recipe 1-D's landmark brief](references.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng) before generation. For overlapping bodies or equipment, state camera-relative depth order and trace continuous objects through their contacts. Choose applicable review rows below before writing the prompt.
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

## Delivery and Completion Records

After applying a base or asset, update the appearance specification reached from `<char>-art.md` and the matching delivery/mapping page reached from `<char>-assets.md`. When these guides are topic indexes, keep them as short routing tables and put details in the owning topic; otherwise update the applicable section in place. Record dimensions, conversion, paths and status on the delivery page. Keep only decision rationale needed for future work in `<char>-history.md`. Explain adopted forms and forms to avoid directly. Full generation calls and version-by-version experiments are optional local records, not prerequisites for understanding the documents.

Before editing shared guidance after long-running generation or packaging, reread the current target sections and apply scoped patches. Preserve intervening edits; an earlier in-memory copy is not a safe replacement for the current file.

Keep one current status per asset family, including the latest candidate, unresolved differences, approval and runtime coverage. Replace superseded status paragraphs rather than accumulating multiple "current" candidates. Summarize historical decisions and their reasons in `<char>-history.md`; leave per-attempt measurements and execution logs in optional drafts. Link history from current status without requiring history to understand today's specification.

Before finishing, read the documentation as if from a fresh checkout without `drafts/`, personal caches, or execution logs. Check tracking and omissions for the base, runtime assets, and specifications using `git ls-files` and `git status`; do not substitute tracking every draft. Identify external games/packages by version, type/member, and summarized observations rather than personal cache paths or decompiler line numbers.

If runtime files changed, run `.\dev.ps1 -Task publish` to update Godot imports and PCK, then check loading. If local installation was requested, follow [build-install](../build-install/SKILL.md). Do not build/install for documentation-only or base-only work. Report file observations, package checks, actual game tests, and unperformed verification separately.
