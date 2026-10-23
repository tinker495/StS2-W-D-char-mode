using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaFightersFormPower : AltheaPower
{
    private sealed class Data { internal int Turns; }
    protected override object InitInternalData() => new Data();

    public override async Task AfterEnergyReset(Player player)
    {
        if (player.Creature != Owner) return;
        await PlayerCmd.GainEnergy(++GetInternalData<Data>().Turns * Amount, player);
    }

    public override Task AfterShuffle(PlayerChoiceContext context, Player shuffler)
    {
        if (shuffler.Creature == Owner) GetInternalData<Data>().Turns = 0;
        return Task.CompletedTask;
    }
}
