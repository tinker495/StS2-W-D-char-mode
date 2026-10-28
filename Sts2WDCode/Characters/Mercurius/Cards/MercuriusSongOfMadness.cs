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

public sealed class MercuriusSongOfMadness() : MercuriusSongCard(CardRarity.Rare)
{

    protected override async Task WhenDrawn(PlayerChoiceContext context)
    {
        decimal damage = MercuriusIntentDamage.TotalFor(Owner);
        if (damage <= 0) return;
        var targets = CombatState!.HittableEnemies.ToList();
        if (!IsUpgraded)
        {
            var selected = Owner.RunState.Rng.CombatTargets.NextItem(targets);
            targets = selected == null ? [] : [selected];
        }
        foreach (var enemy in targets)
            await CreatureCmd.Damage(context, enemy, damage, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this);
    }
}
