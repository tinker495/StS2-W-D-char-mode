# Althea Crimson Energy Art and Delivery

## Appearance

The user selected the base-game Necrobinder energy icon as the exact shape reference and requested only a dark crimson color change. Preserve its faceted hexagonal metal rim, circular inner light, bevel arrangement, orientation, proportions, transparent margins, and number inset. Shift the pink/magenta body to blackened burgundy and blood red, retaining enough separation between rim, center, and highlights for white cost digits.

The selected source is the installed Slay the Spire 2 resource `res://images/atlases/ui_atlas.sprites/card/energy_necrobinder.tres`; the inline reference is `res://images/packed/sprite_fonts/necrobinder_energy_icon.png`. These are local game references, not a newly established character base. This is a selected-image recolor under pipeline Branch D, not new character artwork.

## Delivery and Integration

The runtime uses the accepted crimson variant at `Sts2WD/images/charui/althea/big_energy.png` (74×74 RGBA for card costs) and `text_energy.png` (24×24 RGBA for inline energy), wired through `AltheaCardPool`, `AltheaRelicPool`, and `AltheaPotionPool`. Both use a whole-image Fit that preserves the complete symbol and its alpha. The combat energy counter uses the placeholder character's native presentation. See [deployment status](/docs/deployment-status.md) for recorded local installations.

To recover the original reference from the installed game, mount `SlayTheSpire2.pck` in Godot, load `res://images/atlases/ui_atlas.sprites/card/energy_necrobinder.tres` as `AtlasTexture`, decompress its atlas image and read its declared region. This recovers the asset's native region, rather than deriving a new composition from a larger painting. The inline source is `res://images/packed/sprite_fonts/necrobinder_energy_icon.png`.
