
namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaUnrivaledBladePower : AltheaPower
{
    // The compatibility patch supplies the native Weak multiplier; this power owns its reversal.
    internal decimal ReverseWeakMultiplier(decimal multiplier) => 2m - multiplier;
}
