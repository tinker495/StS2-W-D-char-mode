using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusBondOfProtection : MercuriusRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(12m, ValueProp.Unpowered)];
    private bool _used;

    public override Task BeforeCombatStart()
    {
        _used = false;
        Status = RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override async Task AfterShuffle(PlayerChoiceContext context, Player shuffler)
    {
        // Native initial deck randomization does not emit AfterShuffle.
        if (shuffler != Owner || _used) return;
        _used = true;
        Status = RelicStatus.Disabled;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, null);
    }
}
