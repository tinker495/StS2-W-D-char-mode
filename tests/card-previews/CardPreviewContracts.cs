// Human requester: wjdrb, 2026-10-25 (retrospective items approved with "진행").
// These explicit cases encode the earlier Althea patch request and "Q1 예":
// include all four Reform outcomes and Guard's card even through keywords.
// Expected types/upgrades below are specifications, not generated snapshots.
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using Sts2WD.Sts2WDCode.Characters.Althea;
using Sts2WD.Sts2WDCode.Characters.Realta;

internal static class CardPreviewContracts
{
    private sealed record Contract(Type Source, bool Upgraded, Type[] Cards, bool UpgradedPreviews = false);

    private static IEnumerable<Contract> Cases()
    {
        Type[] reform = [typeof(AltheaSiarcun), typeof(AltheaLleurh), typeof(AltheaMorfiella), typeof(AltheaPureWill)];
        // Both source states are explicit: a regression in IsUpgradable must
        // fail the upgraded case, never silently remove it from collection.
        foreach (bool upgraded in new[] { false, true })
        {
            foreach (Type source in new[] { typeof(AltheaHauntedStrike), typeof(AltheaAncestralGrudge),
                typeof(AltheaCurseOfNoRetreat), typeof(AltheaWhisperingWraith), typeof(AltheaGatheringSouls),
                typeof(AltheaVesselOfDelusion), typeof(AltheaSoulVeil) })
                yield return new(source, upgraded, [typeof(AltheaWraith)]);

            yield return new(typeof(AltheaHeavyStrike), upgraded, [typeof(Doubt)]);
            yield return new(typeof(AltheaKarmicMadness), upgraded, [typeof(Shame)]);
            yield return new(typeof(AltheaFadedCrest), upgraded, [typeof(Folly)]);

            // Warrior Strike knows its own target upgrade. Other Reform
            // generators have no selected target yet, so show base outcomes.
            yield return new(typeof(AltheaWarriorStrike), upgraded, reform, upgraded);
            foreach (Type source in new[] { typeof(AltheaRationalWill), typeof(AltheaPathToGreatness),
                typeof(AltheaResoluteWill), typeof(AltheaLiberation), typeof(AltheaArchipelagoArts) })
                yield return new(source, upgraded, reform);

            foreach (Type source in new[] { typeof(RealtaArcaneShock), typeof(RealtaSnatch), typeof(RealtaFathersAdvice) })
                yield return new(source, upgraded, [typeof(RealtaGuard)]);

            // Tokens describe themselves; their previews must not recurse.
            foreach (Type source in reform.Append(typeof(RealtaGuard)))
                yield return new(source, upgraded, []);
        }

        foreach (Type source in new[] { typeof(AltheaPathToGreatnessPower), typeof(AltheaArchipelagoArtsPower),
            typeof(AltheaReformEchoPower) })
            yield return new(source, false, reform);
        foreach (Type source in new[] { typeof(RealtaFathersAdvicePower), typeof(RealtaBronzeWatchdog), typeof(RealtaKuu) })
            yield return new(source, false, [typeof(RealtaGuard)]);
        yield return new(typeof(AltheaWraith), false, []);
    }

    public static int Run(TestResults results)
    {
        InitializeKeywordText();
        Contract[] cases = Cases().ToArray();
        foreach (Type type in cases.Select(item => item.Source).Concat(cases.SelectMany(item => item.Cards)).Distinct())
            ModelDb.Inject(type);

        foreach (Contract contract in cases)
        {
            AbstractModel model = ModelDb.GetById<AbstractModel>(ModelDb.GetId(contract.Source));
            if (model is CardModel canonical)
            {
                var card = canonical.ToMutable();
                if (contract.Upgraded)
                {
                    card.UpgradeInternal();
                    card.FinalizeUpgradeInternal();
                }
                model = card;
            }

            var getter = contract.Source.GetProperty("ExtraHoverTips", BindingFlags.Instance | BindingFlags.NonPublic)
                ?? throw new MissingMemberException(contract.Source.FullName, "ExtraHoverTips");
            var tips = ((IEnumerable<IHoverTip>)getter.GetValue(model)!).OfType<CardHoverTip>().ToArray();
            Type[] actual = tips.Select(tip => tip.Card.GetType()).ToArray();
            bool passed = actual.Length == contract.Cards.Length
                && actual.Distinct().Count() == actual.Length
                && contract.Cards.All(actual.Contains)
                && !actual.Contains(contract.Source)
                && tips.All(tip => tip.Card.IsUpgraded == contract.UpgradedPreviews)
                && (model is not CardModel sourceCard || sourceCard.IsUpgraded == contract.Upgraded);
            string expected = string.Join(",", contract.Cards.Select(type => type.Name + (contract.UpgradedPreviews ? "+" : "")));
            string observed = string.Join(",", tips.Select(tip => tip.Card.GetType().Name + (tip.Card.IsUpgraded ? "+" : "")));
            results.Check($"{contract.Source.Name}{(contract.Upgraded ? "+" : "")}", passed,
                $"expected=[{expected}] actual=[{observed}] sourceUpgraded={(model as CardModel)?.IsUpgraded}");
        }
        return cases.Length;
    }

    private static void InitializeKeywordText()
    {
        // Fixture only: no localization services or game boot. Native model
        // factories and ExtraHoverTips are real; displayed wording is not tested.
        int nextKeyword = 10000;
        foreach (Type keywords in new[] { typeof(AltheaKeywords), typeof(RealtaKeywords) })
            foreach (FieldInfo field in keywords.GetFields(BindingFlags.Static | BindingFlags.Public))
                if (field.FieldType == typeof(CardKeyword)) field.SetValue(null, (CardKeyword)nextKeyword++);
        var cacheField = typeof(HoverTipFactory).GetField("_keywordHoverTips", BindingFlags.Static | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(HoverTipFactory).FullName, "_keywordHoverTips");
        var cache = (Dictionary<CardKeyword, HoverTip>)cacheField.GetValue(null)!;
        foreach (CardKeyword keyword in Enum.GetValues<CardKeyword>().Concat(
            Enumerable.Range(10000, nextKeyword - 10000).Select(value => (CardKeyword)value)))
            cache[keyword] = new HoverTip { Id = "test-keyword-" + (int)keyword };
    }
}
