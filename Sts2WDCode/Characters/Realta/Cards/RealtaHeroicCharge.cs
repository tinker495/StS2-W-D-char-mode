using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaHeroicCharge() : RealtaCard(2, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaFlyingCharge>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaFlyingCharge>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaFlyingCharge>().BetaPortraitPath;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);
        await CardCmd.Discard(choiceContext, Owner.PlayerCombatState!.Hand.Cards.ToList());
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
}
