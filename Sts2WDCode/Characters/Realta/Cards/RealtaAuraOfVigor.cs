using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaAuraOfVigor() : RealtaCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VigorPower>(8m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal amount = DynamicVars["VigorPower"].BaseValue;
        foreach (var ally in CombatState!.Players.Where(p => p.Creature.IsAlive).Select(p => p.Creature).ToList())
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, ally, amount, Owner.Creature, this);
            if (ally.HasPower<WeakPower>()) await PowerCmd.Remove<WeakPower>(ally);
        }
    }

    protected override void OnUpgrade() => DynamicVars["VigorPower"].UpgradeValueBy(4m);
}
