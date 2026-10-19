using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaMaterialAnalysis() : RealtaCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaMagicMending>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaMagicMending>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaMagicMending>().BetaPortraitPath;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = Owner.PlayerCombatState!.DiscardPile.Cards
            .Where(card => card.IsUpgraded || card.Enchantment != null).ToList();
        await CardPileCmd.Add(cards, PileType.Draw, CardPilePosition.Random);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
