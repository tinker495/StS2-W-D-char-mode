using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaPortableForge() : RealtaCard(2, CardType.Power, CardRarity.Uncommon, TargetType.AllAllies)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaAgdridsAnvil>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaAgdridsAnvil>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaAgdridsAnvil>().BetaPortraitPath;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 5m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var ally in CombatState!.Players.Where(player => player.Creature.IsAlive).ToList())
            await PowerCmd.Apply<RealtaPortableForgePower>(choiceContext, ally.Creature, DynamicVars["Refine"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Refine"].UpgradeValueBy(2m);
}
