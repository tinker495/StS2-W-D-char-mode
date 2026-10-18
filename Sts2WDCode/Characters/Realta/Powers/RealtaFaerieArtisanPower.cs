using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaFaerieArtisanPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => ModelDb.Power<WellLaidPlansPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<WellLaidPlansPower>().ResolvedBigIconPath;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Refine), HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    public void OnCardImproved(CardModel card)
    {
        Flash();
        card.GiveSingleTurnRetain();
    }
}
