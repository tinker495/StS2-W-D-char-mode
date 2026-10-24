using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2WD.Sts2WDCode.Cards;

namespace Sts2WD.Sts2WDCode.Ancients.Aselige;

[Pool(typeof(ColorlessCardPool))]
public sealed class AseligeShiningLeaf() : Sts2WDCard(0, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    public override int MaxUpgradeLevel => 9;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) =>
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
}

[Pool(typeof(ColorlessCardPool))]
public sealed class AseligeVerdantDragonAdvent() : Sts2WDCard(0, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    public override int MaxUpgradeLevel => 0;
    public override bool CanBeGeneratedInCombat => false;
    public override bool CanBeGeneratedByModifiers => false;
    protected override Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay) => Task.CompletedTask;
}
