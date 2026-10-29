using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusBlendingIn() : MercuriusCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<MercuriusIllusionPower>(2m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<MercuriusIllusionPower>()];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MercuriusBlendingInPower>(context, Owner.Creature, 1, Owner.Creature, this);
        await PowerCmd.Apply<MercuriusIllusionPower>(context, Owner.Creature, DynamicVars["MercuriusIllusionPower"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() { DynamicVars["MercuriusIllusionPower"].UpgradeValueBy(1m); }
}
