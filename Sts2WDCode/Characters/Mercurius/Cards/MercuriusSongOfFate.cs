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

public sealed class MercuriusSongOfFate() : MercuriusSongCard(CardRarity.Rare)
{

    protected override async Task WhenDrawn(PlayerChoiceContext context)
    {
        var pile = IsUpgraded ? Owner.PlayerCombatState!.DrawPile : Owner.PlayerCombatState!.DiscardPile;
        if (pile.Cards.Count == 0) return;
        var chosen = (await CardSelectCmd.FromCombatPile(context, pile, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (chosen != null) await CardPileCmd.Add(chosen, PileType.Hand);
    }
}
