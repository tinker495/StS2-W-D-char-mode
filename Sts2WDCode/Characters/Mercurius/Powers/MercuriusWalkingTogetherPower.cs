using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusWalkingTogetherPower : MercuriusPower
{
    public async Task AfterTuned(PlayerChoiceContext context)
    {
        foreach (var player in Owner.Player!.RunState.Players.Where(player => player != Owner.Player && !player.Creature.IsDead))
            await CardPileCmd.Draw(context, Amount, player);
    }
}
