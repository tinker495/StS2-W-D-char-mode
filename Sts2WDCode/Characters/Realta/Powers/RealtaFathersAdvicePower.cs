using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaFathersAdvicePower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromCard<RealtaGuard>()];

    public override LocString Description
    {
        get
        {
            LocString description = base.Description;
            description.Add("Cards", DynamicVars.Cards.IntValue);
            return description;
        }
    }

    public void AddDrawCards(int amount)
    {
        AssertMutable();
        DynamicVars.Cards.BaseValue += amount;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner || cardPlay.Card is not RealtaGuard || Owner.Player == null) return;
        Flash();
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner.Player);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner) && Owner.Player != null)
            await GuardCmd.Guard(Amount, Owner.Player);
    }
}
