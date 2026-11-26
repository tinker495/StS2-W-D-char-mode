---
name: character-asset-pipeline
description: Orchestrate StS2 character artwork from references to in-game delivery. Use for asset sets, unclear or mixed art requests, and character illustrations such as 휴식 일러스트.
---

# Character art orchestration

Select the specialist by the requested in-game use. For a set, identify its outputs first, establish any missing canonical identity once, then run only the matching specialists. Pass each the owner, approved reference, target scene and requested delivery scope. A complete player UI set spans standing, UI and selection. A campfire request starts with the rest specialist even when the current scene reuses standing art. Follow the rest specialist’s first-character comparison before producing a batch.

| Requested output | Production skill |
|---|---|
| Build or replace the canonical standing master of a playable StS2 character from identity references | [character-art-base](/.agents/skills/character-art-base/SKILL.md) |
| Paint Ancient or event scenes and derive their dialogue, history and map miniatures | [character-art-event](/.agents/skills/character-art-event/SKILL.md) |
| Produce standing, combat and merchant character sprites from an approved character base | [character-art-standing](/.agents/skills/character-art-standing/SKILL.md) |
| Create, review or apply seated StS2 rest-site artwork (휴식 일러스트, 휴식처, 모닥불) | [character-art-rest](/.agents/skills/character-art-rest/SKILL.md) |
| Produce character portraits, outlines, map markers and energy icons for the StS2 interface | [character-art-ui](/.agents/skills/character-art-ui/SKILL.md) |
| Create normal and locked character-select tiles and full-screen selection illustrations | [character-art-selection](/.agents/skills/character-art-selection/SKILL.md) |
| Paint a StS2 relic icon or a visually paired relic upgrade for the in-game relic bar | [character-art-relic](/.agents/skills/character-art-relic/SKILL.md) |
| Paint a StS2 power or status icon that reads at buff-bar and tooltip sizes | [character-art-power](/.agents/skills/character-art-power/SKILL.md) |
| Create a standard card portrait or an Ancient full-art card illustration for StS2 | [character-art-card](/.agents/skills/character-art-card/SKILL.md) |
| Edit a selected character asset, clean its surfaces, compare variants or convert an approved painting for delivery | [character-art-review](/.agents/skills/character-art-review/SKILL.md) |

For extraction or packaged-texture inspection alone, use [card observations](scripts/card-observations.md). For requested installation, complete the delivery records and hand the applied assets to [build-install](/.agents/skills/build-install/SKILL.md).

## Shared production contract

Read the applicable sections of [the shared workflow](workflow.md). During generation, read the active harness's section in [engines.md](engines.md); a supplied rough or motif also requires [reference roles and landmarks](references.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng). Read delivery records only when applying or finishing an asset. Do not load unrelated routes, all prompt recipes, or every character subsection.

## Find the target facts

Resolve `<char>` through [character naming and layout](/docs/conventions/characters/naming.md). The [topic-guide convention](/docs/conventions/documentation/character-guides.md) applies to every character and visual owner, including undecided names and Ancients.

- `docs/characters/<char>/<char>-art.md` routes the requested asset's brief, reference roles and visible identity, palette and equipment. If it is a topic index, open only the matching specification. Read character identity only when the character, body parts or canonical equipment appear; a standalone card-specific object needs its object/theme brief.
- `<char>-assets.md` routes filenames, dimensions, runtime mappings and current status. If it is an index, follow the matching delivery page and only the needed inventory; open API references when integration work requires them.
- `<char>.md` and [GLOSSARY.md](/GLOSSARY.md) own card, relic, and power effects and themes. Read the relevant definitions. Use `<char>-history.md` for the rationale behind adopted art decisions.
- [ARTS.md](/ARTS.md) owns rendering and composition. The route names the applicable sections and official pixels. Preserve common chibi scope: portrait/outline/map marker use chibi; selection tiles, standing art, cards and selection scenes keep canonical proportions.

For a missing guide or base, follow [Owners Without Art Guides](/docs/conventions/documentation/character-guides.md#owners-without-art-guides). Keep character-specific content in its topic pages. Drafts, prompts and observation sheets may remain in ignored `drafts/`; approved bases, runtime assets and necessary specifications must stand on their own.

**Application authority:** preserve the base and runtime assets until the user selects, approves or instructs their application. Reuse existing approval within its scope. Base-only approval does not authorize UI/runtime regeneration or game installation. The [shared workflow](workflow.md#delivery-and-completion-records) defines delivery and completion records.
