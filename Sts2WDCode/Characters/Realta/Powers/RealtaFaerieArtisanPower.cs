using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaFaerieArtisanPower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Refine), HoverTipFactory.FromKeyword(CardKeyword.Retain)];

    public void OnCardImproved(CardModel card)
    {
        Flash();
        card.GiveSingleTurnRetain();
    }
}
