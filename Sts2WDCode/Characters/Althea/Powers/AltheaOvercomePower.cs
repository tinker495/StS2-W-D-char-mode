using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaOvercomePower : AltheaPower
{
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner) && Owner.Player is { } player && AltheaRules.HarmfulCount(player) > 0)
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
