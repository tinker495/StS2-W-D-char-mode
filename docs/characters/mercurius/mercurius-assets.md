# Mercurius Asset Status

## UI and Shared Standing

The tracked game resources use the full approved [base.png](base.png) for the shared standing sprite, a low-angle selection tile with relaxed eyes, and a separate flute-playing selection background. The nine textures and four scenes are mapped in [UI and standing assets](ui-assets.md) and [selection assets](selection-assets.md). The [base asset guide](base-assets.md) owns approved dimensions and conversion.

## Cards and Power Icons

The 2026-10-28 local installation included 81 character cards, three internal Tune options, and five character relics with their runtime icons. Use the [deployment record](/docs/deployment-status.md#2026-10-28-메르쿠리우스-카드유물-로컬-설치) for the installed package scope.

The basic Strike portrait uses the opaque 1000 x 760 texture [mercurius_strike.png](/Sts2WD/images/card_portraits/mercurius/mercurius_strike.png). `MercuriusStrike` inherits the model-ID based lookup in `Sts2WDCard` through `MercuriusCard`. Cards without a dedicated texture and the power icons use the native placeholders. The Tune options use the native Acrobatics, Hologram, and Reboot artwork. See [Mercurius's mechanics](mercurius.md) for the generation rules and saved IDs.

## Relic Icons

Each slug below has a 94 x 94 `<slug>.png`, a 94 x 94 `<slug>_outline.png`, and a 256 x 256 `big/<slug>.png` under `Sts2WD/images/relics/`. The icons use whole-painting Fit conversion, and the white outline is a 2 px dilation of the small icon's alpha.

| Korean title | Filename slug |
|---|---|
| 용의 이정표 | `mercurius_dragons_waymark` |
| 샛별 | `mercurius_morning_star` |
| 불꽃 두루마리 | `mercurius_flame_scroll` |
| 방호의 결속 | `mercurius_bond_of_protection` |
| 세계의 기억 | `mercurius_memory_of_the_world` |

`Sts2WDRelic` resolves these filenames for the five models. The [art direction](mercurius-art.md) describes the approved objects.
