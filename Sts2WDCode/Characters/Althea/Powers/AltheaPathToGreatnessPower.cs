using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaPathToGreatnessPower : AltheaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => AltheaKeywords.ReformHoverTips();

    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay,
        ResourceInfo resources, CardLocation cardLocation)
    {
        if (card.Owner.Creature == Owner && AltheaRules.For(card.Owner).CardsPlayed == 0)
        {
            AltheaRules.For(card.Owner).ReformAfterPlay[card] = cardLocation.pileType;
            return cardLocation with { pileType = PileType.Discard };
        }
        return cardLocation;
    }
}
