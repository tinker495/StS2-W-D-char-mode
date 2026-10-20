using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Keep the native power instances, stacking, hooks and audio; customize their titles and icon paths.
internal static class RealtaPowerPresentation
{
    private static string? NativeIconName(PowerModel power)
    {
        if (power is FlameBarrierPower) return "realta_flame_power";
        if (!power.IsMutable || power.Owner?.Player?.Character is not Realta) return null;
        return power switch
        {
            BlurPower => "realta_rinnal_bodb_dearg_power",
            ReflectPower => "realta_spatial_warp_power",
            _ => null
        };
    }

    [HarmonyPatch(typeof(PowerModel), "get_PackedIconPath")]
    private static class NativeSmallIconPatch
    {
        private static bool Prefix(PowerModel __instance, ref string __result)
        {
            if (NativeIconName(__instance) is not { } name) return true;
            __result = $"res://Sts2WD/images/powers/{name}.png";
            return false;
        }
    }

    // Native mutable clones inherit the canonical model's cached resolved path. Resolve the
    // owner-dependent icon before that cache so Blur/Reflect show the correct tooltip and VFX art.
    [HarmonyPatch(typeof(PowerModel), "get_ResolvedBigIconPath")]
    private static class NativeBigIconPatch
    {
        private static bool Prefix(PowerModel __instance, ref string __result)
        {
            if (NativeIconName(__instance) is not { } name) return true;
            __result = $"res://Sts2WD/images/powers/big/{name}.png";
            return false;
        }
    }

    [HarmonyPatch(typeof(PowerModel), "get_Title")]
    private static class PowerTitlePatch
    {
        private static void Postfix(PowerModel __instance, ref LocString __result)
        {
            // Include the canonical model so card hover tips and the applied buff use the same term.
            if (__instance is FlameBarrierPower)
            {
                __result = new LocString("powers", "STS2WD-REALTA_FLAME.title");
                return;
            }

            if (!__instance.IsMutable || __instance.Owner?.Player?.Character is not Realta) return;
            __result = __instance switch
            {
                BlurPower => ModelDb.Card<RealtaRinnalBodbDearg>().TitleLocString,
                ReflectPower => ModelDb.Card<RealtaSpatialWarp>().TitleLocString,
                _ => __result
            };
        }
    }
}
