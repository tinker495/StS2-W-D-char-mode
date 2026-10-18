using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaFaerieBronze() : RealtaMasterworkCard(CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. base.ExtraHoverTips, HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];
    protected override Task OnRepeat(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        RefineCmd.Refine(choiceContext, Owner, DynamicVars["Refine"].IntValue, this);
    protected override void OnUpgrade() => DynamicVars["Refine"].UpgradeValueBy(1m);
}
