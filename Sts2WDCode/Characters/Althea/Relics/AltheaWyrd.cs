using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaWyrd : AltheaRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords) =>
        card.Owner == Owner && card.Tags.Contains(CardTag.Strike) && keywords.Add(CardKeyword.Retain);
}
