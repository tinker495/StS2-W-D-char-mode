using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public abstract class AltheaPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // Native placeholders until dedicated power artwork is approved.
    public override string CustomPackedIconPath => ModelDb.Power<StrengthPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<StrengthPower>().ResolvedBigIconPath;
}
