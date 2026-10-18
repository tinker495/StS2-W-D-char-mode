using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaSmelting() : RealtaCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combat = Owner.PlayerCombatState!;
        if (combat.DiscardPile.Cards.Count > 0)
        {
            CardSelectorPrefs prefs = new(SelectionScreenPrompt, DynamicVars.Cards.IntValue);
            var selected = (await CardSelectCmd.FromCombatPile(choiceContext, combat.DiscardPile, Owner, prefs)).ToList();
            if (selected.Count > 0)
                await CardPileCmd.Add(selected, PileType.Hand);
        }

        var hand = combat.Hand.Cards.ToList();
        var upgradable = hand.Where(c => c.IsUpgradable).ToList();
        var unupgradable = hand.Where(c => !c.IsUpgradable).ToList();
        await RefineCmd.Upgrade(choiceContext, upgradable);
        foreach (var card in unupgradable)
            await CardCmd.Exhaust(choiceContext, card);
    }
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
