# Aselige Runtime Integration

Sts2WDCode/Ancients/Aselige/Aselige.cs derives from BaseLib 3.4.7 CustomAncientModel, entry STS2WD-ASELIGE, and explicitly supplies the scene, map and history icon paths. AseligeHooks replaces the first Act's generated Ancient for any party containing a mod character. Native-only parties and later Acts retain their generated Ancients. NAncientDialogueLine uses the history icon/outline for the speaker; NAncientNameBanner reads the complete title and empty epithet.

AseligeRelics owns the 14 concrete effects and save properties. The two granted cards are implemented as zero-cost Skills. AseligeDialogues selects the authored 13 conversations per visitor and first Act, without caching run-specific text on the canonical model. Korean and English runtime localization includes relics, cards, event descriptions and all dialogue lines/continuation buttons.

In native run creation, `RunState.CreateForNewRun` clones acts via `ToMutable` (`ActModel.DeepCloneFields` allocates a new `RoomSet`) and `AssertMutable` guards `run.Acts[0]`, isolating the replaced first-Act Ancient to that mutable run state rather than mutating canonical models. Initial deck randomization uses `CardPile.RandomizeOrderInternal`, which does not emit `AfterShuffle`, so shuffle-triggered gift relics only fire on actual mid-combat reshuffles.

Resource mappings: [event](event-assets.md), [UI](ui-assets.md), [relics](relics-assets.md). See [deployment status](/docs/deployment-status.md) for recorded local installations and verification scope.
