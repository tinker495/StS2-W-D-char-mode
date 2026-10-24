using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaSkipHandDrawPower : AltheaPower
{
    private int _dueTurn;
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        _dueTurn = Owner.Player!.PlayerCombatState!.TurnNumber + 1;
        return Task.CompletedTask;
    }
    public override decimal ModifyHandDrawLate(Player player, decimal count) =>
        player.Creature == Owner && player.PlayerCombatState!.TurnNumber >= _dueTurn
            ? Math.Max(0, count - CombatManager.baseHandDrawCount) : count;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature == Owner && player.PlayerCombatState!.TurnNumber >= _dueTurn)
            await PowerCmd.Remove(this);
    }
}
