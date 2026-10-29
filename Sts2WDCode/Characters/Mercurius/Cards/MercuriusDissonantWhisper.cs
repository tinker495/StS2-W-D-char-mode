using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusDissonantWhisper() : MercuriusCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move), new PowerVar<VulnerablePower>(1m), new PowerVar<WeakPower>(1m)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromPower<WeakPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
ArgumentNullException.ThrowIfNull(cardPlay.Target); await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        if (cardPlay.Target is { IsAlive: true } target) await PowerCmd.Apply<VulnerablePower>(choiceContext, target, DynamicVars.Vulnerable.BaseValue, Owner.Creature, this);
        foreach (var enemy in CombatState!.HittableEnemies.Where(enemy => enemy != cardPlay.Target).ToList())
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars.Weak.BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(2m); DynamicVars.Vulnerable.UpgradeValueBy(1m); DynamicVars.Weak.UpgradeValueBy(1m); }
}
