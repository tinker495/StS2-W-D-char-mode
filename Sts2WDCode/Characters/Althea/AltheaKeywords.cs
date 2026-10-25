using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public static class AltheaKeywords
{
    [CustomEnum("ALTHEA_FRENZY")] [KeywordProperties(AutoKeywordPosition.After)] public static CardKeyword Frenzy;
    [CustomEnum("ALTHEA_UNFADING")] [KeywordProperties(AutoKeywordPosition.After)] public static CardKeyword Unfading;
    [CustomEnum("ALTHEA_REFORM")] public static CardKeyword Reform;
    [CustomEnum("ALTHEA_CLASH")] public static CardKeyword Clash;

    // With no chosen target, show base outcomes; Reform inherits the target's upgrade.
    internal static IEnumerable<IHoverTip> ReformHoverTips() =>
    [
        HoverTipFactory.FromKeyword(Reform),
        HoverTipFactory.FromCard<AltheaSiarcun>(),
        HoverTipFactory.FromCard<AltheaLleurh>(),
        HoverTipFactory.FromCard<AltheaMorfiella>(),
        HoverTipFactory.FromCard<AltheaPureWill>()
    ];
}
