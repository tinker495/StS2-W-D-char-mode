# Realta Relic Art

## Adopted Refresh Brief (2026-10-24)

The user requested new relic artwork in the original game's isolated-object style. The later starter-reference correction supersedes the initial weapon-based brief: **Bronze Watchdog** is a frontal bronze canine-head relief based on the right-hand green Celtic head reference, while **Kuu** is a more imposing three-quarter snarling metal wolf head based on the left-hand armored-wolf reference. Do not use the broom-glaive for either starter relic. Preserve the family relationship through warm bronze raised planes and deep brown recesses; Kuu adds a layered cheek/mane silhouette, visible teeth and a restrained orange-red eye. Simplify knotwork to a few broad raised bands. Exclude the source images' green line color, full body, circular border and background.

The three exclusive relics use the user's other references: Witch Pointed Hat has a teal/indigo curled cone and broad brim with sparse gold celestial arcs; Eldir Gramr is a straight red/orange incandescent sword with a dark burgundy grip and steel crossguard; Dragon Cloak is a burgundy royal mantle with ivory ermine trim and a few broad gold motifs. No wearer, mannequin or crown belongs in the cloak icon.

Generate each complete object directly in a square transparent composition. Use thick dark contours, two or three connected value families and restrained painted edges, guided by the original relic/potion reference sheet listed in ARTS.md. Avoid fine engraving, dense embroidery and individual hairs. The five illustrations were reviewed at 94 px and 48 px and selected for runtime application; paths and installation status belong to the [relic asset guide](assets.md).

## Runtime Application Approval (2026-10-24)

The user approved applying all five relic illustrations and installing them locally. The described candidate forms are now the selected relic artwork; earlier pending-selection wording records only the previous review stage. Runtime uses the existing model filenames; gameplay behavior was not changed by the image promotion.

## Icon Composition

- **Three files per relic:**
  - `Sts2WD/images/relics/big/realta_<name>.png`: 256×256 RGBA.
  - `Sts2WD/images/relics/realta_<name>.png`: 94×94, resized from large.
  - `Sts2WD/images/relics/realta_<name>_outline.png`: 94×94, white/light silhouette from small alpha expanded 2–3px.
- **Composition:** one isolated object; no background, floor shadow, or outer glow. Start at 72–84% occupancy / 20–35px margins in 256×256. Exaggerate identifying ears/wings/hat tips/hilts for 48–64px top-bar reading and simplify interiors to 3–5 major planes.

## Earlier Concepts Superseded by the Refresh Brief

The following concepts record the earlier design direction. The adopted refresh brief above owns the current five illustrations, including the canine-head starters.

1. `청동의 번견` (`realta_bronze_watchdog`, starting): small crouched bronze hound watching right; heavy `#9D754E` plates with major angular ears, muzzle, forelegs.
2. `쿠우` (`realta_kuu`, upgraded starter): same hound family, standing/roaring or chest-forward confidence, expanded shoulder/mane plates, bright cyan/aqua `#38B6FF` core between plates.
3. `마녀 고깔모자` (`realta_witch_pointed_hat`, Common): isolated canonical petrol-blue left-bent hat, brown band, round bronze buckle.
4. `용의 망토` (`realta_dragon_cloak`, Uncommon): broad-scaled red-bronze/crimson leather cloak and flame-shaped bronze clasp; small warm hem reflections suggest Vigor/Flame.
5. `엘디르 그라므르` (`realta_eldir_gramr`, Rare): ancient fire dagger/sword with heated bronze guard and red-gold heat core. Keep blade shape sharp; concentrate flame contrast at guard rather than burying the whole blade.
