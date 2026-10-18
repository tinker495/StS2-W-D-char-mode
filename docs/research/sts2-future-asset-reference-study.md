# Official Image References for Downstream Asset Production

- Research date: 2026-10-18 (Asia/Seoul).
- Purpose: collect/classify actual cards, relics, status icons, character UI, and selection illustrations before producing assets beyond the standing base.
- Files: [future-assets](sts2-style-refs/future-assets/README.md), including source URLs, publications, dimensions, and crop coordinates.
- Scope: Mega Crit articles and press-kit images, excluding fan art and unofficial redistribution. This research did not change generated images, runtime files, tests, or validation assets.

## 1. Coverage and Limits

| Asset | Direct evidence | Not established |
|---|---|---|
| Relics | Three new relics/three potions; combat top-bar crop | Universal color/outline measurements |
| Power/status icons | Actual Block, health, and status display crop | Complete transparent buff/debuff library; **Power card art and status icons are different** |
| Standard Attack cards | Thrumming Hatchet, Bodyguard, Bone Shards, Blight Strike | Historical values equal current values |
| Standard Skill cards | Scrawl, Soul, Grave Warden | Painting frame/text UI into Realta illustrations |
| Standard Power cards | Rolling Boulder, Automation, Neurosurge | Shrinking a Power card suffices for a status icon |
| Ancient/full-art cards | **No official public original obtained** | Substituting Quest cards or Ancient events as Ancient-card sources |
| Large selection illustration | Full Necrobinder screen and left-UI crop | Separated background source or runtime layer structure in this research pass |
| Two chibi UI uses | Official face-icon/map-node readability references | An official chibi standard for `character_icon`/`map_marker` |
| Canonical-proportion selection UI | Tile group, selected/deselected crops | Deselected and locked states are identical |
| Energy/other UI | Defect combat energy crop, top bar, End Turn | Independent energy texture without composited numbers |

**Official statement:** the August 2025 article identifies the top three objects as relics and bottom three as potions, explaining that small potions now have distinct sprites. [Article](https://www.megacrit.com/news/2025-8-14-neowsletter-issue-13/)

## 2. Relic and Status Information Density

### Relics

**Image observation:** the [relic/potion still](sts2-style-refs/future-assets/relics-potions-frame0.png) shows a red ticket, smoking stone, and folded paper above. Dark contours and a few broad light planes establish each object. Identifying print, holes, and folds remain without uniform tiny texture. Potion containers below have strongly different silhouettes; the objects are not all enclosed in a common coin/crest/gem template. [Original](https://www.megacrit.com/images/2025_08_relics_potions.gif)

**Image observation:** the [combat relic crop](sts2-style-refs/future-assets/relics-in-combat-crop.png) shows how large colors/silhouette survive small display better than collection-art surface detail. It is already composited with a background. [Official frame](https://www.megacrit.com/images/steam_screenshot1_new.png)

**Project judgment:** establish the actual object's shape and highlight its functional identifiers, rather than compressing the character/costume into a relic. The ticket's `W`, numbered sheet, and source backing are not features to import.

### Power and Status Icons

**Image observation:** the [status crop](sts2-style-refs/future-assets/status-icons-in-combat-crop.png) separates a blue shield, health bar, red heart-like symbol, and numbers. Broad light interiors/edges and simple symbols read clearly. Status names were not verified through tooltips; record appearance only. [Official frame](https://www.megacrit.com/images/steam_screenshot1_new.png)

**Project judgment:** center each status on one independent symbol; handle numbers, stacks, health, and frames separately in game UI. Limited examples do not justify a universal shield frame or background. Power-card paintings need not share a file with status icons.

## 3. Attack, Skill, and Power Card Illustrations

**Official statement:** July 2025 revealed Colorless Thrumming Hatchet, Rolling Boulder, Scrawl, and Automation as works in development; October 2025 described Necrobinder cards. This research concerns their images, not current gameplay acceptance criteria. [July](https://www.megacrit.com/news/2025-7-16-neowsletter-issue-12/), [October](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/)

**Image observation:** [Thrumming Hatchet](sts2-style-refs/future-assets/card-attack-thrumming-hatchet.png) uses a large axe and red background diagonals for force. [Bodyguard/Bone Shards](sts2-style-refs/future-assets/card-attack-bodyguard-bone-shards.png) distinguish a defensive fist from radiating fragments. Large weapon/hand shapes precede supporting debris. [Hatchet original](https://www.megacrit.com/images/2025_07_hatchet.png), [two-card original](https://www.megacrit.com/images/bodyguard_boneshards.png)

**Image observation:** [Scrawl](sts2-style-refs/future-assets/card-skill-scrawl.png) uses broad arcs of hand/cards. [Soul/Grave Warden](sts2-style-refs/future-assets/card-skill-soul-grave-warden.png) show a held teal soul or figure/lantern/approaching light, each around one action. Violet/green backgrounds differ rather than reproducing one robe color everywhere. [Scrawl](https://www.megacrit.com/images/2025_07_scrawl.png), [Soul/Grave Warden](https://www.megacrit.com/images/soul_gravewarden.png)

**Image observation:** [Rolling Boulder](sts2-style-refs/future-assets/card-power-rolling-boulder.png) combines a large rolling rock, green creature, and red direction lines. [Automation](sts2-style-refs/future-assets/card-power-automation.png) uses repeated golden machinery and blue background for rotation. [Neurosurge](sts2-style-refs/future-assets/card-attack-power-blight-strike-neurosurge.png) centers a bright blue brain/skull. Power cards are not restricted to static emblems. [Boulder](https://www.megacrit.com/images/2025_07_boulder.png), [Automation](https://www.megacrit.com/images/2025_07_automation.png), [Neurosurge](https://www.megacrit.com/images/blightstrike_neurosurge.png)

**Project judgment:** choose one main action/object and direction per Realta card. Design a close camera around blade, protective hand, or spell center during generation rather than repeating small full-body figures or cropping a finished base. Card title, cost, frame, and rules text remain UI. Reference images of complete cards are not illustration-file dimensions.

### Rechecked Card URLs

The Hatchet, Boulder, and Bodyguard/Bone Shards URLs downloaded successfully, and saved contents matched their names. `topbar_qol.jpg` was also viewed. Development-era numbers/text are not adopted as current game specifications.

## 4. Ancient Cards, Other Frames, and Large Events

**Official statement:** March 2025 introduces Byrdonis Egg as **Quest**; November 2025 introduces **Ancients** and blessings. Neither specifies Ancient full-art card layout. [Quest article](https://www.megacrit.com/news/2025-3-12-neowsletter-issue-8/), [Ancients article](https://www.megacrit.com/news/2025-11-13-neowsletter-issue-16/)

**Image observation:** [Byrdonis Egg](sts2-style-refs/future-assets/card-quest-byrdonis-egg.png) has a nest, glowing egg, orange title area, and rocky border: an alternative frame, **not an Ancient card**. [Tezcatara frame](sts2-style-refs/future-assets/ancient-tezcatara-frame0.png) is an **Ancient event scene** with a large central being, candles, and warm halo; no card frame/text/full-art ratio is visible. [Egg original](https://www.megacrit.com/images/byrdonis_egg.png), [Tezcatara original](https://www.megacrit.com/images/tezcatara_v2.gif)

**Project judgment:** these sources alone do not establish Realta's Ancient/full-art style. Check an actual Ancient-card screen supplied by the user or publicly released before claiming original-game ratios, frame overlap, or text margins. Quest/event images are not substitute evidence. Production can still proceed under explicit project choices; those choices are not verified official specifications.

## 5. Two Chibi UI Uses and Canonical-Proportion Selection UI

### `character_icon` and `map_marker`

**Image observation:** the [portrait crop](sts2-style-refs/future-assets/ui-character-portrait-crop.png) focuses on Silent's mask, hood color, and broad contour rather than a miniature full body. The [map](sts2-style-refs/future-assets/ui-map-qol.jpg) distinguishes room types by color and symbol; no player-character marker was identified. [Top bar](https://www.megacrit.com/images/topbar_qol.jpg), [map original](https://www.megacrit.com/images/map_qol.jpg)

**Project judgment:** chibi for these two uses is the user's shared project rule, **not an official chibi standard**. A few identifiers such as large hat, red hair, and violet eyes can follow official small-icon clarity. Portrait/marker have different cameras, margins, and visible information, so generate each for its purpose instead of extracting one from the other.

### Selection Tile and Locked Variant

**Image observation:** the [tile group](sts2-style-refs/future-assets/ui-select-tiles-crop.png) gives faces/upper bodies substantial occupancy. [Selected Necrobinder](sts2-style-refs/future-assets/ui-select-tile-active-crop.png) is bright with a gold frame; [deselected Silent](sts2-style-refs/future-assets/ui-select-tile-inactive-crop.png) retains internal mask/hood forms despite darkening. This screenshot alone does not establish locked state or full-body head ratios. [Official screen](https://www.megacrit.com/images/steam_screenshot3_new.png)

**Follow-up source inspection:** [ten local normal/locked originals](sts2-style-refs/character-select-tiles/README.md) now establish close-face normal art and black silhouettes on gray for locked art. See the [tile study](sts2-character-select-tile-study.md). Do not reproduce deselection tint as the locked variant.

**Project judgment:** generate a new face-close tile using original occupancy, gaze, and density while preserving the approved youthful non-SD anatomy. Fit-resize the whole master and use the same alpha for a black locked silhouette. Regenerate wrong compositions rather than cropping/enlarging/Cover fitting. Follow ARTS.md section 1.3/5.2.1 and Recipe 4-A/B.

## 6. Selection Illustration Base and UI Space

**Image observation:** the [full screen](sts2-style-refs/future-assets/ui-select-screenshot.png) places Necrobinder's face/body right and huge Osty left/center. Pink, bone, and violet values connect the complete scene; it is distinct from small tile framing. [Official original](https://www.megacrit.com/images/steam_screenshot3_new.png)

**Image observation:** the [information crop](sts2-style-refs/future-assets/ui-select-info-safe-area-crop.png) contains title, health/gold, character description, and starting relic over translucent dark UI. In this 2560×1440 source, text overlaps approximately `x=350~1430,y=450~960`; lower tiles approximately `x=870~1700,y=1180~1360`. These describe **one screen**, not universal runtime coordinates across resolution, localization, or UI scale. The reference index records actual extraction rectangles separately.

**Project judgment:** design Realta's selection scene separately from the combat PNG. Keep face, violet eyes, hat buckle, and weapon socket clear of text. Continue broad forms/colors behind information without competitive tiny runes/ornament. Check face/weapon-tip collisions with lower tiles, arrows, and buttons in actual UI.

**Project judgment:** selection art supports more scene lighting, secondary objects, and brushwork than sprites. This public-screenshot pass did not obtain a separate background source; do not infer file boundaries/layer implementation from it. Later local-resource observations are documented separately.

## 7. Energy, Merchant, and Supporting UI

**Image observation:** [Defect energy](sts2-style-refs/future-assets/ui-energy-in-combat-crop.png) reads through large blue forms, light interior, and edge structure; `2/3` is composited text. [End Turn](sts2-style-refs/future-assets/ui-end-turn.png) uses clear blue planes, gold border, and light text. [Top bar](sts2-style-refs/future-assets/ui-topbar.jpg) separates portrait, health, gold, potions, map, and settings through concise silhouettes. [Combat source](https://www.megacrit.com/images/steam_screenshot1_new.png), [button](https://www.megacrit.com/images/2025_08_endturn.png), [top bar](https://www.megacrit.com/images/topbar_qol.jpg)

**Image observation:** the download from `steam_screenshot4_new.png` was a **Silent merchant screen**, not the historical map. The [saved file](sts2-style-refs/future-assets/ui-merchant-screenshot.png) places character left and merchant right among awning, violet lantern, and props. The map reference was obtained separately as `map_qol.jpg`. [Distributed URL](https://www.megacrit.com/images/steam_screenshot4_new.png)

**Project judgment:** do not paint game-rendered numbers into energy art. Preserve core color/contour between large and inline energy while adjusting density. Review merchant standing art, selection scenes, and portraits in their own layouts/sizes.

## 8. Checks Performed in This Research

- Downloads/dimensions were checked; saved PNG/JPG/GIF stills were opened to verify content/classification.
- Observation crops used original pixels without resizing, repainting, or text removal. GIF frames were extracted without claiming full timing analysis.
- Four previous card URLs were rechecked. Changed public URL contents were recorded alongside actual saved snapshots.
- No game execution, UI runtime verification, or code tests were performed in this research. It does not replace base/downstream production or application approval.
