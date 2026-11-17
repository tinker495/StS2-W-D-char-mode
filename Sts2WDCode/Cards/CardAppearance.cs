using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;

namespace Sts2WD.Sts2WDCode.Cards;

// Native Reload restores frame, plaque and portrait materials. Restore our label overrides
// before applying the visual pool's style and the model's independent portrait treatment.
internal static class CardAppearance
{
    [HarmonyPatch(typeof(NCard), "Reload")]
    private static class ApplyAppearancePatch
    {
        private static void Postfix(NCard __instance)
        {
            if (!__instance.IsNodeReady() || __instance.Model is not { } model) return;

            var energyLabel = __instance.GetNode<MegaLabel>("%EnergyLabel");
            ResetAppearance(__instance, energyLabel);
            if (model.VisualCardPool is ICardPoolAppearance appearance)
            {
                energyLabel.AddThemeConstantOverride("outline_size", 3);
                energyLabel.AddThemeConstantOverride("shadow_outline_size", 3);
                energyLabel.AddThemeConstantOverride("shadow_offset_x", 1);
                energyLabel.AddThemeConstantOverride("shadow_offset_y", 1);
                appearance.ApplyAppearance(__instance);
            }
            if (model is Sts2WDCard card) card.ApplyPortraitAppearance(__instance);

            // SetTextAutoSize skips unchanged text when a pooled node is reused.
            energyLabel.Call("AdjustFontSize");
        }
    }

    private static void ResetAppearance(NCard card, MegaLabel energyLabel)
    {
        energyLabel.MinFontSize = 22;
        energyLabel.MaxFontSize = 32;
        energyLabel.OffsetLeft = -23f;
        energyLabel.OffsetRight = 23f;
        energyLabel.OffsetTop = -26f;
        energyLabel.OffsetBottom = 30f;
        energyLabel.AddThemeConstantOverride("outline_size", 16);
        energyLabel.AddThemeConstantOverride("shadow_outline_size", 16);
        energyLabel.AddThemeConstantOverride("shadow_offset_x", 2);
        energyLabel.AddThemeConstantOverride("shadow_offset_y", 2);
        card.GetNode<TextureRect>("%PortraitBorder").SelfModulate = Colors.White;
        var typeLabel = card.GetNode<Label>("%TypeLabel");
        typeLabel.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f, 0.752941f));
        typeLabel.RemoveThemeColorOverride("font_outline_color");
        typeLabel.RemoveThemeConstantOverride("outline_size");
    }
}
