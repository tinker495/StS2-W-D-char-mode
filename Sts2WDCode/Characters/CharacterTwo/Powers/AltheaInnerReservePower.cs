using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaInnerReservePower : AltheaPower
{
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount,
        Creature? applier, CardModel? source)
    {
        if (power.Owner != Owner || amount == 0 || Owner.Player is not { } player) return;
        if (AltheaRules.IsHarmfulApplication(power, amount, applier)) await CardPileCmd.Draw(context, Amount, player);
    }
}
