using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaFireDragonsBlood() : RealtaMasterworkCard(CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    public override string CustomPortraitPath => "res://Sts2WD/images/card_portraits/realta/realta_fire_dragons_blood.png";
    public override string PortraitPath => CustomPortraitPath;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VigorPower>(3m), new DamageVar(1m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [.. base.ExtraHoverTips, HoverTipFactory.FromPower<VigorPower>()];

    protected override async Task OnRepeat(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, this);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars["VigorPower"].UpgradeValueBy(1m);
}
