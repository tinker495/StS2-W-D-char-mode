using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaDisarmPower : AltheaPower
{
    public override async Task AfterDamageGiven(PlayerChoiceContext context, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? source)
    {
        if (dealer != Owner || (!props.IsPoweredAttack() && source is not AltheaCard
            { Kind: AltheaCardKind.ScabbardSwing or AltheaCardKind.Rawenaid })) return;
        await PowerCmd.Apply<MonarchsGazeStrengthDownPower>(context, target, Amount, Owner, null);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) await PowerCmd.Remove(this);
    }
}
