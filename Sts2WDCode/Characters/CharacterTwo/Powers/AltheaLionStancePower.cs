using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaLionStancePower : AltheaPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner.Creature != Owner || play.Card.Type != CardType.Attack) return;
        await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
        await PowerCmd.Apply<AltheaTemporaryStrengthPower>(context, Owner, Amount, Owner, null);
    }
}
