using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Sts2WD.Sts2WDCode.Characters.CharacterThree;

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
        var owner = Owner ?? throw new InvalidOperationException("Aselige options require an owner.");
        var pools = AseligeRules.GiftPools(owner.RunState.Acts[0], owner.Character is CharacterThree);
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

    protected override AncientDialogueSet DefineDialogues() => AseligeDialogues.CreateSet();

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
