using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public sealed class AltheaNextDrainPower : AltheaPower
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
    public override async Task AfterDamageGiven(PlayerChoiceContext context, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? source)
    {
        if (dealer != Owner || (!props.IsPoweredAttack() && source is not AltheaCard
            { Kind: AltheaCardKind.ScabbardSwing or AltheaCardKind.Rawenaid })) return;
        var data = GetInternalData<Data>();
        if (source == data.ActiveAttack)
            await CreatureCmd.GainBlock(Owner, (result.TotalDamage + result.OverkillDamage) * data.Stacks, ValueProp.Unpowered, null);
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
