using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

// Frame and energy styling follows the visual pool; portrait palettes follow the actual model.
internal static class AltheaCardAppearance
{
    internal static void ApplyTo(NCard card)
    {
        var energyLabel = card.GetNode<MegaLabel>("%EnergyLabel");
        energyLabel.MinFontSize = 18;
        energyLabel.MaxFontSize = 26;
        if (card.Model!.Rarity == CardRarity.Ancient) return;

        card.GetNode<TextureRect>("%Frame").Material = null;
        card.GetNode<NinePatchRect>("%TypePlaque").Material =
            PreloadManager.Cache.GetMaterial("res://Sts2WD/materials/althea/type_plaque.tres");
        var typeLabel = card.GetNode<Label>("%TypeLabel");
        typeLabel.AddThemeColorOverride("font_color", new Color("eee4e4"));
        typeLabel.AddThemeColorOverride("font_outline_color", new Color("101629"));
        typeLabel.AddThemeConstantOverride("outline_size", 2);
    }

    internal static void ApplyPortrait(NCard node, AltheaCard card)
    {
        if (card.Rarity == CardRarity.Ancient) return;
        // Approved custom portraits bypass the temporary palette.
        if (card.PortraitPath.Replace('\\', '/').StartsWith(MainFile.ResPath + "/", StringComparison.OrdinalIgnoreCase)) return;
        var portrait = node.GetNode<TextureRect>("%Portrait");
        // Native Reload clears visible portraits and assigns blur to undiscovered cards.
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
