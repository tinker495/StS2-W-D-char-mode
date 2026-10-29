using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusIncarnationOfStories() : MercuriusCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MercuriusIncarnationOfStoriesPower>()];
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) =>
        await PowerCmd.Apply<MercuriusIncarnationOfStoriesPower>(context, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
        AddKeyword(CardKeyword.Retain);
    }
}
