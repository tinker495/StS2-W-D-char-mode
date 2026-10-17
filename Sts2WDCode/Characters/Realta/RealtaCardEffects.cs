using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

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

    // Let Vigor modify this attack normally without registering an attack that consumes it.
    [HarmonyPatch(typeof(VigorPower), nameof(VigorPower.BeforeAttack))]
    private static class BroomVigorPatch
    {
        private static bool Prefix(AttackCommand command, ref Task __result)
        {
            if (command.ModelSource is not RealtaBroom) return true;
            __result = Task.CompletedTask;
            return false;
        }
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
