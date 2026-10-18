using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaHerosSilhouettePower : CustomPowerModel
{
    private sealed class Data
    {
        public bool UpgradedThisTurn;
        public bool RefinedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<MasterPlannerPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<MasterPlannerPower>().ResolvedBigIconPath;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];
    protected override object InitInternalData() => new Data();

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
        await CardPileCmd.Draw(choiceContext, Amount, Owner.Player);
    }
}
