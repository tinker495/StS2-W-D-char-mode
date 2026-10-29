using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusChoosingFate() : MercuriusCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var first = (await CardPileCmd.Draw(choiceContext, 1, Owner)).FirstOrDefault();
        if (first == null) return;
        while (true)
        {
            var next = (await CardPileCmd.Draw(choiceContext, 1, Owner)).FirstOrDefault();
            if (next == null || next.Type != first.Type) break;
        }
    }
    protected override void OnUpgrade() { RemoveKeyword(CardKeyword.Exhaust); }
}
