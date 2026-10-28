using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusDragonsWaymark : MercuriusRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    private bool _used;

    public override Task BeforeCombatStart()
    {
        _used = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner || _used || Owner.PlayerCombatState!.TurnNumber != 1) return;
        _used = true;
        Flash();
        await MercuriusOpeningHand.Replace(context, Owner);
    }
}
