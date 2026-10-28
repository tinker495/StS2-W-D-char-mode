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

public sealed class MercuriusSongOfBewilderment() : MercuriusSongCard(CardRarity.Uncommon)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<VulnerablePower>(1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task WhenDrawn(PlayerChoiceContext context)
    {
        foreach (var enemy in CombatState!.HittableEnemies.ToList())
            await PowerCmd.Apply<VulnerablePower>(context, enemy, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() { DynamicVars["VulnerablePower"].UpgradeValueBy(1m); }
}
