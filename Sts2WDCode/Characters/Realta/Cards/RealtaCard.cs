using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2WD.Sts2WDCode.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Inherit this class to add a card to this character's pool.
[Pool(typeof(RealtaCardPool))]
public abstract class RealtaCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : Sts2WDCard(cost, type, rarity, target)
{
    public override int MaxUpgradeLevel => Keywords.Contains(RealtaKeywords.Legacy) ? int.MaxValue : base.MaxUpgradeLevel;

    protected static bool IsDuel(Creature target) =>
        !target.HasPower<WeakPower>() && !target.HasPower<VulnerablePower>();

    protected static bool IsValor(Creature? target) =>
        target != null && (target.Block > 0m || target.GetPowerAmount<StrengthPower>() > 0m);
}
