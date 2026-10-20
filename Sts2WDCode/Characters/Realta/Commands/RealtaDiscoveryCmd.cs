using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

internal static class RealtaDiscoveryCmd
{
    public static async Task Choose(PlayerChoiceContext context, Player player, IEnumerable<CardModel> pool,
        bool upgraded, int refinement = 0)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;
        var cards = CardFactory.GetDistinctForCombat(player, pool, 3, player.RunState.Rng.CombatCardGeneration).ToList();
        if (cards.Count == 0) return;

        // Preview all candidates, but publish upgrade/refine effects only for the card the player receives.
        var upgradedCards = upgraded ? cards.Where(card => card.IsUpgradable).ToHashSet() : [];
        foreach (var card in upgradedCards) CardCmd.Upgrade(card);
        var refinedCards = cards.Where(card => refinement > 0 && RefineCmd.CanRefine(card)).ToHashSet();
        foreach (var card in refinedCards)
            RealtaEnchantments.EnchantTemporarily(card, RefineCmd.EnchantmentFor(card), refinement, playSound: false);

        CardModel? selected = await CardSelectCmd.FromChooseACardScreen(context, cards, player, canSkip: false);
        if (selected == null) return;
        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, player);
        if (upgradedCards.Contains(selected)) await RefineCmd.OnCombatCardUpgraded(context, selected);
        if (refinedCards.Contains(selected))
        {
            RealtaCardAudio.Enchant(selected);
            await RefineCmd.OnCardRefined(context, selected, refinement);
        }
    }
}
