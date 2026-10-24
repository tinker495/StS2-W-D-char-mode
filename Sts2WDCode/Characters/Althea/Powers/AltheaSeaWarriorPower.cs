using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaSeaWarriorPower : AltheaPower
{
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player.Creature != Owner) return;
        var selected = await MegaCrit.Sts2.Core.Commands.CardSelectCmd.FromCombatPile(context,
            player.PlayerCombatState!.DiscardPile, player,
            new MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs(
                new MegaCrit.Sts2.Core.Localization.LocString("cards", "STS2WD-ALTHEA_SEA_WARRIOR.selectionScreenPrompt"), Amount));
        foreach (var card in selected.ToList()) await CardPileCmd.Add(card, PileType.Hand);
    }
}
