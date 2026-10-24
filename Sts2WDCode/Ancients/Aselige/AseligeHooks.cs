using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Odds;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2WD.Sts2WDCode.Ancients.Aselige;

// Native hooks do not expose room-roll replacement, post-upgrade or
// post-relic-obtain. Bridge those command boundaries without changing their
// timing or launching unawaited gameplay tasks.
internal static class AseligeHooks
{
    [HarmonyPatch(typeof(RunManager), nameof(RunManager.GenerateRooms))]
    private static class StartingAncient
    {
        [HarmonyPostfix]
        private static void Postfix(RunManager __instance)
        {
            var run = __instance.DebugOnlyGetState();
            if (run == null || !AseligeRules.ShouldReplaceNeow(0, run.Players.Select(p => p.Character))) return;
            var rooms = (RoomSet)AccessTools.Field(typeof(ActModel), "_rooms").GetValue(run.Acts[0])!;
            rooms.Ancient = ModelDb.Event<Aselige>();
        }
    }

    [HarmonyPatch(typeof(RunManager), "SetStartedWithNeowFlag")]
    private static class IncludeStartingRoom
    {
        [HarmonyPostfix]
        private static void Postfix(RunManager __instance)
        {
            var run = __instance.DebugOnlyGetState();
            if (run != null && AseligeRules.ShouldReplaceNeow(0, run.Players.Select(p => p.Character)))
                run.ExtraFields.StartedWithNeow = true;
        }
    }

    [HarmonyPatch(typeof(UnknownMapPointOdds), nameof(UnknownMapPointOdds.Roll))]
    private static class UnknownRestSite
    {
        [HarmonyPostfix]
        private static void Postfix(IRunState runState, IEnumerable<RoomType> blacklist, ref RoomType __result)
        {
            if (__result is not (RoomType.Event or RoomType.Monster or RoomType.Shop) || blacklist.Contains(RoomType.RestSite)) return;
            foreach (var relic in runState.Players.SelectMany(p => p.Relics).OfType<AseligeRegenerationGrace>())
            {
                var chance = relic.RestChance;
                if (chance <= 0 || (chance < 1 && runState.Rng.Niche.NextFloat() >= chance)) continue;
                __result = RoomType.RestSite;
                return;
            }
        }
    }

    [HarmonyPatch(typeof(CardCmd), nameof(CardCmd.Upgrade), [typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle)])]
    private static class UpgradedCards
    {
        [HarmonyPrefix]
        private static void Prefix(ref IEnumerable<CardModel> cards, out Dictionary<CardModel, int> __state)
        {
            var snapshot = cards.ToArray();
            cards = snapshot;
            __state = snapshot.Distinct().ToDictionary(card => card, card => card.CurrentUpgradeLevel);
        }
        [HarmonyPostfix]
        private static void Postfix(Dictionary<CardModel, int> __state)
        {
            foreach (var (card, before) in __state)
                if (card.CurrentUpgradeLevel > before && card.Owner is { } owner)
                    foreach (var scales in owner.Relics.OfType<AseligeDeepBlueScales>()) scales.AfterUpgrade(card);
        }
    }

    [HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), [typeof(RelicModel), typeof(Player), typeof(int)])]
    private static class ObtainedRelic
    {
        [HarmonyPostfix]
        private static void Postfix(Player player, ref Task<RelicModel> __result) => __result = Notify(__result, player);
        private static async Task<RelicModel> Notify(Task<RelicModel> original, Player player)
        {
            var relic = await original;
            foreach (var salt in player.Relics.OfType<AseligeNeatlyArrangedSalt>().ToArray())
                await salt.AfterRelicObtained();
            return relic;
        }
    }
}
