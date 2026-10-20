using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaWorthyRivalPower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.Static(StaticHoverTip.Block)];

    public override decimal ModifyHandDraw(Player player, decimal count) =>
        player == Owner.Player && CombatState.HittableEnemies.Any(e => e.Block > 0m || e.GetPowerAmount<StrengthPower>() > 0m)
            ? count + Amount
            : count;

    public override Task AfterModifyingHandDraw()
    {
        Flash();
        return Task.CompletedTask;
    }
}
