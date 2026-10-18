# Official Visual References for Downstream Assets

- Saved and inspected: 2026-10-18 (Asia/Seoul).
- Purpose: visual research for standard cards, relics, status icons, selection screens, and energy/UI. All are official Mega Crit public materials, not files to copy into runtime assets.
- Public URLs may change contents. These are directly downloaded and inspected snapshots. Originals were not resized or color-corrected; crops and GIF stills were saved separately.
- Detailed analysis: [downstream-asset study](../../sts2-future-asset-reference-study.md).

## Original Files and Sources

| Local file | Source URL | Publication and date | Contents and dimensions |
| --- | --- | --- | --- |
| [card-attack-thrumming-hatchet.png](card-attack-thrumming-hatchet.png) | [PNG](https://www.megacrit.com/images/2025_07_hatchet.png) | [2025-07 article](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/) | Thrumming Hatchet Attack card, 267×354 |
| [card-power-rolling-boulder.png](card-power-rolling-boulder.png) | [PNG](https://www.megacrit.com/images/2025_07_boulder.png) | [2025-07 article](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/) | Rolling Boulder Power card, 279×364 |
| [card-skill-scrawl.png](card-skill-scrawl.png) | [PNG](https://www.megacrit.com/images/2025_07_scrawl.png) | [2025-07 article](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/) | Scrawl Skill card, 279×371 |
| [card-power-automation.png](card-power-automation.png) | [PNG](https://www.megacrit.com/images/2025_07_automation.png) | [2025-07 article](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/) | Automation Power card, 280×364 |
| [card-attack-bodyguard-bone-shards.png](card-attack-bodyguard-bone-shards.png) | [PNG](https://www.megacrit.com/images/bodyguard_boneshards.png) | [2025-10 article](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/) | Bodyguard / Bone Shards, two Attack cards, 735×492 |
| [card-attack-power-blight-strike-neurosurge.png](card-attack-power-blight-strike-neurosurge.png) | [PNG](https://www.megacrit.com/images/blightstrike_neurosurge.png) | [2025-10 article](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/) | Blight Strike Attack / Neurosurge Power, 735×492 |
| [card-skill-soul-grave-warden.png](card-skill-soul-grave-warden.png) | [PNG](https://www.megacrit.com/images/soul_gravewarden.png) | [2025-10 article](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/) | Soul / Grave Warden, two Skill cards, 735×492 |
| [card-quest-byrdonis-egg.png](card-quest-byrdonis-egg.png) | [PNG](https://www.megacrit.com/images/byrdonis_egg.png) | [2025-03 article](https://www.megacrit.com/news/2025-3-12-neowsletter-issue-8/) | **Quest** card, not Ancient/full-art, 495×379 |
| [relics-potions-original.gif](relics-potions-original.gif) | [GIF](https://www.megacrit.com/images/2025_08_relics_potions.gif) | [2025-08 article](https://www.megacrit.com/news/2025-8-14-neowsletter-issue-13/) | Three relics above, three potions below; actual file 768×512, one frame |
| [ui-topbar.jpg](ui-topbar.jpg) | [JPG](https://www.megacrit.com/images/topbar_qol.jpg) | [2024-11 article](https://www.megacrit.com/news/2024-11-07-neowsletter-issue-4/) | Top bar with one public relic and three deliberately obscured relics, 1280×117 |
| [ui-end-turn.png](ui-end-turn.png) | [PNG](https://www.megacrit.com/images/2025_08_endturn.png) | [2025-08 article](https://www.megacrit.com/news/2025-8-14-neowsletter-issue-13/) | End Turn UI including turn display, 258×126 |
| [ancient-tezcatara-original.gif](ancient-tezcatara-original.gif) | [GIF](https://www.megacrit.com/images/tezcatara_v2.gif) | [2025-11 article](https://www.megacrit.com/news/2025-11-13-neowsletter-issue-16/) | **Ancient event screen**, not an Ancient card;800×422, 275 frames |
| [ui-select-screenshot.png](ui-select-screenshot.png) | [PNG](https://www.megacrit.com/images/steam_screenshot3_new.png) | [launch article](https://megacrit.com/news/2026-03-05-early-access-launch/) | Full Necrobinder selection screen, 2560×1440; same snapshot as parent `official-select.png` |
| [ui-merchant-screenshot.png](ui-merchant-screenshot.png) | [PNG](https://www.megacrit.com/images/steam_screenshot4_new.png) | [official press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/) | This download is a **Silent merchant screen**; do not apply the older map description for this URL;2560×1440 |
| [ui-map-qol.jpg](ui-map-qol.jpg) | [JPG](https://www.megacrit.com/images/map_qol.jpg) | [2024-11 article](https://www.megacrit.com/news/2024-11-07-neowsletter-issue-4/) | Actual room-symbol/map-line reference, 700×389; player-character marker not identified |

## Observation Derivatives

Crop coordinates are `(x, y, width, height)` in original pixels. Crops are for observation only, without AI repainting, background removal, or interpolated enlargement.

| File | Source and extraction rectangle | Caveat |
| --- | --- | --- |
| [relics-potions-frame0.png](relics-potions-frame0.png) | `relics-potions-original.gif`, complete frame 0 | PNG view of the GIF container; numbers and blue backing belong to the source |
| [ancient-tezcatara-frame0.png](ancient-tezcatara-frame0.png) | `ancient-tezcatara-original.gif`, complete frame 0 | Do not copy GIF dithering as material texture; timing was not analyzed |
| [status-icons-in-combat-crop.png](status-icons-in-combat-crop.png) | Parent `official-combat.png`, `(435,960,390,150)` | Composited Block value, health bar, and red heart-like status; not independent alpha icons |
| [ui-energy-in-combat-crop.png](ui-energy-in-combat-crop.png) | Parent `official-combat.png`, `(120,1100,240,200)` | Defect blue energy UI **includes numbers**; not a number-free source texture |
| [relics-in-combat-crop.png](relics-in-combat-crop.png) | Parent `official-combat.png`, `(0,105,300,140)` | Two top-bar relics and background; composited small-scale readability reference |
| [ui-character-portrait-crop.png](ui-character-portrait-crop.png) | `ui-topbar.jpg`, `(5,0,60,58)` | Silent top-bar portrait; not evidence of a project chibi standard |
| [ui-select-tiles-crop.png](ui-select-tiles-crop.png) | `ui-select-screenshot.png`, `(845,1160,890,230)` | Five selected/deselected character tiles; locked state is not established here |
| [ui-select-tile-active-crop.png](ui-select-tile-active-crop.png) | `ui-select-tiles-crop.png`, `(510,0,160,220)` | Selected Necrobinder: bright portrait and gold frame |
| [ui-select-tile-inactive-crop.png](ui-select-tile-inactive-crop.png) | `ui-select-tiles-crop.png`, `(215,0,160,220)` | Deselected Silent: dark portrait retaining internal forms; **not a locked original** |
| [ui-select-info-safe-area-crop.png](ui-select-info-safe-area-crop.png) | `ui-select-screenshot.png`, `(0,420,1210,680)` | Left name/ability/relic descriptions and translucent panel over the illustration |

Official crop source: [steam_screenshot1_new.png](https://www.megacrit.com/images/steam_screenshot1_new.png), context: [official press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/). The parent snapshot is a 2560×1440 **Defect vs. blue-creature** combat frame.

## File Identity Records

SHA-256 for the 2026-10-18 download snapshots:

| File | SHA-256 |
| --- | --- |
| `card-attack-thrumming-hatchet.png` | `9C522B1453F12418A07748A3E2D1122C6DF7277BDB72C0C9A95525F1A5FC1747` |
| `relics-potions-original.gif` | `4AA1B9CECE89D528ACA9776815C7B8492FFAF398133950F47332FEEE8AF228C0` |
| `ancient-tezcatara-original.gif` | `89496B115FE0E8549E373AB3FE8A5C99D7D374FC5C7E3C7FD406E5066AD19E95` |
| `ui-select-screenshot.png` | `BD2588AF2489E6F6C45F0D310EF28A51A2E29679B05AC39F72501C399A95D5DE` |

## Unavailable Evidence

- No original explicitly identified as an **Ancient/full-art card** was found in these Mega Crit public articles. Do not misclassify Quest, ordinary Rare cards, or Ancient events as such.
- No complete transparent buff/debuff icon library was obtained. Combat crops provide limited examples, not universal colors, frames, or symbols.
- This public-article pass did not obtain separate selection backgrounds or locked tiles. Later local-game research obtained [selection resources](../character-select/README.md) and [ten normal/locked tiles](../character-select-tiles/README.md). Deselected screenshot crops remain distinct from locked originals.
- Chibi for `character_icon` and `map_marker` is a user/project decision. Official portrait/map-node references do not prove an original-game chibi standard.
