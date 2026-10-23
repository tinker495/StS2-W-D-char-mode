using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public enum AltheaPowerKind
{
    BattleInstinct, NextDouble, NextDrain, Disarm, FleetFoot, RestlessMadness, SharkStance, LionStance,
    OrcaStance, SoulHeat, CurseHarvest, ImmortalObsession, FightingSpirit, PathToGreatness, TemporaryStrength, TemporaryDexterity,
    Overcome, ReformEcho, WarriorsCreed, InnerReserve, UnrivaledBlade, SoulLegion, ArchipelagoArts, SeaWarrior, ErosionCurse
}
public sealed class AltheaBattleInstinctPower() : AltheaPower(AltheaPowerKind.BattleInstinct);
public sealed class AltheaNextDoublePower() : AltheaPower(AltheaPowerKind.NextDouble);
public sealed class AltheaNextDrainPower() : AltheaPower(AltheaPowerKind.NextDrain);
public sealed class AltheaDisarmPower() : AltheaPower(AltheaPowerKind.Disarm);
public sealed class AltheaFleetFootPower() : AltheaPower(AltheaPowerKind.FleetFoot);
public sealed class AltheaRestlessMadnessPower() : AltheaPower(AltheaPowerKind.RestlessMadness);
public sealed class AltheaSharkStancePower() : AltheaPower(AltheaPowerKind.SharkStance);
public sealed class AltheaLionStancePower() : AltheaPower(AltheaPowerKind.LionStance);
public sealed class AltheaOrcaStancePower() : AltheaPower(AltheaPowerKind.OrcaStance);
public sealed class AltheaSoulHeatPower() : AltheaPower(AltheaPowerKind.SoulHeat);
public sealed class AltheaCurseHarvestPower() : AltheaPower(AltheaPowerKind.CurseHarvest);
public sealed class AltheaImmortalObsessionPower() : AltheaPower(AltheaPowerKind.ImmortalObsession);
public sealed class AltheaFightingSpiritPower() : AltheaPower(AltheaPowerKind.FightingSpirit);
public sealed class AltheaPathToGreatnessPower() : AltheaPower(AltheaPowerKind.PathToGreatness);
public sealed class AltheaTemporaryStrengthPower() : AltheaPower(AltheaPowerKind.TemporaryStrength);
public sealed class AltheaTemporaryDexterityPower() : AltheaPower(AltheaPowerKind.TemporaryDexterity);
public sealed class AltheaOvercomePower() : AltheaPower(AltheaPowerKind.Overcome);
public sealed class AltheaReformEchoPower() : AltheaPower(AltheaPowerKind.ReformEcho);
public sealed class AltheaWarriorsCreedPower() : AltheaPower(AltheaPowerKind.WarriorsCreed);
public sealed class AltheaInnerReservePower() : AltheaPower(AltheaPowerKind.InnerReserve);
public sealed class AltheaUnrivaledBladePower() : AltheaPower(AltheaPowerKind.UnrivaledBlade);
public sealed class AltheaSoulLegionPower() : AltheaPower(AltheaPowerKind.SoulLegion);
public sealed class AltheaArchipelagoArtsPower() : AltheaPower(AltheaPowerKind.ArchipelagoArts);
public sealed class AltheaSeaWarriorPower() : AltheaPower(AltheaPowerKind.SeaWarrior);
public sealed class AltheaErosionCursePower() : AltheaPower(AltheaPowerKind.ErosionCurse);

public abstract class AltheaPower(AltheaPowerKind kind) : CustomPowerModel
{
    public AltheaPowerKind Kind { get; } = kind;
    private sealed class Data { internal CardModel? ActiveAttack; internal int Stacks; internal int AttackEnergy; }
    protected override object InitInternalData() => new Data();
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    // Native placeholders until dedicated power artwork is approved.
    public override string CustomPackedIconPath => ModelDb.Power<StrengthPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<StrengthPower>().ResolvedBigIconPath;

    public override Task BeforeCardPlayed(CardPlay play)
    {
        if (play.Card.Owner.Creature == Owner && play.Card.Type == CardType.Attack &&
            Kind is AltheaPowerKind.NextDouble or AltheaPowerKind.NextDrain)
        {
            var data = GetInternalData<Data>(); data.ActiveAttack = play.Card; data.Stacks = Amount;
        }
        return Task.CompletedTask;
    }
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? source)
    {
        if (Kind != AltheaPowerKind.NextDouble || dealer != Owner || source?.Type != CardType.Attack || !props.IsPoweredAttack()) return 1;
        var data = GetInternalData<Data>();
        if (data.ActiveAttack != null && data.ActiveAttack != source) return 1;
        decimal multiplier = 1;
        for (int i = 0; i < (data.ActiveAttack != null ? data.Stacks : Amount); i++) multiplier *= 2;
        return multiplier;
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay play)
    {
        if (play.Card.Owner.Creature != Owner || Owner.Player is not { } player) return;
        var data = GetInternalData<Data>();
        if (Kind is AltheaPowerKind.NextDouble or AltheaPowerKind.NextDrain && data.ActiveAttack == play.Card)
        {
            int consumed = data.Stacks; data.ActiveAttack = null;
            await PowerCmd.Apply(context, this, Owner, -consumed, Owner, null); return;
        }
        if (play.Card.Type != CardType.Attack) return;
        if (Kind == AltheaPowerKind.SoulLegion) play.Card.AddKeyword(AltheaKeywords.Frenzy);
        switch (Kind)
        {
            case AltheaPowerKind.BattleInstinct: await CardPileCmd.Draw(context, Amount, player); break;
            case AltheaPowerKind.SharkStance:
                if (AltheaRules.For(player).Streak % 3 == 0) await CardPileCmd.Draw(context, Amount, player);
                break;
            case AltheaPowerKind.LionStance:
                await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
                await PowerCmd.Apply<AltheaTemporaryStrengthPower>(context, Owner, Amount, Owner, null); break;
            case AltheaPowerKind.OrcaStance:
                await PowerCmd.Apply<DexterityPower>(context, Owner, Amount, Owner, null);
                await PowerCmd.Apply<AltheaTemporaryDexterityPower>(context, Owner, Amount, Owner, null); break;
        }
    }
    public override async Task AfterDamageGiven(PlayerChoiceContext context, Creature? dealer, DamageResult result,
        ValueProp props, Creature target, CardModel? source)
    {
        if (dealer != Owner || (!props.IsPoweredAttack() && source is not AltheaCard
            { Kind: AltheaCardKind.ScabbardSwing or AltheaCardKind.Rawenaid })) return;
        if (Kind == AltheaPowerKind.NextDrain && source == GetInternalData<Data>().ActiveAttack)
            await CreatureCmd.GainBlock(Owner, (result.TotalDamage + result.OverkillDamage) * GetInternalData<Data>().Stacks, ValueProp.Unpowered, null);
        if (Kind == AltheaPowerKind.Disarm)
            await PowerCmd.Apply<MonarchsGazeStrengthDownPower>(context, target, Amount, Owner, null);
    }
    public override async Task AfterCardDrawn(PlayerChoiceContext context, CardModel card, bool fromHandDraw)
    {
        if (card.Owner.Creature == Owner && Kind == AltheaPowerKind.CurseHarvest && AltheaRules.IsCurse(card))
            await CardPileCmd.Draw(context, Amount, card.Owner);
    }
    public override async Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        if (card.Owner.Creature != Owner || !AltheaRules.IsCurse(card)) return;
        if (Kind == AltheaPowerKind.SoulHeat) await PowerCmd.Apply<StrengthPower>(context, Owner, Amount, Owner, null);
        if (Kind == AltheaPowerKind.ImmortalObsession) await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (card.Owner.Creature == Owner && Kind == AltheaPowerKind.ImmortalObsession && AltheaRules.IsCurse(card))
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
    public override async Task AfterShuffle(PlayerChoiceContext context, Player shuffler)
    {
        if (Owner.Player == shuffler && Kind == AltheaPowerKind.FleetFoot) await PowerCmd.Remove(this);
        if (Owner.Player == shuffler && Kind == AltheaPowerKind.WarriorsCreed)
        {
            await PowerCmd.Apply<StrengthPower>(context, Owner, -Amount, Owner, null);
            await PowerCmd.Remove(this);
        }
    }
    public override async Task AfterEnergySpent(CardModel card, int amount)
    {
        if (Kind != AltheaPowerKind.ArchipelagoArts || card.Owner.Creature != Owner || card.Type != CardType.Attack) return;
        var data = GetInternalData<Data>(); data.AttackEnergy += amount;
        int refunds = data.AttackEnergy / 3; data.AttackEnergy %= 3;
        if (refunds > 0) await PlayerCmd.GainEnergy(refunds * Amount, card.Owner);
    }
    public override async Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount,
        Creature? applier, CardModel? source)
    {
        if (Kind != AltheaPowerKind.InnerReserve || power.Owner != Owner || amount == 0 || Owner.Player is not { } player) return;
        bool harmful = power.Type == PowerType.Debuff && amount > 0
            || applier?.Side == CombatSide.Enemy && power.Type != PowerType.Debuff;
        if (harmful) await PlayerCmd.GainEnergy(Amount, player);
    }
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount) =>
        Kind == AltheaPowerKind.ReformEcho && card.Owner.Creature == Owner
            && AltheaRules.For(card.Owner).ReformedThisTurn.Contains(card) ? playCount + Amount : playCount;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (Kind != AltheaPowerKind.SeaWarrior || player.Creature != Owner) return;
        var selected = await CardSelectCmd.FromCombatPile(context, player.PlayerCombatState!.DiscardPile, player,
            new CardSelectorPrefs(new LocString("cards", "STS2WD-ALTHEA_SEA_WARRIOR.selectionScreenPrompt"), Amount));
        foreach (var card in selected.ToList()) await CardPileCmd.Add(card, PileType.Hand);
    }
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (Kind == AltheaPowerKind.Overcome && participants.Contains(Owner) && Owner.Player is { } player
            && AltheaRules.HarmfulCount(player) > 0)
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }
    public override bool ShouldDie(Creature creature) => Kind != AltheaPowerKind.ErosionCurse || creature != Owner;
    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (Kind != AltheaPowerKind.ErosionCurse || creature != Owner) return;
        int strength = Amount;
        await PowerCmd.Remove(this);
        await CreatureCmd.Heal(Owner, Math.Max(1, Owner.MaxHp * 0.3m));
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, strength, Owner, null);
    }
    public override async Task AfterEnergyReset(Player player)
    {
        if (Owner.Player != player) return;
        if (Kind == AltheaPowerKind.FightingSpirit && AltheaRules.Clash(player)) await PlayerCmd.GainEnergy(Amount, player);
        if (Kind == AltheaPowerKind.FleetFoot)
        { int cards = Amount; await PowerCmd.Remove(this); await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), cards, player); }
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        if (Kind == AltheaPowerKind.TemporaryStrength) await PowerCmd.Apply<StrengthPower>(context, Owner, -Amount, Owner, null);
        if (Kind == AltheaPowerKind.TemporaryDexterity) await PowerCmd.Apply<DexterityPower>(context, Owner, -Amount, Owner, null);
        if (Kind is AltheaPowerKind.TemporaryStrength or AltheaPowerKind.TemporaryDexterity or AltheaPowerKind.Disarm
            or AltheaPowerKind.BattleInstinct or AltheaPowerKind.ReformEcho)
            await PowerCmd.Remove(this);
    }
    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Kind != AltheaPowerKind.RestlessMadness || Owner.Player?.PlayerCombatState is not { } combat) return;
        int curses = combat.Hand.Cards.Concat(combat.DrawPile.Cards).Concat(combat.DiscardPile.Cards).Count(AltheaRules.IsCurse);
        await CreatureCmd.Heal(Owner, curses * Amount);
    }
}
