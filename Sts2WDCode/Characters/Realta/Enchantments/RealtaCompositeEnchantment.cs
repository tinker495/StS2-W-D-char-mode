using System.Text;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Occupies a card's single enchantment slot when the card holds more than one enchantment entry, or any
// temporary one (docs/adr/0003). Every base-game read of card.Enchantment sees this model, which delegates to
// the real base-game enchantments it holds. Only RealtaEnchantments creates or changes it.
public sealed class RealtaCompositeEnchantment : CustomEnchantmentModel
{
    // The base game wraps enchantment card text in [purple]; temporary entries must use a different color.
    private const string PermanentColor = "purple";
    private const string TemporaryColor = "aqua";

    private sealed class Entry(EnchantmentModel model, bool temporary, bool pendingModify)
    {
        public EnchantmentModel Model { get; } = model;
        public bool Temporary { get; } = temporary;
        public bool PendingModify { get; set; } = pendingModify;
    }

    private List<Entry> _entries = [];

    // Permanent entries as "CATEGORY.ENTRY=amount;...". Temporary entries only exist on combat copies.
    [SavedProperty]
    public string PermanentEntries
    {
        get => string.Join(';', _entries.Where(e => !e.Temporary).Select(e => $"{e.Model.Id}={e.Model.Amount}"));
        set
        {
            AssertMutable();
            _entries = value.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(ParseEntry).ToList();
        }
    }

    public IEnumerable<EnchantmentModel> Children => AttachedEntries.Select(e => e.Model);

    // Entries are attached to the card lazily, because the composite only learns its card after the slot is set.
    private List<Entry> AttachedEntries
    {
        get
        {
            if (HasCard)
            {
                foreach (Entry entry in _entries.Where(e => !e.Model.HasCard))
                {
                    entry.Model.ApplyInternal(Card, entry.Model.Amount);
                    entry.Model.StatusChanged += RefreshStatus;
                }
            }
            return _entries;
        }
    }

    protected override string? CustomIconPath => _entries.FirstOrDefault()?.Model.IconPath;

    public override bool ShowAmount => _entries.Count == 1 && _entries[0].Model.ShowAmount;
    public override int DisplayAmount => _entries.Count == 1 ? _entries[0].Model.DisplayAmount : 0;
    public override bool ShouldStartAtBottomOfDrawPile => Children.Any(c => c.ShouldStartAtBottomOfDrawPile);
    public override bool ShouldGlowGold => Children.Any(c => c.ShouldGlowGold);
    public override bool ShouldGlowRed => Children.Any(c => c.ShouldGlowRed);
    protected override IEnumerable<IHoverTip> ExtraHoverTips => Children.SelectMany(c => c.HoverTips);

    // Same enchantment + same scope stacks into one entry; anything else becomes a new entry.
    public EnchantmentModel Add(EnchantmentModel enchantment, int amount, bool temporary, bool modifyCard = true)
    {
        Entry? same = _entries.FirstOrDefault(e => e.Temporary == temporary && e.Model.GetType() == enchantment.GetType());
        if (same != null)
        {
            same.Model.Amount += amount;
            if (same.Model.HasCard) same.Model.RecalculateValues();
            return same.Model;
        }

        enchantment.Amount = amount;
        _entries.Add(new Entry(enchantment, temporary, pendingModify: modifyCard));
        ModifyPendingEntries();
        return enchantment;
    }

    // Takes over the card's previous slot enchantment; its effects are already applied to the card.
    public void Adopt(EnchantmentModel enchantment)
    {
        _entries.Add(new Entry(enchantment, temporary: false, pendingModify: false));
    }

    // Some base-game effects copy a whole slot enchantment onto another card (Claws' Maul). The composite itself never
    // moves: RealtaEnchantments re-applies these copies one by one under the target card's own rules, so a normal
    // card keeps at most one of them. Temporary entries stay with their combat copy.
    public IEnumerable<EnchantmentModel> DetachedPermanentCopies()
    {
        return _entries.Where(e => !e.Temporary).Select(e => (EnchantmentModel)e.Model.ClonePreservingMutability()).ToList();
    }

    public override bool CanEnchant(CardModel card)
    {
        return _entries.Where(e => !e.Temporary).Any(e => e.Model.CanEnchant(card));
    }

    // Runs OnEnchant for entries added since the composite was put in the slot.
    public void ModifyPendingEntries()
    {
        if (!HasCard) return;
        foreach (Entry entry in AttachedEntries.Where(e => e.PendingModify))
        {
            entry.PendingModify = false;
            entry.Model.ModifyCard();
        }
    }

    public string EntriesText()
    {
        StringBuilder text = new();
        foreach (Entry entry in AttachedEntries)
        {
            string color = entry.Temporary ? TemporaryColor : PermanentColor;
            if (!entry.Temporary)
            {
                LocString line = new("enchantments", $"{Id.Entry}.permanentEntry");
                line.Add("Title", entry.Model.Title);
                line.Add("AmountText", entry.Model.ShowAmount ? $" {entry.Model.DisplayAmount}" : "");
                text.Append($"\n[{color}]{line.GetFormattedText()}[/{color}]");
            }
            if (entry.Model.DynamicExtraCardText is { } extraText)
                text.Append($"\n[{color}]{extraText.GetFormattedText()}[/{color}]");
        }
        return text.ToString();
    }

    internal bool TemporarilyModifies(DynamicVar variable) => AttachedEntries.Any(entry => entry.Temporary &&
        (variable switch
        {
            DamageVar damage => entry.Model.EnchantDamageAdditive(variable.BaseValue, damage.Props) != 0m ||
                                entry.Model.EnchantDamageMultiplicative(variable.BaseValue, damage.Props) != 1m,
            CalculatedDamageVar damage => entry.Model.EnchantDamageAdditive(variable.BaseValue, damage.Props) != 0m ||
                                          entry.Model.EnchantDamageMultiplicative(variable.BaseValue, damage.Props) != 1m,
            BlockVar => entry.Model.EnchantBlockAdditive(variable.BaseValue) != 0m ||
                        entry.Model.EnchantBlockMultiplicative(variable.BaseValue) != 1m,
            _ => false
        }));

    // Called on first enchant, after loading and after a downgrade reset the card: re-apply every entry.
    protected override void OnEnchant()
    {
        foreach (Entry entry in AttachedEntries)
        {
            entry.PendingModify = false;
            entry.Model.ModifyCard();
        }
    }

    // NCard greys out the slot enchantment when it is disabled; do so once every entry is disabled.
    private void RefreshStatus()
    {
        Status = _entries.Count > 0 && _entries.All(e => e.Model.Status == EnchantmentStatus.Disabled)
            ? EnchantmentStatus.Disabled
            : EnchantmentStatus.Normal;
    }

    public override void RecalculateValues()
    {
        foreach (EnchantmentModel child in Children) child.RecalculateValues();
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        foreach (EnchantmentModel child in Children.ToList())
            await child.OnPlay(choiceContext, cardPlay);
    }

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        decimal damage = originalDamage;
        foreach (EnchantmentModel child in Children) damage += child.EnchantDamageAdditive(damage, props);
        return damage - originalDamage;
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        decimal multiplier = 1m;
        foreach (EnchantmentModel child in Children) multiplier *= child.EnchantDamageMultiplicative(originalDamage * multiplier, props);
        return multiplier;
    }

    public override decimal EnchantBlockAdditive(decimal originalBlock)
    {
        decimal block = originalBlock;
        foreach (EnchantmentModel child in Children) block += child.EnchantBlockAdditive(block);
        return block - originalBlock;
    }

    public override decimal EnchantBlockMultiplicative(decimal originalBlock)
    {
        decimal multiplier = 1m;
        foreach (EnchantmentModel child in Children) multiplier *= child.EnchantBlockMultiplicative(originalBlock * multiplier);
        return multiplier;
    }

    public override int EnchantPlayCount(int originalPlayCount)
    {
        return Children.Aggregate(originalPlayCount, (count, child) => child.EnchantPlayCount(count));
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _entries = _entries
            .Select(e => new Entry((EnchantmentModel)e.Model.ClonePreservingMutability(), e.Temporary, e.PendingModify))
            .ToList();
    }

    private static Entry ParseEntry(string saved)
    {
        string[] parts = saved.Split('=');
        EnchantmentModel model = FromSerializable(new SerializableEnchantment
        {
            Id = ModelId.Deserialize(parts[0]),
            Amount = int.Parse(parts[1])
        });
        return new Entry(model, temporary: false, pendingModify: false);
    }
}
