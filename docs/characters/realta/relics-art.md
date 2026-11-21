# Realta Relic Art

## Approved Relic Subjects

All five Realta relics follow the original game's isolated-object style. **Bronze Watchdog** (`realta_bronze_watchdog`) is a frontal bronze canine-head relief, and **Kuu** (`realta_kuu`) is a more imposing three-quarter snarling metal wolf head; neither starter relic uses the broom-glaive. Preserve their family relationship through warm bronze raised planes, simplified broad raised knotwork bands, and deep brown recesses; Kuu adds a layered cheek/mane silhouette, visible teeth, and a restrained orange-red eye. Exclude green linework, full bodies, circular borders, and backgrounds.

The three non-starter relics are **Witch Pointed Hat** (`realta_witch_pointed_hat` — teal/indigo curled cone and broad brim with sparse gold celestial arcs), **Dragon Cloak** (`realta_dragon_cloak` — burgundy royal mantle with ivory ermine trim and sparse broad gold motifs; no wearer, mannequin, or crown), and **Eldir Gramr** (`realta_eldir_gramr` — straight red/orange incandescent sword with a dark burgundy grip and steel crossguard).

Generate each complete object directly in a square transparent composition with thick dark contours, two or three connected value families, and restrained painted edges guided by the original relic/potion reference sheet in [ARTS.md](/ARTS.md). Avoid fine engraving, dense embroidery, and individual hairs. Runtime paths and installation status belong in [relics-assets.md](relics-assets.md).

## Icon Composition

- **Three files per relic:**
  - `Sts2WD/images/relics/big/realta_<name>.png`: 256×256 RGBA.
  - `Sts2WD/images/relics/realta_<name>.png`: 94×94, resized from large.
  - `Sts2WD/images/relics/realta_<name>_outline.png`: 94×94, white/light silhouette from small alpha expanded 2–3px.
- **Composition:** one isolated object; no background, floor shadow, or outer glow. Start at 72–84% occupancy / 20–35px margins in 256×256. Exaggerate identifying ears/wings/hat tips/hilts for 48–64px top-bar reading and simplify interiors to 3–5 major planes.
