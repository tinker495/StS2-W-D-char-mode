using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace Sts2WD.Sts2WDCode.Cards;

[HarmonyPatch(typeof(NHoverTipCardContainer), nameof(NHoverTipCardContainer.LayoutResizeAndReposition))]
internal static class CardHoverTipLayout
{
    private static void Postfix(NHoverTipCardContainer __instance, Vector2 globalStartLocation, HoverTipAlignment alignment)
    {
        float viewportHeight = __instance.GetViewportRect().Size.Y;
        if (viewportHeight <= 0 || __instance.Size.Y <= viewportHeight) return;

        var tips = __instance.GetChildren().OfType<Control>().ToArray();
        // NCard.Reload renames the native card node in editor-capable builds.
        if (!tips.Any(tip => tip.GetChildren().OfType<NCard>().Any(card => card.Model is Sts2WDCard))) return;

        // Native previews are 323px tall and only stack vertically. Preserve their
        // size and native 4px spacing, wrapping overflow into the next column.
        const float spacing = 4f;
        Vector2 position = Vector2.Zero;
        Vector2 size = Vector2.Zero;
        float columnWidth = 0;
        foreach (var tip in tips)
        {
            if (position.Y > 0 && position.Y + tip.Size.Y > viewportHeight)
            {
                position = new Vector2(position.X + columnWidth + spacing, 0);
                columnWidth = 0;
            }
            tip.Position = position;
            size = new Vector2(Mathf.Max(size.X, position.X + tip.Size.X), Mathf.Max(size.Y, position.Y + tip.Size.Y));
            columnWidth = Mathf.Max(columnWidth, tip.Size.X);
            position.Y += tip.Size.Y + spacing;
        }
        __instance.Size = size;
        if (alignment == HoverTipAlignment.Left)
            __instance.GlobalPosition = globalStartLocation + Vector2.Left * size.X;
        else if (alignment == HoverTipAlignment.Right)
            __instance.GlobalPosition = globalStartLocation;
    }
}
