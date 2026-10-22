# Realta animation sources

Agent-facing authoring/runtime contract. **Native Godot scenes and Animation resources are the sole combat/merchant editing source.** Human workflow: [Godot editing guide](godot-editing.md). Decision: [ADR-0007](../../../adr/0007-native-godot-animation-authoring.md).

## Inputs and ownership

Open the shared `Sts2WD/animations/realta/combat/realta_rig_base.tscn` in the repository's MegaDot 4.5.1.m.14 editor with `dev.ps1 editor -Variant both`. Individual `combat`/`merchant` selections open their inherited scene; other scenes can be opened from the editor filesystem. Edit common joints, rest transforms and Polygon2D geometry/UV/triangles/weights in the base scene only. Six common animations live once in `clips/*.tres`: idle_loop, attack, hurt, cast, die and relaxed_loop. Both native AnimationLibrary resources reference the same objects, so a common timeline edit applies to both variants. Only RESET, revive, initial pose overrides, aliases, autoplay and next/blend mappings differ. Keep dense motion keys; do not regenerate or resample them. See the [official-workflow comparison](../../../research/godot-native-animation-workflow.md).

Saved `.tscn`, `.tres` and `parts/*.png` are packaged directly. There is no external editing project, export/import bridge, procedural animation generator or second live authoring model. Save modified external resources as well as scenes; do not make common clips unique in a variant. `anim`, `check`, `publish` and `deploy` consume saved resources without rewriting them. Rest and selection remain separate scope.

| File | Responsibility |
|---|---|
| realta_rig_base.tscn | Sole common node hierarchy, meshes, weights, textures and per-instance Slash material; default combat presentation. |
| realta_combat_rig.tscn / realta_merchant_rig.tscn | Inherited game entry scenes. Merchant retains only property differences and its library/dispatch settings. |
| clips/idle_loop.tres, attack.tres, hurt.tres, cast.tres, die.tres, relaxed_loop.tres | Shared native Animation resources; edit once for both variants. |
| clips/combat_reset.tres, combat_revive.tres, merchant_reset.tres, merchant_revive.tres | Variant-specific setup and revival resources. |
| combat_library.tres / merchant_library.tres | Dispatch names and aliases pointing to shared or variant-specific Animation resources. |
| Sts2WD/animations/realta/combat/parts/*.png | Textures; preserve pixel/UV registration. |
| realta_combat_rig.gd | Shared editor/runtime presentation and runtime-only loop phase handling. |
| tools/render_clip.gd / tools/review_frames.py | Read-only capture and review sheets. |
| parts.json | Texture rectangles, source offsets, full canvases and scale metadata. |
| death-sequence.json | Adopted cel timing specification read by protected checks; preserve it. |
| blink-patches.json | Separate rest/selection blink registration metadata. |

Coordinates are source pixels, +X right, +Y down, positive clockwise radians. Combat/merchant display at 0.625 scale. Wrapper %Visuals remains an empty Sprite2D at (0,-155), scale .625, with Rig at (-192,-288); bounds and game markers are unchanged.

Keep AnimationPlayer on automatic idle processing in saved game scenes. Inspection scripts may select manual processing on their own instances. A newer ordinary Godot save was observed to serialize `libraries/`, which MegaDot 4.5 could not load; use the configured editor and require successful `check`/package import rather than assuming cross-version serialization compatibility.

The `@tool` controller derives visibility and zero-Z order from `lower_mode`, `free_arm_back`, `free_arm_depth_hold`, `weapon_arm_front` and `weapon_gripped` in both editor and game. These are the sole composition interface; no second visibility/order array exists. Edit the corresponding discrete state keys to change attachment handoffs. Editor preview ignores outgoing-arm transition holds so direct scrubbing resolves the selected pose. Runtime blending retains those holds and discrete-state correction. Random loop entry and restart phase restoration run only during playback, never editor scrubbing. Stable child order does not trigger repeated moves. See [Godot 4.5 tool-script guidance](https://docs.godotengine.org/en/4.5/tutorials/plugins/running_code_in_the_editor.html).

Properties that had identical values across every key of all seven canonical clips now live once on their Polygon2D/attachment nodes: polygon, UV, triangle lists, bone weights and invariant textures. The common played value was retained as the node default before removing its redundant tracks. Only head/grip texture swaps vary and remain keyed; original motion tracks and their dense key times are unchanged. New shape-changing animation may add explicit geometry tracks when actually needed, with defaults in every clip that can interrupt it.

Each variant has a nonlooping, editor-only `RESET` animation and `reset_on_save=true`. RESET contains the variant's idle time-zero values for all animated properties; static geometry/weights remain outside animation ownership. When intentionally changing an animated neutral/setup value, update its RESET key too. On 2026-10-22 the user explicitly authorized adding RESET and changing the exact-name check to allow the original ten dispatch names plus RESET; all other acceptance requirements remain intact. Do not call RESET from gameplay or change the existing autoplay/next mappings.

Seven canonical clips remain authoritative: idle_loop, attack, hurt, cast, die, relaxed_loop and revive. Preserve aliases sharing Animation resources, next/blend/autoplay mappings, node paths and the per-instance Slash ShaderMaterial (`resource_local_to_scene = true`). Run the existing [instance isolation test](../../../../tests/README.md) after runtime changes. The explicitly authorized name-list assertion now permits RESET; other acceptance requirements are unchanged.

## Clips and dispatch

BaseLib 3.4.7 discovers the AnimationPlayer below the visuals. Aliases share resources within each variant.

| Clip | Length | Current contract |
|---|---:|---|
| idle_loop, idle | 2.6 s | Quiet breathing, planted feet, independent grip and blink. Merchant idle aliases relaxed_loop instead. |
| attack | 1.05 s | Clockwise chop; horizontal strike at exactly .15 s; shaft half-turn and recovery. Primary keys at 240 Hz. |
| hurt | .8 s | Short torso/head recoil; hip drops at most 4 source px, slight forward knee give. |
| cast, powerup | 1.3 s | Free-hand thrust; sigil release at exactly .25 s. |
| die | 2.2 s | Forward/together knee buckle; articulated boundary 7/24 s; canonical final at 28/24 s; settle by 1.4 s; holds. |
| relaxed_loop, relaxed | 8 s | Narrow shop stance, viewer-left support, low grip and free arm behind the hip and every body layer; two breaths. |
| revive | .4 s | Fade death out to .2 s; reset while hidden; fade the variant's idle back in. |

Other primary clips retain 60 Hz keys. Attack/hurt/cast/powerup/revive return to the variant's idle. Realta.DeathAnimTime remains 1.4f. Native playback and package loading are separate from game lifecycle/multiplayer verification.

The controller chooses a random loop phase once per rig instance. It saves the last displayed loop position each frame. Stop() then Play() of the same Animation resource, including aliases, restores that saved phase through a serial-guarded deferred seek. Switching resources or returning from an action uses the authored start; it does not randomize again.

## Completed parts and overlap ownership

New hidden surfaces are painted image-tool output registered to source coordinates. Original retained visible RGBA wins over generated additions; foreign pixels and disconnected generation residue are removed by alpha only. Repartitioning the original coat changes occlusion ownership intentionally. This is not a claim that the corrected setup render is byte-identical to the former rig.

| Part family | Completion / ownership |
|---|---|
| coat / coat-front | Generated blue back lining behind legs; original visible blue/fur partitioned along the actual inner diagonal fur boundaries. Front panels draw over every leg part. Rounded waist completion tucks under the belt. No rectangular back-hem fragments on the front layer. |
| torso / neck-overlap / head | Torso owns armor and continuous collar/underarm backing. A generated neck overlap follows head weights but draws behind torso. Existing head/expression paintings remain the visible face/hat. |
| upper_arm_l/r | Blue sleeves have hidden shoulder/elbow ends. New shoulder vertices follow the upper arm. Sleeves normally draw behind chest/pauldrons; attack moves the existing left sleeve in front of the chest from 1/12 to .75 s. Its metal cap keeps torso weights. |
| forearm_l/r / wrist-overlap | Each forearm owns its single ivory cuff and bronze vambrace. Left wrist has a generated dark plug behind the forearm; right cuff/glove already forms a complete object. In relaxed_loop, upper_arm_r and forearm_r move together before coat; there is no separate right-glove or right-overlap mesh to leave in front. |
| hand_l / hand-l-turned | Empty original fist outside attack; generated turned grip from 1/12 to 2/3 s of attack. Both share the same source rectangle/UVs. No shaft pixels in either hand. |
| thigh_l/r | Generated rounded hip/knee remainders under coat and greaves. Distal overlap weights continue into the shin. |
| shin_l/r / foot_l/r | Only dark knee/ankle backing is added to greaves; existing metal is retained once. Rounded boot plugs draw beneath shins. |
| weapon-rigid / shaft-core | Continuous registered shaft repair; edge-on core uses source shaft pixels. No boot-outline residue. |
| ribbon | Existing red tails, generated rounded proximal cloth overlap, no attached wooden strip. |
| pauldron-r | Extracted original metal follows torso only; upper_arm_r owns blue cloth. |

Original mesh cells/weights are retained. Added 8px cells cover generated alpha; new weights use neighboring anatomical influences, with upper-arm extensions explicitly on the corresponding upper arm. Coat hem influence increases from zero at source Y325 to .85 at Y450; waist remains pinned. Neck/wrist overlap meshes inherit their respective head/hand weight fields. Do not regenerate complete meshes for timing edits.

Keep at least two transparent pixels around every skinned texture. Out-of-image UVs otherwise clamp opaque boundary colors into streaks. Whenever PNG bounds/padding change, update parts.json offset and size so source coordinates remain fixed. Texture-swap families must have identical rectangles, including padding. Sprite-only cels also keep explicit source offsets after transparent-padding trims.

## Attack, grip and trail

The material grip is source (131,278). Authoring fits the shaft center from source rows 220..380; the current registered shaft is approximately X131.262 at grip height, rest-axis angle -.148769 rad relative to local Y. Recompute from runtime art rather than copying stale measurements.

Weapon hierarchy (Skeleton2D occupies the frontmost body slot; bones/paths remain unchanged):

    weapon bone
      Weapon                  # held-floor support or released transform
        ShaftCore             # round shaft remains visible during the turn
        ShaftAxis
          WeaponTurn          # scale.x: 1 -> .035 -> -1, then back
            Ink               # exact inverse setup-axis registration
              Blade
        Ribbon                # knot follows reflection, cloth orientation independent

Twist spans .05..10 s and .64..70 s; edge-on frames are exactly 1/12 and 2/3 s. The turned grip swaps on those real edge-on display frames. Wrist sharing is an explicit smooth curve from -.08 to +.10 rad, returning to zero. Never feed an unwrapped elbow angle into a saturating wrist correction. Weapon rotation owns the remaining shaft angle.

Attack authors a continuous signed elbow bend B (forearm heading minus upper-arm heading, positive clockwise). The setup side is positive. At 11/60 s (.183333 s), after the .15 s contact, B passes through zero and becomes negative through follow-through and early recovery. At .65 s it passes through zero again, returning to the setup side before the broom is planted upright. The returning hand has passed beneath the shoulder at this second crossing; retaining the opposite branch into setup would require an abrupt final correction. The contact elbow remains 160.85 degrees open (180 is straight). The free hand follows a separate IK target toward the belt.

For segment lengths a and b, reach is sqrt(a²+b²+2ab cos B). It reaches exactly a+b = 87.77505 source px at both switches, with a quadratic maximum instead of a distance-curve cusp. The analytic IK shoulder offset is atan2(b sin B, a+b cos B), so both branches coincide at zero without the .01 px extension clamp used by the other planted chains. The signed bend and heading use smooth Hermite curves, baked at 240 Hz; the switch times are also exact 60 Hz samples. Floor clearance adjusts heading along the reach circle, retaining the signed bend. Recovery shaft angles keep the full broom clear through the second extension and the .64..70 s axial turn.

The hand slides continuously along the fitted shaft by at most 14 source pixels toward the blade, holding that material grip through .28 s and returning by .85 s. Weapon-bone translation is the inverse rotated material offset, so the shaft remains in the fist. At .15 s the hand is (324,277.9017), while the original weapon origin remains exactly (310,280) at pi/2: the blade tip and slash event are preserved. Turn the shaft early enough to clear the broom before the hand descends; a late turn forces floor support to distort the hand arc and produces a short elbow-speed spike.

The discrete weapon_arm_front track changes the existing sleeve's sibling order only during the crossing interval. The controller keeps every z_index at zero; all clips and hidden revive resets default the property to false. Sleeve depth is independent of joint rotation and does not change the frozen hierarchy, artwork, weights, or other clips' poses.

Raise the actual grip/arm during wind-up when the wide broom needs clearance; moving only the prop by a large amount breaks contact. The .15 s event pose clears without that lift. Dense prop support retains only subpixel attack adjustment.

Slash reuses the unchanged cyan/ivory painting on a Polygon2D strip baked from the actual forward cutting path at 240 Hz (0..0.25 s). The moving weapon origin, held floor support and both signs of the shaft-axis reflection are included. Each of 61 sections follows nine alpha-contour points on the bright blade edge (source rows 36..108); its outer point is the same alpha-derived tip used by FK. Strip thickness tapers toward the start by travelled-distance fraction raised to 1.35. This leaves the face readable while the leading part remains broad at the current blade.

The tip follows a moving grip and an axially reflected prop. Register the sweep strip to that sampled path; a fixed-center circular sprite cannot represent the whole swing.

`slash_reveal.gdshader` maps longitudinal UV to the existing painting's angular coordinate and transverse UV to its measured radial limits. The 128 radial sections come from alpha >24, padded and smoothed across five angular bins; this excludes isolated near-transparent specks from the fit without editing the PNG. The painting is unfolded onto the measured path rather than uniformly enlarged. Reveal is keyed to cumulative tip distance. Future sections have zero alpha; its 9 source-pixel front feather stays behind the current sampled edge. This causal reveal replaces the old global velocity half-plane, which incorrectly erased already-travelled sections as the tip slowed and curved. The resulting historical trail remains in rig space during recovery, rather than moving in front of the returning blade.

Opacity rises from zero through .025/.075 s, peaks at the unchanged .15 s contact, holds .85 at .25 s, and reaches zero at .34 s. Both transverse borders are feathered. Every clip and hidden revive reset owns reveal=0 and opacity=0 through the standard defaults. Slash remains above all body/prop nodes in tree order with zero Z/no top_level; no body poses, raster assets, skinning or other VFX change. Ribbon knot registration still follows axial reflection while the tails remain gravity-facing with bounded lag.

Review the trail against the measured blade/tip path using the [motion review matrix](../../../agents/cutout-motion-review.md). Capture event-time poses as well as normal-speed playback; retain complete native frames for comparison.

## Floor support and secondary motion

Floor is source Y527. Geometry is the convex support hull of all nonzero-alpha pixel EDGES, including faint antialiasing, rather than opaque pixel centers. The complete reflected prop is checked, including the blade.

Held prop support is baked through interpolated bone FK at 960 Hz for attack and 240 Hz for other held clips. It translates Weapon just enough to retain .30 source px clearance. Loops have matching endpoints. Attack's primary grip lift keeps the remaining prop displacement at .30 px; other clips have small cylindrical shaft sliding inside the grip. No procedural runtime painting or simulation is used.

Hat-tip, coat-hem and ribbon springs integrate at 240 Hz. Loops warm for twelve cycles and share exact endpoint values; action tails return to the idle equilibrium. Native AnimationPlayer keys own every pose, overlay and reset.

## Death continuity

Hurt hip excursion is 4px rather than the former 23px straddle. Both death knees turn toward battle-forward (+X), with ankles drawing from (209,466)/(290,466) toward (225,466)/(273,466). Knee branches transition smoothly out of setup. Torso and coat stay attached to the same hip until full-body cels take over.

At 7/24 s lower_mode changes from articulated body (0) to full-body sequence (4). death-seq-start.png is an exact native source-scale capture of the revised supported buckle, without weapon. Recapture it whenever that boundary pose/art changes; do not ask the image tool to redraw the rig endpoint.

Sequence: captured start, death-buckle-00..02, death-seq-04..08, then death-settle-00..02. The first nine cels last 1/12 s each; the last three last 1/24 s each. Incoming cels cover opaque outgoing cels with at most 1/60 s overlap. Whole-painting registration interpolates head/boot landmarks during the final settling drawings.

At 28/24 s lower_mode becomes 3 and the original unchanged death-full.png takes over. Its 460x230 source canvas stays at (82,322); settle ends at 1.4 s. Keep this final painting unchanged.

Broom release is .10 s. Dense 240 Hz world poses retain full-alpha floor support before AND after release, convert through the weapon bone inverse and topple under gravity in front of the body. Final orientation balances blade/bristle contacts; no bounce or later rise. It rests by .8 s. The existing resting placement keeps the blade below the face and the shaft along the foreground floor; the final body painting and prop world trajectory are unchanged. Ribbon follows the released prop. A smooth .22..70 s trailing rotation carries the tails over the brim rather than across the eyes during .42..50 s; the knot remains attached. It compresses as it reaches the floor and has an exactly constant final transform.

Relaxed-to-die blend remains .16 s. Combat idle transitions use .24 s, and idle-to-relaxed uses .30 s, including merchant -> idle_loop. Relaxed-to-attack/hurt/cast (powerup alias) blend for .03/.08/.12 s, finishing before their free-arm slot handoffs. Action-to-relaxed returns blend for .30 s. All relaxed aliases, including merchant idle, receive these mappings. Explicit caller overrides can change these mappings and require a new overlap review.

Revive holds final body/prop state during fade-out, resets all cels/reflection/hand texture/secondary tracks at hidden .2 s and restores the variant's authored support pose. Add new animated properties to defaults and hidden reset handling.

## Rebuild and inspect

Follow [the animation workflow](reproduction.md) for `dev.ps1 anim`, `check`, package rendering and comparison sheets. `anim` renders the saved hand-edited scenes without calling the reference generators.

All CanvasItems keep z_index=0 and use tree order; no top_level. In both variants, the entire weapon (shaft core, blade/bristles and ribbon) draws above every body surface, including head/hat/face, torso, both arms, coat-front, legs and all death cels. The saved scenes place weapon and hand_l last in the articulated order, with the Skeleton2D slot and hand_l after all death cels and before Slash/CastFx. This retains the bone hierarchy, animation paths and skin weights without a transform proxy.

The only body exception is the gripping glove `hand_l` (including its `hand-l-turned` texture): it covers the held shaft. The discrete `weapon_gripped` property defaults true in every held clip, becomes false at die .10 s and moves the released glove immediately below Skeleton2D. Revive keeps it false during death fade-out and resets it true at hidden .20 s. VFX draw above the prop and glove. Attack's separate sleeve-depth change remains below the prop at all times. Standalone native inspection, package loading and actual game verification are different evidence.

## Free-arm state and transitions

In relaxed_loop and its relaxed/merchant idle aliases, the complete screen-right free arm is the **backmost body layer**: upper_arm_r, then forearm_r, then coat, legs, coat-front and all remaining body parts. forearm_r owns the cuff, bronze vambrace and glove together; pauldron-r remains torso-owned armor. The same two Polygon2D nodes change sibling slots, retaining their original skin weights, skeleton paths, UVs and textures. There are no duplicates, nonzero Z values or top_level nodes. Other clips use the original slot immediately after coat-front. The separate weapon sleeve uses its own anchors; it cannot displace this slot.

The relaxed wrist target is hip + rotated (39,-28), ten source pixels outward from the former pose. This leaves a small glove edge and the connected forearm visible beside the hip while the coat covers the hand. Shoulder/elbow lengths, low weapon grip, planted feet, eight-second motion and expressions are retained.

Every clip resets free_arm_back (default false, relaxed true) and free_arm_depth_hold (default false). Incoming idle_loop/attack/hurt/cast/die hold the outgoing actual slot until .25 s / 1/30 s / .10 s / .15 s / 11/60 s respectively. A normal combat entry therefore remains in front; a shop entry stays behind until the arm clears the coat. Die has a brief -.38 rad outward upper-arm balance offset, rising over 0..0.10 s, holding to .18 s and returning to zero at .27 s, before the unchanged 7/24 s captured-buckle boundary. Its weapon trajectory and death paintings are unchanged. Revive explicitly selects the variant's slot at hidden .20 s.

The controller resolves discrete value tracks from the current AnimationPlayer resource/time on mixer_applied, after the live blend. This is intentional: native observation found outgoing discrete time-zero keys overwriting incoming values during blending and leaving stale slot state, expressions and grip release. The current clip owns those discrete states; continuous bone/prop interpolation remains native. Revive resets both bone and prop transforms discretely on its hidden .20 s key. The key lookup allows one microsecond for float32 key storage rounding at exact display times. Godot 4.5 documents the relevant [AnimationMixer signal and discrete blending behavior](https://docs.godotengine.org/en/4.5/classes/class_animationmixer.html). Do not replace this with a clip-name timer or move the arm globally.

## Verification boundary

The [mechanical rig checks](../../../../tests/rig-checks.md) cover structure, dispatch, defaults, sampled floor clearance and death settlement; the [instance isolation test](../../../../tests/README.md) covers two-character effect ownership. Use the [motion review matrix](../../../agents/cutout-motion-review.md) for spatial and painted-surface judgments. Native capture, package loading and actual game/multiplayer playback are distinct evidence.

Small painted fold/contour differences remain between death cels. Corrected part ownership intentionally changes some original setup pixels. Consult [iteration notes](iteration-notes.md) only for previous choices, measurements and delivery reports.
