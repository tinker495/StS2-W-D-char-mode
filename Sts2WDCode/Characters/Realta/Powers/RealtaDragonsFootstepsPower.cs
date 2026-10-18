using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaDragonsFootstepsPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<FlameBarrierPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<FlameBarrierPower>().ResolvedBigIconPath;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<FlameBarrierPower>(), HoverTipFactory.Static(StaticHoverTip.Block)];

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not VigorPower || power.Owner != Owner || amount >= 0m) return;
        decimal granted = -amount * Amount;
        Flash();
        await PowerCmd.Apply<FlameBarrierPower>(choiceContext, Owner, granted, Owner, null);
        await CreatureCmd.GainBlock(Owner, granted, ValueProp.Unpowered, null);
    }
}
