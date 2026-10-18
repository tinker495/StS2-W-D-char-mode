using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaHerosArmorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<DexterityPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<DexterityPower>().ResolvedBigIconPath;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine), HoverTipFactory.Static(StaticHoverTip.Block)];

    public async Task OnCardRefined(decimal refineAmount)
    {
        if (refineAmount <= 0m) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, refineAmount * Amount, ValueProp.Unpowered, null);
    }
}
