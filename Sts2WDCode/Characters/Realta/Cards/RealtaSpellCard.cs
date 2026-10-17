using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Payment is captured before refund hooks; all replays share that original decision.
public abstract class RealtaSpellCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : RealtaCard(cost, type, rarity, target)
{
    protected bool SpellActive { get; private set; }
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Spell)];

    internal void CaptureSpell(int energySpent) =>
        SpellActive = Math.Max(0, Owner.PlayerCombatState!.Energy - Math.Max(0, energySpent)) == 0;
}
