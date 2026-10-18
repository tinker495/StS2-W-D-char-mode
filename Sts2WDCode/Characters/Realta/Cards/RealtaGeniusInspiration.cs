using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaGeniusInspiration() : RealtaCard(1, CardType.Power, CardRarity.Ancient, TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 5m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await PowerCmd.Apply<RealtaGeniusInspirationPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
