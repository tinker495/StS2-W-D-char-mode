using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaWitchPointedHat : RealtaRelic
{
    private bool _triggeredThisCombat;

    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Spell), HoverTipFactory.ForEnergy(this)];

    protected override void AfterCloned()
    {
        base.AfterCloned();
        _triggeredThisCombat = false;
    }

    public override Task BeforeCombatStart()
    {
        _triggeredThisCombat = false;
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (_triggeredThisCombat || cardPlay.Card.Owner != Owner || cardPlay.Card is not RealtaSpellCard { SpellActive: true }) return;
        _triggeredThisCombat = true;
        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _triggeredThisCombat = false;
        return Task.CompletedTask;
    }
}
