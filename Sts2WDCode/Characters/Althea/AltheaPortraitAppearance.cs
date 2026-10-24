using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

// Temporary portraits retain their native composition while rarity controls the palette.
internal static class AltheaPortraitAppearance
{
    [HarmonyPatch(typeof(NCard), "Reload")]
    private static class ApplyPalette
    {
        private static void Postfix(NCard __instance)
        {
            if (!__instance.IsNodeReady() || __instance.Model is not AltheaCard card
                || card.Rarity == CardRarity.Ancient) return;
            var portrait = __instance.GetNode<TextureRect>("%Portrait");
            // Reload clears visible portraits and assigns blur to undiscovered cards.
            // Leave that blur intact; native Reload/pool reset also clears this palette on reuse.
            if (portrait.Material != null) return;
            string palette = card.Kind == AltheaCardKind.Wraith ? "wraith" : card.Rarity switch
            {
                CardRarity.Uncommon => "uncommon",
                CardRarity.Rare => "rare",
                _ => "common"
            };
            portrait.Material = PreloadManager.Cache.GetMaterial(
                $"res://Sts2WD/materials/althea/portrait_{palette}.tres");
        }
    }
}
