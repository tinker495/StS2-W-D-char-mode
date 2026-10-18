using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaPrimalFormPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<FurnacePower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<FurnacePower>().ResolvedBigIconPath;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Refine), HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromCard<RealtaGuard>()];

    public async Task OnCardRefined(decimal refineAmount)
    {
        if (refineAmount <= 0m || Owner.Player == null) return;
        Flash();
        await GuardCmd.Guard(refineAmount * Amount, Owner.Player);
    }
}
