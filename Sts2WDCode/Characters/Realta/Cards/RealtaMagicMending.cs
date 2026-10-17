using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaMagicMending() : RealtaCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prefs = new MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs(SelectionScreenPrompt, 1);
        var card = (await CardSelectCmd.FromCombatPile(choiceContext, Owner.PlayerCombatState!.DiscardPile, Owner, prefs)).FirstOrDefault();
        if (card == null) return;
        await CardPileCmd.Add(card, PileType.Hand);
        if (card.IsUpgradable) CardCmd.Upgrade(card);
        RefineCmd.Refine(card, DynamicVars["Refine"].IntValue);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
