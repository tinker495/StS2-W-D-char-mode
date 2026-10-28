# Branch C-1: `base + refs → Relics` (Focused Relic Route)

Read the requested relic's effect/object brief, mapping and current status, ARTS.md 5.3, [the shared workflow](../workflow.md), and the active [engine section](../engines.md). Read character identity only if canonical costume/equipment appears. A supplied rough/photo or designated motif uses [reference roles and landmarks](../references.md#1-d-rough-sketch-or-reference-image--finished-asset-refs-or-draftsguidepng). Use the recipe below and [observation tools](../observation-tools.md); other production routes are not required.

1. **Establish scope, family masters, and references:** read the relic list, rarities, and object briefs in `<char>.md`, `<char>-art.md`, and `<char>-assets.md`. Open the official StS2 relic anchors ([relics-potions-frame0.png](../../../../docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png) and [relics-in-combat-crop.png](../../../../docs/research/sts2-style-refs/future-assets/relics-in-combat-crop.png); [study §2](../../../../docs/research/sts2-future-asset-reference-study.md#2-relic-and-status-information-density)). For each relic, assemble:
   - Any user-supplied rough sketch, silhouette, or object photo in `refs/` (or an existing approved symbol explicitly designated by the user, such as an energy emblem for a matching crest relic).
   - `base.png` only when the relic depicts canonical character costume or signature equipment.
   - For paired/upgraded relics (such as starter → boss upgrade), generate and approve the starter/base object master in this run first, then pass it as the structural anchor for the upgraded variant.
2. **Generate isolated object masters (Recipe 5 & 1-D):** generate each relic directly as a `256 × 256` RGBA isolated object (roughly 72–84% occupancy, bold dark contour, 2–3 broad connected value families, no background, ground shadow, pedestal, or fine engraving).
3. **Inspect and compare against official relics:** run `inspect` (`CornersTransparent = true`, `BBoxWidthRatio`/`BBoxHeightRatio` around 72–84% with at least 2–3px outer margin at 94px scale so the outline will not clip) and run `compare` at `94 × 94` and `48 × 48` (top-bar scale) beside `relics-potions-frame0.png` / `relics-in-combat-crop.png` on dark `#1B1924` and light `#E8E2D5` backgrounds. When producing a set, build a combined overview sheet. **Completion criterion:** every relic silhouette reads distinctly at `48 × 48`, rough/reference identifiers are preserved without photo noise, and no corner/edge alpha clipping occurs.
4. **Export three files per relic:** upon approval, export `256 × 256` (`Sts2WD/images/relics/big/<relic>.png`), export `94 × 94` (`Sts2WD/images/relics/<relic>.png`, using a 90×90 Fit centered in 94×94 if needed to preserve a 2px outline margin), and derive the `94 × 94` outline (`Sts2WD/images/relics/<relic>_outline.png`) from the small alpha with `asset-pipeline.ps1 outline`.

## Recipe 5: `base + refs → Relic Icon` (Branch C-1)

Follow [Branch C-1](relics.md#branch-c-1-base--refs--relics-focused-relic-route) and the [Branch B/C input rule](../workflow.md#starting-inputs). Open and pass the official StS2 relic anchors ([relics-potions-frame0.png](../../../../docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png) and [relics-in-combat-crop.png](../../../../docs/research/sts2-style-refs/future-assets/relics-in-combat-crop.png)). Pass `<ROUGH_OR_REF_INPUT>` (object photo, rough sketch, or designated symbol) when supplied, `base.png` only when the relic depicts canonical character costume/weaponry, and the newly generated current-run starter/base relic master when producing an upgraded/paired relic variant.

```text
Use case: stylized-concept / reference-guided-generation.
Asset type: isolated relic icon for <OUTPUT_CONTRACT> (256x256 master exported to 94x94 + 94x94 outline).
Engine mode: <ENGINE_MODE>.
Input owners:
- Object / rough reference (<ROUGH_OR_REF_INPUT or current-run family master, when present>): owns silhouette proportions, structural construction, and key functional identifiers; simplify photo grain, engraving noise, and source backing.
- Character anchor (<approved base.png, only when canonical character gear is depicted>): owns canonical material and equipment construction.
- Official StS2 relic anchor (docs/research/sts2-style-refs/future-assets/relics-potions-frame0.png, relics-in-combat-crop.png): guides bold near-black outer contour, 2-3 broad matte value planes, and 48-64px top-bar readability; exclude source blue backing, numbers, and ticket lettering.
Subject: <ASSET_SUBJECT, canonical object construction, MATERIALS, PALETTE>.
Composition: a single centered isolated object, approximately 72-84% occupancy (preserving at least 2-3px transparent margin at 94x94 so the alpha outline never clips). Prioritize a distinct outer silhouette and 2-3 large connected value families at 48-64px display size.
Style: <STYLE>. True transparent background. No floor shadow, pedestal, mannequin, scenery, fine filigree, text or UI frame.
```

Compare at `94 × 94` and `48 × 48` beside `relics-potions-frame0.png` and `relics-in-combat-crop.png` on dark/light boards. Resize `256 × 256` large to `94 × 94` small, then derive the `94 × 94` outline from the small alpha.
