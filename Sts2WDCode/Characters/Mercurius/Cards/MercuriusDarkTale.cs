using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusDarkTale() : MercuriusCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MercuriusIllusionPower>(2m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<MercuriusIllusionPower>(), HoverTipFactory.FromKeyword(MercuriusKeywords.Reenact), HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];
    public override string PortraitPath => ModelDb.Card<Eidolon>().PortraitPath;
    public override string CustomPortraitPath => PortraitPath;
    public override string BetaPortraitPath => PortraitPath;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MercuriusIllusionPower>(choiceContext, Owner.Creature,
            DynamicVars["MercuriusIllusionPower"].BaseValue, Owner.Creature, this);
        await MercuriusReenactCommand.ReenactCurse(choiceContext, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["MercuriusIllusionPower"].UpgradeValueBy(1m);
}
