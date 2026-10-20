using System.Text.Json;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using FileAccess = Godot.FileAccess;

namespace Sts2WD.Sts2WDCode.Localization;

// Merge at the mod's existing localization boundary, before LocTable creates
// its English fallback. BaseLib still owns AncientDialogue construction.
[HarmonyPatch(typeof(LocManager), "LoadTable")]
internal static class CharacterTextLocalization
{
    private const string PresetDirectory = MainFile.ResPath + "/character_text";
    private const string LocalizationDirectory = MainFile.ResPath + "/localization/";
    private static readonly Lazy<IReadOnlyList<CharacterTextPreset>> Presets = new(ReadPresets);

    [HarmonyPostfix]
    [HarmonyBefore("BaseLib")]
    private static void ApplyPresets(string path, ref Dictionary<string, string> __result)
    {
        if (!path.StartsWith(LocalizationDirectory, StringComparison.Ordinal))
            return;

        var relativePath = path[LocalizationDirectory.Length..].Split('/');
        if (relativePath.Length != 2 || relativePath[1] is not ("characters.json" or "ancients.json" or "relics.json"))
            return;

        var language = relativePath[0];
        var table = System.IO.Path.GetFileNameWithoutExtension(relativePath[1]);
        foreach (var preset in Presets.Value)
            __result = preset.Apply(language, table, __result, message => MainFile.Logger.Warn(message));
    }

    private static IReadOnlyList<CharacterTextPreset> ReadPresets()
    {
        using var directory = DirAccess.Open(PresetDirectory);
        if (directory == null)
        {
            MainFile.Logger.Warn($"Character text preset directory not found: {PresetDirectory}");
            return [];
        }

        var presets = new List<CharacterTextPreset>();
        var characterIds = new HashSet<string>();
        foreach (var file in directory.GetFiles().Order(StringComparer.Ordinal))
        {
            if (!file.EndsWith(".json", StringComparison.Ordinal) || file.StartsWith('_'))
                continue;
            try
            {
                var preset = CharacterTextPreset.Parse(FileAccess.GetFileAsString($"{PresetDirectory}/{file}"));
                if (!characterIds.Add(preset.CharacterId))
                    throw new InvalidDataException($"Duplicate characterId: {preset.CharacterId}");
                presets.Add(preset);
            }
            catch (Exception exception) when (exception is JsonException or InvalidDataException)
            {
                MainFile.Logger.Warn($"Skipping character text preset {file}: {exception.Message}");
            }
        }
        return presets;
    }
}
