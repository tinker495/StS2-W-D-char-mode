// Human requester: wjdrb, 2026-10-25, approved the retrospective proposal to
// exercise native NCard initialization/Reload before checking viewport overflow.
// The lightweight fixture had hidden the native node rename. These cases use
// original scenes and card lifecycle methods; only game startup and save I/O
// are isolated. The same request limits Warrior Strike to its Siarcun result.
using System.Reflection;
using BaseLib;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Test;
using Sts2WD.Sts2WDCode;
using Sts2WD.Sts2WDCode.Characters.Althea;

public partial class NativeCardLayoutTests : Node
{
    private static string _userDataDirectory = "";

    public override async void _Ready()
    {
        try
        {
            if (!OS.HasFeature("editor"))
                throw new InvalidOperationException("Use MegaDot with the editor feature to exercise NCard.Reload's node rename.");
            InitializeNativeModels();

            var viewport = new SubViewport { Size = new Vector2I(1920, 1080) };
            AddChild(viewport);
            var game = new TestGame();
            viewport.AddChild(game);
            typeof(NGame).GetProperty(nameof(NGame.Instance))!.SetValue(null, game);
            var hoverRoot = new Control();
            game.AddChild(hoverRoot);
            typeof(NGame).GetProperty(nameof(NGame.HoverTipsContainer))!.SetValue(game, hoverRoot);

            // Compare against an uninitialized original scene, independent of
            // the patched container's output. No screenshot is an oracle.
            var reference = PreloadManager.Cache.GetScene("res://scenes/ui/card_hover_tip.tscn").Instantiate<Control>();
            Vector2 nativeSize = reference.Size;
            reference.Free();
            var results = new TestResults();
            int cases = 0;
            foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080), new Vector2I(2560, 1440) })
            foreach (HoverTipAlignment alignment in new[] { HoverTipAlignment.Left, HoverTipAlignment.Right })
            foreach (int scenario in new[] { 0, 1, 2 })
            {
                viewport.Size = size;
                CardModel source = scenario == 0
                    ? ModelDb.Card<AltheaRationalWill>().ToMutable()
                    : ModelDb.Card<AltheaWarriorStrike>().ToMutable();
                if (scenario == 2)
                {
                    source.UpgradeInternal();
                    source.FinalizeUpgradeInternal();
                }
                Type[] expected = scenario == 0
                    ? [typeof(AltheaSiarcun), typeof(AltheaLleurh), typeof(AltheaMorfiella), typeof(AltheaPureWill)]
                    : [typeof(AltheaSiarcun)];
                var owner = new Control { Position = new Vector2(size.X * 0.4f, size.Y * 0.3f), Size = NCard.defaultSize };
                game.AddChild(owner);
                try
                {
                    var set = NHoverTipSet.CreateAndShow(owner, source.HoverTips, alignment)
                        ?? throw new InvalidOperationException("Native hover tips were blocked.");
                    for (int frame = 0; frame < 8; frame++)
                        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

                    var container = set.GetChildren().OfType<NHoverTipCardContainer>().Single();
                    Control[] tips = container.GetChildren().OfType<Control>().ToArray();
                    NCard[] cards = tips.SelectMany(tip => tip.GetChildren().OfType<NCard>()).ToArray();
                    CardModel[] models = cards.Select(card => card.Model
                        ?? throw new InvalidOperationException("An initialized native card has no model.")).ToArray();
                    string label = $"native {source.GetType().Name}{(source.IsUpgraded ? "+" : "")} {size} {alignment}";
                    bool initialized = cards.Length == expected.Length && cards.All(card =>
                        card.GetType() == typeof(NCard) && card.IsNodeReady() && card.Name != "Card");
                    results.Check(label + " lifecycle", initialized,
                        $"originalReadyAndRenamed={initialized} names=[{string.Join(',', cards.Select(card => card.Name))}]");
                    bool references = models.Select(model => model.GetType()).OrderBy(type => type.Name)
                        .SequenceEqual(expected.OrderBy(type => type.Name))
                        && models.All(model => model.IsUpgraded == (scenario == 2));
                    results.Check(label + " references", references,
                        $"cards=[{string.Join(',', models.Select(model => model.Id))}] upgraded={scenario == 2}");
                    bool overlap = tips.SelectMany((tip, index) => tips.Skip(index + 1)
                        .Select(other => tip.GetGlobalRect().Intersects(other.GetGlobalRect()))).Any(value => value);
                    Rect2 screen = game.GetViewportRect();
                    bool bounded = screen.Encloses(container.GetGlobalRect())
                        && tips.All(tip => screen.Encloses(tip.GetGlobalRect()));
                    bool intact = tips.Length == expected.Length && tips.All(tip => tip.Size == nativeSize);
                    results.Check(label + " layout", intact && bounded && !overlap,
                        $"size={container.Size} position={container.GlobalPosition} intact={intact} bounded={bounded} overlap={overlap}");
                    cases++;
                }
                finally
                {
                    NHoverTipSet.Remove(owner);
                    owner.QueueFree();
                }
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            GD.Print($"Native card previews: {cases} lifecycle cases, {results.Failures} failures.");
            if (results.Failures == 0) GD.Print("NATIVE_CARD_PREVIEW_TESTS_PASSED");
            GetTree().Quit(results.Failures == 0 ? 0 : 1);
        }
        catch (Exception error)
        {
            GD.PushError(error.ToString());
            GetTree().Quit(2);
        }
    }

    private static void InitializeNativeModels()
    {
        string[] arguments = OS.GetCmdlineUserArgs();
        string Argument(string name)
        {
            int index = Array.IndexOf(arguments, name);
            return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1]
                : throw new ArgumentException($"Supply {name} <pack path>.");
        }
        // BaseLib's ModConfig uses System.IO with OS.GetUserDataDir directly,
        // outside SaveManager's injectable I/O. Isolate this filesystem boundary
        // too; leave all original card/tooltip lifecycle methods untouched.
        _userDataDirectory = System.IO.Path.GetFullPath(Argument("--user-data"));
        System.IO.Directory.CreateDirectory(_userDataDirectory);
        new Harmony("sts2wd.tests.native-user-data").Patch(typeof(OS).GetMethod(nameof(OS.GetUserDataDir))!,
            prefix: new HarmonyMethod(typeof(NativeCardLayoutTests).GetMethod(nameof(UserDataDirectory), BindingFlags.Static | BindingFlags.NonPublic)!));
        GD.Print($"Native test configuration directory: {OS.GetUserDataDir()}");
        foreach (string name in new[] { "--game-pack", "--base-pack", "--mod-pack" })
            if (!ProjectSettings.LoadResourcePack(Argument(name), replaceFiles: name != "--game-pack"))
                throw new InvalidOperationException($"Cannot mount {name}: {Argument(name)}");

        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(typeof(NCard).Assembly);
        BaseLibMain.IsMainThread = true;
        var saves = new SaveManager(new MockGodotFileIo("user://native-card-tests"), forceSynchronous: true);
        SaveManager.MockInstanceForTesting(saves);
        saves.InitSettingsDataForTest();
        saves.InitPrefsDataForTest();
        saves.SettingsSave.Language = "kor";
        var mods = (List<Mod>)typeof(ModManager).GetField("_mods", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        mods.Add(new Mod { path = System.IO.Path.GetDirectoryName(Argument("--base-pack"))!,
            state = ModLoadState.Loaded, assembly = typeof(BaseLibMain).Assembly,
            manifest = new ModManifest { id = "BaseLib", name = "BaseLib", hasDll = true, hasPck = true, affectsGameplay = false } });
        mods.Add(new Mod { path = System.IO.Path.GetDirectoryName(Argument("--mod-pack"))!,
            state = ModLoadState.Loaded, assembly = typeof(MainFile).Assembly,
            manifest = new ModManifest { id = MainFile.ModId, name = MainFile.ModId, hasDll = true, hasPck = true, affectsGameplay = true } });
        typeof(ModManager).GetProperty(nameof(ModManager.State))!.SetValue(null, ModManagerState.Initialized);
        BaseLibMain.Initialize();
        MainFile.Initialize();
        LocManager.Initialize();
        ModelDb.Init();
        ModelIdSerializationCache.Init();
        ModelDb.InitIds();
    }

    private static bool UserDataDirectory(ref string __result)
    {
        __result = _userDataDirectory;
        return false;
    }
}
