using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using Sts2WD.Sts2WDCode.Relics;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

[Pool(typeof(AltheaRelicPool))]
public abstract class AltheaRelic : Sts2WDRelic
{
    public override string PackedIconPath => ModelDb.Relic<BurningBlood>().PackedIconPath;
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
}
