using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaSoulLegionPower : AltheaPower
{
    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner.Creature == Owner && play.Card.Type == CardType.Attack)
            play.Card.AddKeyword(AltheaKeywords.Frenzy);
        return Task.CompletedTask;
    }
}
