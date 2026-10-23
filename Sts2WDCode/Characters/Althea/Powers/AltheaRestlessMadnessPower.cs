using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaRestlessMadnessPower : AltheaPower
{
    public override async Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        if (Owner.Player?.PlayerCombatState is not { } combat) return;
        // Victory hooks run after Player.AfterCombatEnd removes powers and clears every combat pile.
        // Count here while relic-based Curse classification and non-exhausted cards still exist.
        int curses = combat.Hand.Cards.Concat(combat.DrawPile.Cards).Concat(combat.DiscardPile.Cards)
            .Count(AltheaRules.IsCurse);
        await CreatureCmd.Heal(Owner, curses * Amount);
    }
}
