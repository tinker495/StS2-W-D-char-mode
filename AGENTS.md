
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

Suggestions to change or remove tests, and co-migration requirements below, are review criteria. They do not authorize an agent to change tests. Execution authority is governed by "Test Ownership and Change Authority" in this document.

### Architectural Recommendations

- Go beyond minimal edits to diff lines. When a change reveals structural problems such as duplicated ownership of a concept, shotgun surgery, delegation-only wrappers, or eroding stage boundaries, propose deeper alternatives explicitly: moving, merging, or deleting modules, or reassigning ownership.
- Optimize for minimum context rather than minimum change. Prefer deep modules with narrow interfaces that reduce the concepts maintainers must understand. Favor removing complexity over adding code.
- Flag speculative generality as a deletion candidate: single-implementation interfaces, configuration nobody sets, and hand-rolled replacements for standard-library functionality.
- Support structural recommendations with evidence of their actual maintenance cost and present them in severity order.

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

Name every character-owned path and identifier after the character's decided English name, following the Realta and Althea layouts. `CharacterN` / `character_n` placeholders remain only for characters whose name is undecided (currently `CharacterThree` and `CharacterFour`); rename them to this layout once the name is decided.

| Area | Layout (`<Char>` = `Realta`, `<char>` = `realta`) |
|---|---|
| Code | `Sts2WDCode/Characters/<Char>/`, namespace `Sts2WD.Sts2WDCode.Characters.<Char>`, subfolders `Cards/`, `Relics/`, `Potions/`, `Powers/` |
| Types | Model `<Char>`; pools `<Char>CardPool`, `<Char>RelicPool`, `<Char>PotionPool`; pool bases `<Char>Card`, `<Char>Relic`, `<Char>Potion` carry `[Pool]`; every other type is `<Char>`-prefixed and its file name matches its main type |
| Assets | `Sts2WD/images/charui/<char>/`, `Sts2WD/images/cards/<char>/`, `Sts2WD/materials/<char>/`, `Sts2WD/character_text/<char>.json` |
| Docs | `docs/characters/<char>/`: design `<char>.md`, plus `<char>-art.md`, `<char>-assets.md`, `<char>-history.md` |

Model IDs are `STS2WD-` plus the UPPER_SNAKE class name, so renaming a model class changes its saved ID; folders and namespaces leave IDs untouched. A model rename also carries the `STS2WD-<CHAR>.*` keys in `Sts2WD/localization/*/characters.json` and `ancients.json` and the preset's `characterId`. Moving a texture rewrites its `.import` `source_file` and the `.ctex` hash, which is the MD5 of the `res://` source path. Record the save-compatibility outcome in the character's design doc.

## Agent Skills

### Issue Tracker

Issues and specifications are managed in GitHub Issues for `tinker495/StS2-W-D-char-mode`. Read `docs/agents/issue-tracker.md` before reading, publishing, or editing them.

### Triage Labels

Issue classification uses five primary roles. Read the mapping in `docs/agents/triage-labels.md` before classifying issues or changing labels.

### Domain Docs

Use the root `GLOSSARY.md` and `docs/adr/` as the single domain context. Follow the reading rules in `docs/agents/domain.md` before exploring code.

### Art Direction

When creating or editing images, or writing image-generation prompts, read the root [ARTS.md](ARTS.md) and the target's `docs/characters/<char>/<char>-art.md` and `<char>-assets.md`. Follow the `character-asset-pipeline` skill (`refs → base → assets`).

`ARTS.md` owns the shared style, chibi scope, and composition principles. `<char>-art.md` owns the character's current appearance, palette, equipment, and prompt material. `<char>-assets.md` owns file mappings and runtime application status. Read `<char>-history.md` only when investigating past choices, rejections, or deliveries. The canonical visual anchor is `base.png` in the same directory, with `refs/` as supporting references. Keep character-specific descriptions out of shared documents and general-purpose skills.

Generate each asset directly in its intended composition using references from the original game for that use case. Regenerate when the composition is wrong; do not solve it through post-generation cropping, extracting and enlarging a region, or automatic Cover fitting. Resize while preserving the whole painting by default. Crop only when the user explicitly requests it for that image. See [ARTS.md section 1.3](ARTS.md#13-composition-principles-for-all-assets).
