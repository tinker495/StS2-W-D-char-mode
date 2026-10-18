using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// 개선 N: choose a card in hand; attacks get temporary Sharp N, everything else temporary Adroit N.
// Eligibility is the shared enchantment rule (RealtaEnchantments): an already enchanted normal card cannot be
// chosen, while 유산 can be refined repeatedly. With no valid target only the refine fizzles.
public static class RefineCmd
{
    private static int _internalUpgradeDepth;

    public static async Task Refine(PlayerChoiceContext choiceContext, Player player, int amount, AbstractModel source)
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        CardModel? card = (await CardSelectCmd.FromHand(choiceContext, player, prefs, CanRefine, source)).FirstOrDefault();
        if (card == null) return;

        await Refine(choiceContext, card, amount);
    }

    public static async Task Refine(PlayerChoiceContext choiceContext, IEnumerable<CardModel> cards, int amount)
    {
        foreach (CardModel card in cards)
            await Refine(choiceContext, card, amount);
    }

    public static async Task Refine(PlayerChoiceContext choiceContext, CardModel card, int amount)
    {
        if (!CanRefine(card)) return;

        int finalAmount = amount + (card.Owner?.Creature.GetPowerAmount<RealtaGoibniusForgingPower>() ?? 0);
        await ApplyRefine(choiceContext, card, finalAmount);

        if (card is not RealtaMoteOfFlame && card.Owner?.PlayerCombatState is { } combatState)
        {
            var motes = combatState.Hand.Cards.OfType<RealtaMoteOfFlame>().Where(CanRefine).ToList();
            foreach (var mote in motes)
                await ApplyRefine(choiceContext, mote, finalAmount);
        }
    }

    public static void RefinePermanent(CardModel card, int amount)
    {
        if (!CanRefine(card)) return;
        CardCmd.Enchant(EnchantmentFor(card).ToMutable(), card, amount);
        CardCmd.Preview(card);
    }

    public static async Task Upgrade(PlayerChoiceContext choiceContext, IEnumerable<CardModel> cards)
    {
        foreach (CardModel card in cards)
            await Upgrade(choiceContext, card);
    }

    public static async Task Upgrade(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (!card.IsUpgradable) return;
        _internalUpgradeDepth++;
        try
        {
            CardCmd.Upgrade(card);
        }
        finally
        {
            _internalUpgradeDepth--;
        }
        await OnCombatCardUpgraded(choiceContext, card);
    }

    internal static bool IsInternalUpgrade => _internalUpgradeDepth > 0;

    internal static async Task OnCombatCardUpgraded(PlayerChoiceContext choiceContext, CardModel card)
    {
        if (card.Owner?.Creature is not { } owner) return;
        owner.GetPower<RealtaFaerieArtisanPower>()?.OnCardImproved(card);
        if (owner.GetPower<RealtaOldWorkshopPower>() is { } workshop)
            await workshop.OnCardImproved(choiceContext);
    }

    private static async Task ApplyRefine(PlayerChoiceContext choiceContext, CardModel card, int amount)
    {
        RealtaEnchantments.EnchantTemporarily(card, EnchantmentFor(card), amount);
        if (card.Owner?.Creature is not { } owner) return;

        owner.GetPower<RealtaFaerieArtisanPower>()?.OnCardImproved(card);
        if (owner.GetPower<RealtaOldWorkshopPower>() is { } workshop)
            await workshop.OnCardImproved(choiceContext);
        if (owner.GetPower<RealtaHerosArmorPower>() is { } armor)
            await armor.OnCardRefined(amount);
        if (owner.GetPower<RealtaPrimalFormPower>() is { } primal)
            await primal.OnCardRefined(amount);
    }

    internal static EnchantmentModel EnchantmentFor(CardModel card) =>
        card.Type == CardType.Attack ? ModelDb.Enchantment<Sharp>() : ModelDb.Enchantment<Adroit>();

    public static bool CanRefine(CardModel card) => EnchantmentFor(card).CanEnchant(card);
}
