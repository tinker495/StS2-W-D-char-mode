using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2WD.Sts2WDCode.Ancients.Aselige;

// Keep the canonical set independent of run/act/visitor. Only resolve the
// priority and interpolate visit data when the native UI asks for candidates.
internal static class AseligeDialogues
{
    private static readonly ConditionalWeakTable<AncientDialogueSet, object> Sets = new();
    private static readonly int[] LineCounts = [3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 2];

    internal static AncientDialogueSet CreateSet()
    {
        var set = new AncientDialogueSet
        {
            FirstVisitEverDialogue = null,
            CharacterDialogues = [],
            AgnosticDialogues = LineCounts.Select(count => new AncientDialogue(Enumerable.Repeat("", count).ToArray())).ToArray()
        };
        Sets.Add(set, new object());
        return set;
    }

    [HarmonyPatch(typeof(AncientDialogueSet), nameof(AncientDialogueSet.GetValidDialogues))]
    private static class SelectForVisit
    {
        [HarmonyPrefix]
        private static bool Prefix(AncientDialogueSet __instance, ModelId characterId, int charVisits,
            ref IEnumerable<AncientDialogue> __result)
        {
            if (!Sets.TryGetValue(__instance, out _)) return true;
            var run = RunManager.Instance.DebugOnlyGetState()
                ?? throw new InvalidOperationException("Aselige dialogue requires a run.");
            __result = Candidates(__instance, ModelDb.GetById<CharacterModel>(characterId), run.Acts[0], charVisits);
            return false;
        }
    }

    internal static IReadOnlyList<AncientDialogue> Candidates(AncientDialogueSet set, CharacterModel character,
        ActModel firstAct, int previousVisits) => AseligeRules.DialogueCandidates(character, firstAct, previousVisits)
        .Select(kind => Clone(set.AgnosticDialogues[(int)kind], previousVisits)).ToArray();

    private static AncientDialogue Clone(AncientDialogue original, int previousVisits)
    {
        var copy = new AncientDialogue(Enumerable.Repeat("", original.Lines.Count).ToArray());
        for (var i = 0; i < copy.Lines.Count; i++)
        {
            copy.Lines[i].Speaker = original.Lines[i].Speaker;
            copy.Lines[i].LineText = new LocString("ancients", original.Lines[i].LineText!.LocEntryKey);
            copy.Lines[i].LineText!.Add("VisitsPlusTwo", previousVisits + 2);
            if (i < copy.Lines.Count - 1)
                copy.Lines[i].NextButtonText = new LocString("ancients", original.Lines[i].NextButtonText!.LocEntryKey);
        }
        return copy;
    }
}
