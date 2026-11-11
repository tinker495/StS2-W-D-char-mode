using System.Collections.Concurrent;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using Sts2WD.Sts2WDCode.Extensions;

namespace Sts2WD.Sts2WDCode.Cards;

// Shared artwork behavior. Inherit a character-specific card base to assign its pool.
public abstract class Sts2WDCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    private static readonly ConcurrentDictionary<string, string> PortraitPathCache = new();

    public override string CustomPortraitPath => ResolvePortraitPath();
    public override string PortraitPath => CustomPortraitPath;
    public override string BetaPortraitPath => CustomPortraitPath;

    protected virtual string FallbackPortraitPath =>
        Rarity == CardRarity.Ancient
            ? Path.Join(MainFile.ResPath, "images", "card_portraits", "big", "ancient_card.png")
            : Path.Join(MainFile.ResPath, "images", "card_portraits", "card.png");

    protected virtual string ResolveCharacterSubfolder()
    {
        var entry = Id.Entry;
        if (entry.StartsWith("STS2WD-", StringComparison.OrdinalIgnoreCase))
        {
            var parts = entry.Substring(7);
            var underscore = parts.IndexOf('_');
            if (underscore > 0)
                return parts.Substring(0, underscore).ToLowerInvariant();
        }

        var ns = GetType().Namespace;
        if (!string.IsNullOrEmpty(ns) && ns.Contains("Characters."))
        {
            var seg = ns.Split("Characters.")[1];
            var dot = seg.IndexOf('.');
            return (dot > 0 ? seg.Substring(0, dot) : seg).ToLowerInvariant();
        }

        return string.Empty;
    }

    private string ResolvePortraitPath()
    {
        var key = Id.Entry;
        if (PortraitPathCache.TryGetValue(key, out var cached))
            return cached;

        var charFolder = ResolveCharacterSubfolder();
        var fileName = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

        if (!string.IsNullOrEmpty(charFolder))
        {
            var charSpecificPath = Path.Join(MainFile.ResPath, "images", "card_portraits", charFolder, fileName);
            if (ResourceLoader.Exists(charSpecificPath))
            {
                PortraitPathCache[key] = charSpecificPath;
                return charSpecificPath;
            }
        }

        var fallback = FallbackPortraitPath;
        PortraitPathCache[key] = fallback;
        return fallback;
    }
}
