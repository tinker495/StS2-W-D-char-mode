using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;

using Sts2WD.Sts2WDCode.Extensions;

namespace Sts2WD.Sts2WDCode.Potions;


public abstract class Sts2WDPotion : CustomPotionModel
{
	public override string? CustomPackedImagePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
	public override string? CustomPackedOutlinePath =>
		$"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}
