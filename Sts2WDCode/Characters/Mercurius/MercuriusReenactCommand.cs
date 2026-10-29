using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using Sts2WD.Sts2WDCode.Characters.Althea;
using Sts2WD.Sts2WDCode.Characters.Realta;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public static class MercuriusReenactCommand
{
    public static IEnumerable<CardModel> OtherCharacterAndColorlessCards(Player player) =>
        ModelDb.AllCharacterCardPools.Where(pool => pool != player.Character.CardPool)
            .Append(ModelDb.CardPool<ColorlessCardPool>())
            .SelectMany(pool => pool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint));

    public static bool IsEligible(CardModel card)
    {
        card = card.CanonicalInstance;
        if (card.Type is not (CardType.Attack or CardType.Skill)
            || card.Rarity is not (CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            || !card.CanBeGeneratedInCombat) return false;

        // Unknown external card implementations require an effects audit before inclusion.
        bool auditedPool = card.Pool is IroncladCardPool or SilentCardPool or DefectCardPool
            or NecrobinderCardPool or RegentCardPool or ColorlessCardPool or RealtaCardPool or AltheaCardPool or MercuriusCardPool;
        if (!auditedPool) return false;

        // Preserve native bans and additionally reject healing, even combat-pet healing.
        return card is not (Feed or HandOfGreed or Alchemize or NotYet or Spur);
    }

    public static async Task Reenact(PlayerChoiceContext context, Player player,
        IEnumerable<CardModel> candidates, bool upgraded = false)
    {
        await Choose(context, player, candidates.Select(card => card.CanonicalInstance).Where(IsEligible), upgraded);
    }

    public static Task ReenactCurse(PlayerChoiceContext context, Player player) =>
        Choose(context, player, ModelDb.CardPool<CurseCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(card => card.Type == CardType.Curse && card.Rarity == CardRarity.Curse
                && card.CanBeGeneratedByModifiers), false);

    private static async Task Choose(PlayerChoiceContext context, Player player,
        IEnumerable<CardModel> candidates, bool upgraded)
    {
        if (player.Creature.CombatState is null || CombatManager.Instance.IsOverOrEnding) return;
        var cards = CardFactory.GetDistinctForCombat(player,
            candidates,
            3, player.RunState.Rng.CombatCardGeneration).ToList();
        if (cards.Count == 0) return;
        foreach (var card in cards)
        {
            // Previewing three options must not consume relic upgrade triggers.
            if (upgraded && card.IsUpgradable)
            {
                card.UpgradeInternal();
                card.FinalizeUpgradeInternal();
            }
            card.AddKeyword(CardKeyword.Ethereal);
        }
        var selected = await CardSelectCmd.FromChooseACardScreen(context, cards, player, canSkip: true);
        if (selected == null) return;
        if (upgraded)
        {
            selected = player.Creature.CombatState.CreateCard(selected.CanonicalInstance, player);
            if (selected.IsUpgradable) CardCmd.Upgrade(selected);
            selected.AddKeyword(CardKeyword.Ethereal);
        }
        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Hand, player);
    }
}
