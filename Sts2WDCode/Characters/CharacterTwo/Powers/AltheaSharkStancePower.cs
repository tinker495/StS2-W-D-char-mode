using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaSharkStancePower : AltheaPower
{
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner.Creature == Owner && play.Card.Type == CardType.Attack && Owner.Player is { } player
            && AltheaRules.For(player).Streak % 3 == 0)
            await CardPileCmd.Draw(context, Amount, player);
    }
}
