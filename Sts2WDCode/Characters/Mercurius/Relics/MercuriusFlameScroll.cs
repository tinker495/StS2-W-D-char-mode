using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusFlameScroll : MercuriusRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        if (card.Owner != Owner || !card.Keywords.Contains(CardKeyword.Ethereal) || CombatManager.Instance.IsOverOrEnding) return;
        var target = Owner.RunState.Rng.CombatTargets.NextItem(Owner.Creature.CombatState!.HittableEnemies);
        if (target == null) return;
        Flash();
        // Native HP-loss path, as used by Poison: bypass Block and powered attack modifiers.
        await CreatureCmd.Damage(context, target, 7m, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, null);
    }
}
