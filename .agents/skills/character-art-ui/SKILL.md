---
name: character-art-ui
description: Produce character portraits, outlines, map markers and energy icons for the StS2 interface.
---

Follow the [shared inputs, engine selection and delivery](/.agents/skills/character-asset-pipeline/SKILL.md#shared-production-contract) for the requested scope.

Use the approved identity and same-use official UI pixels. Derive the white outline from the final portrait alpha and both energy sizes from one master. Inspect markers and portraits at 48–85 screen pixels.

## Recipe 3: `base → Chibi UI`

### 3-A. `character_icon.png`

```text
Use case: identity-preserve / style-transfer.
Asset type: chibi character portrait for <OUTPUT_CONTRACT>.
Input: approved base.png for identity and equipment; official portrait reference (docs/research/sts2-style-refs/future-assets/ui-character-portrait-crop.png) for small-scale readability.
Subject: <IDENTITY_CUES>. Apply the common chibi portrait treatment with <CHIBI_DETAILS>.
Composition: directly paint a head-dominant portrait with only the specified small shoulder/costume cues at the bottom. Keep the face readable at the actual small display size. <FRAMING>.
Style: <STYLE>. Palette: <PALETTE>. True transparent background.
Exclude: weapon, hands, scenery, badge/frame, text and UI unless the actual asset contract requires them.
```

Generate the outline from the final small PNG's alpha. Read radius and color from the target asset guide.

### 3-B. `map_marker.png`

```text
Use case: identity-preserve / stylized-concept.
Asset type: chibi head-only map marker for <OUTPUT_CONTRACT>.
Input: approved base.png and the new chibi portrait created in this production run as an identity/proportion reference.
Subject: <IDENTITY_CUES, CHIBI_DETAILS>. Head and identifying headwear only.
Composition: design the required complete silhouette and transparent margin directly in the square. <FRAMING>. Use the whole painting.
Style: <STYLE>. Palette: <PALETTE>. True transparent background.
Exclude: shoulders, torso, hands, weapons, pin/badge borders, text and drop shadow.
```

### 4-C. `big_energy.png` & `text_energy.png`

```text
Use case: stylized-concept.
Asset type: one energy-icon master exported to the two sizes in <OUTPUT_CONTRACT>.
Input: <ROUGH_OR_REF_INPUT when supplied>; official energy reference (docs/research/sts2-style-refs/future-assets/ui-energy-in-combat-crop.png or installed energy_necrobinder.tres / energy_*.tres) for rim/inset readability.
Subject: <ASSET_SUBJECT, MATERIALS, PALETTE>.
Composition: <FRAMING>. Keep the actual central cost-number overlay area quiet and readable; use a few connected shapes that survive the smallest display size.
Style: <STYLE>. Transparent outside the icon. No painted cost number, text, frame or outer glow.
```
