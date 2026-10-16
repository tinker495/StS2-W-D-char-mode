using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// 담금질 (working name): block, then 개선. The block is gained even when the refine has no valid target.
public sealed class RealtaTempering() : RealtaCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    private const string RefineVar = "Refine";

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move), new DynamicVar(RefineVar, 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(RealtaKeywords.Refine),
        .. HoverTipFactory.FromEnchantment<Sharp>(DynamicVars[RefineVar].IntValue),
        .. HoverTipFactory.FromEnchantment<Adroit>(DynamicVars[RefineVar].IntValue)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await RefineCmd.Refine(choiceContext, Owner, DynamicVars[RefineVar].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[RefineVar].UpgradeValueBy(1m);
    }
}
