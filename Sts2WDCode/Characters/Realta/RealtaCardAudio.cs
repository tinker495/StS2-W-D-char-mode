using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

internal static class RealtaCardAudio
{
    // Deck upgrades already have the rest-site/event presentation. Combat upgrades only flash the hand card.
    public static void Upgrade(CardModel card)
    {
        if (card.Owner?.Character is not Realta || !CanPlay(card)
            || !CombatManager.Instance.IsInProgress || card.Pile!.Type == PileType.Deck) return;
        NDebugAudioManager.Instance?.Play(NCardSmithVfx.smithSfx, 1f, PitchVariance.Small);
    }

    public static void Enchant(CardModel card)
    {
        if (CanPlay(card)) SfxCmd.Play(FmodSfx.enchant);
    }

    private static bool CanPlay(CardModel card) =>
        !NonInteractiveMode.IsActive && !CombatManager.Instance.IsEnding
        && card.Pile != null && LocalContext.IsMine(card);
}
