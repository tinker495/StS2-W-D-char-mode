using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusSongOfSouls() : MercuriusSongCard(CardRarity.Uncommon)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];

    protected override async Task WhenDrawn(PlayerChoiceContext context)
    {
        await CardPileCmd.Draw(context, DynamicVars.Cards.BaseValue, Owner);
    }
    protected override void OnUpgrade() { DynamicVars.Cards.UpgradeValueBy(1m); }
}
