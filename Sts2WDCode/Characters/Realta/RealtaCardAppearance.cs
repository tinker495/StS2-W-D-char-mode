using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

internal static class RealtaCardAppearance
{
    internal static void ApplyTo(NCard card)
    {
        var energyLabel = card.GetNode<MegaLabel>("%EnergyLabel");
        energyLabel.MinFontSize = 14;
        energyLabel.MaxFontSize = 22;
        energyLabel.OffsetLeft = -17f;
        energyLabel.OffsetRight = 17f;
        energyLabel.OffsetTop = -8f;
        energyLabel.OffsetBottom = 28f;

        // Ancient cards have their own full-art frame and animation layers.
        if (card.Model!.Rarity == CardRarity.Ancient) return;

        card.GetNode<TextureRect>("%Frame").Material = null;
        // Native rarity materials remain on the title banner and portrait border.
        card.GetNode<NinePatchRect>("%TypePlaque").Material =
            PreloadManager.Cache.GetMaterial("res://Sts2WD/materials/realta/card_type_plaque.tres");
        var typeLabel = card.GetNode<Label>("%TypeLabel");
        typeLabel.AddThemeColorOverride("font_color", new Color(1f, 0.964706f, 0.886275f));
        typeLabel.AddThemeColorOverride("font_outline_color", new Color(0.15f, 0.06f, 0.04f));
        typeLabel.AddThemeConstantOverride("outline_size", 2);
    }
}
