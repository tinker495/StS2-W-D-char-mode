using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaAncestorsRecognition : AltheaRelic
{
    private bool _applied;
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override Task BeforeCombatStart() { _applied = false; return Task.CompletedTask; }
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext context, ICombatState combat)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await CardPileCmd.Add(player.PlayerCombatState!.DrawPile.Cards.Where(AltheaRules.IsCurse).ToList(), PileType.Discard);
    }
}
