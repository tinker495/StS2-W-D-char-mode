using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sts2WD.Sts2WDCode.Localization;

// Authoring data is separate from the shipped localization baseline. An empty draft
// must never shadow an existing translation or register an empty conversation.
internal sealed class CharacterTextPreset
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectNullableAnnotations = true
    };

    private static readonly HashSet<string> CharacterFields =
    [
        "title", "titleObject", "description", "pronounObject",
        "possessiveAdjective", "pronounPossessive", "pronounSubject",
        "unlockText", "goldMonologue", "eventDeathPrevention", "aromaPrinciple",
        "cardsModifierTitle", "cardsModifierDescription",
        "banter.alive.endTurnPing", "banter.dead.endTurnPing"
    ];

    private static readonly HashSet<string> AncientIds =
    [
        "NEOW", "DARV", "OROBAS", "PAEL", "TEZCATARA", "NONUPEIPE",
        "TANX", "VAKUU", "THE_ARCHITECT"
    ];

    private static readonly HashSet<string> Attackers = ["None", "Player", "Architect", "Both"];

    public required string CharacterId { get; init; }
    public Dictionary<string, DraftText> Characters { get; init; } = [];
    public Dictionary<string, List<DraftDialogue>> Ancients { get; init; } = [];
    public Dictionary<string, DraftText> RelicFlavor { get; init; } = [];
    public DraftText SeaGlassTitle { get; init; } = new();

    public static CharacterTextPreset Parse(string json)
    {
        var preset = JsonSerializer.Deserialize<CharacterTextPreset>(json, JsonOptions)
                     ?? throw new InvalidDataException("Expected a character text preset.");
        preset.Validate();
        return preset;
    }

    public Dictionary<string, string> Apply(
        string language, string table, Dictionary<string, string> baseline, Action<string> warn)
    {
        // Work on a copy so a rejected draft cannot partially change localization.
        var result = new Dictionary<string, string>(baseline);
        switch (table)
        {
            case "characters":
                foreach (var (field, draft) in Characters)
                    AddIfWritten(result, $"{CharacterId}.{field}", draft.Resolve(language));
                break;
            case "relics":
                foreach (var (relicId, draft) in RelicFlavor)
                    AddIfWritten(result, $"{relicId}.flavor", draft.Resolve(language));
                AddIfWritten(result, $"SEA_GLASS.{CharacterId}.title", SeaGlassTitle.Resolve(language));
                break;
            case "ancients":
                ApplyDialogues(language, result, warn);
                break;
        }
        return result;
    }

    private void ApplyDialogues(string language, Dictionary<string, string> result, Action<string> warn)
    {
        foreach (var (ancientId, drafts) in Ancients)
        {
            var ready = new List<(DraftDialogue Dialogue, string Language)>();
            foreach (var draft in drafts)
            {
                var completeLanguage = PreferredLanguages(language)
                    .FirstOrDefault(candidate => draft.Lines.Count > 0 &&
                        draft.Lines.All(line => !string.IsNullOrWhiteSpace(line.Text.In(candidate))));
                if (completeLanguage != null)
                    ready.Add((draft, completeLanguage));
                else if (draft.Lines.Any(line => line.Text.IsWritten))
                    warn($"{CharacterId}/{ancientId}/{draft.Id}: incomplete dialogue skipped; complete all lines in one language.");
            }

            if (ready.Count == 0)
                continue;

            var prefix = $"{ancientId}.talk.{CharacterId}.";
            var existing = result.Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal)).ToArray();

            // The Architect has no ANY dialogue pool. Preserve the original repeating
            // exchange unless the author has supplied a repeating dialogue valid from
            // the beginning. -1 never matches a nonnegative win count, but is eligible
            // for the native repeating pool, so it cannot displace a milestone dialogue.
            var needsArchitectFallback = ancientId == "THE_ARCHITECT" &&
                !ready.Any(item => item.Dialogue.Repeatable && (item.Dialogue.VisitIndex ?? -1) <= 0);
            if (needsArchitectFallback &&
                !existing.Any(pair => pair.Key == $"{prefix}0-0r.char" || pair.Key == $"{prefix}0-0r.ancient"))
            {
                warn($"{CharacterId}/{ancientId}: supply an always-available repeating dialogue; no baseline fallback exists.");
                continue;
            }

            foreach (var pair in existing)
                result.Remove(pair.Key);

            for (var index = 0; index < ready.Count; index++)
            {
                var (dialogue, textLanguage) = ready[index];
                var dialogueKey = $"{prefix}{index}";
                result[$"{dialogueKey}-visit"] = (dialogue.VisitIndex ?? -1).ToString(CultureInfo.InvariantCulture);
                for (var lineIndex = 0; lineIndex < dialogue.Lines.Count; lineIndex++)
                {
                    var line = dialogue.Lines[lineIndex];
                    var lineKey = $"{dialogueKey}-{lineIndex}{(dialogue.Repeatable ? "r" : "")}";
                    result[$"{lineKey}.{line.Speaker}"] = line.Text.In(textLanguage);
                    if (lineIndex < dialogue.Lines.Count - 1)
                    {
                        var next = line.Next.In(textLanguage);
                        result[$"{lineKey}.next"] = string.IsNullOrWhiteSpace(next)
                            ? textLanguage == "kor" ? "계속" : "Continue"
                            : next;
                    }
                    if (!string.IsNullOrWhiteSpace(line.Sfx))
                        result[$"{lineKey}.{line.Speaker}.sfx"] = line.Sfx;
                }
                if (ancientId == "THE_ARCHITECT")
                {
                    result[$"{dialogueKey}-startattack"] = dialogue.StartAttackers;
                    result[$"{dialogueKey}-endattack"] = dialogue.EndAttackers;
                }
            }

            if (needsArchitectFallback)
            {
                var oldPrefix = $"{prefix}0-";
                foreach (var pair in existing.Where(pair => pair.Key.StartsWith(oldPrefix, StringComparison.Ordinal)))
                    result[$"{prefix}{ready.Count}-{pair.Key[oldPrefix.Length..]}"] = pair.Value;
                result[$"{prefix}{ready.Count}-visit"] = "-1";
            }
        }
    }

    private void Validate()
    {
        RequireModId(CharacterId);
        foreach (var (field, draft) in Characters)
        {
            if (!CharacterFields.Contains(field) || draft == null)
                throw new InvalidDataException($"Unknown or null character field: {field}");
        }
        foreach (var (relicId, draft) in RelicFlavor)
        {
            RequireModId(relicId);
            if (draft == null)
                throw new InvalidDataException($"Null relic flavor: {relicId}");
        }
        foreach (var (ancientId, dialogues) in Ancients)
        {
            if (!AncientIds.Contains(ancientId) || dialogues == null)
                throw new InvalidDataException($"Unknown or null ancient: {ancientId}");
            var ids = new HashSet<string>();
            foreach (var dialogue in dialogues)
            {
                if (dialogue == null || string.IsNullOrWhiteSpace(dialogue.Id) || !ids.Add(dialogue.Id))
                    throw new InvalidDataException($"Dialogue IDs must be nonempty and unique within {ancientId}.");
                if (dialogue.VisitIndex < 0 || (dialogue.VisitIndex == null && !dialogue.Repeatable))
                    throw new InvalidDataException($"{ancientId}/{dialogue.Id}: use a nonnegative visitIndex, or null for a repeating dialogue.");
                if (!Attackers.Contains(dialogue.StartAttackers) || !Attackers.Contains(dialogue.EndAttackers))
                    throw new InvalidDataException($"{ancientId}/{dialogue.Id}: invalid Architect attacker.");
                foreach (var line in dialogue.Lines)
                {
                    if (line == null || line.Speaker is not ("ancient" or "char"))
                        throw new InvalidDataException($"{ancientId}/{dialogue.Id}: speaker must be ancient or char.");
                }
            }
        }
    }

    private static void RequireModId(string id)
    {
        const string prefix = "STS2WD-";
        if (!id.StartsWith(prefix, StringComparison.Ordinal) || id.Length == prefix.Length ||
            id[prefix.Length..].Any(c => !char.IsAsciiLetterUpper(c) && !char.IsAsciiDigit(c) && c != '_'))
            throw new InvalidDataException($"Expected a STS2WD model ID, got: {id}");
    }

    private static IEnumerable<string> PreferredLanguages(string language) =>
        new[] { language, "kor", "eng" }.Distinct();

    private static void AddIfWritten(Dictionary<string, string> result, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            result[key] = value;
    }

    internal sealed class DraftText
    {
        public string Kor { get; init; } = "";
        public string Eng { get; init; } = "";
        public bool IsWritten => !string.IsNullOrWhiteSpace(Kor) || !string.IsNullOrWhiteSpace(Eng);
        public string In(string language) => language == "kor" ? Kor : language == "eng" ? Eng : "";
        public string? Resolve(string language) =>
            PreferredLanguages(language).Select(In).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
    }

    internal sealed class DraftDialogue
    {
        public required string Id { get; init; }
        public int? VisitIndex { get; init; }
        public bool Repeatable { get; init; }
        public string StartAttackers { get; init; } = "None";
        public string EndAttackers { get; init; } = "Architect";
        public List<DraftLine> Lines { get; init; } = [];
    }

    internal sealed class DraftLine
    {
        public required string Speaker { get; init; }
        public DraftText Text { get; init; } = new();
        public DraftText Next { get; init; } = new();
        public string Sfx { get; init; } = "";
    }
}
