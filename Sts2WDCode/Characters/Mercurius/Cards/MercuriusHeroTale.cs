using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Sts2WD.Sts2WDCode.Characters.Realta;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusHeroTale() : MercuriusCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 3m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine), HoverTipFactory.FromKeyword(MercuriusKeywords.Reenact), HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];
    public override string PortraitPath => ModelDb.Card<Armaments>().PortraitPath;
    public override string CustomPortraitPath => PortraitPath;
    public override string BetaPortraitPath => PortraitPath;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await RefineCmd.Refine(choiceContext, Owner, DynamicVars["Refine"].IntValue, this);
        var candidates = ModelDb.CardPool<RealtaCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint);
        await MercuriusReenactCommand.Reenact(choiceContext, Owner, candidates, IsUpgraded);
    }

    protected override void OnUpgrade() => DynamicVars["Refine"].UpgradeValueBy(1m);
}
