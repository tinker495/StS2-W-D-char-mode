# Card Portrait API Reference (BaseLib 3.4.7)

[Asset index](realta-assets.md).

## Card Reload Ownership

Shared [`CardAppearance`](/Sts2WDCode/Cards/CardAppearance.cs) owns the single mod postfix on native `NCard.Reload`. It resets common mod overrides (energy/type labels and portrait-border modulation), applies `ICardPoolAppearance` from `model.VisualCardPool`, calls `Sts2WDCard.ApplyPortraitAppearance`, and refreshes cost autosizing so shared presentation code never references character-owned types. Realta's pool appearance lives in `RealtaCardAppearance` (see [frame integration](cards-frames.md)).

Inspection of installed `sts2.dll` product version `0.1.0+cc1f9462ca6c602e88f83f3bd7e2d084d4d4f8d7` established that `NCard.Model` and `NCard.Visibility` call `Reload`, which restores the native frame, type-plaque and portrait materials. BaseLib 3.4.7 `CustomCardModel` exposes frame/banner material and portrait selection, but no cost-label geometry/autosizing or post-Reload styling operation. The existing postfix is retained only for that presentation gap. Native material restoration still owns rarity and visibility; the mod does not duplicate it.

The visual-pool selection and model portrait selection are intentionally independent. `RealtaGuard` keeps its native Token pool and its Realta visual-pool override. An Althea card keeps its portrait treatment when its visual pool changes.

## Card Portraits and Frames (`CardModel`, `CustomCardModel`, `CustomCardPoolModel`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.CardModel`
  - `sts2.dll: MegaCrit.Sts2.Core.Nodes.Cards.NCard`
  - `sts2.dll: MegaCrit.Sts2.Core.Nodes.Screens.NInspectCardScreen`
  - `sts2.dll: MegaCrit.Sts2.Core.Models.ModelDb`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomCardModel`, `CustomCardPortrait`, `CustomCardPortraitPath`, `CustomCardPortraitPngPath`, `CustomCardFrame`, `CustomCardPoolModel`, `CustomCardPoolMaterialPatch`)
  - [`Sts2WDCard.cs`](/Sts2WDCode/Cards/Sts2WDCard.cs)

## Portrait Resolution in `CardModel` and `CustomCardModel`

1. **Vanilla `CardModel` portrait properties (`CardModel.cs:142-156, 1047`):**
   - `public virtual string PortraitPath` -> `res://images/atlases/card_atlas.sprites/{pool}/{entry}.tres`
   - `public virtual string BetaPortraitPath` -> `res://images/atlases/card_atlas.sprites/{pool}/beta/{entry}.tres`
   - `private string PortraitPngPath` -> `res://images/packed/card_portraits/{pool}/{entry}.png`
   - `private string BetaPortraitPngPath` -> `res://images/packed/card_portraits/{pool}/beta/{entry}.png`
   - `public bool HasPortrait => ResourceLoader.Exists(PortraitPngPath);`
   - `public bool HasBetaPortrait => ResourceLoader.Exists(BetaPortraitPngPath);`
   - `public Texture2D Portrait => ResourceLoader.Load<Texture2D>(PortraitPath, null, ResourceLoader.CacheMode.Reuse);`
   - `public virtual IEnumerable<string> AllPortraitPaths => [PortraitPath];`
2. **Vanilla UI rendering (`NCard.cs:1181-1250`, `NInspectCardScreen.cs:222, 276`):**
   - Unlike StS1, StS2 has **no separate `BigPortrait` texture property**.
   - Normal combat/reward/compendium cards (`NCard`) and the inspect screen (`NInspectCardScreen`, which scales the same `NCard` to `Scale = Vector2.One * 2f`) both use **only `Model.Portrait`**.
   - Inside `res://scenes/cards/card.tscn`:
     - Standard cards (`Rarity != CardRarity.Ancient`) place `Model.Portrait` in `%Portrait` (`TextureRect`, UI area `250 x 190`, `offset_left=-125, offset_top=-168, offset_right=125, offset_bottom=22`). Vanilla source PNGs (`res://images/packed/card_portraits/<pool>/<card>.png`) are **`1000 x 760`**, exactly four times `250 x 190` at ratio `25:19`. `500 x 380` or `250 x 190` inputs share that ratio and scale automatically.
     - Ancient cards (`Rarity == CardRarity.Ancient`) place `Model.Portrait` in `%AncientPortrait`, a full-art `TextureRect`. Vanilla full-art PNGs, such as `res://images/packed/card_portraits/event/apotheosis.png`, are **`606 x 852`** (small-thumbnail reference: `250 x 350`).
3. **BaseLib 3.4.7 `CustomCardModel` patches (`BaseLib.Abstracts.CustomCard*`):**
   - `CustomCardModel` has no `CustomBigPortraitPath`. It provides:
     - `public virtual string? CustomPortraitPath => null;`
     - `public virtual Texture2D? CustomPortrait => null;`
   - Three BaseLib Harmony patches intercept `CardModel` getters:
     - `CustomCardPortrait` (`CardModel.Portrait` getter Prefix): returns `CustomPortrait` if set; otherwise, if `CustomPortraitPath` is not `null`, returns `ResourceLoader.Load<Texture2D>(CustomPortraitPath, null, CacheMode.Reuse)`. Only a `null` `CustomPortraitPath` falls through to vanilla `CardModel.Portrait`, which loads `PortraitPath`.
     - `CustomCardPortraitPngPath` (`CardModel.PortraitPngPath` getter Prefix): when `CustomPortraitPath != null`, replaces `PortraitPngPath` with `CustomPortraitPath`, making `HasPortrait` check `ResourceLoader.Exists(CustomPortraitPath)`.
     - `CustomCardPortraitPath` (`CardModel.PortraitPath` base getter Prefix): when `CustomPortraitPath != null`, makes the base `CardModel.PortraitPath` implementation return the `ResourcePath` of `CustomPortraitPath`.
4. **Live portrait resolution in [`Sts2WDCard.cs`](/Sts2WDCode/Cards/Sts2WDCard.cs):**
   - `Sts2WDCard.ResolvePortraitPath()` caches by `Id.Entry`, resolves `res://Sts2WD/images/card_portraits/<char>/<slug>.png`, and unifies `CustomPortraitPath`, `PortraitPath`, and `BetaPortraitPath` so normal and inspect views share one PNG while missing files fall back via `FallbackPortraitPath`.
   - `RealtaCard` overrides `FallbackPortraitPath` to base-game Strike/Defend art, whereas `RealtaGuard` inherits directly from `Sts2WDCard` to stay in `[Pool(typeof(TokenCardPool))]` (using `Sts2WDCard.FallbackPortraitPath` if its dedicated `realta_guard.png` is missing).
   - Explicit per-card overrides remain only for `RealtaDefend` (native Defend art) and the eleven alias cards listed in [additional portrait sources](cards-inventory-additional.md).
