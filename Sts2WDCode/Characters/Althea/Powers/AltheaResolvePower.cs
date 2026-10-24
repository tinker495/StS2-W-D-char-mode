using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaResolvePower : AltheaPower
{
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount,
        Creature? applier, CardModel? source)
    {
        if (power.Owner != Owner || !AltheaRules.IsHarmfulApplication(power, amount, applier)) return;
        await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
        await CardPileCmd.Draw(context, 1, Owner.Player!);
    }
}
