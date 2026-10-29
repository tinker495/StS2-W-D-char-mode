using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusPhantomDuplicatePower : MercuriusPower
{
    private CardPlay? _firstPlay;
    private int PlaysThisTurn => CombatManager.Instance.History.CardPlaysStarted.Count(entry => entry.Actor == Owner
        && entry.CardPlay.IsFirstInSeries && entry.HappenedThisTurn(Owner.CombatState!));
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        // Native history is recorded after BeforeCardPlayed. Capture before nested auto-plays can start.
        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.IsFirstInSeries && PlaysThisTurn == 0) _firstPlay = cardPlay;
        return Task.CompletedTask;
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !cardPlay.IsFirstInSeries) return;
        if (_firstPlay != cardPlay && !(_firstPlay == null && PlaysThisTurn == 1)) return;
        _firstPlay = null;
        for (int i = 0; i < Amount; i++) await MercuriusCardEffects.CopyToHand(cardPlay.Card, cardPlay.Card.Owner);
    }
}
