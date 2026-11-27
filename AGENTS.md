## Language by Audience

Choose language by intended reader, not author or file location.

| Intended reader | Language | Examples |
|---|---|---|
| People | Korean | GitHub PR titles and descriptions; review comments and findings; human-facing setting, lore, character references, and configuration guides |
| Agents | English | `AGENTS.md`; `ARTS.md`; skills and instructions; implementation or asset-production decisions and execution references |

Agent-written PRs and reviews target people (Korean); skills producing Korean output keep their instructions in English. Separate mixed-audience docs by section or linked document. Preserve code identifiers, paths, API names, technical terms, literal quotations, and localized names verbatim. Apply on create or update regardless of existing language.

## Test Ownership and Change Authority

Tests and every asset determining product acceptance (expected values, fixtures, snapshots, validators, thresholds, collection settings, CI validation stages) are user-owned executable specifications.

- Read and run them freely.
- Add a test only on an explicit human instruction, within its scope, naming the requester and context in comments.
- Preserve existing tests and acceptance assets unchanged: no modification, deletion, move, regeneration, or weakening.

Before adding a test, touching `tests/` or CI validation, handling a failing or conflicting test, or writing a completion report, read [test ownership](docs/agents/test-ownership.md).

## Shared Documentation

Use repository-root-relative links (`/docs/...`) for cross-directory targets and local paths for same-folder or descendant links (no `..` segments); resolve leading `/` against the Git repository root. Keep tracked Markdown self-contained, Why-only, and backed only by applied production code, runtime paths, or verified deployments (keep work logs, candidates, and duplicate or post-application artifacts out of tracked `docs/`) — full rules in [comments and Markdown documentation](docs/conventions/documentation/comments-and-docs.md).

## Toolchain and Pipeline Updates

Updating any repository authoring, build, or verification tool (such as the local `rig-c` toolchain) or adopting a new standard pipeline requires refreshing or migrating existing tracked outputs so no character remains on stale build artifacts or a superseded pipeline — follow [Toolchain and Pipeline Updates](docs/agents/workflow.md#toolchain-and-pipeline-updates) and [ADR-0008](docs/adr/0008-native-spine-rigs-and-artifact-refresh.md).

## Task Routing

Read the matching single source of truth before acting. Formatting, linting, and other mechanical checks belong to CI; review suggestions never authorize test edits.

| When your task involves | Read |
|---|---|
| Implementation, asset delivery, or opening/updating a PR | [Complete Requested Work](docs/agents/workflow.md#complete-requested-work) and [PR skill](.agents/skills/pr/SKILL.md) |
| Repository exploration, design interviews, rebasing, temporary observations, skills, or deployment records | [Repository workflow](docs/agents/workflow.md) |
| Domain terms, ADRs, or starting code exploration | [Domain docs](docs/agents/domain.md) (`GLOSSARY.md`, `docs/adr/`) |
| Code changes, standards review, or architectural review | [CODING_STANDARDS.md](CODING_STANDARDS.md) and [architecture criteria](docs/conventions/review/architecture.md) |
| Spec review | [Spec review](docs/agents/domain.md#spec-review) (includes base-game source comparison) |
| Gameplay, model, UI, localization, or resource behavior | [Native StS2 integration](docs/conventions/native/features.md) (reuse native APIs first) |
| Character types, paths, anatomical `_l`/`_r` rig names, or renames | [Character naming and layout](docs/conventions/characters/naming.md) |
| Character or visual-owner guides | [Topic-based character guides](docs/conventions/documentation/character-guides.md) |
| Character artwork requests, including creating or applying rest-site illustrations (휴식 일러스트, 휴식처, 모닥불), art direction, image prompts, or packaged card texture inspection | [character-asset-pipeline](.agents/skills/character-asset-pipeline/SKILL.md) (open only the requested route), [ARTS.md](ARTS.md), and [character guides](docs/conventions/documentation/character-guides.md) |
| Install package, PCK contents, `.import` texture compression tiers, or Release/Debug bundle | [Install package and PCK minimization](docs/conventions/packaging/install-package.md) |
| GitHub issues, PRs, or CI queries (`gh` is off PATH) | [Issue tracker](docs/agents/issue-tracker.md) before the first `gh` call |
| Issue classification or label changes | [Triage labels](docs/agents/triage-labels.md) |
| Local rig-c authoring, rendering, or verification | [Local Rig-C Toolchain](docs/agents/rig-c-local.md) |

### Card Reference Previews

When an in-game description references or produces concrete cards (including through keywords), apply [native card previews](docs/conventions/ui/card-previews.md).
