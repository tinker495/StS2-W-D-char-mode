
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

Suggestions to change or remove tests, and co-migration requirements below, are review criteria. They do not authorize an agent to change tests. Execution authority is governed by "Test Ownership and Change Authority" in this document.

### Architectural Recommendations

For architectural review, apply [architecture criteria](docs/conventions/review/architecture.md). Before code changes or standards reviews, use [CODING_STANDARDS.md](CODING_STANDARDS.md) to load the applicable topic rules for every affected character and shared module.

## Reuse StS2 Features First

Before implementing gameplay, model, UI, localization or resource behavior, read [native StS2 integration](docs/conventions/native/features.md) for the native-first rule and required evidence for custom behavior.

## Card Reference Previews

When a description references or produces concrete cards, including through keywords, apply [native card previews](docs/conventions/ui/card-previews.md) for registration, possible outcomes, upgrade state and recursion limits.

## Test Ownership and Change Authority

### Principle

Tests are executable product specifications owned by the user, not implementation accessories. The agent implements the product; it does not decide what qualifies as correct.

Agents may read and run tests. They may add tests only when a human explicitly instructs them to add tests, and only within the scope of that instruction. Agents may recommend additional tests, but may not add them on their own judgment. Requests to implement features, fix bugs, refactor, or make tests pass do not by themselves authorize test additions or changes to acceptance criteria. Existing tests may not be modified, deleted, moved, or automatically regenerated.

Every test must identify its human author or, for an agent-written test, the human who explicitly requested its addition. Include comments explaining the request and the context for adding the test in detail.

### Protected Assets

Treat all repository tests and assets that determine product acceptance as protected, regardless of their path or name. This includes:

- Test code, assertions, test cases, and parameters.
- Expected values, fixtures, snapshots, golden data, and reference results.
- Independent validators and oracles, evaluation data, acceptance thresholds, and tolerances.
- Test collection, execution, and exclusion settings, and CI validation stages.

Agents may not exempt assets by relabeling them as internal unit tests, regression tests, or test improvements.

### Allowed Work

- Read and run existing tests and validation assets to understand requirements and failures.
- Add tests explicitly requested by a human, within the authorized scope defined above.
- Modify product implementation to satisfy user requirements.
- Create temporary reproductions, exploratory scripts, and observation logs for debugging.
- Propose necessary test cases, counterexamples, and specification ambiguities to the user in text.

Place temporary verification code only in temporary locations outside formal test directories. Do not commit it, register it with CI, or promote it to formal tests. Its success does not constitute user approval or product acceptance.

### Prohibited Work

- Adding tests without an explicit human instruction, or beyond the scope of that instruction.
- Modifying, deleting, moving, or automatically regenerating existing tests.
- Treating current implementation output as the correct answer and fixing expected values or snapshots to match it.
- Weakening assertions, widening tolerances, removing cases, adding skip/xfail handling, changing fixtures, or bypassing validation stages to make tests pass.
- Detecting the test environment or specific test inputs to hardcode answers, or hiding failures, violations, or omissions to pass.
- Relaxing this policy or related protections for convenience.

### Failures and Specification Conflicts

Investigate implementation defects first when a test fails. Failure alone does not establish that a test is wrong. Conversely, an existing test is not automatically a user-approved product specification.

When a test conflicts with user requirements or there is insufficient evidence to establish expected behavior, do not change the test or preserve incorrect implementation behavior. Report the conflicting requirements, reproduction input, actual result, and required user decision. Hold only the changes that depend on that decision.

When additional tests appear necessary, propose the input conditions, behavior to verify, and evidence supporting the expected result to the user. Wait for an explicit human instruction to add those tests before implementing them.

### Completion Report

Distinguish the scope and results of tests actually run, verification not performed, remaining failures, and decisions needed from the user.

## Shared Documentation

Write shared documentation so it remains understandable without local drafts, generation logs, or personal caches. State the rules, rationale, current behavior, and procedures needed to reproduce the work in the document itself. Do not rely on "same as the previous version" or descriptions that require opening Git-ignored files. Drafts, prompts, and comparison images do not all need to be tracked. Track product files and necessary specifications. Identify external evidence through public sources or versions, types, and members, together with observations summarized in the document.

## Character Naming

Before creating or renaming character-owned types, paths or identifiers, apply [character naming and layout](docs/conventions/characters/naming.md), including undecided names, native roles and save compatibility. When creating or reorganizing character documentation, follow [topic-based character guides](docs/conventions/documentation/character-guides.md); the same rules cover every character and visual owner.

## Agent Skills

### Repository Workflow

Before repository exploration, design interviews, or rebasing, read [docs/agents/workflow.md](docs/agents/workflow.md) for settled decisions, fact-finding scope, bounded file reads, and the rebase completion check. Apply these repository rules when using skills. Use its shared-record ownership rules when updating deployment documentation.

### Issue Tracker

Issues and specifications are managed in GitHub Issues for `tinker495/StS2-W-D-char-mode`. Read `docs/agents/issue-tracker.md` before reading, publishing, or editing them.

### Triage Labels

Issue classification uses five primary roles. Read the mapping in `docs/agents/triage-labels.md` before classifying issues or changing labels.

### Domain Docs

Use the root `GLOSSARY.md` and `docs/adr/` as the single domain context. Follow the reading rules in `docs/agents/domain.md` before exploring code.

### Art Direction

When creating or editing images, or writing image-generation prompts, start with [character-asset-pipeline](.agents/skills/character-asset-pipeline/SKILL.md) and open only the requested route. It identifies the required sections of [ARTS.md](ARTS.md), the target's `<char>-art.md` and `<char>-assets.md`, reference images and engine instructions. Use the card-observation route to extract native card portraits or inspect a packaged texture; previous tasks' draft folders are not a prerequisite.

`ARTS.md` owns the shared style, chibi scope, official StS2 reference anchors, and composition principles. `<char>-art.md` and `<char>-assets.md` are short indexes to the requested topic's specification, file mappings and runtime status under `visuals/<topic>/`. Read `<char>-history.md` only when investigating past choices, rejections, or deliveries. The canonical visual anchor is `base.png` in the same directory, with `refs/` supplying character, rough-sketch, and motif/object references. For owners without guides or an approved base, use [the missing-guide procedure](docs/conventions/documentation/character-guides.md#owners-without-art-guides). Keep character-specific descriptions out of shared documents and general-purpose skills.

Generate each asset directly in its intended composition by passing `base.png` (when applicable), any rough/motif reference in `refs/`, and the same-use-case official StS2 reference from [sts2-style-refs](docs/research/sts2-style-refs/README.md) as image inputs, then comparing at actual display size. Regenerate when the composition is wrong; do not solve it through post-generation cropping, extracting and enlarging a region, or automatic Cover fitting. Resize while preserving the whole painting by default. Crop only when the user explicitly requests it for that image. See [ARTS.md section 1.3](ARTS.md#13-composition-principles-for-all-assets) and [section 2.3](ARTS.md#23-actual-official-image-anchors-and-priority).
