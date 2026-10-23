using System.Reflection;
using System.Runtime.CompilerServices;
using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

// Combat-local bookkeeping is owned per PlayerCombatState, never by the canonical character.
internal static class AltheaRules
{
    internal sealed class State
    {
        internal int Attacks;
        internal int Streak;
        internal int CardsPlayed;
        internal int Shuffles;
        internal int CursesExhausted;
        internal bool Draining;
        internal int PlayDepth;
        internal readonly Queue<(CardModel Card, Creature? Target)> Queue = new();
        internal readonly HashSet<CardModel> Pending = [];
        internal readonly Dictionary<CardModel, CardModel[]> Neighbours = [];
        internal readonly HashSet<PowerModel> EnemyEffects = [];
        internal readonly Dictionary<CardModel, PileType> ReformAfterPlay = [];
        internal readonly HashSet<CardModel> ReformedThisTurn = [];
    }
    private static readonly ConditionalWeakTable<PlayerCombatState, State> States = new();
    internal static State For(Player player) => States.GetValue(player.PlayerCombatState!, _ => new State());
    internal static bool Applies(Player? player) => player?.Character is CharacterTwo;
    public static void Initialize() => ModHelper.SubscribeForCombatStateHooks("Sts2WD.Althea",
        combat => combat.Players.Select(p => p.Character).OfType<CharacterTwo>().Distinct());

    internal static bool IsCurse(CardModel card) => card.Type == CardType.Curse || card.Owner != null &&
        (card.Owner.GetRelic<AltheaBloodFrenzy>() != null && card.Type == CardType.Attack ||
         card.Owner.GetRelic<AltheaWyrd>() != null && card.Tags.Contains(CardTag.Strike));
    internal static bool Clash(Player? player) => player?.Creature.CombatState is { } combat &&
        (player.Creature.CurrentHp * 2 <= player.Creature.MaxHp ||
         combat.HittableEnemies.Any(e => e.CurrentHp * 2 <= e.MaxHp));
    internal static int HarmfulCount(Player player) => player.Creature.Powers.Where(p =>
        p.Type == PowerType.Debuff || p is WeakPower or VulnerablePower or FrailPower || For(player).EnemyEffects.Contains(p))
        .Select(p => p.Id).Distinct().Count();
    internal static async Task Unfading(PlayerChoiceContext context, CardModel source)
    {
        if (!source.Keywords.Contains(AltheaKeywords.Unfading) || source.Owner.Creature.CombatState is not { } combat
            || CombatManager.Instance.IsOverOrEnding) return;
        CardModel? original = source.DeckVersion;
        var copy = combat.CreateCard(ModelDb.GetById<CardModel>((original ?? source).Id), source.Owner);
        int upgrades = original?.CurrentUpgradeLevel ?? (source.IsUpgraded ? 1 : 0);
        for (int i = 0; i < upgrades && copy.IsUpgradable; i++) CardCmd.Upgrade(copy);
        if (original?.Enchantment is { } enchantment && enchantment.CanEnchant(copy))
            CardCmd.Enchant((EnchantmentModel)enchantment.ClonePreservingMutability(), copy, enchantment.Amount);
        copy.DeckVersion = original;
        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, source.Owner);
    }

    internal static async Task<CardModel?> Reform(PlayerChoiceContext context, CardModel source, PileType? destination = null)
    {
        if (source.Pile == null || !source.IsTransformable || source.Owner.Creature.CombatState is not { } combat) return null;
        CardModel token = source.Type switch
        {
            CardType.Attack => ModelDb.Card<AltheaSiarcun>(), CardType.Skill => ModelDb.Card<AltheaLleurh>(),
            CardType.Power => ModelDb.Card<AltheaMorfiella>(), _ => ModelDb.Card<AltheaPureWill>()
        };
        var replacement = combat.CreateCard(token, source.Owner);
        if (For(source.Owner).ReformAfterPlay.Remove(source, out var result))
            destination = result == PileType.Exhaust ? PileType.Exhaust : PileType.Draw;
        if (source.IsUpgraded) CardCmd.Upgrade(replacement);
        if (source.Enchantment is { } enchantment && enchantment.CanEnchant(replacement))
            CardCmd.Enchant((EnchantmentModel)enchantment.ClonePreservingMutability(), replacement, enchantment.Amount);
        replacement.DeckVersion = source.DeckVersion;
        await CardCmd.Transform(source, replacement);
        For(source.Owner).ReformedThisTurn.Add(replacement);
        if (destination == PileType.Exhaust) await CardCmd.Exhaust(context, replacement);
        else if (destination.HasValue) await CardPileCmd.Add(replacement, destination.Value, CardPilePosition.Top);
        return replacement;
    }

    internal static decimal DisplayDamage(CardModel card)
    {
        if (card.EnergyCost.CostsX || !card.DynamicVars.TryGetValue("Damage", out var damage)) return 0;
        damage.UpdateCardPreview(card, CardPreviewMode.Normal, null, true);
        decimal hits = card.DynamicVars.TryGetValue("Repeat", out var repeat) ? repeat.BaseValue : 1;
        if (card is AltheaRevenantsRevenge)
            hits = 1 + card.Owner.PlayerCombatState!.ExhaustPile.Cards.Count(IsCurse);
        if (card is AltheaGenMorfiClad) hits = 1 + HarmfulCount(card.Owner);
        return Math.Max(0, damage.PreviewValue) * Math.Max(0, hits);
    }

    internal static void CaptureNeighbours(CardModel card)
    {
        if (!Applies(card.Owner) || card.Pile?.Type != PileType.Hand) return;
        var hand = card.Owner.PlayerCombatState!.Hand.Cards;
        int index = hand.IndexOf(card);
        For(card.Owner).Neighbours[card] = new[] { index - 1, index + 1 }
            .Where(i => i >= 0 && i < hand.Count).Select(i => hand[i])
            .Where(c => c.Keywords.Contains(AltheaKeywords.Frenzy)).ToArray();
    }
    internal static void BeginPlay(CardPlay play)
    {
        if (!Applies(play.Card.Owner)) return;
        var state = For(play.Card.Owner);
        state.CardsPlayed++;
        if (play.Card.Type == CardType.Attack) { state.Attacks++; state.Streak++; } else state.Streak = 0;
        if (play.PlayIndex != 0 || !state.Neighbours.Remove(play.Card, out var neighbours)) return;
        foreach (var neighbour in neighbours)
            if (state.Pending.Add(neighbour)) state.Queue.Enqueue((neighbour, play.Target));
    }
    internal static async Task Drain(PlayerChoiceContext context, Player player)
    {
        var state = For(player);
        if (state.Draining) return;
        state.Draining = true;
        try
        {
            while (state.Queue.TryDequeue(out var next) && !CombatManager.Instance.IsOverOrEnding && player.Creature.IsAlive)
            {
                state.Pending.Remove(next.Card);
                var card = next.Card;
                if (card.Pile?.Type != PileType.Hand || card.Keywords.Contains(CardKeyword.Unplayable)) continue;
                if (!card.CanPlay(out var reason, out _) && reason != UnplayableReason.EnergyCostTooHigh) continue;
                Creature? target = next.Target;
                if (card.TargetType == TargetType.AnyEnemy && target?.IsAlive != true)
                {
                    var enemies = player.Creature.CombatState!.HittableEnemies.ToList();
                    if (enemies.Count == 0) continue;
                    target = enemies[player.RunState.Rng.CombatTargets.NextInt(enemies.Count)];
                }
                int amount = Math.Min(player.PlayerCombatState!.Energy, Math.Max(0, card.EnergyCost.GetAmountToSpend()));
                await (Task)AccessTools.Method(typeof(CardModel), "SpendEnergy").Invoke(card, [amount])!;
                await Hook.BeforeCardAutoPlayed(player.Creature.CombatState!, card, target, AutoPlayType.Default);
                await card.OnPlayWrapper(context, target, true, new ResourceInfo
                {
                    EnergySpent = amount, EnergyValue = amount, StarsSpent = 0, StarValue = 0
                });
            }
        }
        finally { state.Draining = false; if (CombatManager.Instance.IsOverOrEnding) { state.Queue.Clear(); state.Pending.Clear(); } }
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    private static class PlayBoundary
    {
        private static void Prefix(CardModel __instance)
        {
            if (!Applies(__instance.Owner)) return;
            For(__instance.Owner).PlayDepth++;
            CaptureNeighbours(__instance);
        }
        private static void Postfix(CardModel __instance, PlayerChoiceContext choiceContext, ref Task __result)
        {
            if (Applies(__instance.Owner)) __result = Finish(__result, __instance, choiceContext);
        }
        private static async Task Finish(Task original, CardModel card, PlayerChoiceContext context)
        {
            var player = card.Owner;
            try { await original; }
            finally { For(player).PlayDepth--; }
            if (For(player).ReformAfterPlay.TryGetValue(card, out var result))
            {
                if (card.Pile != null && For(player).CardsPlayed > 0)
                    await Reform(context, card, result == PileType.Exhaust ? PileType.Exhaust : PileType.Draw);
                For(player).ReformAfterPlay.Remove(card);
            }
            if (For(player).PlayDepth == 0) await Drain(context, player);
        }
    }
}
