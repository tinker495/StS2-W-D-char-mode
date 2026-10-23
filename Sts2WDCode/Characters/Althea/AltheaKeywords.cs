using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public static class AltheaKeywords
{
    [CustomEnum("ALTHEA_FRENZY")] [KeywordProperties(AutoKeywordPosition.After)] public static CardKeyword Frenzy;
    [CustomEnum("ALTHEA_UNFADING")] [KeywordProperties(AutoKeywordPosition.After)] public static CardKeyword Unfading;
    [CustomEnum("ALTHEA_REFORM")] public static CardKeyword Reform;
    [CustomEnum("ALTHEA_CLASH")] public static CardKeyword Clash;
}
