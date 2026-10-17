using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaRushingBlades() : RealtaSpellCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move), new RepeatVar(2)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithHitCount(DynamicVars.Repeat.IntValue);
        if (SpellActive) attack.TargetingAllOpponents(CombatState!);
        else
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target);
            attack.Targeting(cardPlay.Target);
        }
        await attack.Execute(choiceContext);
    }
    protected override void OnUpgrade() => DynamicVars.Repeat.UpgradeValueBy(1m);
}
