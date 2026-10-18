using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaDamageImmunity() : RealtaCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(5m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<PlatingPower>(), HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromPower<FrailPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal amount = DynamicVars["PlatingPower"].BaseValue;
        foreach (var ally in CombatState!.Players.Where(p => p.Creature.IsAlive).Select(p => p.Creature).ToList())
        {
            await PowerCmd.Apply<PlatingPower>(choiceContext, ally, amount, Owner.Creature, this);
            if (ally.HasPower<VulnerablePower>()) await PowerCmd.Remove<VulnerablePower>(ally);
            if (ally.HasPower<FrailPower>()) await PowerCmd.Remove<FrailPower>(ally);
        }
    }

    protected override void OnUpgrade() => DynamicVars["PlatingPower"].UpgradeValueBy(3m);
}
