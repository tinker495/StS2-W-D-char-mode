using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Follow Forge's creation and per-card growth rules (docs/adr/0002).
public static class GuardCmd
{
    public static async Task Guard(decimal amount, Player player)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;

        var combat = player.PlayerCombatState!;
        if (!combat.AllCards.OfType<RealtaGuard>().Any(c => !c.IsDupe && c.Pile?.Type != PileType.Exhaust))
        {
            RealtaGuard guard = player.Creature.CombatState!.CreateCard<RealtaGuard>(player);
            await CardPileCmd.AddGeneratedCardToCombat(guard, PileType.Hand, player);
        }

        foreach (var guard in combat.AllCards.OfType<RealtaGuard>().Where(c => !c.IsDupe))
            guard.AddBlock(amount);
    }
}
