using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaSelfRepair() : RealtaCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Guard", 5m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromCard<RealtaGuard>()];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await GuardCmd.Guard(DynamicVars["Guard"].BaseValue, Owner);
        var cards = Owner.PlayerCombatState!.AllCards.OfType<RealtaGuard>()
            .Where(c => c.Pile?.Type != PileType.Hand).ToList();
        await CardPileCmd.Add(cards, PileType.Hand);
    }
    protected override void OnUpgrade() => DynamicVars["Guard"].UpgradeValueBy(2m);
}
