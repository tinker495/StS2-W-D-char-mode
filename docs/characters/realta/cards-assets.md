# Realta Card Portrait Deliveries

[Asset index](realta-assets.md).

## Card Portrait Architecture & Delivery Standards

[Asset index](realta-assets.md).

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

Use the [card export and review procedure](/.agents/skills/character-asset-pipeline/scripts/conversion-tools.md#card-export-and-review) when replacing a tracked portrait. For Mountain Splitter, preserve the [applied painting](/Sts2WD/images/card_portraits/realta/realta_mountain_splitter.png)'s rear robe, back-covering broom, and hourglass cut. [Deployment status](/docs/deployment-status.md) records local installation scope.
