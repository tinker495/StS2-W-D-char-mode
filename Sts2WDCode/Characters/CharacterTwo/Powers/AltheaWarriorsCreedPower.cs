using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaWarriorsCreedPower : AltheaPower
{
    public override async Task AfterShuffle(PlayerChoiceContext context, Player shuffler)
    {
        if (Owner.Player != shuffler) return;
        await PowerCmd.Apply<StrengthPower>(context, Owner, -Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}
