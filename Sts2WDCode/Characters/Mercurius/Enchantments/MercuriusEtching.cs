using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusEtching : CustomEnchantmentModel
{
    protected override string? CustomIconPath => ModelDb.Enchantment<PerfectFit>().IconPath;
    public override bool HasExtraCardText => true;
    public override bool ShouldStartAtBottomOfDrawPile => true;
    public override void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
    {
        if (player == Card.Owner && cards.Remove(Card)) cards.Add(Card);
    }
}
