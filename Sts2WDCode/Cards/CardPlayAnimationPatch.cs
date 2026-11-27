using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace Sts2WD.Sts2WDCode.Cards;

/// <summary>
/// Routes defensive Skill card plays to the "Guard" creature animation trigger,
/// while letting Attack cards keep "Attack" priority and Sts2WDCard override per card.
/// </summary>
[HarmonyPatch(typeof(Hook), nameof(Hook.BeforeCardPlayed))]
internal static class CardPlayAnimationPatch
{
    public const string GuardTrigger = "Guard";

    private static void Postfix(CardPlay cardPlay)
    {
        if (CombatManager.Instance.IsOverOrEnding)
            return;

        var creature = cardPlay.Player?.Creature;
        if (creature == null || creature.IsDead)
            return;

        var trigger = ResolveTrigger(cardPlay.Card);
        if (string.IsNullOrEmpty(trigger))
            return;

        NCombatRoom.Instance?.GetCreatureNode(creature)?.SetAnimationTrigger(trigger);
    }

    internal static string? ResolveTrigger(CardModel? card)
    {
        if (card == null)
            return null;

        if (card is Sts2WDCard { CustomPlayAnimTrigger: { } customTrigger })
            return customTrigger.Length > 0 ? customTrigger : null;

        if (card.Type != CardType.Skill)
            return null;

        return IsDefensiveSkill(card) ? GuardTrigger : null;
    }

    private static bool IsDefensiveSkill(CardModel card) =>
        card.GainsBlock
        || card.Tags.Contains(CardTag.Defend)
        || card.DynamicVars.ContainsKey("Block")
        || card.DynamicVars.ContainsKey("Guard");
}
