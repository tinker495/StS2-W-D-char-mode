using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Blitz;
using MegaCrit.Sts2.Core.Runs;
using Sts2WD.Sts2WDCode.Characters.Mercurius;

namespace Sts2WD.Sts2WDCode.Ancients.Aselige;

public sealed class Aselige : CustomAncientModel
{
    public override string CustomScenePath => MainFile.ResPath + "/scenes/ancients/aselige.tscn";
    public override string CustomMapIconPath => MainFile.ResPath + "/images/ancients/aselige_icon.png";
    public override string CustomMapIconOutlinePath => MainFile.ResPath + "/images/ancients/aselige_icon_outline.png";
    public override string CustomRunHistoryIconPath => CustomMapIconPath;
    public override string CustomRunHistoryIconOutlinePath => CustomMapIconOutlinePath;
    public override Color DialogueColor => new("294536");
    public override bool IsValidForAct(ActModel act) => false;

    // AllPossibleOptions also needs the conditionally offered relics for the
    // compendium/debug picker. Actual rolls use the recipient and first act.
    protected override OptionPools MakeOptionPools => new(MakePool(Enum.GetValues<AseligeGift>().Select(Gift).ToArray()));

    public override IEnumerable<EventOption> AllPossibleOptions => Enum.GetValues<AseligeGift>()
        .Select(gift => RelicOption(Gift(gift).ToMutable()));

    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        var owner = RequireOwner();
        if (owner.RunState.RunModels.OfType<BlitzRunModel>().Any())
        {
            // EventModel.OptionKey omits BaseLib's STS2WD- mod prefix; resolve keys from Id.Entry directly.
            return
            [
                new EventOption(
                    this,
                    OnBlitzOptionSelected,
                    L10NLookup($"{Id.Entry}.pages.INITIAL.options.BLITZ.title"),
                    L10NLookup($"{Id.Entry}.pages.INITIAL.options.BLITZ.description"),
                    "BLITZ",
                    Array.Empty<IHoverTip>())
            ];
        }
        if (owner.RunState.Modifiers.Count > 0)
            return GenerateNeowModifiers(owner);

        var pools = AseligeRules.GiftPools(owner.RunState.Acts[0], owner.Character is Mercurius);
        return pools.Select(pool =>
        {
            var roll = Rng.NextInt(pool.Sum(option => option.Weight));
            foreach (var option in pool)
            {
                roll -= option.Weight;
                if (roll < 0) return RelicOption(Gift(option.Gift).ToMutable());
            }
            throw new InvalidOperationException("Empty Aselige gift pool.");
        }).ToArray();
    }

    public override void ConfigureBackgroundScene(Control backgroundScene)
    {
        base.ConfigureBackgroundScene(backgroundScene);
        BindFullscreenBackground(backgroundScene);
    }

    protected override AncientDialogueSet DefineDialogues() => AseligeDialogues.CreateSet();

    private Player RequireOwner() => Owner ?? throw new InvalidOperationException("Aselige options require an owner.");

    private async Task OnBlitzOptionSelected()
    {
        var owner = RequireOwner();
        await BlitzRunModel.GiveNeowRewards(owner);
        var modifiers = GenerateNeowModifiers(owner);
        if (modifiers.Count > 0)
            SetEventState(InitialDescription, modifiers);
        else
            // Match native Neow: finish directly so Blitz card picks are not duplicated into AncientChoices.
            SetEventFinished(L10NLookup($"{Id.Entry}.pages.BLITZ_DONE.description"));
    }

    private IReadOnlyList<EventOption> GenerateNeowModifiers(Player owner)
    {
        List<EventOption> options = [];
        foreach (var modifier in owner.RunState.Modifiers)
        {
            var neowOption = modifier.GenerateNeowOption(this);
            if (neowOption == null) continue;
            var index = options.Count;
            options.Add(new EventOption(
                this,
                () => OnModifierOptionSelected(neowOption, options, index),
                modifier.NeowOptionTitle,
                modifier.NeowOptionDescription,
                modifier.Id.Entry,
                modifier.HoverTips.ToArray()));
        }
        return options.Count > 0 ? [options[0]] : [];
    }

    private async Task OnModifierOptionSelected(Func<Task> modifierFunc, List<EventOption> options, int index)
    {
        await modifierFunc();
        if (index + 1 < options.Count)
            SetEventState(InitialDescription, [options[index + 1]]);
        else
            SetEventFinished(L10NLookup($"{Id.Entry}.pages.DONE.description"));
    }

    // NAncientBgContainer scales and offsets children by aspect ratio (e.g. 0.89x, +40Y at 16:9).
    // Static binding avoids capturing Aselige/RunState on the persistent root Window.
    private static void BindFullscreenBackground(Control backgroundScene)
    {
        Window? trackedWindow = null;
        var syncCallable = Callable.From(SyncLayout);

        void SyncLayout()
        {
            if (!GodotObject.IsInstanceValid(backgroundScene) || !backgroundScene.IsInsideTree()) return;
            if (backgroundScene.GetParent() is not Control parent) return;
            var targetSize = (parent.GetParent() as Control)?.Size ?? backgroundScene.GetViewportRect().Size;
            if (targetSize.X <= 0f || targetSize.Y <= 0f) return;
            var parentTransform = parent.GetTransform();
            if (Mathf.IsZeroApprox(parentTransform.Determinant())) return;

            var inv = parentTransform.AffineInverse();
            var c0 = inv * Vector2.Zero;
            var c1 = inv * new Vector2(targetSize.X, 0f);
            var c2 = inv * new Vector2(0f, targetSize.Y);
            var c3 = inv * targetSize;
            var minX = Math.Min(Math.Min(c0.X, c1.X), Math.Min(c2.X, c3.X)) - 1f;
            var minY = Math.Min(Math.Min(c0.Y, c1.Y), Math.Min(c2.Y, c3.Y)) - 1f;
            var maxX = Math.Max(Math.Max(c0.X, c1.X), Math.Max(c2.X, c3.X)) + 1f;
            var maxY = Math.Max(Math.Max(c0.Y, c1.Y), Math.Max(c2.Y, c3.Y)) + 1f;

            backgroundScene.AnchorLeft = 0f;
            backgroundScene.AnchorTop = 0f;
            backgroundScene.AnchorRight = 0f;
            backgroundScene.AnchorBottom = 0f;
            backgroundScene.Position = new Vector2(minX, minY);
            backgroundScene.Size = new Vector2(maxX - minX, maxY - minY);
        }

        void DisconnectWindow()
        {
            if (trackedWindow != null && GodotObject.IsInstanceValid(trackedWindow) &&
                trackedWindow.IsConnected(Viewport.SignalName.SizeChanged, syncCallable))
                trackedWindow.Disconnect(Viewport.SignalName.SizeChanged, syncCallable);
            trackedWindow = null;
        }

        void OnTreeEntered()
        {
            DisconnectWindow();
            trackedWindow = backgroundScene.GetTree()?.Root;
            SyncLayout();
            syncCallable.CallDeferred();
            // Deferred connection ensures SyncLayout runs after synchronous NAncientBgContainer.OnWindowChange().
            if (trackedWindow != null && !trackedWindow.IsConnected(Viewport.SignalName.SizeChanged, syncCallable))
                trackedWindow.Connect(Viewport.SignalName.SizeChanged, syncCallable, (uint)GodotObject.ConnectFlags.Deferred);
        }

        backgroundScene.TreeEntered += OnTreeEntered;
        backgroundScene.TreeExited += DisconnectWindow;
        if (backgroundScene.IsInsideTree()) OnTreeEntered();
    }

    private static RelicModel Gift(AseligeGift gift) => gift switch
    {
        AseligeGift.RegenerationGrace => ModelDb.Relic<AseligeRegenerationGrace>(),
        AseligeGift.DeepBreath => ModelDb.Relic<AseligeDeepBreath>(),
        AseligeGift.HarmoniousBlessing => ModelDb.Relic<AseligeHarmoniousBlessing>(),
        AseligeGift.LifeSource => ModelDb.Relic<AseligeLifeSource>(),
        AseligeGift.DragonFlute => ModelDb.Relic<AseligeEmbracingFlute>(),
        AseligeGift.ShiningFilm => ModelDb.Relic<AseligeShiningFilm>(),
        AseligeGift.PurifyingBreath => ModelDb.Relic<AseligePurifyingBreath>(),
        AseligeGift.DeepBlueScales => ModelDb.Relic<AseligeDeepBlueScales>(),
        AseligeGift.CleanPebble => ModelDb.Relic<AseligeCleanPebble>(),
        AseligeGift.SmallSprout => ModelDb.Relic<AseligeSmallSprout>(),
        AseligeGift.LargeSapling => ModelDb.Relic<AseligeLargeSapling>(),
        AseligeGift.PrettyStarfish => ModelDb.Relic<AseligePrettyStarfish>(),
        AseligeGift.NeatlyArrangedSalt => ModelDb.Relic<AseligeNeatlyArrangedSalt>(),
        AseligeGift.GrowingBlessing => ModelDb.Relic<AseligeGrowingBlessing>(),
        _ => throw new ArgumentOutOfRangeException(nameof(gift))
    };
}
