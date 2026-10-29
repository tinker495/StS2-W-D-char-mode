using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public abstract class MercuriusFinaleCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : MercuriusCard(cost, type, rarity, target)
{
    public override int MaxUpgradeLevel => int.MaxValue;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(MercuriusKeywords.Finale)];
    public override Task AfterCardDrawn(PlayerChoiceContext context, CardModel card, bool fromHandDraw)
    {
        if (card == this && IsUpgradable) CardCmd.Upgrade(this);
        return Task.CompletedTask;
    }
}
