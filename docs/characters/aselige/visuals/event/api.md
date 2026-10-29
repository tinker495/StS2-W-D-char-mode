# Aselige Runtime Integration

Sts2WDCode/Ancients/Aselige/Aselige.cs derives from BaseLib 3.4.7 CustomAncientModel, entry STS2WD-ASELIGE, and explicitly supplies the scene, map and history icon paths. AseligeHooks replaces the first Act's generated Ancient for any party containing a mod character. Native-only parties and later Acts retain their generated Ancients. NAncientDialogueLine uses the history icon/outline for the speaker; NAncientNameBanner reads the complete title and empty epithet.

AseligeRelics owns the 14 concrete effects and save properties. The two granted cards are implemented as zero-cost Skills. AseligeDialogues selects the authored 13 conversations per visitor and first Act, without caching run-specific text on the canonical model. Korean and English runtime localization includes relics, cards, event descriptions and all dialogue lines/continuation buttons.

Resource mappings: [event](assets.md), [UI](../ui/assets.md), [relics](../relics/assets.md). See [verification scope](../shared/verification.md) for the limits of recorded observations.
