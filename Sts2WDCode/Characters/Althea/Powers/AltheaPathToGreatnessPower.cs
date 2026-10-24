using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaPathToGreatnessPower : AltheaPower
{
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay,
        ResourceInfo resources, PileType pile, CardPilePosition position)
    {
        if (card.Owner.Creature == Owner && AltheaRules.For(card.Owner).CardsPlayed == 0)
        {
            AltheaRules.For(card.Owner).ReformAfterPlay[card] = pile;
            return (PileType.Discard, position);
        }
        return (pile, position);
    }
}
