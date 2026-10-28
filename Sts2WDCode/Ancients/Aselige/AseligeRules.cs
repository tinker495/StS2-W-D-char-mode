using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Characters;
using Sts2WD.Sts2WDCode.Characters.CharacterFour;
using Sts2WD.Sts2WDCode.Characters.Mercurius;
using Sts2WD.Sts2WDCode.Characters.Althea;
using Sts2WD.Sts2WDCode.Characters.Realta;

namespace Sts2WD.Sts2WDCode.Ancients.Aselige;

// Run-independent eligibility, weighting and dialogue priority shared by the
// event adapter and its native room-generation bridge.
internal static class AseligeRules
{
    internal static bool ShouldReplaceNeow(int actIndex, IEnumerable<CharacterModel> party) =>
        actIndex == 0 && party.Any(IsModCharacter);

    private static bool IsModCharacter(CharacterModel character) =>
        character is Realta or Sts2WD.Sts2WDCode.Characters.Althea.Althea or Mercurius or CharacterFour;

    // The three pools are independent. The recipient's eligibility, rather
    // than another party member's character, controls the exclusive flute.
    // Mercurius is bound to Mercurius by the event adapter.
    internal static IReadOnlyList<IReadOnlyList<AseligeGiftOption>> GiftPools(
        ActModel firstAct, bool recipientCanReceiveDragonFlute)
    {
        List<AseligeGiftOption> first =
        [
            new(AseligeGift.RegenerationGrace, 2),
            new(AseligeGift.DeepBreath, 2),
            new(AseligeGift.HarmoniousBlessing, 2),
            new(AseligeGift.LifeSource, 2)
        ];
        if (recipientCanReceiveDragonFlute)
            first.Add(new(AseligeGift.DragonFlute, 8));

        IReadOnlyList<AseligeGiftOption> second =
        [
            new(AseligeGift.ShiningFilm, 2),
            new(AseligeGift.PurifyingBreath, 2),
            new(AseligeGift.DeepBlueScales, 2),
            new(AseligeGift.CleanPebble, 2)
        ];

        List<AseligeGiftOption> third = [];
        if (firstAct is Overgrowth)
        {
            third.Add(new(AseligeGift.SmallSprout, 2));
            third.Add(new(AseligeGift.LargeSapling, 2));
        }
        else if (firstAct is Underdocks)
        {
            third.Add(new(AseligeGift.PrettyStarfish, 2));
            third.Add(new(AseligeGift.NeatlyArrangedSalt, 2));
        }
        // Unconditional, including future first-act variants. Half the weight
        // of each ordinary candidate, not a separate 50% chance to appear.
        third.Add(new(AseligeGift.GrowingBlessing, 1));
        return [first.AsReadOnly(), second, third.AsReadOnly()];
    }

    // Native AncientDialogueSet combines character and common repeat pools.
    // The sheet instead gives character repeats priority over common repeats.
    // Resolve eligibility per encounter; never cache act-filtered dialogue on
    // a canonical Ancient model shared by subsequent runs.
    internal static IReadOnlyList<AseligeConversation> DialogueCandidates(
        CharacterModel character, ActModel firstAct, int previousVisits)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(previousVisits);
        if (previousVisits == 0)
        {
            AseligeConversation? firstVisit = character switch
            {
                Ironclad => AseligeConversation.IroncladFirstVisit,
                Silent => AseligeConversation.SilentFirstVisit,
                Defect => AseligeConversation.DefectFirstVisit,
                Regent => AseligeConversation.RegentFirstVisit,
                Necrobinder => AseligeConversation.NecrobinderFirstVisit,
                Realta => AseligeConversation.RealtaFirstVisit,
                _ => null
            };
            if (firstVisit.HasValue)
                return [firstVisit.Value];
        }
        if (character is Realta && previousVisits == 1)
            return [AseligeConversation.RealtaSecondVisit];

        if (character is Realta)
        {
            if (firstAct is Underdocks)
                return [AseligeConversation.RealtaUnderdocks];
            if (firstAct is Overgrowth)
                return [AseligeConversation.RealtaOvergrowth];
        }

        if (IsModCharacter(character))
        {
            if (firstAct is Underdocks)
                return [AseligeConversation.CommonUnderdocks];
            if (firstAct is Overgrowth)
                return [AseligeConversation.CommonOvergrowth];
        }
        return [AseligeConversation.CommonRest, AseligeConversation.CommonTower];
    }
}

internal readonly record struct AseligeGiftOption(AseligeGift Gift, int Weight);

internal enum AseligeGift
{
    RegenerationGrace,
    DeepBreath,
    HarmoniousBlessing,
    LifeSource,
    DragonFlute,
    ShiningFilm,
    PurifyingBreath,
    DeepBlueScales,
    CleanPebble,
    SmallSprout,
    LargeSapling,
    PrettyStarfish,
    NeatlyArrangedSalt,
    GrowingBlessing
}

internal enum AseligeConversation
{
    IroncladFirstVisit,
    SilentFirstVisit,
    DefectFirstVisit,
    RegentFirstVisit,
    NecrobinderFirstVisit,
    RealtaFirstVisit,
    RealtaSecondVisit,
    RealtaUnderdocks,
    RealtaOvergrowth,
    CommonUnderdocks,
    CommonOvergrowth,
    CommonRest,
    CommonTower
}
