using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaGreatFlame() : RealtaCard(4, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    public override string CustomPortraitPath => "res://Sts2WD/images/card_portraits/realta/realta_great_flame.png";
    public override string PortraitPath => CustomPortraitPath;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(40m, ValueProp.Move), new EnergyVar(1)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromPower<VigorPower>(), HoverTipFactory.FromPower<FlameBarrierPower>()];

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || Owner?.Creature is not { } owner || originalCost <= 0m) return false;
        int reduction = (owner.HasPower<VigorPower>() ? 1 : 0) + (owner.HasPower<FlameBarrierPower>() ? 1 : 0);
        if (reduction <= 0) return false;
        modifiedCost = Math.Max(0m, originalCost - reduction);
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).TargetingAllOpponents(CombatState!).Execute(choiceContext);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
