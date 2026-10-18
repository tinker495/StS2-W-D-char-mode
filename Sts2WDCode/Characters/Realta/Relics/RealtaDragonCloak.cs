using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaDragonCloak : RealtaRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<VigorPower>(5m), new PowerVar<FlameBarrierPower>(5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<FlameBarrierPower>()];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState!.TurnNumber > 1) return;
        Flash();
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<VigorPower>(context, Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, null);
        await PowerCmd.Apply<FlameBarrierPower>(context, Owner.Creature, DynamicVars["FlameBarrierPower"].BaseValue, Owner.Creature, null);
    }
}
