---
name: character-asset-pipeline
description: Create, edit, review, or apply character artwork through focused base, UI/standing/selection, relic, power, and card routes with official StS2 image references.
---

# Character Asset Pipeline

Choose one route before reading production details. Route files are ordinary reference documents in this skill; open the selected file directly. Use the [bounded reading procedure](../../../docs/agents/workflow.md#read-within-an-output-budget), reusing material already read in the session.

| Request | Read next |
|---|---|
| A: establish or replace the canonical base | [Base](routes/base.md) |
| B: portrait, map marker, energy, standing/combat, selection tile or scene | [UI, standing and selection](routes/ui.md); only the requested output's subsection |
| C-1: relic object or paired upgrade | [Relics](routes/relics.md) |
| C-2: power/status symbol | [Powers](routes/powers.md) |
| C-3: standard or Ancient card illustration | [Cards](routes/cards.md); only the matching card recipe |
| D: selected-image edit, cleanup, comparison or delivery conversion | [Review and conversion](routes/review.md) |
| Extract native card references or inspect a packaged texture | [Card observations](scripts/card-observations.md) |

Read the selected route and the applicable sections of [the shared workflow](workflow.md). During generation, read the active harness's section in [engines.md](engines.md); a supplied rough or motif also requires [reference roles and landmarks](references.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng). Read delivery records only when applying or finishing an asset. Do not load unrelated routes, all prompt recipes, or every character subsection.

## Find the target facts

- `docs/characters/<char>/<char>-art.md` routes the requested asset's brief, reference roles and visible identity, palette and equipment. If it is a topic index, open only the matching specification. Read character identity only when the character, body parts or canonical equipment appear; a standalone card-specific object needs its object/theme brief.
- `<char>-assets.md` routes filenames, dimensions, runtime mappings and current status. If it is an index, follow the matching delivery page and only the needed inventory; open API references when integration work requires them.
- `<char>.md` and [GLOSSARY.md](../../../GLOSSARY.md) own card/relic/power effects and themes; read the relevant definitions. `<char>-history.md` is needed only for past decisions or rejected versions.
- [ARTS.md](../../../ARTS.md) owns rendering and composition. The route names the applicable sections and official pixels. Preserve common chibi scope: portrait/outline/map marker use chibi; selection tiles, standing art, cards and selection scenes keep canonical proportions.

If the target has no art specification, first record observed references and settled user decisions in its character directory. Keep character-specific content there, not in this skill. Drafts, prompts and observation sheets may remain in ignored `drafts/`; the approved base, runtime assets and necessary specifications must stand on their own.

**Application authority:** preserve the base and runtime assets until the user selects, approves or instructs their application. Reuse existing approval within its scope. Base-only approval does not authorize UI/runtime regeneration or game installation. The [shared workflow](workflow.md#delivery-and-completion-records) defines delivery and completion records.
