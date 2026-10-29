using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Sts2WD.Sts2WDCode.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

// Inherit this class to add a card to this character's pool.
[Pool(typeof(MercuriusCardPool))]
public abstract class MercuriusCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : Sts2WDCard(cost, type, rarity, target)
{
    // Native placeholders until dedicated card portraits are approved.
    public override string PortraitPath => Type == CardType.Attack
        ? ModelDb.Card<StrikeIronclad>().PortraitPath : ModelDb.Card<DefendIronclad>().PortraitPath;
    public override string CustomPortraitPath => PortraitPath;
    public override string BetaPortraitPath => PortraitPath;
}
