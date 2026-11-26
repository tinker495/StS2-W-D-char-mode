using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace Sts2WD.Sts2WDCode.Characters;

/// <summary>
/// The game updates act lighting on SpineSprite children. Painted rest scenes opt in
/// to the same room events through their ControlRoot/Visuals node.
/// </summary>
internal static class RestSiteLightingBridge
{
    private static readonly string[] ActLoops = ["overgrowth_loop", "hive_loop", "glory_loop"];

    private static Node? RestVisuals(NRestSiteCharacter character)
    {
        if (character.Player == null) return null;
        var visuals = character.GetNodeOrNull("ControlRoot/Visuals");
        return visuals?.IsInGroup("sts2wd_rest_lighting") == true ? visuals : null;
    }

    [HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
    private static class RestActLoop
    {
        private static void Postfix(NRestSiteCharacter __instance)
        {
            var visuals = RestVisuals(__instance);
            if (visuals == null) return;
            var runState = __instance.Player.RunState;
            var clip = runState.Act is Underdocks
                ? "underdocks_loop"
                : runState.CurrentActIndex >= 0 && runState.CurrentActIndex < ActLoops.Length
                    ? ActLoops[runState.CurrentActIndex]
                    : null;
            if (clip != null)
                visuals.Call("play_variant", clip);
        }
    }

    [HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.HideFlameGlow))]
    private static class RestLightOff
    {
        private static void Postfix(NRestSiteCharacter __instance) => RestVisuals(__instance)?.Call("set_light_off", true);
    }
}
