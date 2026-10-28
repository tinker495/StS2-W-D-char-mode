using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusConversationWithTheDead() : MercuriusCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override bool HasEnergyCostX => true;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(0)];
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var pile = Owner.PlayerCombatState!.ExhaustPile;
        int count = Math.Min(Math.Max(0, ResolveEnergyXValue() + DynamicVars.Cards.IntValue), pile.Cards.Count);
        if (count == 0) return;
        var selected = await CardSelectCmd.FromCombatPile(context, pile, Owner, new CardSelectorPrefs(SelectionScreenPrompt, count));
        foreach (var card in selected) await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Top);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
