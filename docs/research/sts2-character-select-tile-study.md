# Selection Tiles: Face Composition and Locked Originals

- Research date: 2026-10-18 (Asia/Seoul).
- Local base game: **v0.107.1 / commit 59260271**; not proof of identical assets in every version.
- Purpose: compose Realta's `char_select.png` around the face and distinguish deselected from locked states.
- Scope: five normal PNGs, five locked PNGs, and tiles in an official selection screen.
- Sources: [ten originals, resource paths, hashes](sts2-style-refs/character-select-tiles/README.md), [press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/), [Steam](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/).

## 1. Source Images versus Actual Buttons

**Image observation:** all five `res://images/packed/character_select/char_select_<character>.png` files are **132×195**, as are the separate `_locked.png` files. These show artwork composition and locked imagery, not all runtime frames, masks, or selection effects.

**Image observation:** in the [tile crop](sts2-style-refs/future-assets/ui-select-tiles-crop.png) from the [official screen](sts2-style-refs/character-select/official-necrobinder-screen.png), selected Necrobinder is bright with a gold frame. Other tiles remain internally illustrated despite being dark: they are **deselected**. The inspected locked PNGs instead use black silhouettes. [Distributed screen](https://www.megacrit.com/images/steam_screenshot3_new.png)

**Project judgment:** document normal art, UI deselection treatment, and locked art separately. A screenshot alone did not identify locked state; the separate local originals now establish its appearance.

## 2. All Five Normal Tiles Emphasize Face, Mask, and Neck

These are **direct image observations**. Do not force masks, robot heads, star faces, and skulls into common human anatomy or eye positions.

| Character | Head/frame relationship | Expression focus | Lower information |
|---|---|---|---|
| [Ironclad](sts2-style-refs/character-select-tiles/ironclad-char-select.png) | Mask fills most width/height; side hair/mask and white hair meet frame boundaries rather than fitting a whole head with margins | Bright orange eyes and long downward mask planes | Only neck, clothing, and partial shoulder; no full chest, waist, or arms |
| [Silent](sts2-style-refs/character-select-tiles/silent-char-select.png) | Large skull mask extends right/down; hood/horns clipped above/left; both horn tips need not fit | Large green mask eyes and dark inner face | Neck cloth and some hair, not whole cloak/torso |
| [Defect](sts2-style-refs/character-select-tiles/defect-char-select.png) | Gold head occupies most width; right neck connection continues off-frame | Large blue lens and smaller upper lens, not a human horizontal eye line | Neck/blue shoulder cloth, excluding chest/waist |
| [Regent](sts2-style-refs/character-select-tiles/regent-char-select.png) | Tall ornament extends above frame; protruding head shape is a major subject | Yellow star eye in black center sits lower because of tall head structure | Blue neck ornament and partial shoulder, no throne/full body |
| [Necrobinder](sts2-style-refs/character-select-tiles/necrobinder-char-select.png) | Rounded skull dominates center; dark blue flame extends above frame | Dark sockets, nose, skull direction; glowing eyes are not universal | Long dark neck, some ribs/robe, no giant Osty/scythe |

**Project judgment:** recognition, not an equipment inventory, is the tile's task. Realta shows a large face, red bob, violet eyes, ear, and partial hat. Partial shoulder/collar may remain below, but do not shrink the face to fit both pauldrons, full chest, belt, and pouch. Preserve the approved non-SD proportions and youthful appearance; a close camera is not chibi anatomy.

## 3. Approximate Observed Regions and Eye Positions

These are **manual approximate boxes on 132×195 originals**, not official masks or automatic detection. Different masks, skulls, and mechanical faces are not identical semantic regions. Do not use decimal precision or these values as test thresholds. Origin is upper-left.

| Character | Approximate face/mask region | Approximate eye/key-mark y |
|---|---|---|
| Ironclad | `x≈0~129, y≈10~175`; mask front, excluding hair/neck | `y≈100~115`, about 51–59%; eye tilt/length prevents a single horizontal line |
| Silent | `x≈40~131, y≈40~182`; skull mask, excluding hood/horns | `y≈80~95`, about 41–49% |
| Defect | `x≈5~129, y≈9~125`; gold face module, excluding neck/cloth | Large lens `y≈75~90`, about 38–46%; small lens above |
| Regent | Orange head approximately `x≈7~131, y≈0~186`; black center/yellow mark are subsets; tall protrusions are not a skin-face box | Yellow mark `y≈115~130`, about 59–67% |
| Necrobinder | `x≈7~127, y≈19~154`; pale skull, excluding flame/neck | Sockets `y≈85~100`, about 44–51% |

**Image observation:** eye/mark locations vary. Originals do not establish universal "eyes at 45%" or "face exactly 70% tall" rules. Large face occupancy and uniform coordinates are different claims.

**Project guidance:** Compose face-close selection masters at the target camera distance while preserving canonical age and proportions. Design partial off-frame headwear as part of the composition. [Realta selection art](/docs/characters/realta/selection-art.md)

## 4. Limits of Fitting the Whole Hat and Waist Equipment

The 132×195 originals reserve most of the tile for the face, mask, and neck. A closer camera gives the eyes, nose, and mouth more space without changing facial proportions. Keep visible collar and shoulder details consistent with the approved character design. Let the hat tip and waist equipment extend outside the frame as needed.

## 5. Locked Originals Use Black Silhouettes

**Image observation:** [Ironclad](sts2-style-refs/character-select-tiles/ironclad-char-select-locked.png), [Silent](sts2-style-refs/character-select-tiles/silent-char-select-locked.png), [Defect](sts2-style-refs/character-select-tiles/defect-char-select-locked.png), [Regent](sts2-style-refs/character-select-tiles/regent-char-select-locked.png), and [Necrobinder](sts2-style-refs/character-select-tiles/necrobinder-char-select-locked.png) use black head/shoulder silhouettes on gray. Most facial color, eyes, and values are absent. Small edge/part remnants prevent claiming every pixel uses exactly two colors.

**Image observation:** normal/locked head identities and directions are recognizable, but this does not prove the original pipeline mathematically shared one alpha.

**Project proposal:** generate one transparent master directly for the selection ratio. Fit-resize the complete image and share its alpha to composite normal/locked variants deterministically: colored figure on project blue; black figure on gray. Do not extract/enlarge the face or discard surroundings with Cover. Regenerate if occupancy is wrong. This avoids separate AI calls changing pose/hat/ear silhouette; it is our workflow, not a verified Mega Crit process.

## 6. Painting versus Game UI Responsibilities

**Image observation:** some normal sources have thin colored edge lines, while the selected tile in the finished screen has a gold frame/state effects. Source edges and UI frames/masks are distinct. [Official screen](https://www.megacrit.com/images/steam_screenshot3_new.png)

**Project proposal:** keep gold borders, button frames, rounded corners, lock icons, letters, and numbers out of Realta's transparent master. Prioritize expression and head silhouette, then inspect how the runtime mask/frame affects eyes/ears. Off-frame headwear in originals guides the generation camera; it does not justify later cropping. Research screenshot crops and new-asset production remain distinct.
