using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusUnknownTale() : MercuriusCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(MercuriusKeywords.Reenact), HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];
    public override string PortraitPath => ModelDb.Card<Coordinate>().PortraitPath;
    public override string CustomPortraitPath => PortraitPath;
    public override string BetaPortraitPath => PortraitPath;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var candidates = Owner.RunState.Players.Where(player => player != Owner)
            .SelectMany(player => player.Deck.Cards).Select(card => card.CanonicalInstance);
        await MercuriusReenactCommand.Reenact(choiceContext, Owner, candidates);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
