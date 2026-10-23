using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2WD.Sts2WDCode.Relics;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

[Pool(typeof(AltheaRelicPool))]
public abstract class AltheaRelic : Sts2WDRelic
{
    public override string PackedIconPath => ModelDb.Relic<BurningBlood>().PackedIconPath;
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
}
public sealed class AltheaWyrd : AltheaRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords) =>
        card.Owner == Owner && card.Tags.Contains(CardTag.Strike) && keywords.Add(CardKeyword.Retain);
}
public sealed class AltheaBloodFrenzy : AltheaRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords) =>
        card.Owner == Owner && card.Type == CardType.Attack && keywords.Add(CardKeyword.Retain);
}
public sealed class AltheaFightersBracer : AltheaRelic
{
    private bool _disabled;
    public override RelicRarity Rarity => RelicRarity.Common;
    public override Task BeforeCombatStart() { _disabled = false; return Task.CompletedTask; }
    public override Task AfterShuffle(PlayerChoiceContext context, Player player)
    {
        if (player == Owner && player.PlayerCombatState!.TurnNumber > 0) _disabled = true;
        return Task.CompletedTask;
    }
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner.Creature) && !_disabled)
            await CreatureCmd.GainBlock(Owner.Creature, 6, ValueProp.Unpowered, null);
    }
}
public sealed class AltheaCrekinMacDubh : AltheaRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
}
public sealed class AltheaAncestorsRecognition : AltheaRelic
{
    private bool _applied;
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override Task BeforeCombatStart() { _applied = false; return Task.CompletedTask; }
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext context, ICombatState combat)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await CardPileCmd.Add(player.PlayerCombatState!.DrawPile.Cards.Where(AltheaRules.IsCurse).ToList(), PileType.Discard);
    }
}
