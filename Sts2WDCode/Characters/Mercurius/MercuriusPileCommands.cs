using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public static class MercuriusPileCommands
{
    public static async Task<CardModel?> DrawBottom(PlayerChoiceContext context, Player player)
    {
        if (player.Creature.CombatState is not { } combat) return null;
        if (CombatManager.Instance.IsOverOrEnding || player.PlayerCombatState!.Hand.Cards.Count >= CardPile.MaxCardsInHand
            || !Hook.ShouldDraw(combat, player, false, out _))
            return (await CardPileCmd.Draw(context, 1, player)).FirstOrDefault();
        await CardPileCmd.ShuffleIfNecessary(context, player);
        if (!Hook.ShouldDraw(combat, player, false, out _))
            return (await CardPileCmd.Draw(context, 1, player)).FirstOrDefault();
        var bottom = player.PlayerCombatState.DrawPile.Cards.LastOrDefault();
        if (bottom == null) return null;
        await CardPileCmd.Add(bottom, PileType.Draw, CardPilePosition.Top, skipVisuals: true);
        // Reuse the native draw command so draw prevention, history and Songs remain intact.
        return (await CardPileCmd.Draw(context, 1, player)).FirstOrDefault();
    }

    public static async Task ShuffleDraw(PlayerChoiceContext context, Player player)
    {
        if (CombatManager.Instance.IsOverOrEnding || player.Creature.CombatState is not { } combat) return;
        var pile = player.PlayerCombatState!.DrawPile;
        var cards = pile.Cards.ToList();
        cards.StableShuffle(player.RunState.Rng.Shuffle);
        Hook.ModifyShuffleOrder(combat, player, cards, false);
        foreach (var card in pile.Cards.ToList()) pile.RemoveInternal(card, silent: true);
        foreach (var card in cards) pile.AddInternal(card, -1, silent: true);
        await Hook.AfterShuffle(combat, context, player);
    }
}
