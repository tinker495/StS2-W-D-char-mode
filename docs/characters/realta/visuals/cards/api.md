# Card Portrait API Reference (BaseLib 3.4.7)

[Asset index](../../realta-assets.md).

## Card Portraits and Frames (`CardModel`, `CustomCardModel`, `CustomCardPoolModel`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.CardModel`
  - `sts2.dll: MegaCrit.Sts2.Core.Nodes.Cards.NCard`
  - `sts2.dll: MegaCrit.Sts2.Core.Nodes.Screens.NInspectCardScreen`
  - `sts2.dll: MegaCrit.Sts2.Core.Models.ModelDb`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomCardModel`, `CustomCardPortrait`, `CustomCardPortraitPath`, `CustomCardPortraitPngPath`, `CustomCardFrame`, `CustomCardPoolModel`, `CustomCardPoolMaterialPatch`)
  - `Sts2WDCode/Cards/Sts2WDCard.cs` (lines `11-26`)

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
4. **Default dual-path lookup in `Sts2WDCard.cs` and a proposed broader improvement:**
   - Cards listed in [delivered portraits](assets.md) are exceptions: each model's `CustomPortraitPath` and `PortraitPath` resolve its character-owned portrait. The defaults and future proposal below describe other cards; a general automatic resolver has not been added.
   - Template-derived code in `Sts2WDCode/Cards/Sts2WDCard.cs:17-25` currently overrides **both** `CustomPortraitPath` (`BigCardImagePath()` -> `images/card_portraits/big/<id>.png`) and `PortraitPath` (`CardImagePath()` -> `images/card_portraits/<id>.png`).
   - Consequences:
     1. At startup, `ModelDb.Preload()` (`ModelDb.cs:453`) calls `allCard.AllPortraitPaths` -> virtual `Sts2WDCard.PortraitPath`, looking for `images/card_portraits/<id>.png`. Missing files produce `"Could not find card image path: ..."` messages and return `images/card_portraits/card.png` (`250x190`). The eleven newer cards reuse the properties listed in [additional portrait sources](inventory-additional.md).
     2. During rendering, `NCard.Reload()` (`NCard.cs:1192`) calls `Model.Portrait` -> `CustomCardPortrait` patch -> `Sts2WDCard.CustomPortraitPath`. It **loads only `images/card_portraits/big/<id>.png`**, falling back to `images/card_portraits/big/card.png` (`1000x760`). The `PortraitPath` override is not used for screen rendering.
   - **Proposed integration: automatic mapping and fallback under `Sts2WD/images/card_portraits/realta/<card_name>.png`:**
     - To manage all 81 Realta cards (79 standard, including the `RealtaGuard` token, plus two Ancient cards) in one directory, with unfinished artwork falling back to `card.png` or `big/card.png`, adapt `StringExtensions.cs` and `RealtaCard.cs` / `RealtaGuard.cs` (or `Sts2WDCard.cs`) as follows:
     ```csharp
     // Add to Sts2WDCode/Extensions/StringExtensions.cs, or adapt CardImagePath:
     public static string CharacterCardImagePath(this string path, string characterFolder)
     {
         string charPath = Path.Join(MainFile.ResPath, "images", "card_portraits", characterFolder, path);
         if (ResourceLoader.Exists(charPath)) return charPath;

         string flatBigPath = Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
         if (ResourceLoader.Exists(flatBigPath)) return flatBigPath;

         string flatPath = Path.Join(MainFile.ResPath, "images", "card_portraits", path);
         if (ResourceLoader.Exists(flatPath)) return flatPath;

         return Path.Join(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
     }
     ```
     - Point `CustomPortraitPath` to the same resolved path in `RealtaCard.cs` and `RealtaGuard.cs` (or detect an `Id.Entry` beginning with `"STS2WD-REALTA_"` in `Sts2WDCard.cs`), then unify `PortraitPath => CustomPortraitPath;`.
       - One PNG per card (`Sts2WD/images/card_portraits/realta/<card_name>.png`, `1000 x 760` or `606 x 852` for Ancient cards) then serves both normal and inspect views (`NInspectCardScreen`).
       - Unfinished cards fall back to `res://Sts2WD/images/card_portraits/big/card.png` (`1000 x 760`) without per-card code changes.
       - `RealtaGuard` (`[Pool(typeof(TokenCardPool))]`) directly inherits `Sts2WDCard`, not `RealtaCard`. Apply shared behavior in `Sts2WDCard` or include overrides in `RealtaGuard` so the token is not missed.
