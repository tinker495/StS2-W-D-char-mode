using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaPerfectDay() : RealtaCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaForgeHeat>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaForgeHeat>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaForgeHeat>().BetaPortraitPath;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<StrengthPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await PowerCmd.Apply<RealtaPerfectDayPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
