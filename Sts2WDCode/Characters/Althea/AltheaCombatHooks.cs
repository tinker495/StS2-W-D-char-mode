using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public partial class Althea
{
    public override Task BeforeCardPlayed(CardPlay play)
    {
        AltheaRules.BeginPlay(play);
        return Task.CompletedTask;
    }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (AltheaRules.Applies(player))
        {
            var state = AltheaRules.For(player); state.Attacks = 0; state.Streak = 0; state.CardsPlayed = 0;
            state.ReformedThisTurn.Clear();
        }
        return Task.CompletedTask;
    }
    public override Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount,
        Creature? applier, CardModel? source)
    {
        if (power.Owner.Player is { } player && AltheaRules.Applies(player) && applier?.Side == CombatSide.Enemy
            && AltheaRules.IsHarmfulApplication(power, amount, applier))
            AltheaRules.For(player).EnemyEffects.Add(power);
        return Task.CompletedTask;
    }
    public override async Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        if (!AltheaRules.Applies(card.Owner)) return;
        if (AltheaRules.IsCurse(card)) AltheaRules.For(card.Owner).CursesExhausted++;
        await AltheaRules.Unfading(context, card);
    }
    public override Task AfterShuffle(PlayerChoiceContext context, Player player)
    {
        if (AltheaRules.Applies(player))
        {
            AltheaRules.For(player).Shuffles++;
            if (player.PlayerCombatState!.TurnNumber > 0)
                foreach (var card in player.PlayerCombatState.DrawPile.Cards.OfType<AltheaCard>()
                    .Where(c => c.Kind is AltheaCardKind.Feint or AltheaCardKind.Regroup)) card.EnergyCost.AddThisCombat(1);
        }
        return Task.CompletedTask;
    }
}
