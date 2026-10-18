using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// 쿠우: upgraded starter relic replacing 청동의 번견; 수호 10 at combat start and 수호 5 at turn end with leftover energy.
public sealed class RealtaKuu : RealtaRelic
{
    private const string CombatStartGuardVar = "CombatStartGuard";
    private const string TurnEndGuardVar = "TurnEndGuard";

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(CombatStartGuardVar, 10m), new DynamicVar(TurnEndGuardVar, 5m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromCard<RealtaGuard>()];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner.Creature) && Owner.PlayerCombatState!.TurnNumber <= 1)
        {
            Flash();
            await GuardCmd.Guard(DynamicVars[CombatStartGuardVar].BaseValue, Owner);
        }
    }

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner.Creature) && Owner.PlayerCombatState!.Energy > 0)
        {
            Flash();
            await GuardCmd.Guard(DynamicVars[TurnEndGuardVar].BaseValue, Owner);
        }
    }
}
