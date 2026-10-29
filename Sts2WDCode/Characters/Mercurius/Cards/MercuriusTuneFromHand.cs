using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusTuneFromHand : MercuriusTuneOption
{
    protected override CardModel PortraitSource => ModelDb.Card<Acrobatics>();

    public override async Task Resolve(PlayerChoiceContext context)
    {
        if (Owner.PlayerCombatState!.Hand.Cards.Count == 0) return;
        var selected = (await CardSelectCmd.FromHand(context, Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1), null, this)).FirstOrDefault();
        if (selected != null) await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
    }
}
