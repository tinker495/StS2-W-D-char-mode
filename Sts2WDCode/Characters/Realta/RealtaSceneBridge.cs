using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

/// <summary>
/// Forwards the game's Spine-only scene events to Realta's Godot-native animation scenes.
/// BaseLib already routes combat triggers, revive and the merchant clips; these three paths only
/// address direct SpineSprite children (rest act/light_off) or call SpineAnimation directly (game over).
/// </summary>
internal static class RealtaSceneBridge
{
    private static readonly string[] ActLoops = ["overgrowth_loop", "hive_loop", "glory_loop"];

    private static Node? RestVisuals(NRestSiteCharacter character) =>
        character.Player?.Character is Realta ? character.GetNodeOrNull("ControlRoot/Visuals") : null;

    [HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))]
    private static class RestActLoop
    {
        private static void Postfix(NRestSiteCharacter __instance)
        {
            var visuals = RestVisuals(__instance);
            var act = __instance.Player.RunState.CurrentActIndex;
            if (visuals != null && act >= 0 && act < ActLoops.Length)
                visuals.Call("play_variant", ActLoops[act]);
        }
    }

    [HarmonyPatch(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter.HideFlameGlow))]
    private static class RestLightOff
    {
        private static void Postfix(NRestSiteCharacter __instance) => RestVisuals(__instance)?.Call("set_light_off", true);
    }

    /// <summary>
    /// Outside combat and the merchant (rest site, events, map) the game over screen spawns fresh combat
    /// visuals and starts "die" through SpineAnimation only. Combat visuals already played their death and
    /// the merchant path reaches BaseLib's custom player, so those are left alone.
    /// </summary>
    [HarmonyPatch(typeof(NGameOverScreen), "MoveCreaturesToDifferentLayerAndDisableUi")]
    private static class GameOverDeath
    {
        private static void Postfix(NGameOverScreen __instance)
        {
            if (NCombatRoom.Instance != null || NMerchantRoom.Instance != null) return;
            var container = Traverse.Create(__instance).Field<Control>("_creatureContainer").Value;
            if (container == null) return;
            foreach (var visuals in container.GetChildren().OfType<NCreatureVisuals>())
            {
                if (visuals.HasSpineAnimation) continue;
                var player = visuals.GetNodeOrNull<AnimationPlayer>("Visuals/Rig/AnimationPlayer");
                if (player != null && player.HasAnimation("die")) player.Play("die");
            }
        }
    }
}
