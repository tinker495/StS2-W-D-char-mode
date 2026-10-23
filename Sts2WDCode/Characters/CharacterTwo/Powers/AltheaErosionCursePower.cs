using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaErosionCursePower : AltheaPower
{
    public override bool ShouldDie(Creature creature) => creature != Owner;

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Owner) return;
        int strength = Amount;
        await PowerCmd.Remove(this);
        await CreatureCmd.Heal(Owner, Math.Max(1, Owner.MaxHp * 0.3m));
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, strength, Owner, null);
    }
}
