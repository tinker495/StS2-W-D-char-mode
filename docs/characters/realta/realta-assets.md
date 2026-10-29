# Realta Asset Production and Integration Guide

This index routes file mappings, current application status and native integration references. [Art direction](realta-art.md) owns appearance and composition; [history](realta-history.md) owns past decisions. Read the requested topic, then its API page only when changing model or scene integration.

## 0. Current Application Status

| Work | Read first | Read when changing integration or investigating older results |
|---|---|---|
| Card illustration | [Delivered portraits](visuals/cards/assets.md), [filename index](visuals/cards/inventory.md) | [Portrait API](visuals/cards/api.md) |
| Card frame and cost label | [Applied frame](visuals/cards/frames.md) | [Frame art](visuals/cards/frame-art.md) |
| Base-derived UI, icons and energy counter | [Runtime PNG inventory](visuals/ui/assets.md) | [Conversion](visuals/shared/conversion.md), [character API](visuals/shared/character-api.md) |
| Combat, rest and merchant animation | [Scene connections](visuals/animation/assets.md) | [Scene factory API](visuals/animation/api.md), [observation scope](visuals/animation/verification.md), [rig sources](animation/README.md) |
| Selection tiles and background | [Runtime PNG inventory](visuals/ui/assets.md), [selection art](visuals/selection/art.md) | [Scene connections](visuals/animation/assets.md), [conversion](visuals/shared/conversion.md) |
| Relic icons | [Deliveries, API and filenames](visuals/relics/assets.md) | [Relic art](visuals/relics/art.md) |
| Power icons and display titles | [Presentation and API](visuals/powers/assets.md), [filename table](visuals/powers/inventory.md) | [Power art](visuals/powers/art.md) |

The card inventory covers 81 design cards; RealtaStrike and RealtaDefend additionally reuse vanilla starting portraits. The relic set has five models. The delivered power set has 24 mod-supplied icons (21 custom powers and three native-power presentations); ForgeHeat retains native art. Package, installation and game-verification records belong to [deployment status](../../deployment-status.md), including the 2026-10-24 card/relic/Aselige release. A historical installation does not establish the current checkout's installed state.

## Shared References

- [Resource paths, import/PCK packaging and model IDs](visuals/shared/resources.md): use when adding files or investigating missing resources.
- [Asset model inheritance](visuals/shared/models.md): use when changing which model owns a visual override.
- [Character property API](visuals/shared/character-api.md): use when selecting a native or BaseLib extension point.
- [Delivery conversion](visuals/shared/conversion.md): use when producing UI/standing/selection PNGs.
- [Cross-category dimensions and extension points](visuals/shared/specifications.md): use for planning across asset families.

These pages retain the StS2 / BaseLib 3.4.7 observations and link to [mod code](../../../Sts2WDCode/), [runtime images](../../../Sts2WD/images/) and [project dependencies](../../../Sts2WD.csproj). Identify external APIs by assembly, type and member, not personal cache paths. Follow [ARTS.md](../../../ARTS.md) for shared style.

## Follow-Up Scope

If the base changes or the user requests revisions, record the differences in the relevant topic and update only affected assets. Image updates do not authorize gameplay or path-helper changes. Multiplayer hands, card trails and additional animations remain separate scope; current combat/rest/merchant/selection connections are documented above.
