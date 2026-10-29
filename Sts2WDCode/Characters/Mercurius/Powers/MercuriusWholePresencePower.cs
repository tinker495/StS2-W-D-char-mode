using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusWholePresencePower : MercuriusPower
{
    public Task AfterTuned() => CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    public override async Task AfterShuffle(PlayerChoiceContext context, Player shuffler)
    {
        if (shuffler == Owner.Player) await AfterTuned();
    }
}
