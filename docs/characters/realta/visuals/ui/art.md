# Realta Character Icons and UI Art

Follow [ARTS.md](../../../../../ARTS.md) for shared chibi scope. This specification supplies Realta's face, expression, costume, and hat silhouette within it. The chibi override below defines the details; other assets retain youthful canonical anatomy. Read [shared safe zones](../../../../../ARTS.md#5-asset-compositions-and-safe-zones) first.

Common runtime directory: `Sts2WD/images/charui/realta/`.

| File | Dimensions | Proportions/background | Composition and derivation |
|---|---|---|---|
| `character_icon.png` | 128×128 RGBA | **Chibi bust**, transparent | Generate directly with large round head/cheeks/violet eyes occupying at least 70% of canvas area. Hat top lies outside the generation frame; tiny paired shoulders and ivory collar remain below |
| `character_icon_outline.png` | 128×128 RGBA | Derived alpha | No AI redraw. Dilate final small alpha 2–3px into white `#FFFFFF` for matching silhouette; current exact radius is in the [UI asset guide](assets.md) |
| `map_marker.png` | 128×128 RGBA | **Chibi head and whole hat only**, transparent | No shoulders/torso. Slightly compress crown height so face/buckle read at 48px; start with 8% / about 10px clear margin |
| `char_select.png` | 132×195 RGBA | **Canonical non-SD close face**, opaque | New right-facing three-quarter portrait dominated by face/red bob. Hat tip/brim extremities are outside the designed camera; partial collar/shoulders below. Use whole generated image; see [selection tiles](../selection/art.md#selection-tiles) |
| `char_select_locked.png` | 132×195 RGBA | Same figure alpha, opaque | Identical contour: black figure on gray. Remove internal eyes/face/armor; do not generate separately |
| `big_energy.png` / `text_energy.png` | 74×74 / 24×24 RGBA | Same flame-heart master, transparent outside | Red-orange curling flame silhouette, orange-gold bevels, and a large dark burgundy heart-shaped inset. The inset contains no small heart or other decoration; it is reserved for the cost number |

## Chibi Override

```text
Proportion override (CHIBI / SD ONLY): Transform into an unmistakable super-deformed chibi style (2 to 2.5 heads tall when torso is shown). Extremely large rounded head and full cheeks, short rounded chin, large simple widely-spaced violet eyes, tiny nose and confident smiling mouth. Preserve Realta's copper-red bob, pointed ears, petrol-blue bent witch hat with brown band and bronze buckle, continuous ivory fur collar and tiny dual bronze shoulder pauldrons, simplified into bold connected cel-like planes readable at 48-128px.
```
