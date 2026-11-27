using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using Sts2WD.Sts2WDCode.Cards;

namespace Sts2WD.Sts2WDCode.DevConsole;

// Auto-discovered by MegaCrit.Sts2.Core.DevConsole.DevConsole for interactive visual/content QA.
public sealed class WdConsoleCmd : AbstractConsoleCmd
{
    private static readonly string[] Subcommands = ["cards", "relics", "status"];
    private static readonly string[] CardFilters = ["all", "attack", "skill", "power", "rare", "ancient", "missing-art"];
    private static readonly string[] UpgradeFlags = ["upgraded"];

    public override string CmdName => "wd";
    public override string Args => "<cards|relics|status> [char] [all|attack|skill|power|rare|ancient|missing-art] [upgraded]";
    public override string Description => "Sts2WD QA helper: spawn character card sets/relics or inspect asset coverage.";
    public override bool IsNetworked => false;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        string sub = args.Length > 0 ? args[0].ToLowerInvariant() : "status";
        return sub switch
        {
            "cards" => ProcessCards(issuingPlayer, args.Skip(1).ToArray()),
            "relics" => ProcessRelics(issuingPlayer, args.Skip(1).ToArray()),
            "status" => ProcessStatus(issuingPlayer, args.Skip(1).ToArray()),
            _ => new CmdResult(false, $"Unknown subcommand '{sub}'. Usage: wd {Args}")
        };
    }

    public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
    {
        if (args.Length <= 1)
            return CompleteArgument(Subcommands, [], args.FirstOrDefault() ?? "");

        string sub = args[0].ToLowerInvariant();
        string[] completed = args.Take(args.Length - 1).ToArray();
        string partial = args.LastOrDefault() ?? "";
        List<string> charNames = GetModCharacters()
            .Select(c => c.GetType().Name.ToLowerInvariant())
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        if (args.Length == 2)
        {
            IEnumerable<string> secondCandidates = sub == "cards"
                ? charNames.Concat(CardFilters)
                : charNames;
            return CompleteArgument(secondCandidates, completed, partial);
        }

        if (sub == "cards" && args.Length == 3)
        {
            bool secondWasFilter = CardFilters.Contains(args[1], StringComparer.OrdinalIgnoreCase);
            return CompleteArgument(secondWasFilter ? UpgradeFlags : CardFilters.Concat(UpgradeFlags), completed, partial);
        }

        if (sub == "cards" && args.Length == 4)
            return CompleteArgument(UpgradeFlags, completed, partial);

        return base.GetArgumentCompletions(player, args);
    }

    private static CmdResult ProcessCards(Player? issuingPlayer, string[] rest)
    {
        if (!RunManager.Instance.IsInProgress || issuingPlayer == null)
            return new CmdResult(false, "Start a run before spawning QA cards.");

        bool upgraded = rest.Any(a => a.Equals("upgraded", StringComparison.OrdinalIgnoreCase) || a == "+");
        string[] tokens = rest
            .Where(a => !a.Equals("upgraded", StringComparison.OrdinalIgnoreCase) && a != "+")
            .ToArray();

        string filter = "all";
        CharacterModel? target = null;

        if (tokens.Length == 0)
        {
            target = ResolveDefaultCharacter(issuingPlayer);
        }
        else if (CardFilters.Contains(tokens[0], StringComparer.OrdinalIgnoreCase))
        {
            filter = tokens[0].ToLowerInvariant();
            target = ResolveDefaultCharacter(issuingPlayer);
        }
        else
        {
            target = FindModCharacter(tokens[0]);
            if (target == null)
                return new CmdResult(false, $"Unknown Sts2WD character '{tokens[0]}'. Available: {ListCharacterNames()}");
            if (tokens.Length >= 2)
            {
                if (!CardFilters.Contains(tokens[1], StringComparer.OrdinalIgnoreCase))
                    return new CmdResult(false, $"Unknown card filter '{tokens[1]}'. Valid: {string.Join(", ", CardFilters)}");
                filter = tokens[1].ToLowerInvariant();
            }
        }

        if (target == null)
            return new CmdResult(false, $"Current character is not a Sts2WD character; specify one of: {ListCharacterNames()}");

        List<CardModel> matched = GetCharacterCards(target)
            .Where(c => MatchesCardFilter(c, filter))
            .OrderBy(c => c.Rarity)
            .ThenBy(c => c.Id.Entry, StringComparer.Ordinal)
            .ToList();

        if (matched.Count == 0)
            return new CmdResult(false, $"No cards matched '{target.GetType().Name}' ({filter}).");

        bool inCombat = CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding;
        ICardScope? scope = inCombat
            ? CombatManager.Instance.DebugOnlyGetState()
            : RunManager.Instance.DebugOnlyGetState();
        if (scope == null)
            return new CmdResult(false, "Active run/combat card scope is unavailable.");

        int handSlots = inCombat
            ? Math.Max(0, CardPile.MaxCardsInHand - PileType.Hand.GetPile(issuingPlayer).Cards.Count)
            : 0;

        int toHand = 0;
        int toSecondary = 0;
        List<Task> tasks = new(matched.Count);

        foreach (CardModel template in matched)
        {
            CardModel created = scope.CreateCard(template, issuingPlayer);
            // Upgrade while unpiled so combat-upgrade gameplay hooks do not fire during QA staging.
            if (upgraded && created.IsUpgradable)
                CardCmd.Upgrade(created, CardPreviewStyle.None);

            PileType destination;
            if (inCombat)
            {
                if (toHand < handSlots)
                {
                    destination = PileType.Hand;
                    toHand++;
                }
                else
                {
                    destination = PileType.Draw;
                    toSecondary++;
                }
            }
            else
            {
                destination = PileType.Deck;
                toSecondary++;
            }

            tasks.Add(CardPileCmd.Add(created, destination));
        }

        string placement = inCombat
            ? $"{toHand} in Hand, {toSecondary} in Draw"
            : $"{toSecondary} in Deck";
        return new CmdResult(
            Task.WhenAll(tasks),
            true,
            $"Spawned {matched.Count}{(upgraded ? " upgraded" : "")} {target.GetType().Name} ({filter}) card(s): {placement}.");
    }

    private static CmdResult ProcessRelics(Player? issuingPlayer, string[] rest)
    {
        if (!RunManager.Instance.IsInProgress || issuingPlayer == null)
            return new CmdResult(false, "Start a run before granting QA relics.");

        CharacterModel? target = rest.Length > 0 ? FindModCharacter(rest[0]) : ResolveDefaultCharacter(issuingPlayer);
        if (target == null)
            return new CmdResult(false, $"Specify a Sts2WD character: {ListCharacterNames()}");

        List<RelicModel> relics = GetCharacterRelics(target)
            .Where(r => issuingPlayer.GetRelicById(r.Id) == null)
            .OrderBy(r => r.Id.Entry, StringComparer.Ordinal)
            .ToList();

        if (relics.Count == 0)
            return new CmdResult(true, $"All {target.GetType().Name} relics are already held (or pool is empty).");

        foreach (RelicModel relic in relics)
            TaskHelper.RunSafely(RelicCmd.Obtain(relic.ToMutable(), issuingPlayer));

        return new CmdResult(true, $"Granted {relics.Count} {target.GetType().Name} relic(s): {string.Join(", ", relics.Select(r => r.Id.Entry))}");
    }

    private static CmdResult ProcessStatus(Player? issuingPlayer, string[] rest)
    {
        List<CharacterModel> targets;
        if (rest.Length > 0)
        {
            CharacterModel? single = FindModCharacter(rest[0]);
            if (single == null)
                return new CmdResult(false, $"Unknown Sts2WD character '{rest[0]}'. Available: {ListCharacterNames()}");
            targets = [single];
        }
        else
        {
            CharacterModel? active = issuingPlayer != null ? ResolveDefaultCharacter(issuingPlayer) : null;
            targets = active != null ? [active] : GetModCharacters().OrderBy(c => c.GetType().Name, StringComparer.Ordinal).ToList();
        }

        List<string> lines = [];
        foreach (CharacterModel character in targets)
        {
            List<CardModel> cards = GetCharacterCards(character);
            int customArt = cards.Count(c => !UsesFallbackPortrait(c));
            int relicCount = GetCharacterRelics(character).Count;
            int potionCount = character.PotionPool?.AllPotions.Count() ?? 0;

            string sceneSummary = "n/a";
            if (character is CustomCharacterModel custom)
            {
                (string Label, string? Path)[] scenes =
                [
                    ("combat", custom.CustomVisualPath),
                    ("merchant", custom.CustomMerchantAnimPath),
                    ("rest", custom.CustomRestSiteAnimPath),
                    ("select", custom.CustomCharacterSelectBg)
                ];
                int ready = scenes.Count(s => !string.IsNullOrEmpty(s.Path) && ResourceLoader.Exists(s.Path));
                sceneSummary = $"{ready}/{scenes.Length} scenes";
            }

            lines.Add($"{character.GetType().Name}: cards {customArt}/{cards.Count} custom art, relics {relicCount}, potions {potionCount}, {sceneSummary}");
        }

        return new CmdResult(true, string.Join(" | ", lines));
    }

    private static IEnumerable<CharacterModel> GetModCharacters() =>
        ModelDb.AllCharacters.Where(c => c.GetType().Namespace?.StartsWith("Sts2WD.", StringComparison.Ordinal) == true);

    private static CharacterModel? ResolveDefaultCharacter(Player player) =>
        player.Character != null && player.Character.GetType().Namespace?.StartsWith("Sts2WD.", StringComparison.Ordinal) == true
            ? player.Character
            : null;

    private static CharacterModel? FindModCharacter(string token) =>
        GetModCharacters().FirstOrDefault(c =>
            c.GetType().Name.Equals(token, StringComparison.OrdinalIgnoreCase)
            || c.Id.Entry.Equals(token, StringComparison.OrdinalIgnoreCase)
            || c.Id.Entry.Equals($"STS2WD-{token}", StringComparison.OrdinalIgnoreCase));

    private static string ListCharacterNames() =>
        string.Join(", ", GetModCharacters().Select(c => c.GetType().Name.ToLowerInvariant()).OrderBy(n => n, StringComparer.Ordinal));

    private static List<CardModel> GetCharacterCards(CharacterModel character)
    {
        HashSet<ModelId> seen = [];
        List<CardModel> result = [];

        if (character.CardPool != null)
        {
            foreach (CardModel card in character.CardPool.AllCards)
            {
                if (seen.Add(card.Id))
                    result.Add(card);
            }

            foreach (CardModel card in ModelDb.AllCards)
            {
                if (card is Sts2WDCard && card.VisualCardPool?.Id == character.CardPool.Id && seen.Add(card.Id))
                    result.Add(card);
            }
        }

        return result;
    }

    private static List<RelicModel> GetCharacterRelics(CharacterModel character)
    {
        HashSet<ModelId> seen = [];
        List<RelicModel> result = [];
        string nsPrefix = $"Sts2WD.Sts2WDCode.Characters.{character.GetType().Name}.";

        if (character.RelicPool != null)
        {
            foreach (RelicModel relic in character.RelicPool.AllRelics)
            {
                if (seen.Add(relic.Id))
                    result.Add(relic);
            }
        }

        foreach (RelicModel relic in ModelDb.AllRelics)
        {
            if (relic.GetType().Namespace?.StartsWith(nsPrefix, StringComparison.Ordinal) == true && seen.Add(relic.Id))
                result.Add(relic);
        }

        return result;
    }

    private static bool MatchesCardFilter(CardModel card, string filter) =>
        filter switch
        {
            "attack" => card.Type == CardType.Attack,
            "skill" => card.Type == CardType.Skill,
            "power" => card.Type == CardType.Power,
            "rare" => card.Rarity == CardRarity.Rare,
            "ancient" => card.Rarity == CardRarity.Ancient,
            "missing-art" => UsesFallbackPortrait(card),
            _ => true
        };

    private static bool UsesFallbackPortrait(CardModel card)
    {
        string path = card.PortraitPath.Replace('\\', '/');
        return path.EndsWith("/card.png", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith("/ancient_card.png", StringComparison.OrdinalIgnoreCase);
    }
}
