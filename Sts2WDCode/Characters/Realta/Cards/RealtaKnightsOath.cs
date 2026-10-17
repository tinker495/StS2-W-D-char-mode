using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaKnightsOath() : RealtaCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VigorPower>(6m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromKeyword(RealtaKeywords.Duel)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        bool duel = IsDuel(cardPlay.Target);
        decimal vigor = DynamicVars["VigorPower"].BaseValue;
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, vigor, Owner.Creature, this);
        if (duel) await PowerCmd.Apply<RealtaVigorNextTurnPower>(choiceContext, Owner.Creature, vigor, Owner.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars["VigorPower"].UpgradeValueBy(2m);
}
