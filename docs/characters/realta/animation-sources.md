# Realta combat, merchant, and select animation sources

The combat and merchant scenes share the articulated Spine rig cut from the `1152×1728` [base painting](base.png), while the character-select scene layers a localized Spine overlay rig over [char_select_bg.png](/Sts2WD/images/charui/realta/char_select_bg.png). Both rigs are authored and verified with the repository's locked `rig-c` Spine 4.3 compiler (`A00`–`A53` static assertions plus 24 fps `rigc render --folds` mesh-fold gates), then converted by [export_spine42.py](/scripts/export_spine42.py) into native Spine `4.2.43` `.spjson`, `.spatlas`, and 4px-aligned `.png` atlases for `libspine_godot` `SpineSprite` ([ADR-0008](/docs/adr/0008-native-spine-rigs-and-artifact-refresh.md)).

Paired bone, slot, and attachment suffixes (`_r`/`_l`) follow the character's anatomical perspective (`_r` = right/near side, `_l` = left/far side; see [paired limb and part naming](/docs/conventions/characters/naming.md#paired-limb-and-part-naming)), while broom ribbons use `ribbon_long` and `ribbon_short`. See the [scene mapping](animation-assets.md#current-scene-and-property-connections).

## Inputs and ownership

| Source | Owns |
|---|---|
| [layout.json](animation/rigc/layout.json), [atlas.txt](animation/rigc/atlas.txt), and [realta_skeleton.png](/Sts2WD/animations/realta/combat/realta_skeleton.png) | Source canvas, registered bone pivots, part rectangles, state names, texture resolution, and final part pixels for the combat/merchant rig. [unpack_atlas.py](animation/rigc/authoring/unpack_atlas.py) unpacks all 58 part PNGs losslessly from `realta_skeleton.png` during builds. Keep `textureScale: 3` on the eyes and mouth. |
| [author_rig.py](animation/rigc/authoring/author_rig.py) | Bone hierarchy (39 bones, 41 slots), setup slots, regions, weighted meshes, separate blade/shaft registration, spell/ward sigil layers (`sigil_outer`, `sigil_inner`, `sigil_core`), thrust FX (`thrust_streak`, `thrust_flash`), and per-clip attachment choices. |
| [author_motion.py](animation/rigc/authoring/author_motion.py) | The eight combat/merchant Spine clips (`idle_loop`, `blink_loop`, `attack`, `guard`, `hurt`, `cast`, `die`, `relaxed_loop`), IK targets, hand switches, per-clip draw order, face states, and Spine event cues. [Kinematics](animation/rigc/authoring/kinematics.py) and [sampling](animation/rigc/authoring/sampling.py) resolve poses to linear transform keys. |
| [garment.py](animation/rigc/authoring/garment.py) and [wrist_seams.py](animation/rigc/authoring/wrist_seams.py) | Shared weights and supports for the skirt, and distal bracer supports used by the two-handed attack and diagonal guard brace. |
| [author_select.py](animation/rigc/authoring/author_select.py), [select_atlas.txt](animation/rigc/select_atlas.txt), and [realta_select_atlas.png](/Sts2WD/animations/realta/select/realta_select_atlas.png) | Minimal character-select overlay parts (`ribbon_patch`, compact bevel-only `blade_glow`, `blink_patch`, `mote_0`..`mote_4`) and single `8.00 s` `"animation"` loop packed into a `512×512` Tier 2 atlas. |
| [build.ps1](animation/rigc/build.ps1), [export_spine42.py](/scripts/export_spine42.py), and [import_game_rig.py](/scripts/realta/import_game_rig.py) | Compile both `rig-c` packages, downgrade Spine 4.3 JSON/atlas outputs to Spine `4.2.43` `.spjson`/`.spatlas`/`.png`, and synchronize [atlas.txt](animation/rigc/atlas.txt) and [select_atlas.txt](animation/rigc/select_atlas.txt). |
| [spine_rig_mount.gd](/Sts2WD/scenes/shared/spine_rig_mount.gd) | Caches `SpineSkeletonDataResource`, mounts `SpineSprite` in `combat.tscn`, `merchant.tscn`, and `char_select_bg_realta.tscn`, manages Track 1 `blink_loop` overlay during combat idle, and fits the select rig across aspect ratios. |

A rebuild derives all runtime `.spjson`, `.spatlas`, and `.png` outputs deterministically from these inputs; see [Rebuild and package playback](animation/reproduction.md).

## Clips and dispatch

| Rig | Clip | Duration | Authored action |
|---|---|---:|---|
| Combat / merchant | `idle_loop` | 2.60 s | Upright breathing loop on Track 0 (eyes stay `neutral` on Track 0 while `blink_loop` runs independently on Track 1). |
| Combat / merchant | `blink_loop` | 3.60 s | Track 1 eye-only blink overlay (`eye_r`/`eye_l` switch to `closed` at `1.84`–`1.975 s`, asynchronous with the `2.60 s` breath cycle). Cleared automatically during non-idle clips. |
| Combat / merchant | `attack` | 1.05 s | Lower the glaive, catch the shaft with the supporting hand, coil back, and lunge forward with `thrust_streak` and additive `thrust_flash`. Emits `attack_hit` at `0.54 s`. |
| Combat / merchant | `guard` | 0.72 s | Sink into a braced stance, angle the broom-glaive diagonally across the torso, raise `hand_l-casting` to project an arcane ward (`sigil_inner` + `sigil_core`), and emit `guard_brace` at `0.18 s` before recovering. |
| Combat / merchant | `hurt` | 0.80 s | Torso and pelvis recoil, knees yield, and the `hurt` expression appears. |
| Combat / merchant | `cast` | 1.30 s | Gather low and extend the anatomical left arm (`hand_l-casting`) forward; emits three-layer sigil on `cast_release` at `0.25 s`. |
| Combat / merchant | `relaxed_loop` | 8.00 s | Crouching merchant loop propping the chin on `hand_r-back` while the broom-glaive hovers beside the body. |
| Combat / merchant | `die` | 2.20 s | Releases the weapon on `weapon_release` at `0.12 s`, loses the hat, and collapses forward. Core body (`pelvis`, `chest`), cheek, and `hand_r-ground` settle on the floor by `1.66 s`; hair and clothing finish settling through `2.20 s`. |
| Character select | `animation` | 8.00 s | Single seamless loop for `NSpineAutoPlayer`: gentle ribbon sway, pulsing blade-bevel shimmer, blink at `3.10`–`3.19 s`, and five staggered rising arcane motes. |

Runtime `CreatureAnimator` triggers, defensive `Skill` `"Guard"` routing, and timing constants live in [Spine dispatch](animation-api.md), [Realta.cs](/Sts2WDCode/Characters/Realta/Realta.cs), and [CardPlayAnimationPatch.cs](/Sts2WDCode/Cards/CardPlayAnimationPatch.cs).

## Completed parts and overlap ownership

- **Body and arms.** `pauldron_r`, `upper_arm_r`, `forearm_r`, and `hand_r` form the anatomical right arm (near/camera side, holding the broom-glaive). The shoulder and elbow pass in front of the breastplate during `attack` and `guard`. The opposite anatomical left arm (`pauldron_l`, `upper_arm_l`, `forearm_l`, `hand_l`) stays behind the torso in `idle_loop`, switches to the front grip (reusing `hand_r.png`) during `attack` (`0.31`–`0.695 s`), and switches to `hand_l-casting` during `cast` and `guard`. Distal bracer meshes (`cuff_r`, `cuff_l`) keep wrist seams tucked inside the gloves.
- **Weapon.** At `0.145 s` of `attack`, the one-piece `weapon` switches to the shaft plus a rigidly mirrored blade/socket (`weapon_head`) registered to the shaft axis, returning to the default orientation at `0.815 s`. During `guard`, the one-piece polearm pivots into a diagonal parry while ribbons trail the brace impulse. Released during `die`, the broom lies horizontally with bristles to the left while the hat settles ahead of the face.
- **Skirt and legs.** `coat_back` owns the blue lining behind both legs. The cream-trimmed front panels (`coat_r`, `coat_l` with `skirt_hem_r`, `skirt_hem_l`) cover the legs (`thigh_r`/`shin_r`/`foot_r`/`boot_r`, `thigh_l`/`shin_l`/`foot_l`/`boot_l`) on either side of the slit and share waist/side-seam weights so advancing knees never fold or tear triangles.
- **Face.** `head` (with `hair_r`, `hair_l`) owns the cheek, nose, and hairline. `eye_r` and `eye_l` own their corresponding eyebrow, eyelid, and lashes; `mouth` owns the mouth across six states (`neutral`, `closed`, `hurt`, `attack`, `ponder`, `fallen`) at `3×` texel density. [Expression mapping](animation-verification.md#combat-expressions)
