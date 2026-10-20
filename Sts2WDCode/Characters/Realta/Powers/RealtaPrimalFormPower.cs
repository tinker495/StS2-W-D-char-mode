using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaPrimalFormPower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Refine), HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromCard<RealtaGuard>()];

    public async Task OnCardRefined(decimal refineAmount)
    {
        if (refineAmount <= 0m || Owner.Player == null) return;
        Flash();
        await GuardCmd.Guard(refineAmount * Amount, Owner.Player);
    }
}
