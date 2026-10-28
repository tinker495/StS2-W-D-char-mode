using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusPerfectTimbrePower : MercuriusPower
{
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player != Owner.Player) return;
        for (int i = 0; i < Amount; i++)
        {
            await CardPileCmd.Draw(context, 1, player);
            var selected = (await CardSelectCmd.FromHand(context, player,
                new MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs(new MegaCrit.Sts2.Core.Localization.LocString("cards", "STS2WD-MERCURIUS_PERFECT_TIMBRE.selectionScreenPrompt"), 1), null, this)).FirstOrDefault();
            if (selected != null) await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Bottom);
        }
    }
}
