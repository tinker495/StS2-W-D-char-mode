using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaInnerReservePower : AltheaPower
{
    private async Task AfterHarmfulEffectRemoved()
    {
        if (CombatManager.Instance.IsOverOrEnding || !Owner.Powers.Contains(this)) return;
        await PowerCmd.Apply<EnergyNextTurnPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
    }

    // Native removal has no global hook. Observe the awaited command once,
    // including duration expiry and direct cleansing; reductions do not count.
    [HarmonyPatch(typeof(PowerCmd), nameof(PowerCmd.Remove), [typeof(PowerModel)])]
    private static class RemovalBridge
    {
        private static void Prefix(PowerModel? power, out AltheaInnerReservePower? __state)
        {
            __state = null;
            if (power?.Owner?.Player is not { } player || player.PlayerCombatState == null ||
                !power.Owner.Powers.Contains(power) || power.Owner.GetPower<AltheaInnerReservePower>() is not { } reserve)
                return;
            if (AltheaRules.IsHarmful(power, AltheaRules.For(player).EnemyEffects.Contains(power))) __state = reserve;
        }
        private static void Postfix(AltheaInnerReservePower? __state, ref Task __result)
        {
            if (__state != null) __result = Notify(__result, __state);
        }
        private static async Task Notify(Task original, AltheaInnerReservePower reserve)
        {
            await original;
            await reserve.AfterHarmfulEffectRemoved();
        }
    }
}
