using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

[HarmonyPatch(typeof(Player), nameof(Player.FromSerializable))]
internal static class RealtaStartingDeckMigration
{
    private static void Prefix(SerializablePlayer save)
    {
        if (save.CharacterId != ModelDb.Character<Realta>().Id) return;

        // Remap only the model ID before normal loading, preserving upgrades, enchantments and saved properties.
        foreach (var card in save.Deck)
        {
            if (card.Id == ModelDb.Card<StrikeIronclad>().Id)
                card.Id = ModelDb.Card<RealtaStrike>().Id;
            else if (card.Id == ModelDb.Card<DefendIronclad>().Id)
                card.Id = ModelDb.Card<RealtaDefend>().Id;
        }
    }
}
