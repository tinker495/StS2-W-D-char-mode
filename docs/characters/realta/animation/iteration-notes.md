# Realta animation iteration notes

Agent-facing historical observations. Read this file when investigating a past decision or verification run. The [current contract](README.md), [repeatable workflow](reproduction.md), and tracked tests define current work; dates below describe the scope of their original runs. Local captures mentioned in historical reports are optional evidence, not execution prerequisites.

## Instance isolation follow-up (2026-10-20)

The 2026-10-20 check on MegaDot 4.5.1.m.14 failed all four cases before this ownership setting: another rig's idle changed the attacking rig's reveal from about .941 to 0. After rebaking, all four cases retained their before/after value and passed. The rebake added only the material ownership flag to each scene. Publishing exited 0 after one sandbox-access retry, with certificate settings preserved. A separate host containing only an observation script and an empty project mounted the PCK and observed distinct materials and unchanged attack reveal in both variants; test resources were absent. These results do not establish in-game or multiplayer behavior.

### Merchant free arm order (2026-10-20)

In relaxed_loop and its relaxed/merchant idle aliases, the complete screen-right free arm is the **backmost body layer**: upper_arm_r, then forearm_r, then coat, legs, coat-front and all remaining body parts. forearm_r owns the cuff, bronze vambrace and glove together; pauldron-r remains torso-owned armor. The same two Polygon2D nodes change sibling slots, retaining their original skin weights, skeleton paths, UVs and textures. There are no duplicates, nonzero Z values or top_level nodes. Other clips use the original slot immediately after coat-front. The separate weapon sleeve uses its own anchors; it cannot displace this slot.

The relaxed wrist target is hip + rotated (39,-28), ten source pixels outward from the former pose. This leaves a small glove edge and the connected forearm visible beside the hip while the coat covers the hand. Shoulder/elbow lengths, low weapon grip, planted feet, eight-second motion and expressions are retained.

Every clip resets free_arm_back (default false, relaxed true) and free_arm_depth_hold (default false). Incoming idle_loop/attack/hurt/cast/die hold the outgoing actual slot until .25 s / 1/30 s / .10 s / .15 s / 11/60 s respectively. A normal combat entry therefore remains in front; a shop entry stays behind until the arm clears the coat. Die has a brief -.38 rad outward upper-arm balance offset, rising over 0..0.10 s, holding to .18 s and returning to zero at .27 s, before the unchanged 7/24 s captured-buckle boundary. Its weapon trajectory and death paintings are unchanged. Revive explicitly selects the variant's slot at hidden .20 s.

The controller resolves discrete value tracks from the current AnimationPlayer resource/time on mixer_applied, after the live blend. This is intentional: native observation found outgoing discrete time-zero keys overwriting incoming values during blending and leaving stale slot state, expressions and grip release. The current clip owns those discrete states; continuous bone/prop interpolation remains native. Revive's discrete bone reset likewise stays on its hidden .20 s key. The key lookup allows one microsecond for float32 key storage rounding at exact display times. Godot 4.5 documents the relevant [AnimationMixer signal and discrete blending behavior](https://docs.godotengine.org/en/4.5/classes/class_animationmixer.html). Do not replace this with a clip-name timer or move the arm globally.

Rebuild all seven clips with the procedure above, then review relaxed_loop at 1x and enlarged shoulder/cuff/hip views. Exercise live relaxed phases 0, 3.375 and 6.25 through idle_loop, attack, hurt, cast/powerup and die in both variants, including aliases and automatic action returns. Compare identical poses with each slot around the authored handoffs; inspect the death cel boundary and hidden revive reset separately.

The final seven rebuilds completed with exit 0 (396 native frames). Multiple native 1x/zoom passes covered the relaxed arm and transitions. Ninety live source/target/phase cases across both variants and all relaxed aliases confirmed the slot handoffs, plus automatic action returns and hidden revive reset. Idle/attack/hurt/cast/revive isolated native frames remain identical to the prior output; relaxed and the initial free-arm death balance are the intended pose changes. Same-pose slot comparisons show no visible overlap change at the handoffs (idle has only two one-channel-level antialias differences). `dev.ps1 publish` and a fresh separate package host both exited 0; the PCK loaded both variants/all ten names, with zero Z/no top_level and the required held/released weapon order. Runtime PNGs, meshes and protected rest/select inputs are unchanged. Formal tests and actual game/multiplayer playback were not run; no installation or commit occurred. Existing NU1900 metadata warnings remain, and temporary editor settings were restored.

### Weapon/head draw order (2026-10-20)

Baseline and two revised passes each rebuilt/rendered all seven clips (396 native 640x420 frames per pass, 24 fps, scale .625). Native 1x and 2x overlap views covered every clip, with the attack head crossings, every attack grip frame, the death fall/settle and revive reset examined closely. First-pass death ribbon occlusion of the eyes motivated the second-pass trailing rotation. The released glove also loses its foreground exception at release. Idle/hurt/cast/relaxed retain their poses; attack retains its signed elbow motion, grip, shaft turn and .15 s event. No runtime artwork or meshes changed.

The final seven rebuilds exited 0 and reproduced the second-pass pixels. `dev.ps1 publish` exited 0; a fresh package host loaded both variants/all ten playback names and observed held/released order, die .10 s release, revive .20 s reset and recursive zero Z/no top_level. All twelve protected rest/select files retained their task-start hashes. Local editor settings were restored after publishing. Formal tests and actual game/multiplayer playback were not run; no installation or commit was performed.

### Attack elbow flip observations (2026-10-20)

The previous fixed-side elbow was rejected. Baseline plus three rendered revisions covered all 26 display frames at 1x and 3x, with native 60/240 Hz angle observations and exact switch-neighbor images. Both switches reach a 180-degree interior; signed bend ranges from -40.00 to +56.01 degrees, wrist from -4.58 to +5.73 degrees. At 240 Hz the samples bordering the first switch are B=+1.765/0/-1.695 degrees, and those bordering the return are -0.988/0/+0.990 degrees. The largest elbow-bend step in the whole clip is 2.824 degrees. Existing sleeve/cuff overlaps and torso-weighted pauldron remain connected on both sides; no mesh or draw-order changes were required.

Native 960 Hz full-alpha support retained .2999 source-pixel minimum floor clearance. Hand-to-shaft-axis distance was at most .3033 source pixels, including the fitted setup-axis offset. Native slash origin differed from its authored tip registration by less than .0001 source pixels. These are descriptive observations, not acceptance thresholds.

The exact .15 s weapon transform and slash lead, both blade-turn windows, and grip texture changes are preserved; non-attack source clips are unchanged. Formal tests and actual game/multiplayer playback were not run.

All seven final rebuild/render runs exited 0 (396 display frames), followed by `dev.ps1 publish` exit 0. A separate package host loaded both variants/all ten playback names and observed both straight crossings and the opposite-side follow-through. Rest/selection hashes and mesh bytes match task start. Local editor certificate settings were restored; existing NU1900 metadata warnings remain. No installation or commit was performed.

### Slash span delivery verification (2026-10-20)

Five revised native attack renders covered the blade flip, contact, follow-through and fade at 1x/2x, with the unchanged recovery inspected per frame. The final seven rebuilds exited 0 (396 frames); attack equals the selected render and all other clips are pixel-identical to the previous output. `dev.ps1 publish` and a fresh isolated PCK host exited 0. Both variants/all ten playback names load, retain the required weapon/VFX tree order, and have zero Z/no top_level. Runtime PNGs, body meshes, primary motion and rest/select files are unchanged. No formal tests, installation, commits or actual game/multiplayer verification were performed. Existing NU1900 metadata warnings remain; temporary local editor certificate settings were restored.

## Current verification boundaries

The completed combat refinement includes whole-part completion, two or more visual passes per affected item, 24fps GIFs, enlarged part comparisons and official hurt/die comparisons. Native 960Hz observations cover the full weapon alpha edges in all six authored clips, ten same-resource loop restarts and relaxed-to-die transitions at three source phases. These are descriptive observations, not formal tests or acceptance thresholds.

Runtime PNGs, manifests, meshes and authoring inputs are sufficient to rebuild without drafts. [The reproduction procedure](reproduction.md) supplies the capture instructions and temporary example; historical image-tool outputs, prompts and comparisons are not build or inspection prerequisites.

Formal tests were not added, modified or run. No game installation or in-game/multiplayer observation is implied. Small painted fold/contour changes remain between death cels. Corrected layer ownership and alpha hygiene intentionally alter some former setup pixels; strict byte-identical setup rendering has not been established.


### Final package observation (2026-10-19–20)

All seven final author/import/bake/native-render iterations exited 0, producing 396 source frames at 24 fps. The requested dev.ps1 publish exited 0 to artifacts/mods/Sts2WD; the temporary local editor CA setting was restored byte-for-byte. NU1900 network-metadata warnings remained, with zero C# compilation errors.

A fresh package host mounted the PCK and loaded both variants, all ten playback names and every new overlap/grip/settling texture. Action completion, death hold, hidden revive reset and zero Z/top_level were observed. Both visual and headless observation hosts exited 0. The visual host still reported sandbox shader-cache write restrictions; no resource or GDScript failures were found.

Packaged 960 Hz full-alpha-edge observations stayed above source floor527 in all six authored clips: maximum Y526.7065, with merchant Y525.0001. Ten same-resource loop restarts preserved phase. Three relaxed-to-die source phases had zero initial hip jump and maximum prop Y526.7018. Settled ribbon transforms at1.8/2.19s were identical. These observations do not prove gameplay or multiplayer behavior.

All twelve protected rest/selection files and the approved final death painting retained their starting hashes. Active skinned PNG dimensions/offsets fit their source canvases; eighteen meshes have2774 vertices with normalized weights. Current part production records include the final proximal registration refinement; runtime build inputs do not depend on those records.

## Slash path measurements (2026-10-20)

The tip travels 536.650 source px during that interval, through a weapon-relative span of 89.250 degrees at radius 242.441..242.909 px. The center is moving: it starts at (131,277.7) and ends at (313.114,283.050), with bounds (118.489,270.628)..(315.280,333.431). A fixed-circle fit has center (521.139,-157.617), radius 395.534 and RMS error 22.656 px, so a circular sprite cannot register the whole swing. Full 1.05 s observation includes recovery and both blade turns: 253 samples, 1053.623 px traveled, returning to the initial tip.

Follow [Rebuild and inspect](reproduction.md), selecting `attack` for a short capture. Inspect all 26 display frames at native .625 scale and enlarged, then capture all seven clips and publish. Compare official Ironclad/Silent at the same absolute times: their trails retain a large fraction of the completed swing rather than a small tip flash. Native 240 Hz FK observation differs from the authored tip by at most .000188 source px. At .125/.166667/.208333 s, a descriptive 4-display-pixel neighborhood measurement of the travelled tip path increases from 8.4/26.6/23.4% to 99.5/97.7/97.7%. The measurement uses RGB difference >8 against same-pose renders with Slash hidden; it is not an acceptance threshold or a formal test.

## Mechanical guard follow-up (2026-10-20)

The user requested mechanical rig checks and a tracked author/import/bake/render/review workflow. The new ground-clearance check exposed a revive defect at .199975 s: prop offsets interpolated toward idle before the prone bones reset at .20 s, putting the blade below source floor527. The baker now resets weapon/ribbon transforms discretely with the bones at the hidden key. The same check, including the existing subframe sample, then passed for both variants. The source run covered 41,864 native samples and the four unchanged instance-isolation cases. The tracked animation command produced all 26 attack frames for each variant and complete-frame dark/light sheets and GIFs. These are tool and mechanical observations, not new human approval of visual quality.

The guarded `dev.ps1 publish` ran both suites before export and exited 0 after the documented sandbox-access retry, without certificate-setting changes. `dev.ps1 anim all -Variant both -PackagePath <published PCK>` used a host containing only project.godot and the tracked render script: seven clips / 396 frames per variant, 792 total, with GIFs and full-frame dark/light sheets. A separate packaged attack run used the source capture as `-Baseline`; paired sheets were generated and visually inspected. No game installation, actual gameplay/multiplayer check, or CI wiring was performed. Existing NU1900 metadata warnings remained.
