using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Sts2WD.Sts2WDCode.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Inherit this class to add a card to this character's pool.
[Pool(typeof(RealtaCardPool))]
public abstract class RealtaCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : Sts2WDCard(cost, type, rarity, target);
