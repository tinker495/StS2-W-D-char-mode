using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusPerfectFinale() : MercuriusFinaleCard(0, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(0)];
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) =>
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}
