using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public sealed class AltheaArchipelagoArtsPower : AltheaPower
{
    private sealed class Data { internal int AttackEnergy; }
    protected override object InitInternalData() => new Data();

    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (card.Owner.Creature != Owner || (card.Type != CardType.Attack && !AltheaRules.For(card.Owner).Reformed.Contains(card))) return;
        var data = GetInternalData<Data>();
        data.AttackEnergy += amount;
        int refunds = data.AttackEnergy / 3;
        data.AttackEnergy %= 3;
        if (refunds > 0) await PlayerCmd.GainEnergy(refunds * Amount, card.Owner);
    }
}
