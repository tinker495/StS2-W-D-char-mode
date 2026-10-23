using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaFightingSpiritPower : AltheaPower
{
    public override async Task AfterEnergyReset(Player player)
    {
        if (Owner.Player == player && AltheaRules.Clash(player)) await PlayerCmd.GainEnergy(Amount, player);
    }
}
