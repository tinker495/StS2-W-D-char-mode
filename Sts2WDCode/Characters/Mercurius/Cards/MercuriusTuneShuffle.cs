using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusTuneShuffle : MercuriusTuneOption
{
    protected override CardModel PortraitSource => ModelDb.Card<Reboot>();
    public override Task Resolve(PlayerChoiceContext context) => CardPileCmd.Shuffle(context, Owner);
}
