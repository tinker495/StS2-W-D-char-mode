using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaHerosSilhouette() : RealtaCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaHerosArmor>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaHerosArmor>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaHerosArmor>().BetaPortraitPath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await PowerCmd.Apply<RealtaHerosSilhouettePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
