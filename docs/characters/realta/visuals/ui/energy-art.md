# Realta Flame-Heart Energy Art

The cost icon uses the complete approved flame-heart image, proportionally resized without cropping to both delivery sizes. Its dark heart is below the silhouette's geometric center. The cost label uses a centered 34×36px box shifted 8px down from the native layout, live 14–22px autosizing, and a restrained 3px text/shadow outline. Keep the original font, dynamic cost colors, and X/unknown-cost behavior. Do not reuse the native 32px maximum and 16px outline in this smaller inset. The lower-left combat counter uses the same complete master as a 256px texture displayed at 128px; current/max energy sits in a 54×44px heart-inset label with 16–26px autosizing and 3px outlines. Keep the upright flame silhouette static, the native energy-gain VFX and transitions, and native darkening/red text at zero energy. Paths, node restoration, packaging, and installation verification belong in the [UI asset guide](assets.md).

## Flame-Heart Energy Emblem

```text
Use case: stylized-concept.
Asset type: game card energy orb icon, exported to 74x74 and 24x24, square transparent icon.
Subject & Composition: preserve the user-selected red-orange flame silhouette wrapping around a large dark burgundy heart-shaped inset. Use broad curling flame tongues, dark red bevel shadows, and selective orange-gold highlights. The inset is empty: remove the former small inner heart and keep the field quiet for a white energy numeral. Keep the complete silhouette and transparent margins when resizing.
Style/medium: <insert ARTS.md section 7.2 [Block A]>. Broad connected shapes and restrained surface detail that survive 24x24.
Backdrop & Exclusions: true transparent alpha outside the emblem; the heart inset stays dark and opaque. No painted numbers, inner symbols, runes, stars, particles or outer glow.
```
