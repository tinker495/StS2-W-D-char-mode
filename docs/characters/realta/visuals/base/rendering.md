# Realta Palette and Rendering

## Canonical Palette and Theme Accents

HEX values are project anchors for consistent color relationships with the base and official references.

| Category | Role | Reference HEX | Treatment |
|---|---|---|---|
| Costume | Petrol-blue base | `#1E5C8A`–`#245F80` | Largest costume area, roughly 40–45% |
| | Blue light | `#3B82B0`–`#4286A0` | Selected shoulders, hat top, major fold corners |
| | Deep blue shadow | `#1D2C42`–`#253748` | Brim underside, coat lining, overlaps; blue-dark rather than pure black |
| Armor/metal | Bronze base | `#9D754E`–`#B08450` | Chest, shoulders, forearms, knees, boots, socket, buckle; warm contrast to blue |
| | Bronze highlight | `#D6B06A`–`#E5C47E` | One or two broad ridges/upper planes, no white speckles |
| | Bronze shadow/contour | `#5A3D28` / `#382316` | Warm umber underplates and joints |
| Identity accents | Copper-red hair base/light | `#C8432B` / `#E36842` | Most saturated warm face-framing color, against brim darkness |
| | Red ribbon | `#C43428` | Identifies the blade/shaft connection |
| | Violet eyes | `#8E5CC2`–`#9273B4` | Dark upper iris and lighter lower violet |
| Neutral links | Ivory fur base/shadow | `#EDE6D6` / `#B8B4C2` | Warm ivory rather than pure `#FFFFFF`, with cool gray/violet shadow |
| | Leather/wood | `#6E442C` / `#8A5E3B` | Reddish leather versus less saturated aged yellow-brown wood |
| | Dry straw | `#C2964F` / `#8C622B` | Yellower than bronze; dark terminal gaps separate bundles |
| | Dark blade/bevel | `#3A434E` / `#B8C2CC` | Broad dark steel face versus bright cutting bevel |
| Magic/effects | Spell, infusion, Refine | `#38B6FF` / `#9CE5FF` | Clear cool aqua/cyan paths, rings/wedges, brighter than costume blue |
| | Guard/Plating | `#7A93AC` / `#D6E4F0` | Heavy steel-blue and cool reflections |
| | Vigor/Flame | `#FF6B2B` / `#FFD15C` | Orange/gold flame and heat reflected on bronze/skin |

> `Realta.cs` UI color `#e76f51` identifies deck entries/name labels; it does not recolor the costume orange.

## Material Surfaces and Failure Patterns

| Material | Target | Avoid |
|---|---|---|
| Blue cloth | Broad base, force-driven shadow masses, firm overlaps, restrained soft curvature, quiet directional brushwork | Fine fold lines, uniform noise, watercolor blotches, repeated small triangular shading |
| Bronze | Dominant shadow/base/broad reflection with curved transitions; selected thickness highlights | Fragmented gem facets, white scratches, spotted corrosion, chrome, bright rims around every plate |
| Ivory fur | Chunky cloud-like connected silhouette, quiet inner gray-violet overlaps | Uniform sharp teeth, black dots/lines throughout |
| Aged wood/straw | Long shaft values, two or three grain lines; straw in 5–8 bundles with irregular V gaps | Dense bark cracks, exaggerated knots, hundreds of combed black lines |
| Face/hair | Bob in 4–6 major locks; clean forehead/cheek/jaw planes; minimal clear nose/mouth | Glassy gacha eyes, airbrushed cheek gloss, continuous white hair halo |

## Translation by Body Part

| Part | Adopt | Reduce |
|---|---|---|
| Hat/coat | Silent-like long connected cloth masses, motivated folds, restrained transitions | Equal blue triangles, bright outline around entire hat |
| Breastplate/pauldrons | Defect/Ironclad-like broad dull reflection and selected structural ridge | Random patches, gold rims everywhere, white flecks |
| Neck/cuff/hem fur | Connected ivory strip at distance, a few large terminal breaks | Pixel-scale teeth and scratched internal lines |
| Old wood shaft | Long gradual values, sparse long grain, very shallow bow | Plastic shine, cracks, frequent knots/bends, dense contours |
| Broom | One broad fan split into a few overlapping bundles and large end gaps | Light/dark line on every strand, needle-like separation everywhere |
| Pouch/gloves | Minimal shape/flap/contact lines and low gloss | Equal emphasis on stitching, sheen, buckles, ornaments |

## Face and Hair Preferences

Combat expression heads retain the original head silhouette, hair, hat and alpha. Closed uses gently shut eyelids and a small relaxed closed mouth. Hurt uses tightly squeezed crinkled lids, furrowed brows, clenched teeth and faint strain blush; it must remain clearly distinct from the peaceful closed state at scale 0.625. Attack uses focused violet eyes, downward-angled brows and an open shouting mouth. Composite generated facial skin as one continuous surface bounded by the hairline, cheeks and jaw; avoid inset cheek patches and retain the original dark chin/hair junction. Skin-color selection must not leave original eye fragments in closed expressions.

Raw references establish identity, not every rendering method. Official masks/skulls do not require erasing Realta's face. Adapt its information to combat size.

- Keep copper-red bob, violet eyes, pointed ears, and confidence; do not replace eyes with black dots or add a mask.
- **Preserve approved anatomy:** `base.png` owns eye size, short jaw, forehead, expression, and silhouette. Simplification removes internal sparkle/lines within those proportions.
- Eyes first read as a short dark upper-lid wedge and small violet iris. Reduce multiple sparkles, concentric rings, and pronounced lower lashes. Gacha-style large eyes/glossy skin are not the combat rendering target.
- A small side nose shadow and short asymmetric mouth curve or small bright tooth plane suffice. Reduce all-cheek pink gradients and lip gloss.
- One broad brim shadow may cover forehead/upper eyes while keeping expression readable; do not black out the face.
- Hair reads through silhouette, part, and ear-adjacent masses. Keep one or two face-adjacent lines; merge the rest into red-brown planes.
- These apply to non-SD standing art and selection tiles. Preserve the separately approved large chibi eyes/round cheeks for portraits and markers.
