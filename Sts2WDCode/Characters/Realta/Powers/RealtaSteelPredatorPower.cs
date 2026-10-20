using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaSteelPredatorPower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromCard<RealtaGuard>()];

    public async Task OnOwnerGuarded(decimal amount, Player owner)
    {
        Flash();
        var allies = owner.Creature.CombatState!.Players.Where(p => p.Creature.IsAlive && p != owner).ToList();
        foreach (var ally in allies)
            await GuardCmd.Guard(amount, ally, this);
    }
}
