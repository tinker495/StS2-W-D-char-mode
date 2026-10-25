// Human requester: wjdrb, 2026-10-25. Four valid model previews had overflowed
// the viewport; they explicitly requested formal overflow checks alongside
// model contracts. Native card size is an input, never a saved output oracle.
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Sts2WD.Sts2WDCode.Cards;
using Sts2WD.Sts2WDCode.Characters.Althea;

internal static class CardLayoutContracts
{
    public static int Run(Node host, string gamePack, TestResults results)
    {
        Vector2 previewSize = ReadNativePreviewSize(gamePack);
        GD.Print($"Native preview input size: {previewSize}");
        var game = new TestGame();
        host.AddChild(game);
        typeof(NGame).GetProperty("Instance")!.SetValue(null, game);
        ModelDb.Inject(typeof(Wound));
        var patch = typeof(Sts2WDCard).Assembly.GetType("Sts2WD.Sts2WDCode.Cards.CardHoverTipLayout", throwOnError: true)!;
        new Harmony("sts2wd.tests.card-preview-layout").CreateClassProcessor(patch).Patch();

        int cases = 0;
        foreach (Vector2I viewport in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080), new Vector2I(2560, 1440) })
        {
            host.GetTree().Root.Size = viewport;
            host.GetTree().Root.ContentScaleSize = viewport;
            foreach (int count in new[] { 1, 2, 4 })
            foreach (bool modCard in new[] { true, false })
            foreach (HoverTipAlignment alignment in new[] { HoverTipAlignment.Left, HoverTipAlignment.Right })
            {
                var container = new NHoverTipCardContainer();
                game.AddChild(container);
                try
                {
                    for (int index = 0; index < count; index++)
                    {
                        var tip = new Control { Size = previewSize };
                        container.AddChild(tip);
                        var card = new TestCard { Name = "Card" };
                        typeof(NCard).GetField("_model", BindingFlags.Instance | BindingFlags.NonPublic)!
                            .SetValue(card, modCard ? ModelDb.Card<AltheaSiarcun>().ToMutable() : ModelDb.Card<Wound>().ToMutable());
                        tip.AddChild(card);
                        card.Owner = tip;
                        card.UniqueNameInOwner = true;
                    }

                    Vector2 start = new(alignment == HoverTipAlignment.Left ? viewport.X : 0, 0);
                    container.LayoutResizeAndReposition(start, alignment);
                    Control[] tips = container.GetChildren().Cast<Control>().ToArray();
                    bool intact = tips.Length == count && tips.All(tip => tip.Size == previewSize);
                    bool overlap = tips.SelectMany((tip, index) => tips.Skip(index + 1)
                        .Select(other => tip.GetRect().Intersects(other.GetRect()))).Any(value => value);
                    var screen = new Rect2(Vector2.Zero, new Vector2(viewport.X, viewport.Y));
                    bool bounded = screen.Encloses(container.GetGlobalRect())
                        && tips.All(tip => screen.Encloses(tip.GetGlobalRect()));
                    // wjdrb approved preserving native size and spacing. Vanilla
                    // groups retain the native vertical layout (4px padding).
                    bool nativeLayout = container.Size == new Vector2(previewSize.X, count * previewSize.Y + (count - 1) * 4)
                        && tips.Select((tip, index) => tip.Position == new Vector2(0, index * (previewSize.Y + 4))).All(value => value);
                    bool passed = intact && !overlap && (modCard ? bounded : nativeLayout);
                    results.Check($"layout {viewport} count={count} mod={modCard} {alignment}", passed,
                        $"size={container.Size} position={container.GlobalPosition} intact={intact} overlap={overlap} bounded={bounded} native={nativeLayout}");
                    cases++;
                }
                finally { container.Free(); }
            }
        }
        return cases;
    }

    private static Vector2 ReadNativePreviewSize(string gamePack)
    {
        // Read root Control offsets from the installed scene without loading its
        // textures, game services or saves. Fail if the scene contract changes.
        if (!ProjectSettings.LoadResourcePack(gamePack, replaceFiles: false))
            throw new InvalidOperationException($"Cannot mount installed game pack: {gamePack}");
        const string scene = "res://scenes/ui/card_hover_tip.tscn";
        string source = Godot.FileAccess.GetFileAsString(scene);
        Match root = Regex.Match(source, @"(?ms)^\[node [^\r\n]*\]\r?\n(?<properties>.*?)(?=^\[|\z)");
        if (!root.Success) throw new InvalidOperationException($"Cannot read root Control in {scene}.");
        string properties = root.Groups["properties"].Value;
        float Offset(string key, bool required = false)
        {
            Match value = Regex.Match(properties, @"(?m)^" + Regex.Escape(key) + @" = (?<value>[-\d.]+)\r?$");
            if (!value.Success && required) throw new InvalidOperationException($"Missing {key} in {scene}.");
            return value.Success ? float.Parse(value.Groups["value"].Value, CultureInfo.InvariantCulture) : 0;
        }
        if (new[] { "anchor_left", "anchor_top", "anchor_right", "anchor_bottom" }.Any(key => Offset(key) != 0))
            throw new InvalidOperationException("Preview scene now uses anchors; update the fixture only with human approval.");
        var size = new Vector2(Offset("offset_right", true) - Offset("offset_left"), Offset("offset_bottom", true) - Offset("offset_top"));
        if (size.X <= 0 || size.Y <= 0) throw new InvalidOperationException($"Invalid native preview size: {size}");
        return size;
    }
}
