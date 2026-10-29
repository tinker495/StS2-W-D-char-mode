using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusPhantomDuplicate() : MercuriusCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MercuriusPhantomDuplicatePower>()];
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) =>
        await PowerCmd.Apply<MercuriusPhantomDuplicatePower>(context, Owner.Creature, 1, Owner.Creature, this);
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
