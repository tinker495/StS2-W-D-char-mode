using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

internal static class RealtaSceneBridge
{
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
