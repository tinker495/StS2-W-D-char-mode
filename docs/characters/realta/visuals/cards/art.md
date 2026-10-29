# Realta Card Illustration Art

## Basic Strike candidate (2026-10-25)

The user requested the weapon from the character image being swung. Production brief: one oversized upper broom-glaive with its curved dark-steel blade, pale bevel, two-rivet bronze socket, red ribbon and connected wooden pole, sweeping across a crimson/burgundy graphic background with a cyan/ivory cutting arc. No person, frame, text or UI. The lower broom end is outside this weapon-focused composition; the blade remains aligned with its shaft rather than becoming a right-angle scythe. Inputs were [base.png](../../base.png) for equipment construction and native `StrikeIronclad`/`StrikeNecrobinder` portraits for staging and density. The built-in image generator created a new complete landscape painting. At 250 x 190 the blade, ribbon and socket remain readable; motion accents are sharper and brighter than native Ironclad's softer orange arc. Pending user review.

## Standard Card Illustrations

**Hands and tools, application requested 2026-10-29:** the four selected paintings below show only Realta's dark-brown gloved hands and short forearms with plain bronze vambraces, ivory fur cuffs and petrol-blue sleeves. Keep her face, head and body outside these card compositions. Preserve continuous hand grips, tool connections and working contacts, with restrained secondary detail and fully painted opaque edges. These are close views of canonical equipment and ordinary craft tools; do not add unrelated ornaments, scenery or full figures.

- [Tempering / 담금질](../../card-art/realta_tempering.png): a hand entering from the upper right grips tongs around a detached, orange-hot curved blade; its lower-left tip enters blue water beneath broad steam curls. Preserve the visible tong jaws, immersion and warm/cool contrast.
- [Whetstone / 숯돌](../../card-art/realta_whetstone.png): the upper-left hand presses a rectangular gray stone against the pale cutting bevel at one small glint. The lower-right hand grips the canonical wood shaft below its two-rivet bronze socket and red ribbon. Keep the stone, cutting edge and both grips distinct.
- [Broom / 싸릿대](../../card-art/realta_broom.png): one hand at the upper right grips a continuous diagonal wood shaft leading to the broad straw fan at lower left. Preserve the red binding, two dark bands, grouped straw and warm sweeping arc; avoid individually outlined strands or floor-cleaning staging.
- [Hammering / 망치질](../../card-art/realta_hammering.png): left-hand tongs clamp the cool end of a curved blade blank on a broad blue-black anvil. The right hand holds one wood-handled rectangular hammer whose lower face contacts the glowing center. Keep the small warm contact, two sparse flecks and clear supporting anvil plane; avoid duplicated tools or disconnected contact.

The tracked paintings, runtime mappings and conversion procedure are in [the asset guide](assets.md).

**Great Flame, application requested 2026-10-29:** the selected [painting](../../card-art/realta_great_flame.png) preserves the user's rough as a close-up of a single straight longsword: the grip enters from the upper left, a broad plain crossguard slopes from lower left to upper right, and the continuous blade descends toward the lower right beyond the frame. The steel itself glows pale yellow-white with orange bevels and a red-hot guard, as if just drawn from a furnace. Broad orange/crimson flame tongues rise around it against a quiet burgundy background, with the larger flames on the right. Preserve the visible center ridge, straight edges and hilt-to-blade connection; avoid ornamental details, particle fields and scenery. No character or hands appear. This card-specific longsword is separate from the canonical broom-glaive. Mapping and verification are in the [asset guide](assets.md).

**Fire Dragon's Blood, application requested 2026-10-29:** the selected [painting](../../card-art/realta_fire_dragons_blood.png) follows the user's hand-and-blade staging: Realta looks down toward screen-left, one closed hand grips a short hilt above the guard, and the opposite forearm crosses her torso with fingers hanging downward onto the blade. The one-handed curved sword descends toward the lower-left edge; it is a card-specific prop, separate from her canonical broom-glaive. Preserve the visible torso and robe. Near-black red liquid with orange molten edges coats the blade, deliberately ambiguous between dragon blood and lava; the brightest heat is at the hand/blade interaction. Keep fur, hair, armor, robe, and the burgundy/navy setting in broad connected shapes, with only a few liquid streams and droplets. These prop and staging choices apply to this card. Mapping and verification are in the [asset guide](assets.md).

**Mountain Splitter, application requested 2026-10-29:** the selected [painting](../../card-art/realta_mountain_splitter.png) shows Realta from behind after cleaving Aeonglass into two gold-framed turquoise sections. Preserve the simple connected color planes, the large straw broom head covering the back, the right-hand connection to the lowered glaive, and one continuous blue rear robe panel streaming left. Its front opening faces away from the camera; do not add a V-shaped opening on the visible back or a closed skirt-like lower hem. At the user's request, the setting extends upward through dark-blue architecture and downward through the ground and shadows to fill the complete `25:19` card window. Preserve this painted coverage; do not restore the former top/bottom letterboxing. This staging applies to this card, not to the canonical base or other assets. Mapping and conversion are in the [asset guide](assets.md).

- `Sts2WD/images/card_portraits/realta/realta_<name>.png`: 1000×760 opaque PNG, 25:19; standard display 250×190, enlarged 500×380.
- Keep action, face, hand, and impact within `x:15–85%,y:12–82%`, away from upper-left energy and frame curves. Outer 10–15% may contain expendable environment, cloth tips, or trails.
- **Depth/contrast starting points:** foreground action/tool 100% with sharpest edges and extreme values; midground target/prop 60%; background 25–35%, grouped into two or three broad values.
- **Attack (24):** strong diagonals, weapon arcs, asymmetric impacts; close to blade/contact.
- **Skill (26):** medium-close precise tool work, defense, or coordination with a guardian.
- **Power (18):** stable symbolic composition for persistent spaces/states/rules: forge, duel, witch circle, ancient relic/anvil.

## Ancient Full-Art Cards

- Two vertical 606×852 assets: `realta_iordina_mac_bethlehem.png` (`요르디나 막 들레헴`) and `realta_genius_inspiration.png` (`천재의 영감`).
- Based on `NCard` Ancient layout:
  - Top `y:0–14%` / 0–120px: title banner and upper-left cost.
  - Upper-middle `y:15–58%` / 128–495px: **primary focal window** for face, main weapon, or inspiration light.
  - Lower `y:62–100%` / 528–852px: `AncientTextBg` and description. Use subdued dark vertical cloth, fading magic, or workbench shadows; avoid high-contrast faces/ornate metal competing with text.
