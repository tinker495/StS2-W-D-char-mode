using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaReclaim() : RealtaCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<RealtaFlyingGlaive>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = Owner.PlayerCombatState!.AllCards.OfType<RealtaFlyingGlaive>()
            .Where(c => c.Pile?.Type != PileType.Hand).ToList();
        await CardPileCmd.Add(cards, PileType.Hand);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
