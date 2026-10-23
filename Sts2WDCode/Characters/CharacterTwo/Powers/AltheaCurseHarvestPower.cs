using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaCurseHarvestPower : AltheaPower
{
    public override async Task AfterCardDrawn(PlayerChoiceContext context, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature == Owner && AltheaRules.IsCurse(card))
            await CardPileCmd.Draw(context, Amount, card.Owner);
    }
}
