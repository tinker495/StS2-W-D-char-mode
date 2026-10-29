using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

internal static class MercuriusCardEffects
{
    public static bool IsOffColor(CardModel card, Player owner) => card.Pool != owner.Character.CardPool;
    public static bool HasDominion(CardModel played) => played.Owner.PlayerCombatState!.Hand.Cards.All(card => card == played);

    public static async Task CopyToHand(CardModel original, Player recipient)
    {
        var copy = original.CreateClone();
        if (copy.Owner != recipient)
        {
            copy.Owner = null!;
            copy.Owner = recipient;
        }
        copy.AddKeyword(CardKeyword.Ethereal);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, recipient);
    }
}
