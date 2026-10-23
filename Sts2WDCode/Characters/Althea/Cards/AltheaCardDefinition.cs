using MegaCrit.Sts2.Core.Entities.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

internal sealed record AltheaCardDefinition(int Cost, CardType Type, CardRarity Rarity, int Damage, int DamageUpgrade, int Block, int BlockUpgrade, int Amount, int AmountUpgrade, int Cards, int CardsUpgrade, int Repeat, int RepeatUpgrade, string Flags);
