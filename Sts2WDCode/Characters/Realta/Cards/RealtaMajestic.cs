using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaMajestic() : RealtaCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaDamageImmunity>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaDamageImmunity>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaDamageImmunity>().BetaPortraitPath;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<VulnerablePower>(1m), new BlockVar(10m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<VulnerablePower>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<VulnerablePower>(choiceContext, Owner.Creature, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
        foreach (var ally in CombatState!.Players.Where(player => player != Owner && player.Creature.IsAlive).ToList())
            await CreatureCmd.GainBlock(ally.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
