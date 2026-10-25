using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using Sts2WD.Sts2WDCode.Characters.Althea;
using Sts2WD.Sts2WDCode.Relics;

namespace Sts2WD.Sts2WDCode.Ancients.Aselige;

[Pool(typeof(EventRelicPool))]
public abstract class AseligeRelic : Sts2WDRelic
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
}

public sealed class AseligeRegenerationGrace : AseligeRelic
{
    internal float RestChance => Owner.Creature.CurrentHp >= Owner.Creature.MaxHp ? 0 :
        Owner.Creature.CurrentHp * 2 <= Owner.Creature.MaxHp ? 1 : 0.5f;
}

public sealed class AseligeDeepBreath : AseligeRelic
{
    private bool _spent;
    [SavedProperty] public bool Spent
    {
        get => _spent;
        set { _spent = value; Status = value ? RelicStatus.Disabled : RelicStatus.Normal; }
    }
    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner.Creature || delta >= 0 || Spent || creature.CurrentHp <= 0) return;
        Spent = true;
        Status = RelicStatus.Disabled;
        Flash();
        await CreatureCmd.Heal(creature, 10);
    }
    public override Task AfterRestSiteHeal(Player player, bool isMimicked)
    {
        if (player == Owner && !isMimicked) { Spent = false; Status = RelicStatus.Normal; }
        return Task.CompletedTask;
    }
}

public sealed class AseligeHarmoniousBlessing : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPotion<FairyInABottle>()];
    public override bool HasUponPickupEffect => true;
    public override async Task AfterObtained() => await RewardsCmd.OfferCustom(Owner,
        [new PotionReward(ModelDb.Potion<FairyInABottle>().ToMutable(), Owner)]);
    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (potion.Owner != Owner) return;
        Flash();
        await CreatureCmd.GainMaxHp(Owner.Creature, 2);
    }
}

public sealed class AseligeLifeSource : AseligeRelic
{
    public override bool HasUponPickupEffect => true;
    public override async Task AfterObtained() => await RunManager.Instance.GenerateMap();
    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex) =>
        map is SavedActMap or AseligeLifeMap ? map : new AseligeLifeMap(map);
}

public sealed class AseligeEmbracingFlute : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<AseligeVerdantDragonAdvent>();
    public override bool HasUponPickupEffect => true;
    public override async Task AfterObtained() =>
        await CardPileCmd.Add(Owner.RunState.CreateCard<AseligeVerdantDragonAdvent>(Owner), PileType.Deck);
}

public sealed class AseligeShiningFilm : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Sown>();
    public override bool HasUponPickupEffect => true;
    public override Task AfterObtained()
    {
        foreach (var card in PileType.Deck.GetPile(Owner).Cards
                     .Where(card => ModelDb.Enchantment<Sown>().CanEnchant(card))
                     .ToList().StableShuffle(Owner.RunState.Rng.Niche).Take(2))
            CardCmd.Enchant<Sown>(card, 1);
        return Task.CompletedTask;
    }
}

public sealed class AseligePurifyingBreath : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
    private bool _used;
    public override Task BeforeCombatStart() { _used = false; Status = RelicStatus.Normal; return Task.CompletedTask; }
    public override async Task AfterShuffle(PlayerChoiceContext context, Player shuffler)
    {
        // Native initial deck randomization calls ModifyShuffleOrder only;
        // AfterShuffle is emitted by the actual discard-to-draw shuffle command.
        if (shuffler != Owner || _used) return;
        _used = true;
        Status = RelicStatus.Disabled;
        Flash();
        var cards = Owner.PlayerCombatState!.DrawPile.Cards
            .Where(card => card.Type == CardType.Status || AltheaRules.IsCurse(card)).ToArray();
        foreach (var card in cards) await CardCmd.Exhaust(context, card);
        await PlayerCmd.GainEnergy(1, Owner);
    }
}

public sealed class AseligeDeepBlueScales : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Steady>();
    private int _uses;
    [SavedProperty] public int Uses
    {
        get => _uses;
        set { _uses = value; Status = value >= 3 ? RelicStatus.Disabled : RelicStatus.Normal; }
    }
    public override bool ShowCounter => true;
    public override int DisplayAmount => Math.Max(0, 3 - Uses);
    internal void AfterUpgrade(CardModel card)
    {
        if (card.Owner != Owner || Uses >= 3 || !ModelDb.Enchantment<Steady>().CanEnchant(card)) return;
        CardCmd.Enchant<Steady>(card, 1);
        Uses++;
        InvokeDisplayAmountChanged();
        if (Uses == 3) Status = RelicStatus.Disabled;
        Flash();
    }
}

public sealed class AseligeCleanPebble : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords) =>
        card.Owner == Owner && (card.Type == CardType.Status || AltheaRules.IsCurse(card)) && keywords.Add(CardKeyword.Ethereal);
}

public sealed class AseligeSmallSprout : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<AseligeShiningLeaf>();
    public override bool HasUponPickupEffect => true;
    public override async Task AfterObtained() =>
        await CardPileCmd.Add(Owner.RunState.CreateCard<AseligeShiningLeaf>(Owner), PileType.Deck);
    public override Task AfterRestSiteHeal(Player player, bool isMimicked)
    {
        if (player == Owner && !isMimicked)
            foreach (var card in PileType.Deck.GetPile(Owner).Cards.OfType<AseligeShiningLeaf>().Where(c => c.IsUpgradable))
                CardCmd.Upgrade(card);
        return Task.CompletedTask;
    }
}

public sealed class AseligeLargeSapling : AseligeRelic
{
    [SavedProperty] public int CardsAdded { get; set; }
    public override bool ShowCounter => true;
    public override int DisplayAmount => CardsAdded;
    public override Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Owner != Owner || card.Pile?.Type != PileType.Deck || oldPileType == PileType.Deck) return Task.CompletedTask;
        CardsAdded = (CardsAdded + 1) % 3;
        InvokeDisplayAmountChanged();
        if (CardsAdded == 0)
        {
            var target = PileType.Deck.GetPile(Owner).Cards.Where(c => c.IsUpgradable)
                .ToList().StableShuffle(Owner.RunState.Rng.Niche).FirstOrDefault();
            if (target != null) { Flash(); CardCmd.Upgrade(target); }
        }
        return Task.CompletedTask;
    }
}

public sealed class AseligePrettyStarfish : AseligeRelic
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Glam>();
    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> options, CardCreationOptions creationOptions)
    {
        if (player != Owner || creationOptions.Source != CardCreationSource.Encounter || Owner.RunState.CurrentRoom?.RoomType != RoomType.Elite)
            return false;
        var changed = false;
        foreach (var result in options.Where(result => result.Card.Rarity == CardRarity.Common && ModelDb.Enchantment<Glam>().CanEnchant(result.Card)))
        {
            var card = Owner.RunState.CloneCard(result.Card);
            CardCmd.Enchant<Glam>(card, 1);
            result.ModifyCard(card, this);
            changed = true;
        }
        return changed;
    }
}

public sealed class AseligeNeatlyArrangedSalt : AseligeRelic
{
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card.Owner != Owner || card.Pile?.Type != PileType.Deck || oldPileType == PileType.Deck) return;
        Flash();
        await CreatureCmd.GainMaxHp(Owner.Creature, 1);
    }
    internal async Task AfterRelicObtained()
    {
        Flash();
        await CreatureCmd.GainMaxHp(Owner.Creature, 1);
    }
}

public sealed class AseligeGrowingBlessing : AseligeRelic
{
    [SavedProperty] public int Battles { get; set; }
    [SavedProperty] public bool RewardPending { get; set; }
    public override bool ShowCounter => true;
    public override int DisplayAmount => Battles;
    public override Task AfterCombatVictory(CombatRoom room)
    {
        if (room.RoomType == RoomType.Monster)
        {
            Battles = (Battles + 1) % 2;
            RewardPending = Battles == 0;
            InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != Owner || !RewardPending || room?.RoomType != RoomType.Monster) return false;
        RewardPending = false;
        Flash();
        rewards.Add(new CardReward(CardCreationOptions.ForRoom(Owner, RoomType.Monster), 3, Owner));
        return true;
    }
}
