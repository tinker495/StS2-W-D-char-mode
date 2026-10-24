using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaBloodFrenzy : AltheaRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords) =>
        card.Owner == Owner && card.Type == CardType.Attack && keywords.Add(CardKeyword.Retain);
}
