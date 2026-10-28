using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusFormlessWingsPower : MercuriusPower
{
    private HashSet<CardPlay> _lastCards = [];
    protected override void DeepCloneFields() { base.DeepCloneFields(); _lastCards = []; }
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Owner.PlayerCombatState!.Hand.Cards.Count == 0) _lastCards.Add(cardPlay);
        return Task.CompletedTask;
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (!_lastCards.Remove(cardPlay)) return;
        foreach (var enemy in Owner.CombatState!.HittableEnemies.ToList())
            await CreatureCmd.Damage(context, enemy, Amount, ValueProp.Unpowered | ValueProp.Unblockable, Owner, null);
    }
}
