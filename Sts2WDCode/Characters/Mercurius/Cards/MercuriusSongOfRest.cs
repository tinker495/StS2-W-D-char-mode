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

public sealed class MercuriusSongOfRest() : MercuriusCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<RegenPower>(3m)];
    public override bool CanBeGeneratedInCombat => false;
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RegenPower>()];

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay cardPlay)
    {
        foreach (var player in Owner.RunState.Players.Where(player => !player.Creature.IsDead))
            await PowerCmd.Apply<RegenPower>(context, player.Creature, DynamicVars["RegenPower"].BaseValue, Owner.Creature, this);
    }
    protected override void OnUpgrade() { DynamicVars["RegenPower"].UpgradeValueBy(1m); }
}
