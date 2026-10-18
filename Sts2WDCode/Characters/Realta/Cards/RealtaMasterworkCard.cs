using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Masterwork repeats the card body and its enchantment OnPlay inside one card play (docs/adr/0005).
// Attack repeats share a single AttackContext so Vigor applies to every hit and is consumed once at the end.
public abstract class RealtaMasterworkCard(CardType type, CardRarity rarity, TargetType target)
    : RealtaCard(0, type, rarity, target)
{
    protected override bool HasEnergyCostX => true;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Masterwork)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int repeats = ResolveEnergyXValue();
        if (repeats <= 0) return;

        await using AttackContext? attackContext = Type == CardType.Attack
            ? await AttackCommand.CreateContextAsync(CombatState!, choiceContext, this)
            : null;

        bool executedAnyRepeat = false;
        for (int i = 0; i < repeats && !Owner.Creature.IsDead; i++)
        {
            if (CombatManager.Instance.IsOverOrEnding || cardPlay.Target is { IsDead: true }) break;
            executedAnyRepeat = true;
            await OnRepeat(choiceContext, cardPlay);
            if (Enchantment != null && !Owner.Creature.IsDead)
            {
                await Enchantment.OnPlay(choiceContext, cardPlay);
                Enchantment.InvokeExecutionFinished();
            }
        }

        if (Type == CardType.Attack && executedAnyRepeat && !Owner.Creature.IsDead && Owner.Creature.GetPower<VigorPower>() is { Amount: > 0 } vigor)
            await PowerCmd.ModifyAmount(choiceContext, vigor, -vigor.Amount, null, null);
    }

    protected abstract Task OnRepeat(PlayerChoiceContext choiceContext, CardPlay cardPlay);
}
