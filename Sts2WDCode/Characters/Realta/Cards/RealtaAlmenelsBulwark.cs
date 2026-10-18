using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaAlmenelsBulwark() : RealtaCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaMending>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaMending>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaMending>().BetaPortraitPath;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RealtaKeywords.Legacy];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(5m), new CalculationExtraVar(1m),
        new CalculatedVar("Plating").WithMultiplier(static (card, _) => RefineCmd.GetAmount(card))
    ];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            ((CalculatedVar)DynamicVars["Plating"]).Calculate(null), Owner.Creature, this);

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
