using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusJourneyMemoriesPower : MercuriusPower
{
    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player is not { } player) return Task.CompletedTask;
        var pools = ModelDb.AllCharacterCardPools.Where(pool => pool != player.Character.CardPool)
            .Append(ModelDb.CardPool<ColorlessCardPool>()).Distinct().ToList();
        for (int i = 0; i < Amount; i++)
        {
            // Real pool IDs and ordinary reward odds preserve native save/load and rerolls.
            var options = new CardCreationOptions(pools, CardCreationSource.Encounter, CardRarityOddsType.RegularEncounter);
            room.AddExtraReward(player, new CardReward(options, 3, player));
        }
        return Task.CompletedTask;
    }
}
