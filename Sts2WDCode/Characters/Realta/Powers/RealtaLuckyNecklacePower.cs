using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaLuckyNecklacePower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<RoyaltiesPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<RoyaltiesPower>().ResolvedBigIconPath;

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player is not { } player) return Task.CompletedTask;
        for (int i = 0; i < Amount; i++)
            room.AddExtraReward(player, new CardReward(CardCreationOptions.ForRoom(player, RoomType.Boss), 3, player));
        return Task.CompletedTask;
    }
}
