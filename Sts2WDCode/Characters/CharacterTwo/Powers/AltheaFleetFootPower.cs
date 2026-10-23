using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaFleetFootPower : AltheaPower
{
    public override async Task AfterShuffle(PlayerChoiceContext context, Player shuffler)
    {
        if (Owner.Player == shuffler) await PowerCmd.Remove(this);
    }
    public override async Task AfterEnergyReset(Player player)
    {
        if (Owner.Player != player) return;
        int cards = Amount;
        await PowerCmd.Remove(this);
        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), cards, player);
    }
}
