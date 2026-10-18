using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaBoundaryReforging() : RealtaCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 5m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1);
        var card = (await CardSelectCmd.FromCombatPile(choiceContext, Owner.PlayerCombatState!.ExhaustPile, Owner, prefs)).FirstOrDefault();
        if (card == null) return;
        await CardPileCmd.Add(card, PileType.Hand);
        await RefineCmd.Upgrade(choiceContext, card);
        await RefineCmd.Refine(choiceContext, card, DynamicVars["Refine"].IntValue);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars["Refine"].UpgradeValueBy(2m);
    }
}
