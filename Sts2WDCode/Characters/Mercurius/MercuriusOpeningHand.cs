using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

internal static class MercuriusOpeningHand
{
    internal static async Task Replace(PlayerChoiceContext context, Player owner)
    {
        // Snapshot once: replacements are not inspected recursively.
        var cards = owner.PlayerCombatState!.Hand.Cards.Where(card =>
            card.Keywords.Contains(CardKeyword.Unplayable) ||
            card.Keywords.Contains(CardKeyword.Ethereal)).ToArray();
        if (cards.Length > 0) await CardCmd.DiscardAndDraw(context, cards, cards.Length);
    }
}
