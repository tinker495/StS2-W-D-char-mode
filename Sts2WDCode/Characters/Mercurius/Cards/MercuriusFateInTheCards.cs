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

public sealed class MercuriusFateInTheCards() : MercuriusCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawn = (await CardPileCmd.Draw(choiceContext, 1, Owner)).FirstOrDefault();
        if (drawn == null) return;
        int energy = drawn.EnergyCost.CostsX ? 0 : Math.Max(0, drawn.EnergyCost.GetWithModifiers(CostModifiers.All));
        await PlayerCmd.GainEnergy(energy, Owner);
    }
    protected override void OnUpgrade() { EnergyCost.UpgradeBy(-1); }
}
