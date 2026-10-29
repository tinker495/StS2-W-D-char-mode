# Realta Selection Art

## Selection Tiles

Read the [five-character normal/locked study](../../../../research/sts2-character-select-tile-study.md) and [original PNGs](../../../../research/sts2-style-refs/character-select-tiles/README.md). Design the tile separately from the large illustration. Original masks/skulls dominate their narrow frames; Silent's horns and Regent's head ornament extend beyond the top. Eye heights vary. **Non-SD preserves canonical age/anatomy; it does not require an adult face or full body inside the tile.**

- **Camera:** violet eyes, brows, nose, and mouth read first; red hair/pointed ear form outer masses. Start with forehead-to-jaw skin height around half the image. This is a Realta guideline, not official measurements or an automated threshold; judge at 132×195 beside originals.
- **Hat:** partial angled brim, band, and bronze ring may suffice. Do not shrink the face to show the whole crown. Hair, ear tips, and brim may extend beyond side boundaries; keep shadow from obscuring expression.
- **Costume:** continuous ivory collar and partial paired pauldrons below. Preserve their construction without forcing off-camera chest, belt, pouch, weapon, and hands into the portrait.
- **Values/background:** separate light skin, red-brown hair, and blue hat/collar. Use simple blue backing; no scenery, magic particles, or excessive vignette. Selection border, mask, and hover treatment belong to UI.
- **Normal/locked:** generate one transparent face master, resize it whole, and share final alpha. Composite colored figure on blue and black figure on gray; both outputs have alpha 255. No eye glow, face values, lock glyphs, or confusion with deselection tint.
- **Records:** state output dimensions, full-image use, colors, and decision rationale in the [UI asset guide](../ui/assets.md). Prompts/comparisons are optional local records. Regenerate composition changes; follow [Recipe 4-A/B](../../../../../.agents/skills/character-asset-pipeline/routes/ui.md#4-a-char_selectpng-132--195-opaque-tile).

The current tile was generated from base identity and actual Defect/Silent face occupancy references, then whole-image Fit resized with partial hat, face, and lower collar. It was not extracted from standing art.

## Tile Colors

Normal backing is `#2476A6`; locked uses black figure `#000000` on gray `#3B3B3B`. Composite both from the same resized alpha. These are Realta delivery choices, not exact RGB values asserted for every official character.

## Selection Illustration

Separate 1920×1080 non-SD scene produced with Branch B UI. Base owns identity/equipment; pose/camera are designed for selection. Read the [five-character study](../../../../research/sts2-character-select-composition-study.md) and [original resources](../../../../research/sts2-style-refs/character-select/README.md). Their distinctive swords, giant hands, thrones, or flowing cloaks establish character through gesture. Do not default to full combat figures before scenery or require boots/weapon tips in frame. Design close/perspective staging during generation, not later cropping.

The current scene is **Realta holding her improved broom-glaive upright, inspecting the blade in the light, and smiling confidently**. A low camera links large right-side face, open gloved hand, and rising blade. Broad bronze diagonals and dark blue coat establish background masses; cyan magic concentrates near the blade. Preserve paired pauldrons, continuous collar, chest, pouch, and single polearm. It was generated from `base.png` plus actual Ironclad/Regent selection references. Future replacement production follows the [new-artwork rule](../../../../../.agents/skills/character-asset-pipeline/workflow.md#starting-inputs), not this scene as a fixed template.

Low-contrast cloak/weapon/ability forms may continue behind left name/ability/relic text. **Safe space preserves text and avoids key face/hand/contact, rather than leaving an empty rectangle.** Inspect lower tiles and bottom-right confirmation with an observational UI overlay. Keep panels, words, and buttons out of runtime art. Runtime image: `images/charui/realta/char_select_bg.png`; Control/TextureRect scene: `scenes/realta/char_select_bg_realta.tscn`; property: `CustomCharacterSelectBg`.

See [future-asset research](../../../../research/sts2-future-asset-reference-study.md) for official examples/UI crops. Shared ARTS.md owns chibi scope; Realta-specific face/hat/costume choices stay here.

[Scene integration](../animation/assets.md) owns the selection background's current scene and property connections.
