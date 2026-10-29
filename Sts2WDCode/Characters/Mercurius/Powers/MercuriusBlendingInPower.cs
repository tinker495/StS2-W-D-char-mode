using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusBlendingInPower : MercuriusPower, MercuriusIllusionLossListener
{
    public async Task AfterIllusionLost(PlayerChoiceContext context, int amount)
    {
        await CardPileCmd.Draw(context, amount * Amount, Owner.Player!);
    }
}
