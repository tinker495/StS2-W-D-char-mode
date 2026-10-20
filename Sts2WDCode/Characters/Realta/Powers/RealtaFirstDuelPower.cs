using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaFirstDuelPower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.Static(StaticHoverTip.Block)];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not StrengthPower || amount <= 0m || power.Owner.Side == Owner.Side) return;
        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, amount * Amount, Owner, null);
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (amount <= 0m || creature.Side == Owner.Side) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, amount * Amount, ValueProp.Unpowered, null);
    }
}
