using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public static class MercuriusRewardEnchantments
{
    private static readonly ConditionalWeakTable<CombatRoom, Dictionary<ulong, int>> Pending = new();
    public static void Queue(CombatRoom room, Player player, int count)
    {
        if (count <= 0) return;
        var players = Pending.GetOrCreateValue(room);
        players[player.NetId] = players.GetValueOrDefault(player.NetId) + count;
    }
    private static EnchantmentModel[] Choices => [ModelDb.Enchantment<MercuriusEtching>(),
        ModelDb.Enchantment<PerfectFit>(), ModelDb.Enchantment<Swift>(), ModelDb.Enchantment<Steady>()];

    public static void Apply(RewardsSet set)
    {
        if (set.Room is not CombatRoom room || !Pending.TryGetValue(room, out var players)
            || !players.Remove(set.Player.NetId, out int count)) return;
        var rewards = set.Rewards.OfType<CardReward>().ToList();
        var quotas = new Dictionary<CardReward, int>();
        for (int i = 0; i < count; i++)
        {
            var card = EnchantOne(rewards.SelectMany(reward => reward.Cards), set.Player);
            if (card == null) break;
            var reward = rewards.First(reward => reward.Cards.Contains(card));
            quotas[reward] = quotas.GetValueOrDefault(reward) + 1;
        }
        // Native rerolls replace the options without regenerating RewardsSet.
        foreach (var (reward, quota) in quotas)
            reward.AfterGenerated += () => { for (int i = 0; i < quota; i++) EnchantOne(reward.Cards, set.Player); };
    }
    private static CardModel? EnchantOne(IEnumerable<CardModel> cards, Player player)
    {
        var choices = Choices;
        var eligible = cards.Where(card => card.Enchantment == null && choices.All(choice => choice.CanEnchant(card))).ToList();
        if (eligible.Count == 0) return null;
        var rng = player.PlayerRng.Rewards;
        var card = rng.NextItem(eligible)!;
        var enchantment = rng.NextItem(choices)!;
        CardCmd.Enchant(enchantment.ToMutable(), card, 1);
        return card;
    }
    [HarmonyPatch(typeof(RewardsSet), nameof(RewardsSet.GenerateWithoutOffering))]
    private static class RewardGeneration
    {
        private static void Postfix(RewardsSet __instance, ref Task __result) => __result = Finish(__result, __instance);
        private static async Task Finish(Task original, RewardsSet set) { await original; Apply(set); }
    }
}
