using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusIllusionPower : CustomPowerModel
{
    private int _amountBeforeChange;
    public override Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
    {
        if (power == this) _amountBeforeChange = Math.Max(0, Amount);
        return Task.CompletedTask;
    }
    public override async Task AfterRemoved(Creature oldOwner)
    {
        if (Amount <= 0 || CombatManager.Instance.IsOverOrEnding || oldOwner.CombatState == null) return;
        int lost = Amount;
        SetAmount(0);
        await NotifyLoss(new ThrowingPlayerChoiceContext(), lost);
    }
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && amount < 0)
            await NotifyLoss(context, Math.Min(Math.Max(0, _amountBeforeChange), Math.Max(0, -(int)amount)));
    }
    private async Task NotifyLoss(PlayerChoiceContext context, int amount)
    {
        if (amount <= 0 || Owner.Player is not { } player || CombatManager.Instance.IsOverOrEnding) return;
        var listeners = player.Piles.Where(pile => pile.IsCombatPile).SelectMany(pile => pile.Cards)
            .Cast<object>().Concat(Owner.Powers).OfType<MercuriusIllusionLossListener>().ToList();
        foreach (var listener in listeners) await listener.AfterIllusionLost(context, amount);
    }
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<IntangiblePower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<IntangiblePower>().ResolvedBigIconPath;

    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource) =>
        props.IsPoweredAttack() && (dealer == Owner || target == Owner) ? 1m : decimal.MaxValue;

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        // One charge per attack command, including multi-hit and area attacks.
        // TotalDamage includes Block: an attack absorbed by Block is still positive damage.
        if (!command.DamageProps.IsPoweredAttack()) return;
        bool affected = command.Results.SelectMany(hit => hit).Any(result =>
            result.TotalDamage > 0 && (command.Attacker == Owner || result.Receiver == Owner));
        if (!affected) return;
        Flash();
        await PowerCmd.Decrement(this);
    }
}
