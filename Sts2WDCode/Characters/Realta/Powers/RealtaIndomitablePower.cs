using Sts2WD.Sts2WDCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaIndomitablePower : Sts2WDPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        bool lostHpLastTurn = CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>()
            .Any(e => e.Receiver == Owner && !e.Result.WasFullyBlocked && e.HappenedLastPlayerTurn(player));
        if (!lostHpLastTurn) return;
        Flash();
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
