# Aselige Relic Assets

All fourteen [approved relic objects](art.md) are connected to the Ancient implementation as of 2026-10-24.

| Role | Path | Dimensions / wiring |
|---|---|---|
| Relic small / outline | Sts2WD/images/relics/aselige_<slug>.png / aselige_<slug>_outline.png | 94 x 94; outlines use white 2 px alpha dilation |
| Relic large | Sts2WD/images/relics/big/aselige_<slug>.png | 256 x 256 |

The fourteen slugs are regeneration_grace, deep_breath, harmonious_blessing, life_source, embracing_flute, shining_film, purifying_breath, deep_blue_scales, clean_pebble, small_sprout, large_sapling, pretty_starfish, neatly_arranged_salt and growing_blessing. Each has the three exports listed above, totaling 42 textures. The current overview is [relics-overview.png](../../relics-overview.png).

Concrete model names are Aselige followed by the PascalCase slug. Sts2WDRelic resolves their filenames from the model entry without the STS2WD- prefix. The internal gift enum retains DragonFlute; the concrete model and files use EmbracingFlute. See [runtime integration](../event/api.md) for effect and save ownership.
