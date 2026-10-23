using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaSoulHeatPower : AltheaPower
{
    public override async Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature == Owner && AltheaRules.IsCurse(card))
            await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
    }
}
