using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.addons.mega_text;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Keep the native counter, energy subscriptions, zero-energy shading, gain VFX and entrance animations.
internal static class RealtaEnergyCounterAppearance
{
    [HarmonyPatch(typeof(NEnergyCounter), nameof(NEnergyCounter._Ready))]
    private static class ApplyAppearancePatch
    {
        private static void Postfix(NEnergyCounter __instance, Player ____player)
        {
            if (____player.Character is not Realta) return;

            __instance.GetNode<TextureRect>("Layers/Layer1").Texture =
                PreloadManager.Cache.GetTexture2D("res://Sts2WD/images/charui/realta/combat_energy.png");
            __instance.GetNode<Control>("Layers/RotationLayers").Visible = false;
            __instance.GetNode<Control>("Layers/Layer4").Visible = false;
            __instance.GetNode<Control>("Layers/Layer5").Visible = false;

            var label = __instance.GetNode<MegaLabel>("Label");
            label.SetAnchorsPreset(Control.LayoutPreset.Center);
            label.OffsetLeft = -24f;
            label.OffsetRight = 30f;
            label.OffsetTop = -2f;
            label.OffsetBottom = 42f;
            label.MinFontSize = 16;
            label.MaxFontSize = 26;
            label.AddThemeConstantOverride("outline_size", 3);
            label.AddThemeConstantOverride("shadow_outline_size", 3);
            label.AddThemeConstantOverride("shadow_offset_x", 1);
            label.AddThemeConstantOverride("shadow_offset_y", 1);
            label.Call("AdjustFontSize");
        }
    }
}
