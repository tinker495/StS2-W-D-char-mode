using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaReclaim() : RealtaCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromCard<RealtaFlyingGlaive>(), HoverTipFactory.FromKeyword(CardKeyword.Ethereal), HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = Owner.PlayerCombatState!.AllCards
            .Where(c => c is RealtaFlyingGlaive or RealtaIordinaMacBethlehem).ToList();
        await CardPileCmd.Add(cards.Where(c => c.Pile?.Type != PileType.Hand).ToList(), PileType.Hand);

        // Recover every Glaive, but create only one copy; prefer the original when available.
        var source = cards.FirstOrDefault(c => c.CloneOf == null && !c.IsDupe) ?? cards.FirstOrDefault();
        if (source == null) return;
        var copy = source.CreateClone();
        copy.AddKeyword(CardKeyword.Ethereal);
        copy.AddKeyword(CardKeyword.Exhaust);
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
