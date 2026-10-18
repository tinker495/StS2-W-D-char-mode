using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

internal static class RealtaCardEffects
{
    [HarmonyPatch(typeof(CardModel), "SpendEnergy")]
    private static class SpellPaymentPatch
    {
        private static void Prefix(CardModel __instance, int amount)
        {
            if (__instance is RealtaSpellCard spell) spell.CaptureSpell(amount);
        }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    private static class SpellAutoPlayPatch
    {
        private static void Prefix(CardModel __instance, bool isAutoPlay)
        {
            if (isAutoPlay && __instance is RealtaSpellCard spell) spell.CaptureSpell(0);
        }
    }

    // Let Vigor modify Broom and Masterwork attacks normally without per-hit consumption;
    // Masterwork consumes all accumulated Vigor once after its repeats finish.
    [HarmonyPatch(typeof(VigorPower), nameof(VigorPower.BeforeAttack))]
    private static class VigorBeforeAttackPatch
    {
        private static bool Prefix(AttackCommand command, ref Task __result)
        {
            if (command.ModelSource is not (RealtaBroom or RealtaMasterworkCard)) return true;
            __result = Task.CompletedTask;
            return false;
        }
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle))]
    private static class CombatUpgradeHookPatch
    {
        private static void Prefix(IEnumerable<CardModel> cards, out List<CardModel>? __state)
        {
            __state = !RefineCmd.IsInternalUpgrade && CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding
                ? cards.Where(c => c.IsUpgradable && c.Pile != null && c.Pile.Type != PileType.Deck).ToList()
                : null;
        }

        private static void Postfix(List<CardModel>? __state)
        {
            if (__state is not { Count: > 0 }) return;
            foreach (CardModel card in __state)
            {
                if (card.IsUpgraded)
                    _ = RefineCmd.OnCombatCardUpgraded(new ThrowingPlayerChoiceContext(), card);
            }
        }
    }

    [HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
    private static class ArchaicToothTranscendencePatch
    {
        private static void Postfix(Dictionary<ModelId, CardModel> __result) =>
            __result[ModelDb.Card<RealtaFlyingGlaive>().Id] = ModelDb.Card<RealtaIordinaMacBethlehem>();
    }

    // Masterwork owns its per-repeat enchantment calls, including zero calls at X=0.
    // Replace only the wrapper's automatic invocation, leaving every enchantment implementation intact.
    [HarmonyPatch]
    private static class MasterworkEnchantmentPatch
    {
        private static MethodBase TargetMethod() =>
            AccessTools.AsyncMoveNext(AccessTools.Method(typeof(CardModel), nameof(CardModel.OnPlayWrapper)));

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var onPlay = AccessTools.Method(typeof(EnchantmentModel), nameof(EnchantmentModel.OnPlay));
            int replaced = 0;
            foreach (var instruction in instructions)
            {
                if (instruction.Calls(onPlay))
                {
                    // Keep labels and exception blocks on the original instruction.
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(MasterworkEnchantmentPatch), nameof(PlayEnchantment));
                    replaced++;
                }
                yield return instruction;
            }
            if (replaced != 1) throw new InvalidOperationException("Could not locate the card wrapper enchantment call for Masterwork.");
        }

        private static Task PlayEnchantment(EnchantmentModel enchantment, PlayerChoiceContext context, CardPlay? play) =>
            play?.Card is RealtaMasterworkCard ? Task.CompletedTask : enchantment.OnPlay(context, play);
    }
}
