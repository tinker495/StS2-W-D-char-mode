using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public abstract class MercuriusSongCard(CardRarity rarity) : MercuriusCard(1, CardType.Skill, rarity, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(MercuriusKeywords.Song)];
    public override Task AfterCardDrawn(PlayerChoiceContext context, CardModel card, bool fromHandDraw) =>
        card == this ? WhenDrawn(context) : Task.CompletedTask;
    protected override Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) => Task.CompletedTask;
    protected abstract Task WhenDrawn(PlayerChoiceContext context);
}
