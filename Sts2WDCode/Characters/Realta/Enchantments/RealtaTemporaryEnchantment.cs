using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// 일시적 인챈트 (docs/adr/0003): a combat-only copy of a base-game enchantment, kept outside the card's
// single enchantment slot. It is only attached to combat copies of cards, so it disappears with them.
// Same kind + same scope stacks into one entry; permanent enchantments stay in the slot and are shown separately.
public abstract class RealtaTemporaryEnchantment : CardModifier
{
    // Defensive: if some effect ever clones the card out of combat, the modifier stays inert there.
    protected bool IsActive => Owner?.Pile?.IsCombatPile == true;

    public static bool IsOn(CardModel card) => card.GetModifiers().OfType<RealtaTemporaryEnchantment>().Any();

    public override bool ApplyStacked(CardModifier newApplied)
    {
        Amount += newApplied.Amount;
        return true;
    }

    public override void ModifyDescriptionPost(Creature? target, ref string description)
    {
        if (!IsActive) return;
        description += "\n" + GetLoc().GetFormattedText();
    }
}

// Temporary MegaCrit.Sts2.Core.Models.Enchantments.Sharp.
public sealed class RealtaTemporarySharp : RealtaTemporaryEnchantment
{
    public override decimal ModifyBaseDamageAdditive(decimal originalDamage, ValueProp props)
    {
        if (!IsActive || !props.IsPoweredAttack()) return 0m;
        return Amount;
    }
}

// Temporary MegaCrit.Sts2.Core.Models.Enchantments.Adroit.
public sealed class RealtaTemporaryAdroit : RealtaTemporaryEnchantment
{
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsActive) return;
        await CreatureCmd.GainBlock(Owner!.Owner.Creature, Amount, ValueProp.Move, cardPlay);
    }
}
