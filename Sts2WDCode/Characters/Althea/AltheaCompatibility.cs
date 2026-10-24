using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

internal static class AltheaCompatibility
{
    // Retain the real Attack type. Only native effect comparisons specifically asking "is Curse"
    // use the additional identity; card rendering, targeting and reform still see the original type.
    [HarmonyPatch]
    private static class NativeCurseChecks
    {
        private static readonly MethodInfo Getter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.Type));
        private static IEnumerable<MethodBase> TargetMethods()
        {
            byte[] token = BitConverter.GetBytes(Getter.MetadataToken);
            foreach (var type in typeof(CardModel).Assembly.GetTypes().Where(t =>
                t.Namespace is "MegaCrit.Sts2.Core.Models.Cards" or "MegaCrit.Sts2.Core.Models.Powers" or "MegaCrit.Sts2.Core.Models.Relics"))
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (method.ContainsGenericParameters || method.IsAbstract) continue;
                byte[]? body = method.GetMethodBody()?.GetILAsByteArray();
                if (body == null) continue;
                for (int i = 0; i + 6 < body.Length; i++)
                    if (body[i] == 0x6f && body.AsSpan(i + 1, 4).SequenceEqual(token) &&
                        body[i + 5] == (byte)(0x16 + (int)CardType.Curse))
                    { yield return method; break; }
            }
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            for (int i = 0; i + 1 < code.Count; i++)
                if (code[i].Calls(Getter) && code[i + 1].LoadsConstant((int)CardType.Curse))
                {
                    code[i].opcode = OpCodes.Call;
                    code[i].operand = AccessTools.Method(typeof(AltheaRules), nameof(AltheaRules.IsCurse));
                    code[i + 1].opcode = OpCodes.Ldc_I4_1;
                    code[i + 1].operand = null;
                }
            return code;
        }
    }

    [HarmonyPatch]
    private static class TransformUnfading
    {
        private static MethodBase TargetMethod() => typeof(CardCmd).GetMethods().Single(m => m.Name == nameof(CardCmd.Transform)
            && m.GetParameters().Length == 3 && m.GetParameters()[0].ParameterType.IsGenericType);
        private static void Prefix(ref IEnumerable<CardTransformation> transformations, out CardModel[] __state)
        {
            transformations = transformations.ToArray();
            __state = transformations.Select(t => t.Original).Where(c => c.Keywords.Contains(AltheaKeywords.Unfading)).ToArray();
        }
        private static void Postfix(CardModel[] __state, ref Task<IEnumerable<CardPileAddResult>> __result) => __result = Complete(__result, __state);
        private static async Task<IEnumerable<CardPileAddResult>> Complete(Task<IEnumerable<CardPileAddResult>> original, CardModel[] sources)
        {
            var result = await original;
            foreach (var source in sources.Where(c => c.HasBeenRemovedFromState || c.Pile == null))
                await AltheaRules.Unfading(new ThrowingPlayerChoiceContext(), source);
            return result;
        }
    }

    [HarmonyPatch(typeof(TouchOfOrobas), "get_RefinementUpgrades")]
    private static class StarterUpgrade
    {
        private static void Postfix(Dictionary<ModelId, RelicModel> __result) =>
            __result[ModelDb.Relic<AltheaWyrd>().Id] = ModelDb.Relic<AltheaBloodFrenzy>();
    }
    [HarmonyPatch(typeof(ArchaicTooth), "get_TranscendenceUpgrades")]
    private static class UltimateStrikeUpgrade
    {
        private static void Postfix(Dictionary<ModelId, CardModel> __result) =>
            __result[ModelDb.Card<AltheaRageStrike>().Id] = ModelDb.Card<AltheaUltimateStrike>();
    }
    [HarmonyPatch(typeof(WeakPower), nameof(WeakPower.ModifyDamageMultiplicative))]
    private static class WeakReversal
    {
        private static void Postfix(WeakPower __instance, Creature? dealer, ValueProp props, ref decimal __result)
        {
            if (dealer == __instance.Owner && props.IsPoweredAttack()
                && dealer.GetPower<AltheaUnrivaledBladePower>() is { } power)
                __result = power.ReverseWeakMultiplier(__result);
        }
    }
    [HarmonyPatch(typeof(VulnerablePower), nameof(VulnerablePower.ModifyDamageMultiplicative))]
    private static class VulnerableReduction
    {
        private static void Postfix(VulnerablePower __instance, Creature? target, ValueProp props, ref decimal __result)
        {
            if (target == __instance.Owner && props.IsPoweredAttack() && target.Player?.GetRelic<AltheaCrekinMacDubh>() != null) __result = 1.2m;
        }
    }
    [HarmonyPatch(typeof(FrailPower), nameof(FrailPower.ModifyBlockMultiplicative))]
    private static class FrailReduction
    {
        private static void Postfix(FrailPower __instance, Creature target, ValueProp props, ref decimal __result)
        {
            if (target == __instance.Owner && props.IsPoweredCardOrMonsterMoveBlock() && target.Player?.GetRelic<AltheaCrekinMacDubh>() != null) __result = 0.8m;
        }
    }
}
