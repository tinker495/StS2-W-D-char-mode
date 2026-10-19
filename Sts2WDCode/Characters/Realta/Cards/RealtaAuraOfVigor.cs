using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaAuraOfVigor() : RealtaCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VigorPower>(8m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal amount = DynamicVars["VigorPower"].BaseValue;
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, amount, Owner.Creature, this);
        if (Owner.Creature.HasPower<WeakPower>()) await PowerCmd.Remove<WeakPower>(Owner.Creature);
    }

    protected override void OnUpgrade() => DynamicVars["VigorPower"].UpgradeValueBy(4m);
}
