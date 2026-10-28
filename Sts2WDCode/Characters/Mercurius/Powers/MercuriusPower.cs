using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public abstract class MercuriusPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<CreativeAiPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<CreativeAiPower>().ResolvedBigIconPath;
}
