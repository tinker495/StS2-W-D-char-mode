using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaGeniusInspirationPower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player is not { } player || Owner.IsDead) return;
        int eligible = PileType.Deck.GetPile(player).Cards.Count(RefineCmd.CanRefine);
        int count = Math.Min(Amount, eligible);
        if (count <= 0) return;

        CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, count)
        {
            Cancelable = false,
            RequireManualConfirmation = true
        };
        foreach (CardModel card in await CardSelectCmd.FromDeckGeneric(player, prefs, RefineCmd.CanRefine))
            RefineCmd.RefinePermanent(card, 5);
    }
}
