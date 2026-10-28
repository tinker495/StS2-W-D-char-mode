# Realta Art Specification

## Basic Strike candidate (2026-10-25)

The user requested the weapon from the character image being swung. Production brief: one oversized upper broom-glaive with its curved dark-steel blade, pale bevel, two-rivet bronze socket, red ribbon and connected wooden pole, sweeping across a crimson/burgundy graphic background with a cyan/ivory cutting arc. No person, frame, text or UI. The lower broom end is outside this weapon-focused composition; the blade remains aligned with its shaft rather than becoming a right-angle scythe. Inputs were `base.png` for equipment construction and native `StrikeIronclad`/`StrikeNecrobinder` portraits for staging and density. The built-in image generator created a new complete landscape painting. At 250 x 190 the blade, ribbon and socket remain readable; motion accents are sharper and brighter than native Ironclad's softer orange arc. Pending user review.

Single source for Realta's identity, apparent age, costume, weapon, palette, asset-specific staging, and prompt material. Follow [ARTS.md](../../../ARTS.md) for shared rendering; [character-asset-pipeline](../../../.agents/skills/character-asset-pipeline/SKILL.md) for execution/recipes; [realta-assets.md](realta-assets.md) for file mapping, code wiring, and status; and [realta-history.md](realta-history.md) for decision rationale. [realta.md](realta.md) and [GLOSSARY.md](../../../GLOSSARY.md) own human-facing gameplay and setting.

The approved [base.png](base.png) establishes the youthful appearance, open forehead, and refined weapon thickness now reflected in UI, standing art, and selection art. Character-specific values and construction are not defaults for other characters. Observations and choices below are neither official StS2 specifications nor automatic acceptance criteria.

## 1. Canonical Base and Raw References

### 1.1 Roles and Conflicts of Four Raw References

The four originals in [refs/](refs/) have distinct adopted/excluded roles.

| File | Adopted role/features | Excluded features |
|---|---|---|
| [ref1.png](refs/ref1.png) | Primary identity/color/weapon: large bent petrol-blue hat, copper-red bob, violet eyes, confident smile, blue coat/ivory fur, broom-glaive with blade + metal socket + red ribbon + wood shaft + broom | Floating crossed-leg pose, one-sided shoulder armor, thin anime outlines and skin highlights |
| [ref2.png](refs/ref2.png) | Both bronze pauldrons, continuous ivory collar around both sides of neck, breastplate/belt/pouch construction, grounded two-foot stance | Desaturated gray-violet costume instead of petrol blue, long subdued face, fine metal/fur texture |
| [ref3.png](refs/ref3.png) | Visible pointed elf ear, playful brows/mouth, bronze shin/foot plate construction | Extreme leaning perspective/body exaggeration; straw resembling long hair spread across the floor |
| [ref4.png](refs/ref4.png) | Petite youthful build, small lively face, playful confidence; forehead guided by the user's request and ref2's part | Crouched/extreme perspective, low brim hiding face, teal palette replacement, intricate engraved armor; do not replace the approved rendering style |

### 1.2 Canonical Base Specification

[base.png](base.png) is the user-approved 1152×1728 RGBA master and **single visual identity anchor**. Preserve the complete character, weapon, canvas, and transparent margins. The current combat/rest/merchant image is a proportional reduction of that whole file. The user reverted the separately generated combat pose; generating another pose is not part of the current delivery. Generate UI/selection scenes separately with purpose-specific cameras while preserving the following identity. Dimensions/conversions/status belong in the [asset guide](realta-assets.md#0-current-application-status).

1. **Head, face, ears:** apparent age around 13–14, short soft jaw/cheeks, short copper-red bob parted toward both temples to expose a broad forehead and eyebrows. Violet eyes, a clearly pointed ear visible through the hair on the left side of the image, confident half-smile. Keep a two-eye three-quarter face and forward gaze without an extreme profile.
2. **Witch hat:** broad brim, crown bent/drooping left-back, deep petrol blue. Brown leather band and near-round bronze/brass front buckle. Dark navy-violet underside (`#253748`) supports the face.
3. **Shoulders/collar:** rounded bronze pauldrons on **both shoulders**, plus an **unbroken ivory fur collar** around both sides of the neck.
4. **Chest/arms:** matte curved bronze breastplate, small round/square upper connections, gently V-shaped lower center. Dark gloves and bronze vambraces on both arms; chunky ivory fur at cuffs/cape edges.
5. **Coat/waist:** petrol-blue A-line coat with ivory fur along both front openings and hem. Brown belt with bronze buckle; a rectangular flapped pouch with metal fastener at the character's front-right (viewer center-left).
6. **Legs/boots:** dark brown/charcoal leggings, diamond-shaped bronze knee guards, sturdy boots with folded leather cuffs and bronze shin/instep plates.
7. **Hybrid broom-glaive:**
   - A **single long polearm**, never a scythe or separate sword and broom.
   - Gently curved single-edged dark steel blade with a distinct pale bevel, following the approved base.
   - Wedge-shaped **bronze metal socket and two rivets**, not cloth/leather wrapping.
   - Bright red ribbon (`#B54535`) tied immediately below the socket, two flowing tails.
   - Aged wood shaft with an extremely restrained natural bow and only two or three long grain lines. Avoid smooth plastic straightness and severe branch-like zigzags.
   - Fan-shaped coarse dry straw, tied with red cord and **two dark bands**; a few broad bundles.
8. **Surface density:** broad connected color planes with selected angular values. Avoid accumulated tiny patches/lines in armor, fur, or wood. Greater official-style similarity does not authorize changing the approved face/silhouette.

### 1.3 Expressions to Preserve

**A youthful 13–14 appearance and exposed forehead in natural full-body proportions.** This is apparent age, not a new canonical age declaration. Non-SD does not mean adult; do not default prompts to `adult`.

- **Face/build:** short soft lower face, mildly round cheeks, small simple nose, slightly larger head/body relationship and petite build, without the icon's2–2.5-head chibi anatomy. Preserve playfulness, confidence, violet eyes, and pointed ears. Judge age visually rather than by head count alone.
- **Hair/hat:** open part, hair swept toward temples, forehead visible from central hairline to brows. Long diagonal bangs or a low brim must not cover it again.
- **Style/equipment:** `base.png` owns broad connected planes, selected angular values, low surface noise, blue/bronze palette, paired shoulders, continuous collar, and single broom polearm.
- **Forward/head direction:** battle-forward is screen-right. Keep both eyes, short jaw, and open forehead while directing pupils forward. Large head rotation into profile changes identity. Follow [ARTS.md section 5.1](../../../ARTS.md#51-combat-rest-and-merchant-standing-art) and section 3.1.
- **Weapon ratio:** broom tip reaches the feet and blade tip rises above the hat, contrasting a small body with a long polearm. Preserve a shaft narrow enough for one-hand grip and a long narrow blade. Use `base.png` for exact length/width/thickness relationships. Changing one axis does not imply enlarging the whole weapon or shrinking the character.

See [history](realta-history.md) for reasons and the [asset guide](realta-assets.md#0-current-application-status) for delivery status.

## 2. Palette, Materials, and Face Rendering

### 2.1 Canonical Palette and Theme Accents

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

### 2.2 Material Surfaces and Failure Patterns

| Material | Target | Avoid |
|---|---|---|
| Blue cloth | Broad base, force-driven shadow masses, firm overlaps, restrained soft curvature, quiet directional brushwork | Fine fold lines, uniform noise, watercolor blotches, repeated small triangular shading |
| Bronze | Dominant shadow/base/broad reflection with curved transitions; selected thickness highlights | Fragmented gem facets, white scratches, spotted corrosion, chrome, bright rims around every plate |
| Ivory fur | Chunky cloud-like connected silhouette, quiet inner gray-violet overlaps | Uniform sharp teeth, black dots/lines throughout |
| Aged wood/straw | Long shaft values, two or three grain lines; straw in 5–8 bundles with irregular V gaps | Dense bark cracks, exaggerated knots, hundreds of combed black lines |
| Face/hair | Bob in 4–6 major locks; clean forehead/cheek/jaw planes; minimal clear nose/mouth | Glassy gacha eyes, airbrushed cheek gloss, continuous white hair halo |

### 2.3 Translation by Body Part

| Part | Adopt | Reduce |
|---|---|---|
| Hat/coat | Silent-like long connected cloth masses, motivated folds, restrained transitions | Equal blue triangles, bright outline around entire hat |
| Breastplate/pauldrons | Defect/Ironclad-like broad dull reflection and selected structural ridge | Random patches, gold rims everywhere, white flecks |
| Neck/cuff/hem fur | Connected ivory strip at distance, a few large terminal breaks | Pixel-scale teeth and scratched internal lines |
| Old wood shaft | Long gradual values, sparse long grain, very shallow bow | Plastic shine, cracks, frequent knots/bends, dense contours |
| Broom | One broad fan split into a few overlapping bundles and large end gaps | Light/dark line on every strand, needle-like separation everywhere |
| Pouch/gloves | Minimal shape/flap/contact lines and low gloss | Equal emphasis on stitching, sheen, buckles, ornaments |

### 2.4 Face and Hair Preferences

Combat expression heads retain the original head silhouette, hair, hat and alpha. Closed uses gently shut eyelids and a small relaxed closed mouth. Hurt uses tightly squeezed crinkled lids, furrowed brows, clenched teeth and faint strain blush; it must remain clearly distinct from the peaceful closed state at scale 0.625. Attack uses focused violet eyes, downward-angled brows and an open shouting mouth. Composite generated facial skin as one continuous surface bounded by the hairline, cheeks and jaw; avoid inset cheek patches and retain the original dark chin/hair junction. Skin-color selection must not leave original eye fragments in closed expressions.

Raw references establish identity, not every rendering method. Official masks/skulls do not require erasing Realta's face. Adapt its information to combat size.

- Keep copper-red bob, violet eyes, pointed ears, and confidence; do not replace eyes with black dots or add a mask.
- **Preserve approved anatomy:** `base.png` owns eye size, short jaw, forehead, expression, and silhouette. Simplification removes internal sparkle/lines within those proportions.
- Eyes first read as a short dark upper-lid wedge and small violet iris. Reduce multiple sparkles, concentric rings, and pronounced lower lashes. Gacha-style large eyes/glossy skin are not the combat rendering target.
- A small side nose shadow and short asymmetric mouth curve or small bright tooth plane suffice. Reduce all-cheek pink gradients and lip gloss.
- One broad brim shadow may cover forehead/upper eyes while keeping expression readable; do not black out the face.
- Hair reads through silhouette, part, and ear-adjacent masses. Keep one or two face-adjacent lines; merge the rest into red-brown planes.
- These apply to non-SD standing art and selection tiles. Preserve the separately approved large chibi eyes/round cheeks for portraits and markers.

## 3. Realta Staging by Asset

### Relic refresh brief (2026-10-24)

The user requested new relic artwork in the original game's isolated-object style. The later starter-reference correction supersedes the initial weapon-based brief: **Bronze Watchdog** is a frontal bronze canine-head relief based on the right-hand green Celtic head reference, while **Kuu** is a more imposing three-quarter snarling metal wolf head based on the left-hand armored-wolf reference. Do not use the broom-glaive for either starter relic. Preserve the family relationship through warm bronze raised planes and deep brown recesses; Kuu adds a layered cheek/mane silhouette, visible teeth and a restrained orange-red eye. Simplify knotwork to a few broad raised bands. Exclude the source images' green line color, full body, circular border and background.

The three exclusive relics use the user's other references: Witch Pointed Hat has a teal/indigo curled cone and broad brim with sparse gold celestial arcs; Eldir Gramr is a straight red/orange incandescent sword with a dark burgundy grip and steel crossguard; Dragon Cloak is a burgundy royal mantle with ivory ermine trim and a few broad gold motifs. No wearer, mannequin or crown belongs in the cloak icon.

Generate each complete object directly in a square transparent composition. Use thick dark contours, two or three connected value families and restrained painted edges, guided by the original relic/potion reference sheet listed in ARTS.md. Avoid fine engraving, dense embroidery and individual hairs. The five illustrations were reviewed at 94 px and 48 px and selected for runtime application; paths and installation status belong to the asset guide.

Follow [ARTS.md](../../../ARTS.md) for shared chibi scope. This specification supplies Realta's face, expression, costume, and hat silhouette within it. Section 5.2 defines chibi details; other assets retain youthful canonical anatomy. Read [shared safe zones](../../../ARTS.md#5-asset-compositions-and-safe-zones) first.

### 3.1 Combat, Rest, and Merchant Standing Art

- **전투·상인 무기 앞뒤 순서:** 빗자루창 전체와 리본은 머리·모자·얼굴을 포함한 모든 신체 파츠보다 앞에 그린다. 자루를 감싸는 쥔 손과 VFX만 그 위에 둔다. 사망 시 손을 놓은 무기도 몸 앞에서 떨어져 바닥에 놓이며, 리본은 낙하 중 눈을 가리지 않도록 모자 쪽으로 흘렀다가 내려온다. 최종 칼날은 얼굴 아래에 놓인다. 트리 순서와 해제·부활 전환은 [animation/README.md](animation/README.md)가 소유한다.
- **상점의 빈팔:** 화면 오른쪽 빈팔은 소매부터 커프·전완·장갑까지 몸통, 코트 앞뒤, 다리보다 뒤에 둔다. 코트 옆으로 전완과 장갑 가장자리만 조금 보여 손을 허리 뒤에 편히 둔 것으로 읽히게 한다. 전투 동작에서는 해당 자세의 앞쪽 순서로 돌아오며, 팔이 코트에서 벗어난 순간에 순서를 바꾼다.

- **전투 리그의 연결 면과 동작:** 기존에 보이는 얼굴·의상·장비 그림을 유지하면서 가려진 소매·허벅지·무릎·발목·목·손목 면을 완성한다. 공격은 손등이 보이는 그립으로 자루를 감싸고, 피격은 작은 골반 반동, 사망 초반은 양 무릎이 전방으로 모여 접히는 자세를 사용한다. 앞 코트와 ivory fur는 허벅지 위에 놓고, 견갑은 몸통에 고정한다. 저작 입력과 좌표·가중치·클립 계약은 [animation/README.md](animation/README.md)가 소유한다.

- **Merchant cutout animation:** the native `relaxed_loop` now uses a narrow planted stance with weight toward the viewer-left leg, the free glove resting by the belt/hip, and a low one-handed shaft grip. Keep the upper arm/elbow visible at full source length. The broom-glaive rests beside her with bristles just above floor support, the blade high, and its ribbon knot clearly outside the hat brim. Preserve the complete prop and fixed grip attachment. Use two restrained breaths over eight seconds with slow weight sway and small warmed spring motion; retain the youthful confident face. This animation-specific staging supersedes the old relaxed source keys, while the canonical base and standing still retain their approved composition. Technical coordinates, aliases and transition behavior are in [animation/README.md](animation/README.md).

- **Shared standing image:** use the complete [base.png](base.png), including transparent margins, without regeneration. `combat.png`, 384×576 RGBA, is shared by combat, rest, and merchant scenes. There is no separate runtime `standing.png`.
- **Current pose:** retain the base's upright, one-handed broom-glaive stance, youthful face, open forehead, paired pauldrons, and continuous collar. The separately generated two-handed combat pose was removed at the user's request. This character-specific delivery overrides the generic Branch B split until the user requests otherwise.
- **Application status:** combat/merchant display at 240×360 with scale 0.625; rest displays at 326.4×489.6 with scale 0.85. See the [asset guide](realta-assets.md#05-standing-and-combat-split) for the rollback and verification limits.
- **Composition/margins:**
  - Three-quarter standing idle toward screen-right. **Forward means the body's battle direction, not facing the camera.**
  - Specify head and pupils separately. Look toward the opponent, not the viewer. Preserve both eyes, face width, forehead, and short jaw; do not rotate into profile or erase the far eye. Limit necessary head adjustment and do not force body/nose/pupils to identical angles.
  - Keep roughly 3–5% fully transparent margins beyond blade tip, soles, broom, and brim; prevent boundary clipping.
  - Retain the base's one-handed grip and keep the shaft and socket away from the face.
- **Scene wiring/placement:** the [asset guide](realta-assets.md) owns current runtime anchors. This section owns face, pose, and margins.

### 3.2 Character UI, Energy, and Outline

Common runtime directory: `Sts2WD/images/charui/realta/`.

| File | Dimensions | Proportions/background | Composition and derivation |
|---|---|---|---|
| `character_icon.png` | 128×128 RGBA | **Chibi bust**, transparent | Generate directly with large round head/cheeks/violet eyes occupying at least 70% of canvas area. Hat top lies outside the generation frame; tiny paired shoulders and ivory collar remain below |
| `character_icon_outline.png` | 128×128 RGBA | Derived alpha | No AI redraw. Dilate final small alpha 2–3px into white `#FFFFFF` for matching silhouette; current exact radius is in the asset guide |
| `map_marker.png` | 128×128 RGBA | **Chibi head and whole hat only**, transparent | No shoulders/torso. Slightly compress crown height so face/buckle read at 48px; start with 8% / about 10px clear margin |
| `char_select.png` | 132×195 RGBA | **Canonical non-SD close face**, opaque | New right-facing three-quarter portrait dominated by face/red bob. Hat tip/brim extremities are outside the designed camera; partial collar/shoulders below. Use whole generated image; see 3.2.1 |
| `char_select_locked.png` | 132×195 RGBA | Same figure alpha, opaque | Identical contour: black figure on gray. Remove internal eyes/face/armor; do not generate separately |
| `big_energy.png` / `text_energy.png` | 74×74 / 24×24 RGBA | Same flame-heart master, transparent outside | Red-orange curling flame silhouette, orange-gold bevels, and a large dark burgundy heart-shaped inset. The inset contains no small heart or other decoration; it is reserved for the cost number |

### 3.2.2 Applied Heated-Iron Card Frame and Energy Emblem

The user selected the flame-heart emblem and the heated-iron card-frame concept on 2026-10-20 and requested local application. The frame starts near black/charcoal at the top, transitions through dark iron-red, and ends in bright vermilion along the lower rails and bottom bevel. Preserve broad forged planes, restrained painted texture, and a dark warm description panel. Avoid lava cracks, sparks, smoke, excessive scratches, gold filigree, or an additional decorative emblem.

The applied body texture is generated for the game's existing card-body layer, with a transparent portrait aperture. It omits the cost badge, title ribbon, and type plaque because the game renders those separately. Preserve the native title-banner and portrait-border materials and opacity: their gray/cyan/gold distinction communicates Common/Uncommon/Rare. Do not replace the banner with one dark material or hide the native portrait border. The type plaque uses subdued warm iron. Existing card art, text positions, and upgrade colors remain under game control. Standard Attack/Skill/Power cards and the Guard token use the new body. Ancient cards retain their native full-art frame.

The cost icon uses the complete approved flame-heart image, proportionally resized without cropping to both delivery sizes. Its dark heart is below the silhouette's geometric center. The cost label uses a centered 34×36px box shifted 8px down from the native layout, live 14–22px autosizing, and a restrained 3px text/shadow outline. Keep the original font, dynamic cost colors, and X/unknown-cost behavior. Do not reuse the native 32px maximum and 16px outline in this smaller inset. The lower-left combat counter uses the same complete master as a 256px texture displayed at 128px; current/max energy sits in a 54×44px heart-inset label with 16–26px autosizing and 3px outlines. Keep the upright flame silhouette static, the native energy-gain VFX and transitions, and native darkening/red text at zero energy. Paths, node restoration, packaging, and installation verification belong in the asset guide.

### 3.2.1 Generate Face-Centered Selection Tiles Directly

Read the [five-character normal/locked study](../../research/sts2-character-select-tile-study.md) and [original PNGs](../../research/sts2-style-refs/character-select-tiles/README.md). Design the tile separately from the large illustration. Original masks/skulls dominate their narrow frames; Silent's horns and Regent's head ornament extend beyond the top. Eye heights vary. **Non-SD preserves canonical age/anatomy; it does not require an adult face or full body inside the tile.**

- **Camera:** violet eyes, brows, nose, and mouth read first; red hair/pointed ear form outer masses. Start with forehead-to-jaw skin height around half the image. This is a Realta guideline, not official measurements or an automated threshold; judge at 132×195 beside originals.
- **Hat:** partial angled brim, band, and bronze ring may suffice. Do not shrink the face to show the whole crown. Hair, ear tips, and brim may extend beyond side boundaries; keep shadow from obscuring expression.
- **Costume:** continuous ivory collar and partial paired pauldrons below. Preserve their construction without forcing off-camera chest, belt, pouch, weapon, and hands into the portrait.
- **Values/background:** separate light skin, red-brown hair, and blue hat/collar. Use simple blue backing; no scenery, magic particles, or excessive vignette. Selection border, mask, and hover treatment belong to UI.
- **Normal/locked:** generate one transparent face master, resize it whole, and share final alpha. Composite colored figure on blue and black figure on gray; both outputs have alpha 255. No eye glow, face values, lock glyphs, or confusion with deselection tint.
- **Records:** state output dimensions, full-image use, colors, and decision rationale in the asset guide. Prompts/comparisons are optional local records. Regenerate composition changes; follow [Recipe 4-A/B](../../../.agents/skills/character-asset-pipeline/PROMPT-RECIPES.md#4-a-char_selectpng-132--195-opaque-tile).

The current tile was generated from base identity and actual Defect/Silent face occupancy references, then whole-image Fit resized with partial hat, face, and lower collar. It was not extracted from standing art.

### 3.3 Relic Icons

- **Three files per relic:**
  - `Sts2WD/images/relics/big/realta_<name>.png`: 256×256 RGBA.
  - `Sts2WD/images/relics/realta_<name>.png`: 94×94, resized from large.
  - `Sts2WD/images/relics/realta_<name>_outline.png`: 94×94, white/light silhouette from small alpha expanded 2–3px.
- **Composition:** one isolated object; no background, floor shadow, or outer glow. Start at 72–84% occupancy / 20–35px margins in 256×256. Exaggerate identifying ears/wings/hat tips/hilts for 48–64px top-bar reading and simplify interiors to 3–5 major planes.
- **Five Realta relic designs:**
  1. `청동의 번견` (`realta_bronze_watchdog`, starting): small crouched bronze hound watching right; heavy `#9D754E` plates with major angular ears, muzzle, forelegs.
  2. `쿠우` (`realta_kuu`, upgraded starter): same hound family, standing/roaring or chest-forward confidence, expanded shoulder/mane plates, bright cyan/aqua `#38B6FF` core between plates.
  3. `마녀 고깔모자` (`realta_witch_pointed_hat`, Common): isolated canonical petrol-blue left-bent hat, brown band, round bronze buckle.
  4. `용의 망토` (`realta_dragon_cloak`, Uncommon): broad-scaled red-bronze/crimson leather cloak and flame-shaped bronze clasp; small warm hem reflections suggest Vigor/Flame.
  5. `엘디르 그라므르` (`realta_eldir_gramr`, Rare): ancient fire dagger/sword with heated bronze guard and red-gold heat core. Keep blade shape sharp; concentrate flame contrast at guard rather than burying the whole blade.

### 3.4 Power Icons

- The 19 user-provided named symbols in [refs/powers](refs/powers/README.md) own the corresponding primary motifs. Preserve the specified symbol and arrangement while adding the shared game rendering; do not replace a user-selected symbol with an interpretation of the card name. These are new raw references, not approved runtime artwork.
- User revisions supersede two original motifs: 요정 장인 uses green leaf-like wings around a rectangular craft hammer, with no central orb, concentric socket, or circular wing spots that suggest an eye. 강철 포식자 emphasizes a visibly broken sword and separated steel shards against a straight, thick, intact wall slab; the wall must not curve, bend, crack, or appear to give way.
- 불멸의 충성 temporarily reuses 원초의 형상 with an exact left/right reflection at both runtime sizes, as selected by the user. Preserve its colors, transparency, dimensions and complete canvas without redrawing.
- **Two files per power:** `Sts2WD/images/powers/big/realta_<name>_power.png` at 256×256 RGBA; `Sts2WD/images/powers/realta_<name>_power.png` at 64×64, resized.
- Use a **single persistent-state emblem**, not a miniature card scene: two or three bold shapes and two or three primary colors.
- `Amount` overlays the bottom-right `x/y:60–100%`. Put identifying tips/eyes/clock hands/motifs central-to-upper-left, `x/y:15–80%`.
- Distinguish related powers by contour (shield/anvil/crossed weapons/wing/ring) and dominant color.

### 3.5 Standard Card Illustrations

**Mountain Splitter, application requested 2026-10-29:** the selected [painting](card-art/realta_mountain_splitter.png) shows Realta from behind after cleaving Aeonglass into two gold-framed turquoise sections. Preserve the simple connected color planes, the large straw broom head covering the back, the right-hand connection to the lowered glaive, and one continuous blue rear robe panel streaming left. Its front opening faces away from the camera; do not add a V-shaped opening on the visible back or a closed skirt-like lower hem. At the user's request, the setting extends upward through dark-blue architecture and downward through the ground and shadows to fill the complete `25:19` card window. Preserve this painted coverage; do not restore the former top/bottom letterboxing. This staging applies to this card, not to the canonical base or other assets. Mapping and conversion are in the [asset guide](realta-assets.md#0-current-application-status).

- `Sts2WD/images/card_portraits/realta/realta_<name>.png`: 1000×760 opaque PNG, 25:19; standard display 250×190, enlarged 500×380.
- Keep action, face, hand, and impact within `x:15–85%,y:12–82%`, away from upper-left energy and frame curves. Outer 10–15% may contain expendable environment, cloth tips, or trails.
- **Depth/contrast starting points:** foreground action/tool 100% with sharpest edges and extreme values; midground target/prop 60%; background 25–35%, grouped into two or three broad values.
- **Attack (24):** strong diagonals, weapon arcs, asymmetric impacts; close to blade/contact.
- **Skill (26):** medium-close precise tool work, defense, or coordination with a guardian.
- **Power (18):** stable symbolic composition for persistent spaces/states/rules: forge, duel, witch circle, ancient relic/anvil.

### 3.6 Ancient Full-Art Cards

- Two vertical 606×852 assets: `realta_iordina_mac_bethlehem.png` (`요르디나 막 들레헴`) and `realta_genius_inspiration.png` (`천재의 영감`).
- Based on `NCard` Ancient layout:
  - Top `y:0–14%` / 0–120px: title banner and upper-left cost.
  - Upper-middle `y:15–58%` / 128–495px: **primary focal window** for face, main weapon, or inspiration light.
  - Lower `y:62–100%` / 528–852px: `AncientTextBg` and description. Use subdued dark vertical cloth, fading magic, or workbench shadows; avoid high-contrast faces/ornate metal competing with text.

### 3.7 Selection Illustration

Separate 1920×1080 non-SD scene produced with Branch B UI. Base owns identity/equipment; pose/camera are designed for selection. Read the [five-character study](../../research/sts2-character-select-composition-study.md) and [original resources](../../research/sts2-style-refs/character-select/README.md). Their distinctive swords, giant hands, thrones, or flowing cloaks establish character through gesture. Do not default to full combat figures before scenery or require boots/weapon tips in frame. Design close/perspective staging during generation, not later cropping.

The current scene is **Realta holding her improved broom-glaive upright, inspecting the blade in the light, and smiling confidently**. A low camera links large right-side face, open gloved hand, and rising blade. Broad bronze diagonals and dark blue coat establish background masses; cyan magic concentrates near the blade. Preserve paired pauldrons, continuous collar, chest, pouch, and single polearm. It was generated from `base.png` plus actual Ironclad/Regent selection references. Future replacement production follows the [new-artwork rule](../../../.agents/skills/character-asset-pipeline/SKILL.md#branch-b-and-c-start-with-new-artwork), not this scene as a fixed template.

Low-contrast cloak/weapon/ability forms may continue behind left name/ability/relic text. **Safe space preserves text and avoids key face/hand/contact, rather than leaving an empty rectangle.** Inspect lower tiles and bottom-right confirmation with an observational UI overlay. Keep panels, words, and buttons out of runtime art. Runtime image: `images/charui/realta/char_select_bg.png`; Control/TextureRect scene: `scenes/realta/char_select_bg_realta.tscn`; property: `CustomCharacterSelectBg`.

See [future-asset research](../../research/sts2-future-asset-reference-study.md) for official examples/UI crops. Shared ARTS.md owns chibi scope; Realta-specific face/hat/costume choices stay here.

## 4. Gameplay Themes and Recurring Objects

Use consistent symbols across 70 cards, relics, and powers so imagery communicates theme membership. Korean names below are localized product terms, preserved literally.

### 4.1 Ten Theme Translations

| Theme | Visual motif | Accent | Distinction |
|---|---|---|---|
| Infusion/Refine (`Refine`) | Artisan hands/tools engraving vivid aqua/cyan `#38B6FF` into armor/weapons | Aqua/cyan + bronze heat | Show the processing contact where light enters a surface, not a generic explosion |
| Legacy (`Legacy`) | Persistent form of broom-glaive or heirloom `린날 보브데그` despite reforging | Bronze + cyan inscriptions | Preserve canonical weapon construction rather than inventing a different weapon per card |
| Guard (`Guard`) | Steel/bronze guardian hound or golem blocking, assembling, or being repaired | Steel-blue `#7A93AC` + cyan eyes/core | Solid armored guardian, distinct from Spell's round translucent barrier |
| Plating (`Plating`) | Layered bronze/steel plates, rivets, repair patches | Warm bronze `#B08450` + gold reflection | Physical hammered plate thickness/weight rather than ghostly aura |
| Vigor (`Vigor`) | Compressed orange/gold force before a strike; shoulder/weapon preparation | Amber/orange `#FF8C3B` | Sharp readiness/glow rather than Flame's literal fire |
| Spell (`Spell`) | Final-energy blue rings, geometric trajectories, spatial distortion | Cobalt/cyan `#2B95FF`–`#9CE5FF` | Phenomena from hands/glaive, never an energy 0 HUD number |
| Masterwork (`Masterwork`) | One masterpiece or repeated strikes through rhythmic afterimages/multiple magic rings | Gold + cyan trajectories | Repetition focused on one subject, not a cluttered weapon collection |
| Duel (`Duel`) | Realta and one opponent face each other with gaze/blade tension | Cool background + foreground spotlight | Two-subject distance and fair confrontation rather than melee |
| Valor (`Valor`) | Direct charge toward a powerful or armored opponent | Strong backlight + bronze/copper contrast | Large threatening shield/body silhouette conveys courage against strength |
| Flame (`Flame`) | Orange/crimson fire rising from armor/blade with hot sparks | `#FF5520`–`#FFD15C` | Rising flame tongues and warmth, distinct from cool Spell/Refine |

### 4.2 Three Recurring Object Families

1. **Flying glaive / `요르디나 막 들레헴`**, used in `비행 글레이브`, `되돌리기`, and related cards:
   - Preserve curved dark blade + bronze two-rivet socket + red ribbon + subtly bowed aged shaft + lower broom.
   - The Ancient form keeps this weapon, adding old inscriptions and strong cyan/gold return trajectories toward its owner.
2. **`강철의 수호자`**, appearing in `강철의 수호자`, `역장 아가리`, `협공`, `자가 수리`, `불멸의 충성`, `강철 포식자`:
   - A quadruped steel/bronze armored guardian hound, sharing the design family of `청동의 번견` and `쿠우`.
   - Common identifiers: angular metal jaw, broad shield-like shoulder plates, blue magical eyes.
3. **Realta's hands and craft tools**, in `담금질`, `숯돌`, `강철 집게`, `마법 땜질`, `오래된 끌`, `망치질`, `뜨임`, `수선`, `제련`, `경계 재련`:
   - Close-up arms retain **dark glove + bronze vambrace + ivory fur cuff + petrol-blue sleeve**, making ownership recognizable.

## 5. Character Prompt Material

Insert only relevant parts into `<CHARACTER_ANCHOR>`, `<PROPORTION_BRIEF>`, `<PALETTE>`, and `<ASSET_SUBJECT>`. Limit equipment descriptions to visible parts. [base.png](base.png) owns identity; official images guide style/composition. Assemble new requests from this specification and recipes without old logs.

### 5.1 Canonical Identity Anchor

```text
Character anchor (match supplied approved base.png): Realta, youthful female elf witch-artisan warrior with an apparent age around 13-14 and natural non-SD proportions. Short copper-red bob (#C8432B) parted back toward both temples to reveal a broad open forehead, violet eyes (#9273B4), clearly pointed ears, soft short jaw and confident lively expression. Wide-brimmed petrol-blue witch hat (#245F80) with a left-bent crown, brown leather band and round bronze buckle. Continuous ivory fur neck collar (#EDE6D6), BOTH shoulders protected by rounded weathered bronze pauldrons (#9D754E), clean bronze breastplate and forearm vambraces, petrol-blue coat and cape with chunky ivory fur trim, brown leather belt with rectangular front pouch, dark gloves, dark leggings, diamond bronze knee guards and sturdy bronze-plated boots.
Weapon anchor (when visible): a SINGLE hybrid broom-glaive polearm — upper curved dark steel glaive blade with pale edge bevel, connected via a wedge-shaped bronze metal socket with two rivets, a red ribbon (#C43428) tied just below the socket, a long aged wooden shaft with a very subtle natural bow, and a lower broom bundle of coarse golden-brown straw bound with red cord and two dark bands.
```

### 5.2 Chibi Override

```text
Proportion override (CHIBI / SD ONLY): Transform into an unmistakable super-deformed chibi style (2 to 2.5 heads tall when torso is shown). Extremely large rounded head and full cheeks, short rounded chin, large simple widely-spaced violet eyes, tiny nose and confident smiling mouth. Preserve Realta's copper-red bob, pointed ears, petrol-blue bent witch hat with brown band and bronze buckle, continuous ivory fur collar and tiny dual bronze shoulder pauldrons, simplified into bold connected cel-like planes readable at 48-128px.
```

### 5.3 Flame-Heart Energy Emblem

```text
Use case: stylized-concept.
Asset type: game card energy orb icon, exported to 74x74 and 24x24, square transparent icon.
Subject & Composition: preserve the user-selected red-orange flame silhouette wrapping around a large dark burgundy heart-shaped inset. Use broad curling flame tongues, dark red bevel shadows, and selective orange-gold highlights. The inset is empty: remove the former small inner heart and keep the field quiet for a white energy numeral. Keep the complete silhouette and transparent margins when resizing.
Style/medium: <insert ARTS.md section 7.2 [Block A]>. Broad connected shapes and restrained surface detail that survive 24x24.
Backdrop & Exclusions: true transparent alpha outside the emblem; the heart inset stays dark and opaque. No painted numbers, inner symbols, runes, stars, particles or outer glow.
```

### 5.4 Selection Tile and Locked Colors

Normal backing is `#2476A6`; locked uses black figure `#000000` on gray `#3B3B3B`. Composite both from the same resized alpha. These are Realta delivery choices, not exact RGB values asserted for every official character.

## 6. Decision History

This specification owns current appearance and production constraints. [realta-history.md](realta-history.md) explains equipment, age, gaze, weapon-ratio, and composition decisions without requiring drafts or comparisons. Versioned originals and execution logs are optional local working material.

### 공격 궤적 범위 (2026-10-20)

공격 궤적은 기존 청록·아이보리 그림을 실제 칼날 경로에 맞춰 길게 펼친다. 칼끝 뒤에만 차례로 나타나며 앞쪽은 굵고 꼬리는 가늘다. 0.15초 타격을 지나 0.25초까지 경로를 유지하고 0.34초에 사라진다. 얼굴은 보이도록 두고 무기보다 위에 그린다.


## Relic application approval (2026-10-24)

The user approved applying all five relic illustrations and installing them locally. The described candidate forms are now the selected relic artwork; earlier pending-selection wording records only the previous review stage. Runtime uses the existing model filenames; gameplay behavior was not changed by the image promotion.
