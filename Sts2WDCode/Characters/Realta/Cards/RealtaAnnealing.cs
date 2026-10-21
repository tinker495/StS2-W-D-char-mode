using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaAnnealing() : RealtaCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 5m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 0, 999999999);
        var cards = (await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, prefs, null, this)).ToList();
        if (cards.Count == 0) return;
        await CardCmd.Discard(choiceContext, cards);
        foreach (var card in cards)
        {
            if (card.Keywords.Contains(CardKeyword.Unplayable))
                await CardCmd.Exhaust(choiceContext, card);
            else
            {
                await RefineCmd.Upgrade(choiceContext, card);
                await RefineCmd.Refine(choiceContext, card, DynamicVars["Refine"].IntValue);
            }
        }
    }
    protected override void OnUpgrade() => DynamicVars["Refine"].UpgradeValueBy(2m);
}
