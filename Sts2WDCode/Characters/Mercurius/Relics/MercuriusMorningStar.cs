using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusMorningStar : MercuriusRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
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
        var pile = Owner.PlayerCombatState.DrawPile;
        if (pile.Cards.Count > 0)
        {
            var selected = (await CardSelectCmd.FromCombatPile(context, pile, Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
            if (selected != null) await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
        }
        await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
    }
}
