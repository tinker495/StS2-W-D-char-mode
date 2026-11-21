using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

internal static class AltheaReformLifecycle
{
    private sealed class State
    {
        internal int CardsPlayed;
        internal readonly Dictionary<CardModel, PileType> AfterPlay = [];
        internal readonly HashSet<CardModel> ThisTurn = [];
        internal readonly HashSet<CardModel> ThisCombat = [];
    }

    private static readonly ConditionalWeakTable<PlayerCombatState, State> States = new();
    private static State For(Player player) => States.GetValue(player.PlayerCombatState!, _ => new State());

    internal static void StartTurn(Player player)
    {
        var state = For(player);
        state.CardsPlayed = 0;
        state.ThisTurn.Clear();
    }

    internal static void RecordPlay(CardModel card) => For(card.Owner).CardsPlayed++;

    internal static CardLocation ReserveAfterFirstPlay(CardModel card, CardLocation location)
    {
        var state = For(card.Owner);
        if (state.CardsPlayed != 0) return location;
        state.AfterPlay[card] = location.pileType;
        return location with { pileType = PileType.Discard };
    }

    internal static bool WasReformedThisTurn(CardModel card) => For(card.Owner).ThisTurn.Contains(card);
    internal static bool WasReformed(CardModel card) => For(card.Owner).ThisCombat.Contains(card);

    internal static async Task CompletePlay(PlayerChoiceContext context, Player player, CardModel card)
    {
        // Play completion belongs to the player who started it, even if an effect transfers the card.
        var state = For(player);
        if (!state.AfterPlay.TryGetValue(card, out var result)) return;
        if (card.Pile != null && state.CardsPlayed > 0)
            await Reform(context, card, result == PileType.Exhaust ? PileType.Exhaust : PileType.Draw);
        state.AfterPlay.Remove(card);
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
        if (For(source.Owner).AfterPlay.Remove(source, out var result))
            destination = result == PileType.Exhaust ? PileType.Exhaust : PileType.Draw;
        if (source.IsUpgraded)
        {
            replacement.UpgradeInternal();
            replacement.FinalizeUpgradeInternal();
        }
        if (source.Enchantment is { } enchantment && enchantment.CanEnchant(replacement))
            CardCmd.Enchant((EnchantmentModel)enchantment.ClonePreservingMutability(), replacement, enchantment.Amount);
        replacement.DeckVersion = source.DeckVersion;
        // CardCmd.Transform removes a self-reforming card from Play without freeing its NCard node, so OnPlayWrapper skips play-area cleanup; return the stale node to the pool once Transform removes the source.
        var playedCard = source.Pile.Type == PileType.Play ? NCard.FindOnTable(source) : null;
        await CardCmd.Transform(source, replacement);
        if (source.HasBeenRemovedFromState && playedCard?.Model == source) playedCard.QueueFreeSafely();
        For(source.Owner).ThisTurn.Add(replacement);
        For(source.Owner).ThisCombat.Add(replacement);
        if (destination == PileType.Exhaust) await CardCmd.Exhaust(context, replacement);
        else if (destination.HasValue) await CardPileCmd.Add(replacement, destination.Value, CardPilePosition.Top);
        return replacement;
    }

    internal static async Task TakeCombatCard(CardModel card, Player recipient)
    {
        var previousOwner = card.Owner;
        var combat = recipient.Creature.CombatState!;
        bool reformed = For(previousOwner).ThisCombat.Remove(card);
        bool reformedThisTurn = For(previousOwner).ThisTurn.Remove(card);
        // Resolve visible movement before changing ownership, then transfer provenance before native pile hooks.
        await CardPileCmd.Add(card, PileType.Discard);
        card.RemoveFromCurrentPile();
        combat.RemoveCard(card);
        combat.AddCard(card, recipient);
        if (reformed) For(recipient).ThisCombat.Add(card);
        if (reformedThisTurn) For(recipient).ThisTurn.Add(card);
        // This is the same combat instance, not a newly generated card or another combat entry.
        var discard = recipient.PlayerCombatState!.DiscardPile;
        discard.AddInternal(card);
        await Hook.AfterCardChangedPiles(recipient.RunState, combat, card, PileType.Discard, null);
        discard.InvokeCardAddFinished();
    }
}
