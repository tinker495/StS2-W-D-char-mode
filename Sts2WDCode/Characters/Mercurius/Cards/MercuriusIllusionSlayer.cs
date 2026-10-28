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

public sealed class MercuriusIllusionSlayer() : MercuriusCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), MercuriusIllusionLossListener
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(11m)];
    public async Task AfterIllusionLost(PlayerChoiceContext context, int amount)
    {
        for (int i = 0; i < amount && Pile?.Type == PileType.Discard; i++) await CardPileCmd.Add(this, PileType.Hand);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Target is { } target) await CreatureCmd.Damage(context, target, DynamicVars.HpLoss.BaseValue, ValueProp.Unpowered | ValueProp.Unblockable, Owner.Creature, this);
    }
    protected override void OnUpgrade() { DynamicVars.HpLoss.UpgradeValueBy(4m); }
}
