using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusStoryCollectionPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<CreativeAiPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<CreativeAiPower>().ResolvedBigIconPath;
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(MercuriusKeywords.Reenact), HoverTipFactory.FromKeyword(CardKeyword.Ethereal)];

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.Player is not { } player || Amount <= 0) return;

        // This hook runs before combat history is cleared and powers are removed.
        // Reading the complete history includes cards played before this power was applied.
        var candidates = CombatManager.Instance.History.CardPlaysStarted
            .Where(entry => entry.CardPlay.Card.Owner == player && !entry.CardPlay.IsAutoPlay)
            .Select(entry => entry.CardPlay.Card.CanonicalInstance)
            .Where(card => MercuriusCardEffects.IsOffColor(card, player) && MercuriusReenactCommand.IsEligible(card))
            .Distinct().ToList();
        if (candidates.Count == 0) return;

        var selections = Amount;
        for (var stack = 0; stack < selections; stack++)
        {
            var choices = candidates.TakeRandom(3, player.RunState.Rng.CombatCardGeneration)
                .Select(canonical =>
                {
                    var card = player.RunState.CreateCard(canonical, player);
                    card.AddKeyword(CardKeyword.Ethereal);
                    return card;
                }).ToList();
            // Choose directly so unrelated card-reward rerolls cannot replace these
            // audited, unupgraded Ethereal options with ordinary reward cards.
            var selected = await CardSelectCmd.FromChooseACardScreen(
                new BlockingPlayerChoiceContext(), choices, player, canSkip: true);
            if (selected != null)
                CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(selected, PileType.Deck));
        }
    }
}
