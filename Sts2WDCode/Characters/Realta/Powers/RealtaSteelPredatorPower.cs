using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaSteelPredatorPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => ModelDb.Power<HammerTimePower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<HammerTimePower>().ResolvedBigIconPath;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromCard<RealtaGuard>()];

    public async Task OnOwnerGuarded(decimal amount, Player owner)
    {
        Flash();
        var allies = owner.Creature.CombatState!.Players.Where(p => p.Creature.IsAlive && p != owner).ToList();
        foreach (var ally in allies)
            await GuardCmd.Guard(amount, ally, this);
    }
}
