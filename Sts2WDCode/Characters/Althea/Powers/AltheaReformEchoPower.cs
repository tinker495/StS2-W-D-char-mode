using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaReformEchoPower : AltheaPower
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => AltheaKeywords.ReformHoverTips();

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card,
        bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position) =>
        card.Owner.Creature == Owner && AltheaRules.For(card.Owner).ReformedThisTurn.Contains(card)
            ? (PileType.Exhaust, position) : (pileType, position);
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        card.Owner.Creature == Owner && AltheaRules.For(card.Owner).ReformedThisTurn.Contains(card)
            ? playCount + Amount : playCount;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}
