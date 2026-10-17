using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaMagicBronze() : RealtaCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override bool HasEnergyCostX => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Masterwork), HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int repeats = ResolveEnergyXValue();
        for (int i = 0; i < repeats && !Owner.Creature.IsDead; i++)
        {
            if (MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsOverOrEnding) break;
            await RefineCmd.Refine(choiceContext, Owner, DynamicVars["Refine"].IntValue, this);
            if (Enchantment != null && !Owner.Creature.IsDead)
            {
                await Enchantment.OnPlay(choiceContext, cardPlay);
                Enchantment.InvokeExecutionFinished();
            }
        }
    }
    protected override void OnUpgrade() => DynamicVars["Refine"].UpgradeValueBy(1m);
}
