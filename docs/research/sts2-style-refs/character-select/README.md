# Base-Game Selection Illustration References

- Research date:2026-10-18 (Asia/Seoul).
- Comparative study: [selection compositions](../../sts2-character-select-composition-study.md).
- Distinguish official distributed screens from locally read illustrations and their UI/compositing state. Keep generated images separate.

## Official Distributed Screenshot

| File | Source and contents |
| --- | --- |
| [official-necrobinder-screen.png](official-necrobinder-screen.png) | [Mega Crit PNG](https://www.megacrit.com/images/steam_screenshot3_new.png), [launch article](https://megacrit.com/news/2026-03-05-early-access-launch/). Actual 2560×1440 Necrobinder selection screen with UI; same 2026-10-18 snapshot as parent `official-select.png`. |

`official-necrobinder-screen.png` SHA-256: `BD2588AF2489E6F6C45F0D310EF28A51A2E29679B05AC39F72501C399A95D5DE`.

Official URLs may be replaced. Use saved snapshots whose filename, character, and UI presence were checked directly.

## Original Material from the Local Installation

Installation metadata in `release_info.json`: **v0.107.1**, commit `59260271`, distribution timestamp `2026-06-18T15:43:56-07:00`. This identifies inspected local files, not necessarily the latest public version. `SlayTheSpire2.pck` was read only.

Spine illustrations under `res://animations/character_select/<character>/` were rendered for observation at approximately 0.4 seconds, fitting skeleton bounds into 1920×1080. All five PNGs below were opened directly. **These are not complete in-game screenshots: UI and some separate backgrounds, postprocessing, and particles are absent.** Gray backing, margins, and separated-layer edges do not establish finished composition or background color. These are not generated images.

| Original character | Observation PNG |
| --- | --- |
| Ironclad | [ironclad-selection-skeleton.png](ironclad-selection-skeleton.png) |
| Silent | [silent-selection-skeleton.png](silent-selection-skeleton.png) |
| Defect | [defect-selection-skeleton.png](defect-selection-skeleton.png) |
| Regent | [regent-selection-skeleton.png](regent-selection-skeleton.png) |
| Necrobinder | [necrobinder-selection-skeleton.png](necrobinder-selection-skeleton.png) |

Work and distribution context: [Mega Crit press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/), [official Steam page](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/). Distinguish public downloads from local installation resources. The resource locations, render procedure, and limitations above identify these observations without requiring local extraction logs.

### Separately Saved Background Layers

| Research file | Original resource path | Saved PNG dimensions and SHA-256 |
| --- | --- | --- |
| [silent-selection-background.png](silent-selection-background.png) | `res://animations/character_select/silent/character_select_silent_bg.png` | 2564×1204. `C43490EE6D24B72C19EDF763ED08F9F48983E907F22A3E80CF5DE66257386499` |
| [necrobinder-selection-background.png](necrobinder-selection-background.png) | `res://animations/character_select/necrobinder/character_select_necrobinder_bg.png` | 2564×1204. `8D2B1AE71EFB5F7B9F870CFFFE8113414627B0BC4A22E8AAD78BC5F67CCA0B86` |

These are original background layers, not repainted images. Dimensions describe PNGs saved through Godot and may differ from logical size/padding. Silent shows broad green-to-blue transitions and long thin diagonals; Necrobinder shows pink/violet flow and dark curved motifs. Figures, UI, and extra effects overlay them in the finished screen.

## Research Limits

- Searching the Steam gallery and official articles did not find additional full selection-screen PNGs for the other four characters in this pass.
- [Early Access Trailer](https://www.youtube.com/watch?v=PW22jwFNxU8) and [Gameplay Trailer](https://www.youtube.com/watch?v=ttVtllHkb4E) are distributed by the [official press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/). Do not claim uncaptured frames as image evidence.
- Illustrations without UI and separate game layers are not complete selection screenshots. Distinguish composited tiles, text, buttons, and animation.
