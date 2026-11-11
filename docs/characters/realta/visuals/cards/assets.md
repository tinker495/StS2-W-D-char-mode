# Realta Card Portrait Deliveries

[Asset index](../../realta-assets.md).

## Card Portrait Architecture & Delivery Standards

[Asset index](../../realta-assets.md).

All Realta card portraits are resolved dynamically by `Sts2WDCard` through naming convention:
- **Runtime Path:** `res://Sts2WD/images/card_portraits/realta/<card_slug>.png`
- **Standard Cards:** Exactly `1000 × 760` (`25:19`), fully opaque (`alpha = 255` across every pixel).
- **Ancient Full-Art Cards:** Exactly `606 × 852` (`101:142`), fully opaque, registered in `scripts/card-portrait-layouts.json`.
- **Fallback:** Cards without dedicated mod artwork automatically fall back to base-game placeholders (`StrikeIronclad` / `DefendIronclad`) via `RealtaCard.FallbackPortraitPath`.

Delivered textures are validated via:
```powershell
& .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 check-cards
```
Individual card classes require no manual `CustomPortraitPath` or `PortraitPath` overrides. Post-delivery narrative prose is pruned to prevent documentation bloat.

Reproduce these three deliveries from their tracked paintings through the [card export and review procedure](../../../../../.agents/skills/character-asset-pipeline/scripts/conversion-tools.md#card-export-and-review): `export-card -CardType Standard` to an ignored temporary candidate, inspect and open the `250 × 190` preview, then copy the reviewed file to the runtime mapping and run `check-cards`. All three full painted sources scale by `1000/1439` and round to `1000 × 760`, leaving zero padding pixels. For Mountain Splitter, the painted architecture and ground replace the former top/bottom bands; retain the rear robe, back-covering broom, and hourglass cut. Package/resource observations and installation status belong to [the shared record](../../../../deployment-status.md).
