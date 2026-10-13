using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using Sts2WD.Sts2WDCode.Extensions;

namespace Sts2WD.Sts2WDCode.Relics;

// Shared artwork behavior. Inherit a character-specific relic base to assign its pool.
public abstract class Sts2WDRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}
