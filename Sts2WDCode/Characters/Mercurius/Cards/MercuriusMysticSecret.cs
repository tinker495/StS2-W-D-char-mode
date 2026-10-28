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

public sealed class MercuriusMysticSecret() : MercuriusCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        var card = (await CardPileCmd.Draw(context, 1, Owner)).FirstOrDefault();
        if (card != null && card.Keywords.Contains(CardKeyword.Ethereal)) await CardCmd.AutoPlay(context, card, null);
    }
    protected override void OnUpgrade() { EnergyCost.UpgradeBy(-1); }
}
