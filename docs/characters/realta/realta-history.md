# Realta Art Decision History

These decisions and their rationale were established through production as of 2026-10-18. They explain the constraints without requiring draft images, version nicknames, or generation logs. The [art specification](realta-art.md) owns the current appearance; the [asset guide](realta-assets.md) owns file dimensions, conversion, integration, and verification scope. This history does not replace either specification.

## 1. Why One Canonical Base Defines the Appearance

The raw references differed in armor, collar construction, facial proportions, and rendering. Using each reference as an independent primary input produced inconsistent equipment, including a missing pauldron or a broken fur collar. The user-approved [base.png](base.png) therefore supplies the identity for every derived character asset.

The base has two bronze pauldrons, a continuous ivory fur collar around the neck, a bronze cuirass and bracers, a blue coat, leather pouches, and sturdy boots. Raw references support individual expressions; they do not override the approved equipment.

Shared style and chibi scope belong in [ARTS.md](../../../ARTS.md). Realta's apparent age, colors, clothing, weapon, and selection scene belong in character documents so they do not become defaults for other characters.

## 2. Youthful Appearance and Open Forehead

A long lower face and adult body proportions departed from the original impression the user wanted. The adopted direction is **a compact build that appears approximately 13–14 years old, a short soft jaw, and lively purple eyes**. This describes visual age, not a confirmed age in the setting.

Sweep the bangs to both sides, exposing the forehead from the central hairline to above the eyebrows. Retain the copper-red bob, pointed ears, and confident smile. A youthful appearance does not authorize turning the full body into an icon-style chibi or redesigning the approved rendering style.

## 3. Separate Forward Gaze from Head Rotation

The combat figure looks forward toward screen right. Correcting eye contact by turning the whole head strongly into profile changed the approved facial impression. Adjust the pupils while preserving a three-quarter face with both eyes visible and the open forehead. Specify head rotation, pupil direction, and body orientation separately.

## 4. Weapon Materials and Proportions

The broom-glaive is **one polearm**: a curved single-edged blade, a two-rivet bronze socket, a red ribbon, a long wooden shaft, and a fan-shaped straw bundle at the bottom. Do not split it into a broom and a separate weapon.

- **Wood:** Use muted reflections and sparse long grain associated with aged wood, rather than a smooth plastic rod. An exaggerated zigzag branch was also rejected. Keep a very gentle bend over the shaft's full length.
- **Joint:** After material exploration, the approved base uses a wedge-shaped bronze socket and two rivets. Distinguish the wooden shaft's surface from the metal socket's structure.
- **Proportions:** The intended contrast is a long polearm beside a small figure. Increasing only width and thickness made the weapon heavy; excessive length disrupted the whole composition. The final choice retains a tip above the hat and broom ends near the feet while slightly reducing blade width and shaft thickness. Treat length, width, and thickness as separate adjustment axes.
- **Surface:** Repeated edits introduced small spots and fine lines that became visual noise at display size. Retain broad connected color masses, selective angular shading, and a few substantial straw bundles.

When applying the base, preserve the complete painting and transparent margins. A request to apply the final selection does not authorize additional generation or silhouette redesign.

## 5. Chibi Scope and Face-Centered Selection Tiles

The user selected chibi portraits and map markers, then clarified that selection buttons should retain the existing non-SD style. Chibi therefore applies only to portraits, their alpha-derived outlines, and map markers. Selection tiles, locked tiles, full bodies, cards, and selection scenes preserve natural proportions and the approved youthful appearance. This scope is a shared project rule.

Including everything from the hat tip to the belt and pouches made the selection-tile face too small. The [original-game tile study](../../research/sts2-character-select-tile-study.md) supports a close camera where both eyes, eyebrows, nose, and mouth read first. Partial hat and lower collar are sufficient; the tile need not show all equipment.

The user requested reference-based regeneration instead of cropping an existing image. Design the target composition during generation and export the whole painting with Fit. Normal and locked variants share the same downscaled alpha: full-color figure on blue versus black silhouette on gray.

## 6. A Distinctive Action and Fresh Selection Composition

Placing the combat figure in front of a fortress workshop emphasized a generic fantasy setting over the character's personality. The [original-game selection study](../../research/sts2-character-select-composition-study.md) identified how gestures, foreground tools, abilities, and large background forms combine into one action.

The adopted scene shows **Realta smiling as she holds her improved broom-glaive upright and inspects its blade in the light**. Her large face sits on the right, with the rising blade and hands in front of her. Bronze diagonals and the dark blue coat organize the wide frame, while cyan magic concentrates near the blade. The scene was generated anew using the canonical base and the original Ironclad and Regent selection illustrations.

Editing the face or colors of an old background repeated its composition. At the user's direction, **Branch B and C replacement production defaults to excluding previous deliveries and drafts from earlier production rounds as inputs**. Design a fresh composition from the approved base, necessary raw references, and original-game references for the same use case. Reusing a portrait or related object newly created within the current round, and deriving delivery variants from the same alpha or master, are allowed. A user request to edit a particular image authorizes that specific edit.

## 7. Delivery and Documentation Storage

The canonical base is 1152×1728 RGBA. The initial Branch B delivery contained nine PNGs: six UI assets, a portrait outline, a shared combat/rest/merchant standing figure, and a selection background. A temporary standing/combat split increased this to ten; section 8 records its subsequent rollback to nine. Runtime paths, scene placement, and export procedures belong in the [asset guide](realta-assets.md#0-current-application-status). Relic, power, and card production requires a separate request.

Historical verification for this delivery covered `dev.ps1 -Task publish` with zero C# warnings and errors, Godot import and PCK export, and loading nine textures and four scenes from the package. Installation, actual gameplay, and other aspect ratios were not checked. An approximate UI overlay obscured part of the hand behind the left description area; actual game readability remains to be checked. These are one-time observations, not formal tests or product acceptance.

**Git-shareable documentation does not require sharing every production intermediate.** Share the canonical base, runtime files, and necessary specifications; drafts, prompts, comparisons, and execution logs may remain Git-ignored local material. State rules, rationale, current behavior, and required procedures directly in shared documents. Do not replace explanations with version nicknames or local-history links that only make sense after opening those files.

## 8. Separate Ordinary Standing from Combat Readiness

The user requested that Branch B use the approved base directly for ordinary standing and generate a separate pose appropriate for combat, then requested placing the new combat image in the runtime asset directory. The former shared PNG was replaced by two files: base-derived `standing.png` for rest/merchant and newly posed `combat.png` for combat.

The generated combat pose used a grounded staggered stance and two hands on a diagonally raised broom-glaive, preserving the base's identity and rendering. Both full paintings were Fit-exported to 384×576 without cropping. Publish completed with zero C# warnings/errors, and temporary PCK observations loaded ten textures and four scenes with the separated references. A later blade refinement narrowed the pale cutting bevel, and that package was installed locally; actual gameplay was not verified.

The user subsequently reverted this split: the separate combat painting was removed from runtime, the base-derived standing image was restored to `combat.png`, and rest/merchant references were changed back to `combat.png`. The extra `standing.png` and its import descriptor were removed. This rollback changes the current character delivery, while the generic Branch B production workflow still supports separate poses. The [art specification](realta-art.md#31-combat-rest-and-merchant-standing-art) and [asset guide](realta-assets.md#05-standing-and-combat-split) own the current state.
