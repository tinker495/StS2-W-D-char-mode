using BaseLib.Patches.Localization;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs.History;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Card-level owner of Realta's enchantment rules (docs/adr/0003):
// - a normal card keeps the base-game single slot, so an enchanted card cannot be enchanted again;
// - a 유산 card ignores that limit for every source, permanent or temporary;
// - temporary enchantments (개선) are added only to combat copies of cards.
// Whatever does not fit one permanent slot enchantment is held by RealtaCompositeEnchantment in the slot.
public static class RealtaEnchantments
{
    private const string HookSubscriptionId = "Sts2WD.RealtaEnchantments";

    public static void Initialize()
    {
        ModHelper.SubscribeForRunStateHooks(HookSubscriptionId, runState =>
            runState.Players.SelectMany(p => p.Deck.Cards).SelectMany(CompositeChildren));
        ModHelper.SubscribeForCombatStateHooks(HookSubscriptionId, combatState =>
            combatState.Players.SelectMany(p => p.PlayerCombatState?.AllCards ?? []).SelectMany(CompositeChildren));
        DescriptionOverrides.CustomizeDescriptionPost += AppendCompositeEntries;
    }

    public static EnchantmentModel EnchantTemporarily(CardModel card, EnchantmentModel canonicalEnchantment, int amount)
    {
        EnchantmentModel enchantment = canonicalEnchantment.ToMutable();
        if (!enchantment.CanEnchant(card))
            throw new InvalidOperationException($"Cannot enchant {card.Id} with {enchantment.Id}.");

        EnchantmentModel result = AddToComposite(card, enchantment, amount, temporary: true);
        card.FinalizeUpgradeInternal();
        return result;
    }

    private static bool IsLegacy(CardModel card) => card.Keywords.Contains(RealtaKeywords.Legacy);

    private static EnchantmentModel AddToComposite(CardModel card, EnchantmentModel enchantment, int amount, bool temporary)
    {
        if (card.Enchantment is RealtaCompositeEnchantment existing)
            return existing.Add(enchantment, amount, temporary);

        var composite = (RealtaCompositeEnchantment)ModelDb.Enchantment<RealtaCompositeEnchantment>().ToMutable();
        if (card.Enchantment is { } previous)
        {
            card.ClearEnchantmentInternal();
            composite.Adopt(previous);
        }
        EnchantmentModel result = composite.Add(enchantment, amount, temporary);
        card.EnchantInternal(composite, 0m);
        composite.ModifyPendingEntries();
        return result;
    }

    private static IEnumerable<AbstractModel> CompositeChildren(CardModel card)
    {
        return card.Enchantment is RealtaCompositeEnchantment composite ? composite.Children : [];
    }

    private static void AppendCompositeEntries(CardModel card, Creature? target, ref string description)
    {
        if (card.Enchantment is RealtaCompositeEnchantment composite)
            description += composite.EntriesText();
    }

    // The base check refuses any card whose slot is taken; 유산 keeps every other base-game rule.
    [HarmonyPatch(typeof(EnchantmentModel), nameof(EnchantmentModel.CanEnchant))]
    private static class LegacyCanEnchantPatch
    {
        private static void Postfix(EnchantmentModel __instance, CardModel card, ref bool __result)
        {
            if (__result || card.Enchantment == null || !IsLegacy(card)) return;
            __result = card.Type is not (CardType.Status or CardType.Curse or CardType.Quest)
                       && __instance.CanEnchantCardType(card.Type)
                       && !(card.Pile?.Type == PileType.Deck && card.Keywords.Contains(CardKeyword.Unplayable));
        }
    }

    // Base CardCmd.Enchant only stacks the same enchantment type; route other enchantments on 유산 into the composite.
    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Enchant), typeof(EnchantmentModel), typeof(CardModel), typeof(decimal))]
    private static class LegacyEnchantPatch
    {
        private static bool Prefix(EnchantmentModel enchantment, CardModel card, decimal amount, ref EnchantmentModel? __result)
        {
            bool needsComposite = card.Enchantment is RealtaCompositeEnchantment
                                  || (card.Enchantment != null && card.Enchantment.GetType() != enchantment.GetType());
            if (!needsComposite || !IsLegacy(card)) return true;

            enchantment.AssertMutable();
            if (!enchantment.CanEnchant(card))
                throw new InvalidOperationException($"Cannot enchant {card.Id} with {enchantment.Id}.");

            __result = AddToComposite(card, enchantment, (int)amount, temporary: false);
            card.FinalizeUpgradeInternal();
            if (card.Pile?.Type == PileType.Deck)
            {
                card.Owner.RunState.CurrentMapPointHistoryEntry?.GetEntry(card.Owner.NetId).CardsEnchanted
                    .Add(new CardEnchantmentHistoryEntry(card, enchantment.Id));
            }
            return false;
        }
    }
}
