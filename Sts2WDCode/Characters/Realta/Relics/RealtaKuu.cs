using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Replaces Bronze Watchdog: Guard 10 at combat start; retrieve Guard cards at turn end with leftover energy.
public sealed class RealtaKuu : RealtaRelic
{
    private const string CombatStartGuardVar = "CombatStartGuard";

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar(CombatStartGuardVar, 10m)];

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
            var cards = Owner.PlayerCombatState.AllCards.OfType<RealtaGuard>()
                .Where(card => card.Pile?.Type != PileType.Hand).ToList();
            await CardPileCmd.Add(cards, PileType.Hand);
        }
    }
}
