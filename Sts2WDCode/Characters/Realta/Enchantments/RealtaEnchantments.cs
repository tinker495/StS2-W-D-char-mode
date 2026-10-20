using System.Reflection;
using System.Reflection.Emit;
using BaseLib.Patches.Localization;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
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

    public static EnchantmentModel EnchantTemporarily(CardModel card, EnchantmentModel canonicalEnchantment, int amount,
        bool playSound = true)
    {
        EnchantmentModel enchantment = canonicalEnchantment.ToMutable();
        if (!enchantment.CanEnchant(card))
            throw new InvalidOperationException($"Cannot enchant {card.Id} with {enchantment.Id}.");

        EnchantmentModel result = AddToComposite(card, enchantment, amount, temporary: true);
        card.FinalizeUpgradeInternal();
        if (playSound && amount > 0) RealtaCardAudio.Enchant(card);
        return result;
    }

    private static bool IsLegacy(CardModel card) => card.Keywords.Contains(RealtaKeywords.Legacy);

    // Whether a Legacy card must hold the new enchantment in a composite rather than in the plain slot.
    private static bool NeedsComposite(CardModel card, EnchantmentModel enchantment)
    {
        return IsLegacy(card)
               && (card.Enchantment is RealtaCompositeEnchantment
                   || (card.Enchantment != null && card.Enchantment.GetType() != enchantment.GetType()));
    }

    private static EnchantmentModel AddToComposite(CardModel card, EnchantmentModel enchantment, int amount, bool temporary,
        bool modifyCard = true)
    {
        if (card.Enchantment is RealtaCompositeEnchantment existing)
            return existing.Add(enchantment, amount, temporary, modifyCard);

        var composite = (RealtaCompositeEnchantment)ModelDb.Enchantment<RealtaCompositeEnchantment>().ToMutable();
        if (card.Enchantment is { } previous)
        {
            card.ClearEnchantmentInternal();
            composite.Adopt(previous);
        }
        EnchantmentModel result = composite.Add(enchantment, amount, temporary, modifyCard);
        card.EnchantInternal(composite, 0m);
        composite.ModifyPendingEntries();
        return result;
    }

    private static EnchantmentModel? EnchantFromComposite(RealtaCompositeEnchantment source, CardModel card)
    {
        EnchantmentModel? result = null;
        foreach (EnchantmentModel copy in source.DetachedPermanentCopies())
        {
            if (copy.CanEnchant(card)) result = CardCmd.Enchant(copy, card, copy.Amount);
        }
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

    // The slot may also contain permanent entries; only label it temporary when temporary effects exist.
    [HarmonyPatch(typeof(EnchantmentModel), "get_HoverTip")]
    private static class TemporaryHoverTipPatch
    {
        private static void Postfix(EnchantmentModel __instance, ref HoverTip __result)
        {
            if (__instance is not RealtaCompositeEnchantment { HasTemporaryEntries: true }) return;
            __result = new HoverTip(
                new LocString("enchantments", $"{__instance.Id.Entry}.temporaryTitle"),
                new LocString("enchantments", $"{__instance.Id.Entry}.temporaryDescription"), __instance.Icon);
        }
    }

    // Keep upgrade previews and combat buffs/debuffs in the game's own colors. A value changed only by a
    // temporary enchantment is aqua, without adding the enchantment's name to the card body.
    [HarmonyPatch(typeof(DynamicVar), nameof(DynamicVar.ToHighlightedString))]
    private static class TemporaryValueColorPatch
    {
        private static void Postfix(DynamicVar __instance, AbstractModel? ____owner, ref string __result)
        {
            if (__instance.WasJustUpgraded || __instance.PreviewValue != __instance.EnchantedValue ||
                ____owner is not CardModel { Enchantment: RealtaCompositeEnchantment composite } ||
                !composite.TemporarilyModifies(__instance)) return;
            __result = $"[aqua]{((int)__instance.PreviewValue).ToString(System.Globalization.CultureInfo.InvariantCulture)}[/aqua]";
        }
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
            if (enchantment is RealtaCompositeEnchantment source)
            {
                __result = EnchantFromComposite(source, card);
                return false;
            }
            if (!NeedsComposite(card, enchantment)) return true;

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

    // The deck enchant screen previews by calling EnchantInternal on a clone, which would drop a Legacy card's
    // existing enchantments; route the preview the same way CardCmd.Enchant is routed.
    [HarmonyPatch(typeof(NEnchantPreview), nameof(NEnchantPreview.Init))]
    private static class EnchantPreviewPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo enchantInternal = AccessTools.Method(typeof(CardModel), nameof(CardModel.EnchantInternal));
            MethodInfo modifyCard = AccessTools.Method(typeof(EnchantmentModel), nameof(EnchantmentModel.ModifyCard));
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(enchantInternal))
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnchantPreviewPatch), nameof(PreviewEnchant)));
                else if (instruction.Calls(modifyCard))
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EnchantPreviewPatch), nameof(PreviewModifyCard)));
                else
                    yield return instruction;
            }
        }

        private static void PreviewEnchant(CardModel preview, EnchantmentModel enchantment, decimal amount)
        {
            if (NeedsComposite(preview, enchantment))
            {
                AddToComposite(preview, enchantment, (int)amount, temporary: false, modifyCard: false);
                return;
            }
            if (preview.Enchantment is { } same && IsLegacy(preview))
                amount += same.Amount; // Same enchantment stacks, as CardCmd.Enchant does.
            preview.EnchantInternal(enchantment, amount);
        }

        // A stacked composite entry keeps the existing model, so the new one is never attached.
        private static void PreviewModifyCard(EnchantmentModel enchantment)
        {
            if (enchantment.HasCard) enchantment.ModifyCard();
        }
    }

    // NCard refreshes the slot enchantment's variables (Dexterity on Adroit's block, ...) before drawing the
    // description; do the same for the entries a composite prints.
    [HarmonyPatch(typeof(CardModel), nameof(CardModel.UpdateDynamicVarPreview))]
    private static class EntryVarPreviewPatch
    {
        private static void Postfix(CardModel __instance, CardPreviewMode previewMode, Creature? target, DynamicVarSet dynamicVarSet)
        {
            if (__instance.Enchantment is not RealtaCompositeEnchantment composite || dynamicVarSet != composite.DynamicVars) return;
            foreach (EnchantmentModel child in composite.Children)
            {
                child.DynamicVars.ClearPreview();
                __instance.UpdateDynamicVarPreview(previewMode, target, child.DynamicVars);
            }
        }
    }
}
