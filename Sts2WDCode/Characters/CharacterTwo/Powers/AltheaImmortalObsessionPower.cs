using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaImmortalObsessionPower : AltheaPower
{
    public override async Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature == Owner && AltheaRules.IsCurse(card))
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card.Owner.Creature == Owner && AltheaRules.IsCurse(card))
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
}
