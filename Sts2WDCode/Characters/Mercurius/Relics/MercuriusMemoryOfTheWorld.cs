using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusMemoryOfTheWorld : MercuriusRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromCard<MercuriusTuneFromHand>(),
        HoverTipFactory.FromCard<MercuriusTuneFromDiscard>(),
        HoverTipFactory.FromCard<MercuriusTuneShuffle>()
    ];
    private bool _openingHandled;

    public override Task BeforeCombatStart()
    {
        _openingHandled = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner) return;
        if (Owner.PlayerCombatState!.TurnNumber == 1)
        {
            if (_openingHandled) return;
            _openingHandled = true;
            Flash();
            await MercuriusOpeningHand.Replace(context, Owner);
        }
        else
        {
            Flash();
            await MercuriusTuneCommand.Tune(context, Owner);
        }
    }
}
