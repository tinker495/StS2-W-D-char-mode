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

public sealed class MercuriusEtchedMemory() : MercuriusCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    public override bool CanBeGeneratedInCombat => false;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromEnchantment<MercuriusEtching>().First(), HoverTipFactory.FromPower<MercuriusEtchedMemoryPower>()];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        await PowerCmd.Apply<MercuriusEtchedMemoryPower>(context, Owner.Creature, 1, Owner.Creature, this);
    }
    protected override void OnUpgrade() { EnergyCost.UpgradeBy(-1); }
}
