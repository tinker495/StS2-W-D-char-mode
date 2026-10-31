# Mercurius Asset Status

## 공식 베이스 — 2026-10-31

사용자가 선택한 색조 수정본 v7을 다른 전신 베이스에 맞춰 [base.png](base.png)에 적용했다. 현재 규격은 **1024 × 1536 RGBA**다. 사용자 요청에 따라 투명 여백만 정리하고 비례 확대하여 인물 높이를 캔버스의 82.7%로 맞췄으며, 그림의 색상·포즈·신체 비율은 유지했다. 파일·승인·런타임 적용 범위는 [베이스 에셋](visuals/base/assets.md), 확정 외형·색상·구도는 [베이스 제작 명세](visuals/base/art.md)를 따른다. 게임 내 UI·전투 이미지와 설치 상태는 이번 작업에서 변경하지 않았다.

## Current runtime delivery (2026-10-28)

The current package includes 81 character cards, three internal Tune option cards, five relics and their 15 runtime icon textures. Card portraits and power icons still use the documented native placeholders; the Strike candidate below is not connected to runtime. The six renamed character UI textures retain their placeholder artwork. The local package was installed; build, installation, dependency, backup and verification status is owned by the [shared deployment record](../../deployment-status.md#2026-10-28-메르쿠리우스-카드유물-로컬-설치). This does not establish live UI or network gameplay validation.

## Initial relic integration (2026-10-26)

The five relic effects and their 15 icon PNGs are connected to runtime under `Sts2WD/images/relics/` and `big/`, using the slugs in the table below. The user's approved fourth ring refinement is the Dragon's Waymark artwork. The character model, pools, bases and UI paths now use `Mercurius` / `mercurius`; the six unchanged placeholder UI PNGs moved to `Sts2WD/images/charui/mercurius/` with their import source paths and MD5-derived cache paths updated. The original seven script UID sidecars were preserved across rename; the engine generated UID sidecars for the twelve new scripts.

Three Token-rarity Tune option models use native Acrobatics, Hologram and Reboot portraits as temporary UI artwork. They do not use the new Strike candidate and cannot be randomly generated in combat. This task adds relics and Tune, not the rest of the character's card pool or canonical appearance. Behavior, timing and saved-ID compatibility are specified in `mercurius.md`.

C# build completed with zero warnings/errors. Direct `dotnet publish` produced the mod PCK, and a separate resource process loaded all 15 relic textures, all six renamed character UI textures and both languages' five relic / three option entries. Temporary native observations covered 43 effect/formatting cases; their isolated bootstrap supplies the mod model/pool registrations that normal BaseLib startup provides. These are diagnostic observations rather than formal acceptance tests. No Steam installation or Git upload was performed. Local MCP configuration was restored byte-for-byte after packaging with the development plugin temporarily disabled.

## Basic Strike candidate (2026-10-25)

`card-candidates/2026-10-25/mercurius_strike.png` is a 1000 x 760 opaque PNG pending user review. The built-in image generator created a new 1439 x 1093 landscape painting. Delivery proportionally Fit-resized the complete painting without cropping or region extraction. A 250 x 190 preview was opened beside the Realta/Althea candidates and native `StrikeIronclad` at the same scale. The crossbow and bolt remain readable. Input roles and provisional design decisions are in `mercurius-art.md`.

No canonical character base, runtime card mapping or installation was established by this task.

## Relic candidates (2026-10-25)

Five independently generated relic paintings are delivered under `relic-candidates/2026-10-25/`. Each slug has `big/<slug>.png` (256 x 256), `small/<slug>.png` (94 x 94) and `small/<slug>_outline.png` (94 x 94, white 2 px alpha-derived outline). Titles were confirmed by the user; the same slugs now map to implemented relic model IDs through Sts2WDRelic.

| Korean title | Filename slug |
|---|---|
| 용의 이정표 | mercurius_dragons_waymark |
| 샛별 | mercurius_morning_star |
| 불꽃 두루마리 | mercurius_flame_scroll |
| 방호의 결속 | mercurius_bond_of_protection |
| 세계의 기억 | mercurius_memory_of_the_world |

The built-in image generator produced five separate 1254 x 1254 RGBA paintings from the user object descriptions/references and the official relic/potion style sheet. All four corners are transparent. Whole-canvas Fit conversion preserved each complete painting; no crop, region extraction or Cover fitting was used. Source minimum transparent margins range from 3.5% to 11.2%. The narrow pendant intentionally occupies less width than the round ring/medallion.

Opened dark/light comparison sheets at 94 and 48 px show distinct blue stone, warm lantern core, flame-scroll silhouette, green pendant and three-armed gold spiral. The ring's broad wear marks, lantern ribs and pendant cap are simplified rather than photoreal. Fine material facets and rim decoration become secondary at 48 px. All 15 deliverable PNGs and the two review overviews are preserved. They were first delivered as candidates; current runtime application is recorded above. The human-facing file index is `relic-candidates/2026-10-25/README.md`.

The current fourth ring refinement replaces its three candidate exports and both family overviews. Whole-painting Fit conversion and a new alpha-derived outline preserve the smooth hoop and fuller, enlarged low-profile sapphire inset. Earlier ring paintings remain in local production history. Before/after previews at 94 px and the revised 48 px preview were opened; no runtime or other relic artwork changed.
