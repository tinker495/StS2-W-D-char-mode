using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public abstract class AltheaCard(AltheaCardKind kind) : CharacterTwoCard(
    AltheaCardDefinitions.All[kind].Cost, AltheaCardDefinitions.All[kind].Type,
    AltheaCardDefinitions.All[kind].Rarity, TargetFor(kind))
{
    public AltheaCardKind Kind { get; } = kind;
    internal AltheaCardDefinition Definition => AltheaCardDefinitions.All[Kind];
    internal bool ClashAtPlay { get; private set; }
    internal LocString ChoicePrompt => SelectionScreenPrompt;
    protected override bool HasEnergyCostX => Kind == AltheaCardKind.VesselOfDelusion;
    public override int MaxUpgradeLevel => Kind == AltheaCardKind.Wraith ? 0 : base.MaxUpgradeLevel;
    public override bool CanBeGeneratedInCombat => Definition.Rarity is not (CardRarity.Token or CardRarity.Ancient or CardRarity.Basic);
    public override bool CanBeGeneratedByModifiers => CanBeGeneratedInCombat;
    public override CardMultiplayerConstraint MultiplayerConstraint =>
        Kind is AltheaCardKind.SoulVeil or AltheaCardKind.CurseDominion or AltheaCardKind.FinalBlow
            ? CardMultiplayerConstraint.MultiplayerOnly : base.MultiplayerConstraint;
    protected override bool IsPlayable => Kind != AltheaCardKind.DoggedStrike || Owner.PlayerCombatState?.Hand.Cards
        .Any(card => card != this && AltheaRules.IsCurse(card)) == true;
    public override bool GainsBlock => Definition.Block > 0 || Kind is AltheaCardKind.Deflect or AltheaCardKind.SelfDiscipline;
    private CardModel Art => Kind == AltheaCardKind.Wraith ? ModelDb.Card<Soul>() : Definition.Type switch
    {
        CardType.Attack => ModelDb.Card<StrikeIronclad>(),
        CardType.Power => ModelDb.Card<Inflame>(),
        CardType.Curse => ModelDb.Card<Doubt>(),
        _ => ModelDb.Card<DefendIronclad>()
    };
    public override string PortraitPath => Art.PortraitPath;
    public override string CustomPortraitPath => Art.PortraitPath;
    public override string BetaPortraitPath => Art.BetaPortraitPath;
    protected override HashSet<CardTag> CanonicalTags => Kind.ToString().EndsWith("Strike", StringComparison.Ordinal)
        || Kind == AltheaCardKind.Strike ? [CardTag.Strike] : [];
    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            if (Definition.Flags.Contains('F')) yield return AltheaKeywords.Frenzy;
            if (Definition.Flags.Contains('U')) yield return AltheaKeywords.Unfading;
            if (Definition.Flags.Contains('X')) yield return CardKeyword.Exhaust;
            if (Definition.Flags.Contains('E')) yield return CardKeyword.Ethereal;
            if (Definition.Flags.Contains('T')) yield return CardKeyword.Retain;
            if (Definition.Flags.Contains('N')) yield return CardKeyword.Innate;
        }
    }
    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (Kind is AltheaCardKind.WarriorStrike or AltheaCardKind.RationalWill or AltheaCardKind.PathToGreatness
                or AltheaCardKind.ResoluteWill or AltheaCardKind.Liberation)
                yield return HoverTipFactory.FromKeyword(AltheaKeywords.Reform);
            if (Definition.Flags.Contains('C')) yield return HoverTipFactory.FromKeyword(AltheaKeywords.Clash);
        }
    }
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(Definition.Damage, ValueProp.Move), new BlockVar(Definition.Block, ValueProp.Move),
        Kind is AltheaCardKind.PureWill or AltheaCardKind.FightingSpirit or AltheaCardKind.FightersForm
            ? new EnergyVar("Amount", Definition.Amount) : new DynamicVar("Amount", Definition.Amount),
        new CardsVar(Definition.Cards),
        new RepeatVar(Definition.Repeat), new DynamicVar("Upgraded", 0)
    ];
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(Definition.DamageUpgrade);
        DynamicVars.Block.UpgradeValueBy(Definition.BlockUpgrade);
        DynamicVars["Amount"].UpgradeValueBy(Definition.AmountUpgrade);
        DynamicVars.Cards.UpgradeValueBy(Definition.CardsUpgrade);
        DynamicVars.Repeat.UpgradeValueBy(Definition.RepeatUpgrade);
        DynamicVars["Upgraded"].UpgradeValueBy(1);
        if (Definition.Flags.Contains('D')) EnergyCost.UpgradeBy(-1);
        if (Definition.Flags.Contains('I')) AddKeyword(CardKeyword.Innate);
    }
    protected override Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        ClashAtPlay = AltheaRules.Clash(Owner);
        return AltheaCardEffects.Play(this, context, play);
    }
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource) => cardSource == this && Kind == AltheaCardKind.DeathStrike
            && (Pile?.Type == PileType.Play ? ClashAtPlay : AltheaRules.Clash(Owner)) ? 2m : 1m;
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card != this) return false;
        int discount;
        if (Kind == AltheaCardKind.SeaParting && Owner.PlayerCombatState is { } state)
            discount = state.DiscardPile.Cards.Concat(state.ExhaustPile.Cards).Count(AltheaRules.IsCurse);
        else if (Kind == AltheaCardKind.FinalBlow)
            discount = CombatManager.Instance.History.CardPlaysStarted.Count(entry =>
                entry.CardPlay.Card.Type == CardType.Attack && entry.HappenedThisTurn(CombatState));
        else return false;
        modifiedCost = Math.Max(0, originalCost - discount);
        return modifiedCost != originalCost;
    }
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card != this || Pile?.Type != PileType.Deck || oldPileType == PileType.Deck) return;
        if (Kind == AltheaCardKind.HeavyStrike) await CardPileCmd.AddCurseToDeck<Doubt>(Owner);
        if (Kind == AltheaCardKind.KarmicMadness) await CardPileCmd.AddCurseToDeck<Shame>(Owner);
    }
    private static TargetType TargetFor(AltheaCardKind kind)
    {
        if (kind is AltheaCardKind.SoulVeil or AltheaCardKind.CurseDominion) return TargetType.AllAllies;
        var def = AltheaCardDefinitions.All[kind];
        if (def.Type != CardType.Attack) return TargetType.Self;
        return def.Flags.Contains('A') || def.Flags.Contains('R') ? TargetType.AllEnemies : TargetType.AnyEnemy;
    }
}
