# Slay the Spire 2 Rest-Site Illustration and Lighting Study

- Research date: 2026-11-23 (Asia/Seoul).
- Purpose: document how base-game Slay the Spire 2 rest-site rooms, log seats, four-player co-op slots, and vanilla character Spine rigs actually compose and light seated characters, grounding the project's stage-inpaint rest-site illustration pipeline.
- Distinguish **code/scene observations**, **image observations**, and **project judgments**. Observed vanilla values guide mod integration and harmony; they are not automated acceptance thresholds.
- Related: [runtime scene contract](sts2-cutout-runtime-contract.md#room-scale-and-ground-line-contract), [official animation comparison](sts2-official-animation-comparison.md), [ARTS.md](/ARTS.md#51-combat-rest-and-merchant-standing-art).

## 1. Inspected Game Build and Sources

All observations below were verified directly against the installed local game build (`v0.113.1`, commit `d09d48e5`) via `sts2.dll` and `SlayTheSpire2.pck` rendered through MegaDot `4.5.1.m.14.mono.custom_build` (`gl_compatibility`) with the game's `libspine_godot` extension.

| Target | Observed asset / type | Scope inspected |
|---|---|---|
| Room layout & slot placement | `res://scenes/rooms/rest_site_room.tscn`, `MegaCrit.Sts2.Core.Nodes.Rooms.NRestSiteRoom` | `BgContainer`, `Character_1..4` coordinates/scales, child ordering, `FlipX()` dispatch, `ChoicesScreen` UI bounds |
| Character lifecycle & lighting | `MegaCrit.Sts2.Core.Nodes.RestSite.NRestSiteCharacter`, `MegaCrit.Sts2.Core.Models.Acts.Underdocks` | Act-index loop dispatch (`overgrowth_loop`, `hive_loop`, `glory_loop`), `HideFlameGlow()` (`_tracks/light_off`), `FlipX()` implementation |
| Act rest backgrounds (4) | `res://scenes/rest_site/{overgrowth,hive,glory,underdocks}_rest_site.tscn` | Seat textures (`RestSiteLLog`, `RestSiteRLog`), campfire logs/flames, `RestSiteLighting` layers, vignette/foreground cutters |
| Vanilla seated characters (5) | `res://scenes/rest_site/characters/{ironclad,silent,defect,necrobinder,regent}_rest_site.tscn` and `res://animations/rest_site/<c>/rest_site_<c>_skel_data.tres` | Screen bounding boxes, mean luminance, Spine slot stacks, per-Act slot RGBA tints, `cast shadow` and `highlight` mask structures |

## 2. Room Geometry, Log Seats, and Co-op Slot Layout

### 2.1 Container Hierarchy and Draw Order (`1920 × 1080`, `+Y` down)

`NRestSiteRoom._Ready()` mounts the Act background scene at child index `0` of `BgContainer`, behind all four player containers (`Character_3`, `Character_4`, `Character_1`, `Character_2` in back-to-front draw order):

- `BgContainer`: screen origin `(26.0, 74.0)` (`anchors = 0.5`, `offsets = (-934, -466)`).
- Every `Character_1..4` container uses `scale = (0.5, 0.5)`.
- `NRestSiteRoom._Ready()` resets each instantiated `NRestSiteCharacter.Position = Vector2.Zero` and calls `FlipX()` on odd 0-based player indices (`i % 2 == 1` → right-side slots `Character_2` and `Character_4`).
- `NRestSiteCharacter.FlipX()` negates `Scale.X` and `Position.X` on direct `SpineSprite` and `Sprite2D` children, and negates `ControlRoot.Scale.X` (leaving `ControlRoot.Position` unchanged, so `ControlRoot` must remain horizontally anchored around `x = 0`).

| Slot (player index) | Container pos in `BgContainer` | Screen origin (`1920 × 1080`) | Side / Flip | Seat role |
|---|---|---|---|---|
| `Character_1` (`0`) | `(625.0, 642.0)` | `(651.0, 716.0)` | Left front (unflipped) | Primary solo slot; lower-left front half of left log |
| `Character_2` (`1`) | `(1239.0, 649.0)` | `(1265.0, 723.0)` | Right front (`FlipX()`) | Mirror of `Character_1` on lower-right front half of right log |
| `Character_3` (`2`) | `(750.0, 585.0)` | `(776.0, 659.0)` | Left back (unflipped) | Upper-right back half of left log (`+125, −57` from `Character_1`) |
| `Character_4` (`3`) | `(1092.0, 583.0)` | `(1118.0, 657.0)` | Right back (`FlipX()`) | Upper-left back half of right log (`−147, −66` from `Character_2`) |

**Key geometric invariant:** the vector between front and back seats on the same log is `(+125, −57)` on the left and `(−147, −66)` on the right—a diagonal slope of **`~24.5°`** rising toward the upper center behind the campfire (`RestSiteFireLogs` center at screen `(964, 806)`). A character seated on `Character_1` must keep a compact horizontal footprint on the lower-left half of the log so `Character_3` can occupy the upper-right half of the same log without visual collision.

### 2.2 Seat Alignment Across All Four Act Backgrounds

Although each Act replaces the seat prop visually—a mossy wooden log (`Overgrowth`), a gnarled root (`Hive`), a fallen stone pillar (`Glory`), and a weathered dock timber (`Underdocks`, alternate Act 1 with `Index == 0` -> `overgrowth_loop`)—all four left-log assets share the same screen seating ridge at `Character_1` (`x = 651`) and `Character_3` (`x = 776`):

| Act background | Seat material | Left log screen rect (`1920 × 1080`) | Vertical log span at `x = 651` | Vertical log span at `x = 776` |
|---|---|---|---|---|
| `overgrowth_rest_site.tscn` | Mossy wooden log | `(545, 624)–(856, 825)` | `y = 692..792` | `y = 631..729` |
| `hive_rest_site.tscn` | Curved organic root | `(535, 621)–(853, 825)` | `y ≈ 690..795` | `y ≈ 630..735` |
| `glory_rest_site.tscn` | Fallen stone column | `(545, 622)–(855, 821)` | `y = 682..802` | `y = 629..738` |
| `underdocks_rest_site.tscn` | Weathered dock timber | `(470, 619)–(874, 852)` | `y = 684..813` | `y = 619..724` |

Across all four Acts, the top seating ridge sits right near `Character_1` origin (`y ≈ 690–725`, `rel_y ≈ −26..+9` screen px), the log's front face drops to `y ≈ 792–813`, and the floor directly in front of the log sits at `y ≈ 800–835`. `ChoicesScreen` buttons occupy `y = 255..418`, well above seated character heads (`y >= 508`).

## 3. Vanilla Seated Characters: Proportions, Perspective, and Four-Layer Lighting

### 3.1 Observed Screen Bounds and Luminance (`Character_1 = (651, 716)`)

| Character | Screen bbox (`overgrowth_loop`) | Screen size (`W × H`) | Relative to `(651, 716)` | Mean luminance (lit / `light_off`) | Seated posture observations |
|---|---|---:|---|---:|---|
| Ironclad | `(600, 598)–(746, 831)` | `147 × 234` | `x: −51..+95`, `y: −118..+115` | `0.247` / `0.237` | Bare log seat; leans slightly forward, hands clasped between knees; boots staggered along the `~24.5°` log front |
| Silent | `(576, 576)–(698, 807)` | `123 × 232` | `x: −75..+47`, `y: −140..+91` | `0.349` / `0.333` | Bare log seat; chin propped on hand, legs crossed/draped down log front, narrow cloak hanging vertically beside seat |
| Defect | `(546, 572)–(788, 830)` | `243 × 259` | `x: −105..+137`, `y: −144..+114` | `0.377` / `0.333` | Bare log seat; wide relaxed splay, feet grounded in front of log |
| Necrobinder (+ Osty) | `(492, 553)–(719, 897)` | `228 × 345` | `x: −159..+68`, `y: −163..+181` | `0.356` / `0.322` | Compact seated figure on log top (`~210 px` tall) with dangling feet; Osty rests separately on the foreground grass |
| Regent | `(589, 508)–(722, 793)` | `134 × 286` | `x: −62..+71`, `y: −208..+77` | `0.251` / `0.196` | Unique character gag: carried on a narrow floating throne perched over the log |

**Image observations common to bare-log characters:**
- **Camera elevation:** all characters are drawn from the clearing's **elevated `~35–40°` top-down three-quarter camera**, showing the upper planes of head/hat/shoulders and thighs angled forward-down over the log's front edge rather than an eye-level side profile.
- **No baked seat prop or combat weapon:** none of the bare-log characters bake a rock, cushion, or wooden log into their sprite, and none carry their combat weapon.
- **Campfire-directed gaze:** heads and eyes turn three-quarter right/down-right toward the campfire rather than making eye contact with the camera.
- **Reference background slot in authoring:** both `rest_site_defect_skel_data.tres` (`BG`, hidden at runtime) and `rest_site_regent_skel_data.tres` (`Background ref`, hidden at runtime) retain disabled background reference slots, confirming that vanilla artists authored seated characters directly over the rest-site background layout.

### 3.2 Vanilla Four-Layer Lighting Architecture

Inspecting all five Spine skeletons (`restsite_*.png` atlases and runtime slot states across `overgrowth_loop`, `hive_loop`, `glory_loop`, and track-1 `_tracks/light_off`) reveals a consistent four-layer stack:

1. **Ground & log contact shadow (`shadow` slot, drawn behind body):** soft brush-painted dark translucent shadow (`alpha ≈ 0.63–1.0` depending on texture alpha) hugging the diagonal log ridge beneath the hips and pooling beneath the boots/cloak on the floor. Works identically over wood, hive roots, and stone.
2. **Neutral-dimmed base parts + Act ambient tint:** base textures are painted at controlled mid-dark dusk values without a baked Act fire color, then lightly tinted by Act loop (observed on Ironclad and Silent body slots):
   - `overgrowth_loop`: `Color(0.89, 1.00, 0.84)`
   - `hive_loop`: `Color(1.00, 0.96, 0.79)` (Silent)
   - `glory_loop`: `Color(0.82, 0.91, 1.00)`
3. **Directional back-side shadow (`cast shadow` / `shade *` slot, drawn over body):** black (`#000000`) silhouette mask at `alpha ≈ 0.50–0.51` darkening the back-left side facing away from the campfire. **Remains active during `_tracks/light_off`** so the character stays grounded in the dark clearing after the fire goes out.
4. **Directional campfire rim highlight (`highlight` / `light *` slot, drawn over cast shadow):** white (`#FFFFFF`) rim-light mask on fire-facing front-right edges, tinted per Act and faded to `alpha = 0.0` by `_tracks/light_off`:
   - `overgrowth_loop`: `Color(0.92, 1.00, 0.32)` (`#EBFF52` yellow-green), `alpha ≈ 0.24–0.39` (higher on glossy metal/bone)
   - `hive_loop`: `Color(1.00, 0.84, 0.17)` (`#FFD62B` warm amber-gold)
   - `glory_loop`: `Color(0.00, 0.87, 0.96)` (`#00DEF5` bright cyan)

## 4. Project Judgments for Mod Rest-Site Production

1. **Author directly on the native log stage (`Inpaint → Cutout → Post-Process`):** generating a seated sprite blindly on blank transparency routinely introduces eye-level cameras, baked rocks/benches, floating feet, or oversized coats that collide with `Character_3`. Inpainting directly onto a supersampled native rest-site log stage locks camera elevation, `~24.5°` log contact, ground plane, and fire-directed gaze in one pass, then analytical background-difference matting recovers crisp anti-aliased RGBA pixels and a clean contact shadow with exact mathematical scene coordinates.
2. **Preserve the character's canonical `base.png` rendering style, apparent age, and relative stature tier:** measure the character's actual `base.png` head-to-body ratio and stature tier (rather than defaulting every character to adult height or 7-head proportions). Standard/adult builds sit in the Ironclad/Silent `~225–235 px` body-height band with boots planted well onto the clearing floor; petite/youthful builds (such as Realta, comparable to Necrobinder's compact `~200–210 px` seated body) must be painted visibly smaller (`~190–205 px` body height, plus headwear clearance) with compact limbs, preserved youthful head/face proportions, and boots resting near the lower edge of the thick log. Do not rely on post-generation Godot downscaling to shrink an adult-proportioned painting—scaling down an elongated body shrinks the head too without restoring petite/youthful anatomy.
3. **Keep base artwork Act-neutral and drive Act/fire lighting dynamically:** inpaint with a neutral warm campfire cue rather than Overgrowth's neon green glow, calibrate cloth/material hues back to `base.png`, and apply Act ambient tint, persistent back-side `cast_shadow`, and fire-toggled `rim_highlight` at runtime so one master integrates seamlessly across Overgrowth, Underdocks, Hive, Glory, `light_off`, and four-player co-op.

For the actionable production recipe and conversion steps, see [ARTS.md section 5.1.1](/ARTS.md#511-rest-site-seated-illustrations) and [UI, standing and selection route](/.agents/skills/character-asset-pipeline/ui.md#1-e-rest-site-seated-illustration-stage-inpaint--cutout--post-process).
