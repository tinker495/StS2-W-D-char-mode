# Realta Art Decision History

These decisions explain the applied appearance and resource choices. The [art specification](realta-art.md) owns the current appearance and the [asset guide](realta-assets.md) owns dimensions, conversion, and integration.

## 1. Why One Canonical Base Defines the Appearance

The raw references differed in armor, collar construction, facial proportions, and rendering. Using each reference as an independent primary input produced inconsistent equipment, including a missing pauldron or a broken fur collar. The user-approved [base.png](base.png) therefore supplies the identity for every derived character asset.

Raw references support individual expressions; they do not override the approved equipment in [base-appearance.md](base-appearance.md).

Shared style and chibi scope belong in [ARTS.md](/ARTS.md). Realta's apparent age, colors, clothing, weapon, and selection scene belong in character documents so they do not become defaults for other characters.

## 2. Youthful Appearance and Open Forehead

A long lower face and adult body proportions departed from the original impression the user wanted. The adopted direction is **a compact build that appears approximately 13–14 years old, a short soft jaw, and lively purple eyes**. This describes visual age, not a confirmed age in the setting.

Keeping the forehead open and preserving natural non-SD proportions prevents a youthful appearance from collapsing into icon-style chibi or altering the approved rendering style.

## 3. Separate Forward Gaze from Head Rotation

The combat figure looks forward toward screen right. Correcting eye contact by turning the whole head strongly into profile changed the approved facial impression. Adjust the pupils while preserving a three-quarter face with both eyes visible and the open forehead. Specify head rotation, pupil direction, and body orientation separately.

## 4. Weapon Materials and Proportions

The broom-glaive stays **one unified polearm** rather than a separate weapon and broom so its silhouette remains distinctive across assets.

- **Wood:** Use muted reflections and sparse long grain associated with aged wood, rather than a smooth plastic rod. An exaggerated zigzag branch was also rejected. Keep a very gentle bend over the shaft's full length.
- **Joint:** A wedge-shaped bronze socket with two rivets separates the wooden shaft's surface from the metal blade mount more clearly than cloth or leather wrapping.
- **Proportions:** The intended contrast is a long polearm beside a small figure. Increasing only width and thickness made the weapon heavy; excessive length disrupted the whole composition. The final choice retains a tip above the hat and broom ends near the feet while slightly reducing blade width and shaft thickness. Treat length, width, and thickness as separate adjustment axes.
- **Surface:** Repeated edits introduced small spots and fine lines that became visual noise at display size. Retain broad connected color masses, selective angular shading, and a few substantial straw bundles.

## 5. Chibi Scope and Face-Centered Selection Tiles

Chibi readability is reserved for small top-bar portraits, their alpha-derived outlines, and map markers; selection tiles, full bodies, cards, and selection scenes retain natural non-SD proportions to preserve the approved youthful identity. This scope is a shared project rule in [ARTS.md](/ARTS.md).

Including everything from the hat tip to the belt and pouches made the selection-tile face too small. The [original-game tile study](/docs/research/sts2-character-select-tile-study.md) supports a close camera where both eyes, eyebrows, nose, and mouth read first. Partial hat and lower collar are sufficient; the tile need not show all equipment.

Cropping standing art distorted face framing and resolution, so selection tiles are generated natively for their tight frame and exported whole with Fit; sharing the resulting alpha keeps normal and locked silhouettes aligned.

## 6. A Distinctive Action and Fresh Selection Composition

Placing the combat figure in front of a fortress workshop emphasized a generic fantasy setting over the character's personality. The [original-game selection study](/docs/research/sts2-character-select-composition-study.md) identified how gestures, foreground tools, abilities, and large background forms combine into one action.

The adopted blade-inspection pose pairs a close character gesture with the weapon and cyan craft magic so the wide selection frame conveys Realta's artisan identity rather than generic scenery (see [selection-art.md](selection-art.md)).

Editing the face or colors of an old background repeated its composition. At the user's direction, **Branch B and C replacement production defaults to excluding previous deliveries and drafts from earlier production rounds as inputs**. Design a fresh composition from the approved base, necessary raw references, and original-game references for the same use case. Reusing a portrait or related object newly created within the current round, and deriving delivery variants from the same alpha or master, are allowed. A user request to edit a particular image authorizes that specific edit.

## 7. Shared Standing Source and Animated Scenes

Deriving the combat and merchant cutout rigs from [base.png](base.png) keeps silhouette, proportions, and equipment consistent across animated scenes (the earlier static fallback still was retired once both scenes used the shared rig; see [scene connections](animation-assets.md#current-scene-and-property-connections)).
