using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

[HarmonyPatch(typeof(TouchOfOrobas), "get_RefinementUpgrades")]
internal static class MercuriusStarterUpgrade
{
    private static void Postfix(Dictionary<ModelId, RelicModel> __result) =>
        __result[ModelDb.Relic<MercuriusDragonsWaymark>().Id] = ModelDb.Relic<MercuriusMemoryOfTheWorld>();
}
