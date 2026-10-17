using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public static class RealtaKeywords
{
    // 유산: exempt from the one-enchantment limit, so it can take the same enchantment repeatedly.
    [CustomEnum("REALTA_LEGACY")] [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Legacy;

    // Hover-tip only: these explain card/relic text and are never put on a card.
    [CustomEnum("REALTA_REFINE")] public static CardKeyword Refine;
    [CustomEnum("REALTA_GUARD")] public static CardKeyword Guard;
    [CustomEnum("REALTA_SPELL")] public static CardKeyword Spell;
    [CustomEnum("REALTA_MASTERWORK")] public static CardKeyword Masterwork;
    [CustomEnum("REALTA_DUEL")] public static CardKeyword Duel;
    [CustomEnum("REALTA_VALOR")] public static CardKeyword Valor;
}
