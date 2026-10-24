using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// The card body, title banner, portrait border and type plaque are separate vanilla layers.
// Keep the game's labels, layout, rarity outlines and upgrade highlighting intact.
internal static class RealtaCardAppearance
{
    [HarmonyPatch(typeof(NCard), "Reload")]
    private static class ApplyAppearancePatch
    {
        private static void Postfix(NCard __instance)
        {
            if (!__instance.IsNodeReady() || __instance.Model is not { } model) return;

            // NCard nodes are pooled and may next display a different character's card.
            var energyLabel = __instance.GetNode<MegaLabel>("%EnergyLabel");
            var portraitBorder = __instance.GetNode<TextureRect>("%PortraitBorder");
            var typeLabel = __instance.GetNode<Label>("%TypeLabel");
            bool isRealta = model.VisualCardPool is RealtaCardPool;
            bool isAlthea = model.VisualCardPool is Althea.AltheaCardPool;
            bool customCost = isRealta || isAlthea;
            // Preserve live cost autosizing, including X, ?, and multi-digit modified costs.
            energyLabel.MinFontSize = isRealta ? 14 : isAlthea ? 18 : 22;
            energyLabel.MaxFontSize = isRealta ? 22 : isAlthea ? 26 : 32;
            energyLabel.OffsetLeft = isRealta ? -17f : -23f;
            energyLabel.OffsetRight = isRealta ? 17f : 23f;
            energyLabel.OffsetTop = isRealta ? -8f : -26f;
            energyLabel.OffsetBottom = isRealta ? 28f : 30f;
            energyLabel.AddThemeConstantOverride("outline_size", customCost ? 3 : 16);
            energyLabel.AddThemeConstantOverride("shadow_outline_size", customCost ? 3 : 16);
            energyLabel.AddThemeConstantOverride("shadow_offset_x", customCost ? 1 : 2);
            energyLabel.AddThemeConstantOverride("shadow_offset_y", customCost ? 1 : 2);
            // SetTextAutoSize does nothing when the text is unchanged on a pooled node.
            energyLabel.Call("AdjustFontSize");
            portraitBorder.SelfModulate = Colors.White;
            typeLabel.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f, 0.752941f));
            typeLabel.RemoveThemeColorOverride("font_outline_color");
            typeLabel.RemoveThemeConstantOverride("outline_size");
            if (isAlthea && model.Rarity != CardRarity.Ancient)
            {
                __instance.GetNode<TextureRect>("%Frame").Material = null;
                __instance.GetNode<NinePatchRect>("%TypePlaque").Material =
                    PreloadManager.Cache.GetMaterial("res://Sts2WD/materials/althea/type_plaque.tres");
                typeLabel.AddThemeColorOverride("font_color", new Color("eee4e4"));
                typeLabel.AddThemeColorOverride("font_outline_color", new Color("101629"));
                typeLabel.AddThemeConstantOverride("outline_size", 2);
            }
            if (!isRealta) return;

            // Ancient cards have their own full-art frame and animation layers.
            if (model.Rarity == CardRarity.Ancient) return;

            __instance.GetNode<TextureRect>("%Frame").Material = null;
            // NCard.Reload assigns the native rarity material to the banner and portrait border.
            // Keep both layers visible so Common/Uncommon/Rare remain distinguishable.
            __instance.GetNode<NinePatchRect>("%TypePlaque").Material =
                PreloadManager.Cache.GetMaterial("res://Sts2WD/materials/realta/card_type_plaque.tres");
            typeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.964706f, 0.886275f));
            typeLabel.AddThemeColorOverride("font_outline_color", new Color(0.15f, 0.06f, 0.04f));
            typeLabel.AddThemeConstantOverride("outline_size", 2);
        }
    }
}
