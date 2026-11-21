using BaseLib.Abstracts;
using BaseLib.Extensions;
using Sts2WD.Sts2WDCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Sts2WD.Sts2WDCode.Powers;

public abstract class Sts2WDPower : CustomPowerModel
{
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();

    public abstract override PowerType Type { get; }

    public abstract override PowerStackType StackType { get; }
}
