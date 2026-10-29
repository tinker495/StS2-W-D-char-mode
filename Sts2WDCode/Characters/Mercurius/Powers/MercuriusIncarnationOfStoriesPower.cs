using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusIncarnationOfStoriesPower : MercuriusPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || !MercuriusCardEffects.IsOffColor(cardPlay.Card, cardPlay.Card.Owner)) return;
        await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
        await PowerCmd.Apply<DexterityPower>(context, Owner, Amount, Owner, null);
    }
}
