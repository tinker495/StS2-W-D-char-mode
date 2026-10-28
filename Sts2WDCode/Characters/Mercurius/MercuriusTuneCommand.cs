using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public static class MercuriusTuneCommand
{
    public static async Task Tune(PlayerChoiceContext context, Player player)
    {
        if (player.Creature.CombatState is not { } combat || CombatManager.Instance.IsOverOrEnding) return;
        // Use the same synchronized three-card selection screen as Knowledge Demon.
        CardModel[] options = [
            combat.CreateCard<MercuriusTuneFromHand>(player),
            combat.CreateCard<MercuriusTuneFromDiscard>(player),
            combat.CreateCard<MercuriusTuneShuffle>(player)
        ];
        if (await CardSelectCmd.FromChooseACardScreen(context, options, player) is MercuriusTuneOption selected)
        {
            await selected.Resolve(context);
            await PowerCmd.Apply<MercuriusTunedThisTurnPower>(context, player.Creature, 1, player.Creature, selected);
            if (player.Creature.GetPower<MercuriusWholePresencePower>() is { } presence)
                await presence.AfterTuned();
            if (player.Creature.GetPower<MercuriusWalkingTogetherPower>() is { } walking)
                await walking.AfterTuned(context);
        }
    }
}
