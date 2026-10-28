
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

- Go beyond minimal edits to diff lines. When a change reveals structural problems such as duplicated ownership of a concept, shotgun surgery, delegation-only wrappers, or eroding stage boundaries, propose deeper alternatives explicitly: moving, merging, or deleting modules, or reassigning ownership.
- Optimize for minimum context rather than minimum change. Prefer deep modules with narrow interfaces that reduce the concepts maintainers must understand. Favor removing complexity over adding code.
- Flag speculative generality as a deletion candidate: single-implementation interfaces, configuration nobody sets, and hand-rolled replacements for standard-library functionality.
- Support structural recommendations with evidence of their actual maintenance cost and present them in severity order.

## Reuse StS2 Features First

Prefer StS2's existing features and extension points for gameplay, commands, models, UI, tooltips, localization, and resource handling. Before adding a mod-owned equivalent or a Harmony patch, inspect the relevant native type, member, and vanilla call site in the installed game. Use the native API directly or through established BaseLib integration points; keep content and presentation changes within that native flow.

Add custom behavior only for a required capability the native implementation lacks or a confirmed defect. Record the game version, relevant type/member, observed limitation, and why existing APIs or extension points are insufficient in the implementation documentation. Keep the extension limited to that gap and preserve native behavior outside it. For named base-game effects and their mappings, follow [ADR-0001](docs/adr/0001-reuse-base-game-effects.md).

## Card Reference Previews

In-game descriptions, including those on cards, relics, powers, and potions, must include a native card preview for every concrete card they reference, including cards named or produced through a keyword such as Reform or Guard. Register previews through the model's native hover-tip API, such as `ExtraHoverTips`, with `HoverTipFactory.FromCard<T>()`; use `FromCardWithCardHoverTips<T>()` when the referenced card's keyword explanations are also needed. Localized names and color tags alone do not register previews.

Show only outcomes possible for the described target, and explain the mechanic's mapping in its keyword description. When the target determines one result, preview that result alone; Warrior Strike reforms itself from an Attack into Siarcun. When the target is not yet known, show all possible base outcomes and explain upgrade inheritance. Share a mechanic's preview list across cards and powers with the same possible outcomes. Match preview upgrades to the referenced card's rules: an unupgradable token stays unupgraded; a known transformation target supplies its own upgrade state. A generator's upgrade state alone does not imply an upgraded result.

Generic references such as "a random card" or "the selected card" have no fixed model to preview. A card already being displayed needs no preview of itself. Keep preview dependencies finite and avoid recursive self-reference.

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

Name every character-owned path and identifier after the character's decided English name, following the Realta and Althea layouts. `CharacterN` / `character_n` placeholders remain only for characters whose name is undecided (currently `CharacterFour`); rename them to this layout once the name is decided. The third character is now `Mercurius`.

| Area | Layout (`<Char>` = `Realta`, `<char>` = `realta`) |
|---|---|
| Code | `Sts2WDCode/Characters/<Char>/`, namespace `Sts2WD.Sts2WDCode.Characters.<Char>`, subfolders `Cards/`, `Relics/`, `Potions/`, `Powers/` |
| Types | Model `<Char>`; pools `<Char>CardPool`, `<Char>RelicPool`, `<Char>PotionPool`; pool bases `<Char>Card`, `<Char>Relic`, `<Char>Potion` carry `[Pool]`; every other type is `<Char>`-prefixed. One top-level type per file, named after that type: each card, relic, potion and power model gets its own `<Type>.cs` and `.cs.uid`, and a class stays in one file (no `partial` split) |
| Assets | `Sts2WD/images/charui/<char>/`, `Sts2WD/images/cards/<char>/`, `Sts2WD/materials/<char>/`, `Sts2WD/character_text/<char>.json` |
| Docs | `docs/characters/<char>/`: design `<char>.md`, plus `<char>-art.md`, `<char>-assets.md`, `<char>-history.md` |

Model IDs are `STS2WD-` plus the UPPER_SNAKE class name, so renaming a model class changes its saved ID; folders and namespaces leave IDs untouched. A model rename also carries the `STS2WD-<CHAR>.*` keys in `Sts2WD/localization/*/characters.json` and `ancients.json` and the preset's `characterId`. Moving a texture rewrites its `.import` `source_file` and the `.ctex` hash, which is the MD5 of the `res://` source path. Record the save-compatibility outcome in the character's design doc.

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

`ARTS.md` owns the shared style, chibi scope, official StS2 reference anchors, and composition principles. `<char>-art.md` owns the character's current appearance, palette, equipment, rough/reference roles, and prompt material. `<char>-assets.md` owns file mappings and runtime application status. Read `<char>-history.md` only when investigating past choices, rejections, or deliveries. The canonical visual anchor is `base.png` in the same directory, with `refs/` (including category subfolders such as `refs/relics/`, `refs/powers/`, `refs/cards/`) supplying character, rough-sketch, and motif/object references. Keep character-specific descriptions out of shared documents and general-purpose skills.

Generate each asset directly in its intended composition by passing `base.png` (when applicable), any rough/motif reference in `refs/`, and the same-use-case official StS2 reference from [sts2-style-refs](docs/research/sts2-style-refs/README.md) as image inputs, then comparing at actual display size. Regenerate when the composition is wrong; do not solve it through post-generation cropping, extracting and enlarging a region, or automatic Cover fitting. Resize while preserving the whole painting by default. Crop only when the user explicitly requests it for that image. See [ARTS.md section 1.3](ARTS.md#13-composition-principles-for-all-assets) and [section 2.3](ARTS.md#23-actual-official-image-anchors-and-priority).
