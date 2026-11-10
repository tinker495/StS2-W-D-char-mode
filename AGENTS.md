
## Language by Audience

Choose the language by the intended reader, not by who writes the content or where the file lives.

| Intended reader | Language | Examples |
|---|---|---|
| People | Korean | GitHub PR titles and descriptions; review comments and findings; human-facing setting, lore, character references, and configuration guides |
| Agents | English | `AGENTS.md`; `ARTS.md`; skills and their supporting instructions; detailed implementation or asset-production decisions and reference material used to execute work |

An agent-written PR or review is still human-facing and must be in Korean. A skill that produces Korean output still uses English for its instructions. For mixed-purpose documentation, keep human explanations in Korean and agent execution instructions in English, with clearly separated sections or linked documents. Preserve code identifiers, paths, API names, established technical terms, literal quotations, and localized names in their original form.

Apply this rule when creating or updating documentation. Existing language does not override the intended audience.

## Code Review Rules


These rules apply to both human and AI reviewers. Formatting, linting, and other mechanical checks belong to CI and are outside review scope.

For spec reviews, follow [the domain review guidance](docs/agents/domain.md#spec-review), including the source comparison required for reused base-game effects.

Suggestions to change or remove tests, and co-migration requirements below, are review criteria. They do not authorize an agent to change tests. Execution authority is governed by [test ownership](docs/agents/test-ownership.md).

### Architectural Recommendations

For architectural review, apply [architecture criteria](docs/conventions/review/architecture.md). Before code changes or standards reviews, use [CODING_STANDARDS.md](CODING_STANDARDS.md) to load the applicable topic rules for every affected character and shared module.

## Reuse StS2 Features First

Before implementing gameplay, model, UI, localization or resource behavior, read [native StS2 integration](docs/conventions/native/features.md) for the native-first rule and required evidence for custom behavior.

## Card Reference Previews

When a description references or produces concrete cards, including through keywords, apply [native card previews](docs/conventions/ui/card-previews.md) for registration, possible outcomes, upgrade state and recursion limits.

## Test Ownership and Change Authority

Tests and every asset that determines product acceptance (expected values, fixtures, snapshots, validators, thresholds, collection settings, CI validation stages) are user-owned executable specifications.

- Read and run them freely.
- Add a test only on an explicit human instruction, within its scope, naming the requester and context in comments.
- Leave existing tests and acceptance assets as they are: no modification, deletion, move, regeneration, or weakening.

Before adding a test, touching anything under `tests/` or CI validation, handling a failing test or a test that conflicts with requirements, or writing a completion report, read [test ownership](docs/agents/test-ownership.md) for protected assets, allowed and prohibited work, conflict handling, and report contents.

## Shared Documentation

Write shared documentation so it remains understandable without local drafts, generation logs, or personal caches. State the rules, rationale, current behavior, and procedures needed to reproduce the work in the document itself. Do not rely on "same as the previous version" or descriptions that require opening Git-ignored files. Drafts, prompts, and comparison images do not all need to be tracked. Track product files and necessary specifications. Identify external evidence through public sources or versions, types, and members, together with observations summarized in the document.

## Character Naming

Before creating or renaming character-owned types, paths or identifiers, apply [character naming and layout](docs/conventions/characters/naming.md), including undecided names, native roles and save compatibility. When creating or reorganizing character documentation, follow [topic-based character guides](docs/conventions/documentation/character-guides.md); the same rules cover every character and visual owner.

## Agent Skills

### Repository Workflow

Before repository exploration, design interviews, or rebasing, read [docs/agents/workflow.md](docs/agents/workflow.md) for settled decisions, fact-finding scope, bounded file reads, and the rebase completion check. Apply these repository rules when using skills. Use its shared-record ownership rules when updating deployment documentation.

### Issue Tracker

Issues and specifications are managed in GitHub Issues for `tinker495/StS2-W-D-char-mode`. `gh` is off PATH on this PC: read `docs/agents/issue-tracker.md` for the executable path and command rules before the first `gh` call, including PR and CI-run queries.

### Triage Labels

Issue classification uses five primary roles. Read the mapping in `docs/agents/triage-labels.md` before classifying issues or changing labels.

### Domain Docs

Use the root `GLOSSARY.md` and `docs/adr/` as the single domain context. Follow the reading rules in `docs/agents/domain.md` before exploring code.

### Art Direction

When creating or editing images, or writing image-generation prompts, start with [character-asset-pipeline](.agents/skills/character-asset-pipeline/SKILL.md) and open only the requested route. It identifies the required sections of [ARTS.md](ARTS.md), the target's `<char>-art.md` and `<char>-assets.md`, reference images and engine instructions. Use the card-observation route to extract native card portraits or inspect a packaged texture; previous tasks' draft folders are not a prerequisite.

`ARTS.md` owns the shared style, chibi scope, official StS2 reference anchors, and composition principles. `<char>-art.md` and `<char>-assets.md` are short indexes to the requested topic's specification, file mappings and runtime status under `visuals/<topic>/`. Read `<char>-history.md` only when investigating past choices, rejections, or deliveries. The canonical visual anchor is `base.png` in the same directory, with `refs/` supplying character, rough-sketch, and motif/object references. For owners without guides or an approved base, use [the missing-guide procedure](docs/conventions/documentation/character-guides.md#owners-without-art-guides). Keep character-specific descriptions out of shared documents and general-purpose skills.

Generate each asset directly in its intended composition by passing `base.png` (when applicable), any rough/motif reference in `refs/`, and the same-use-case official StS2 reference from [sts2-style-refs](docs/research/sts2-style-refs/README.md) as image inputs, then comparing at actual display size. Regenerate when the composition is wrong; do not solve it through post-generation cropping, extracting and enlarging a region, or automatic Cover fitting. Resize while preserving the whole painting by default. Crop only when the user explicitly requests it for that image. See [ARTS.md section 1.3](ARTS.md#13-composition-principles-for-all-assets) and [section 2.3](ARTS.md#23-actual-official-image-anchors-and-priority).
