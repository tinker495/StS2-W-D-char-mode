# Realta Asset Production and Integration Guide

This index routes file mappings, current application status and native integration references. [Art direction](realta-art.md) owns appearance and composition; [history](realta-history.md) owns past decisions. Read the requested topic, then its API page only when changing model or scene integration.

## 0. Current Application Status

| Work | Read first | Read when changing integration or investigating older results |
|---|---|---|
| Card illustration | [Delivered portraits](cards-assets.md), [filename index](cards-inventory.md) | [Portrait API](cards-api.md) |
| Card frame and cost label | [Applied frame](cards-frames.md) | [Frame art](cards-frame-art.md) |
| Base-derived UI, icons and energy counter | [Runtime PNG inventory](ui-assets.md) | [Conversion](shared-conversion.md), [character API](shared-character-api.md) |
| Combat, rest and merchant animation | [Scene connections](animation-assets.md) | [Scene factory API](animation-api.md), [observation scope](animation-verification.md), [rig sources](animation-sources.md) |
| Selection tiles and background | [Runtime PNG inventory](ui-assets.md), [selection art](selection-art.md) | [Scene connections](animation-assets.md), [conversion](shared-conversion.md) |
| Relic icons | [Deliveries, API and filenames](relics-assets.md) | [Relic art](relics-art.md) |
| Power icons and display titles | [Presentation and API](powers-assets.md), [filename table](powers-inventory.md) | [Power art](powers-art.md) |

The card inventory covers 81 design cards; RealtaStrike and RealtaDefend additionally reuse vanilla starting portraits. The relic set has five models. The delivered power set has 24 mod-supplied icons (21 custom powers and three native-power presentations); ForgeHeat retains native art. Package, installation and game-verification records belong to [deployment status](/docs/deployment-status.md), including the 2026-10-24 card/relic/Aselige release. A historical installation does not establish the current checkout's installed state.

## Shared References

- [Resource paths, import/PCK packaging and model IDs](shared-resources.md): use when adding files or investigating missing resources.
- [Asset model inheritance](shared-models.md): use when changing which model owns a visual override.
- [Character property API](shared-character-api.md): use when selecting a native or BaseLib extension point.
- [Delivery conversion](shared-conversion.md): use when producing UI/standing/selection PNGs.
- [Cross-category dimensions and extension points](shared-specifications.md): use for planning across asset families.

These pages retain the StS2 / BaseLib 3.4.7 observations and link to [mod code](/Sts2WDCode/), [runtime images](/Sts2WD/images/) and [project dependencies](/Sts2WD.csproj). Identify external APIs by assembly, type and member, not personal cache paths. Follow [ARTS.md](/ARTS.md) for shared style.

## Follow-Up Scope

If the base changes or the user requests revisions, record the differences in the relevant topic and update only affected assets. Image updates do not authorize gameplay or path-helper changes. Multiplayer hands, card trails and additional animations remain separate scope; current combat/rest/merchant/selection connections are documented above.
