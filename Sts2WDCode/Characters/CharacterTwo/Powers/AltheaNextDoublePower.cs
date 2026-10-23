using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaNextDoublePower : AltheaPower
{
    private sealed class Data { internal CardModel? ActiveAttack; internal int Stacks; }
    protected override object InitInternalData() => new Data();

    public override Task BeforeCardPlayed(CardPlay play)
    {
        if (play.Card.Owner.Creature == Owner && play.Card.Type == CardType.Attack)
        {
            var data = GetInternalData<Data>();
            data.ActiveAttack = play.Card;
            data.Stacks = Amount;
        }
        return Task.CompletedTask;
    }
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? source)
    {
        if (dealer != Owner || source?.Type != CardType.Attack || !props.IsPoweredAttack()) return 1;
        var data = GetInternalData<Data>();
        if (data.ActiveAttack != null && data.ActiveAttack != source) return 1;
        decimal multiplier = 1;
        for (int i = 0; i < (data.ActiveAttack != null ? data.Stacks : Amount); i++) multiplier *= 2;
        return multiplier;
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner.Creature != Owner) return;
        var data = GetInternalData<Data>();
        if (data.ActiveAttack != play.Card) return;
        int consumed = data.Stacks;
        data.ActiveAttack = null;
        await PowerCmd.Apply(context, this, Owner, -consumed, Owner, null);
    }
}
