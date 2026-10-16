using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// 수호 N, mirroring MegaCrit.Sts2.Core.Commands.ForgeCmd (docs/adr/0002):
// creates a Guard card in hand when no un-exhausted one exists, then adds N block to every Guard card,
// exhausted ones included. Dupes are neither tracked nor grown.
public static class GuardCmd
{
    public static async Task Guard(decimal amount, Player player)
    {
        if (CombatManager.Instance.IsOverOrEnding) return;

        if (!GetGuards(player, includeExhausted: false).Any())
        {
            RealtaGuard guard = player.Creature.CombatState!.CreateCard<RealtaGuard>(player);
            await CardPileCmd.AddGeneratedCardToCombat(guard, PileType.Hand, player);
        }

        foreach (RealtaGuard guard in GetGuards(player, includeExhausted: true))
            guard.AddBlock(amount);
    }

    private static IEnumerable<RealtaGuard> GetGuards(Player player, bool includeExhausted)
    {
        return player.PlayerCombatState!.AllCards
            .Where(c => !c.IsDupe && (includeExhausted || c.Pile?.Type != PileType.Exhaust))
            .OfType<RealtaGuard>();
    }
}
