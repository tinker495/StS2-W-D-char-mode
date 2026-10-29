using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public static class MercuriusIntentDamage
{
    public static int TotalFor(Player player)
    {
        if (player.Creature.CombatState is not { } combat) return 0;
        int total = 0;
        foreach (var enemy in combat.HittableEnemies)
        {
            if (enemy.Monster is not { } monster) continue;
            foreach (var intent in monster.NextMove.Intents.OfType<AttackIntent>())
            {
                if (intent.DamageCalc is not { } damageCalc) continue;
                // Never use AttackIntent.GetTotalDamage: it depends on the local client player.
                decimal damage = Hook.ModifyDamage(player.RunState, combat,
                    player.Creature, enemy, damageCalc(), ValueProp.Move, null,
                    ModifyDamageHookType.All, CardPreviewMode.None, out _);
                total += Math.Max(0, (int)damage) * Math.Max(0, intent.Repeats);
            }
        }
        return total;
    }
}
