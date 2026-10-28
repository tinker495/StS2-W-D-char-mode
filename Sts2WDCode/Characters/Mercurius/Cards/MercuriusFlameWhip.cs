using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusFlameWhip() : MercuriusCard(1, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
{
    private List<(CardPlay Origin, decimal Damage)> _pending = [];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];
    protected override void DeepCloneFields() { base.DeepCloneFields(); _pending = []; }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await Strike(context, DynamicVars.Damage.BaseValue);
        _pending.Add((cardPlay, DynamicVars.Damage.BaseValue));
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !MercuriusCardEffects.IsOffColor(cardPlay.Card, Owner)) return;
        var ready = _pending.Where(entry => entry.Origin != cardPlay).ToList();
        _pending.RemoveAll(entry => entry.Origin != cardPlay);
        foreach (var entry in ready) await Strike(context, entry.Damage);
    }
    private async Task Strike(PlayerChoiceContext context, decimal damage)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        var target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState!.HittableEnemies);
        if (target != null) await DamageCmd.Attack(damage).FromCard(this).Targeting(target).Execute(context);
    }
    public override Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner.Creature)) _pending.Clear();
        return Task.CompletedTask;
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
