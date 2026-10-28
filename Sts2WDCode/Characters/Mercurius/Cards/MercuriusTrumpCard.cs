using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusTrumpCard() : MercuriusCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var keep = (await CardSelectCmd.FromHand(context, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), null, this)).FirstOrDefault();
        if (keep == null) return;
        var others = Owner.PlayerCombatState!.Hand.Cards.Where(card => card != keep).ToList();
        await CardPileCmd.Add(others, PileType.Draw);
        await MercuriusPileCommands.ShuffleDraw(context, Owner);
    }
    protected override void OnUpgrade() { EnergyCost.UpgradeBy(-1); }
}
