using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaPortableForgePower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner) return;
        int refinement = Amount;
        Flash();
        // Consume before resolving synergies, even if this card is not eligible for refinement.
        await PowerCmd.Remove(this);
        // Refinement can draw a card through Hero's Silhouette and trigger another player choice.
        // Reuse the running action's pause/resume protocol; enemy-turn autoplays have no player action to pause.
        PlayerChoiceContext context = RunManager.Instance.ActionExecutor.CurrentlyRunningAction is { } action
            ? new GameActionPlayerChoiceContext(action)
            : new BlockingPlayerChoiceContext();
        await RefineCmd.Refine(context, cardPlay.Card, refinement);
    }
}
