using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaFightersBracer : AltheaRelic
{
    private bool _disabled;
    public override RelicRarity Rarity => RelicRarity.Common;
    public override Task BeforeCombatStart() { _disabled = false; return Task.CompletedTask; }
    public override Task AfterShuffle(PlayerChoiceContext context, Player player)
    {
        if (player == Owner && player.PlayerCombatState!.TurnNumber > 0) _disabled = true;
        return Task.CompletedTask;
    }
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner.Creature) && !_disabled)
            await CreatureCmd.GainBlock(Owner.Creature, 6, ValueProp.Unpowered, null);
    }
}
