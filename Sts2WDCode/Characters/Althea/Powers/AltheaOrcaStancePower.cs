using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaOrcaStancePower : AltheaPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner.Creature != Owner || play.Card.Type != CardType.Attack) return;
        await PowerCmd.Apply<DexterityPower>(context, Owner, Amount, Owner, null);
        await PowerCmd.Apply<AltheaTemporaryDexterityPower>(context, Owner, Amount, Owner, null);
    }
}
