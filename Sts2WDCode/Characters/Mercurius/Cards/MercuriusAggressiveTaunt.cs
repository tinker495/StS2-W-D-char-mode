using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusAggressiveTaunt() : MercuriusCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<MercuriusTuneFromHand>(), HoverTipFactory.FromCard<MercuriusTuneFromDiscard>(), HoverTipFactory.FromCard<MercuriusTuneShuffle>()];
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this || !Owner.Creature.HasPower<MercuriusTunedThisTurnPower>()) return false;
        modifiedCost = 0;
        return true;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
ArgumentNullException.ThrowIfNull(cardPlay.Target); await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(3m); }
}
