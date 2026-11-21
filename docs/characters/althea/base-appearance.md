# Althea Appearance and Canonical Base

The [canonical base](base.png) defines current appearance, with the [bust reference](refs/ref1.jpg) supplying upper-body details.

## Adopted Base

The user selected `base.png` for its clean 2D rendering, bright silver-white, white and navy palette, and graceful proportions. Preserve the complete painting, including half-tied hair, calm blue eyes, navy collar and lining, white split tabard with red cross, gold-accented silver plates, and the grounded right-facing three-quarter stance. The drawn straight sword angles up-right while the opposite hand supports the empty sheath. Weapon naming and final construction are open design decisions.

Use `base.png` directly as the image input anchor for identity, facial appeal, proportions, palette, and surface rendering in downstream work. The narrow right sword-tip margin (`12px`) remains a known characteristic of the unedited `base.png` master; do not silently crop or repaint `base.png`.

## Raw References and Input Roles

| Input | Adopted | Excluded or unconfirmed |
|---|---|---|
| `base.png` | User-confirmed visual anchor: graceful young-adult proportions, calm blue eyes, bright silver-white half-tied hair, dark cross earring, navy collar with gold clasp, clean silver plate armor, white split tabard with red cross and gold trim | Weapon naming and final construction |
| `refs/ref1.jpg` (user-supplied semi-realistic painted bust) | Supporting reference for upper-body identity cues (face, blue eyes, diagonal fringe, half-tied silver-white hair, dark cross earring, high navy collar, gold clasp, silver pauldrons, white tabard with red cross) | Photorealistic individual hair strands, noisy micro-speculars, dark vignette background, and lower-left signature |
| Official combat references ([ARTS.md 2.3](/ARTS.md#23-actual-official-image-anchors-and-priority)) | Three-quarter combat camera, grounded stance, and grouping high-frequency surface noise into clean 2D planes | Each official character's robot/warrior build, stocky proportions, dark/muted palette, masks, skulls, horns, weapons, and ground shadow |

Pass `base.png` directly as an image input to preserve Althea's facial features, hair flow, palette, and proportions. Include `refs/ref1.jpg` when the requested work uses its bust details.

## Full-Body Base Specification

- **Camera and body (user-confirmed):** weak side-on three-quarter combat camera. Torso and pelvis face the enemy at screen right; knees slightly bent with weight across front and back feet.
- **Head angle (user-confirmed):** right-facing three-quarter, between frontal and profile. The head turns slightly toward the camera so the far eye and part of the far cheek show, the far eye narrower than the near one.
- **Gaze (user-confirmed):** pupils toward the enemy at screen right, independent of head rotation. Changing the head angle does not change body direction, the drawn sword or either hand.
- **Weapon state (user-confirmed):** one straight sword, fully drawn. The screen-right hand grips the hilt with the tip pointing up-right and clear space between face and blade. The other hand supports the empty scabbard at the waist; the scabbard holds no second hilt or blade. No extra weapons, shield or magic effects.
- **Sword form and name (provisional):** follows the design's sword/scabbard theme; unique shape and name are undecided.
- **Lower body (current base):** navy inner garment, white split skirts with gold trim, clean silver knee and shin plates, silver sabatons.
- **Construction (production rule):** tip, blade, guard center, grip and pommel share one straight axis. Gauntlet and vambrace, fingers and grip, belt and scabbard ring each read separately. No floating metal pieces or duplicate guards. Hems and gold trim follow continuous cloth edges. Armor resolves into clean plates with clear overlaps.
- **Canvas:** generate a vertical 2:3 transparent canvas containing the whole figure and equipment, with ARTS.md 5.1 margins at every extremity; do not crop or extract regions. Check whole-image Fit previews at `240 × 360` and `326 × 490`.

## Prompt Material

`<CHARACTER_ANCHOR>`: Althea, a graceful young adult woman knight with fair skin and calm, resolute blue eyes. Long, bright silver-white hair in clean flowing locks swept back and half-tied at the crown, a fringe falling diagonally across her forehead, and a small dark cross-shaped earring. High navy collar with a gold cross-shaped clasp, clean silver plate armor over a white split tabard with gold trim and a dark red cross emblem on the left chest, navy inner lining and trousers. One straight sword with a gold crossguard and a dark waist scabbard with gold fittings.

`<PALETTE>` and `<MATERIALS>` come from [rendering and palette](base-rendering.md). For standing exports only, preserve the full-body staging above. For newly generated assets, read camera, body/head direction, gaze, hand contacts and weapon state from the matching topic in the [art index](althea-art.md); the common identity anchor supplies equipment construction, not its current use.
