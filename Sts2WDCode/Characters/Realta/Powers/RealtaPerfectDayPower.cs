using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaPerfectDayPower : CustomPowerModel
{
    private sealed class Data
    {
        public int VigorSpent;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => DynamicVars["Remaining"].IntValue;
    public override string CustomPackedIconPath => ModelDb.Power<StrengthPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<StrengthPower>().ResolvedBigIconPath;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Remaining", 10m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<StrengthPower>()];
    protected override object InitInternalData() => new Data();

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is not VigorPower || power.Owner != Owner || amount >= 0m) return;
        Data data = GetInternalData<Data>();
        data.VigorSpent += (int)-amount;
        int rewards = data.VigorSpent / 10;
        data.VigorSpent %= 10;
        DynamicVars["Remaining"].BaseValue = 10 - data.VigorSpent;
        InvokeDisplayAmountChanged();
        if (rewards == 0) return;

        Flash();
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, rewards * Amount, Owner, null);
    }
}
