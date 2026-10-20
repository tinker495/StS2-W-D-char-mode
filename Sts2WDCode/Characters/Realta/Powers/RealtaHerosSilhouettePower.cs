using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaHerosSilhouettePower : Sts2WDPower
{
    private sealed class Data
    {
        public bool UpgradedThisTurn;
        public bool RefinedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];
    protected override object InitInternalData() => new Data();

    public override LocString Description
    {
        get
        {
            LocString description = base.Description;
            description.Add("Cards", DynamicVars.Cards.IntValue);
            return description;
        }
    }

    // Amount counts energy rewards; base and upgraded cards contribute different draw totals.
    public void AddDrawCards(int amount)
    {
        AssertMutable();
        DynamicVars.Cards.BaseValue += amount;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        Data data = GetInternalData<Data>();
        data.UpgradedThisTurn = false;
        data.RefinedThisTurn = false;
        return Task.CompletedTask;
    }

    public async Task OnCardUpgraded()
    {
        if (Owner.Player == null) return;
        Data data = GetInternalData<Data>();
        if (data.UpgradedThisTurn) return;
        data.UpgradedThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy(Amount, Owner.Player);
    }

    public async Task OnCardRefined(PlayerChoiceContext choiceContext)
    {
        if (Owner.Player == null) return;
        Data data = GetInternalData<Data>();
        if (data.RefinedThisTurn) return;
        data.RefinedThisTurn = true;
        Flash();
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner.Player);
    }
}
