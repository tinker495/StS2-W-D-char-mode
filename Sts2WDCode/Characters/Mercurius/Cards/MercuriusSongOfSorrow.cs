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

public sealed class MercuriusSongOfSorrow() : MercuriusSongCard(CardRarity.Common)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(10m)];

    protected override async Task WhenDrawn(PlayerChoiceContext context)
    {
        var target = Owner.RunState.Rng.CombatTargets.NextItem(CombatState!.HittableEnemies);
        if (target != null) await CreatureCmd.Damage(context, target, DynamicVars.HpLoss.BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this);
    }
    protected override void OnUpgrade() { DynamicVars.HpLoss.UpgradeValueBy(4m); }
}
