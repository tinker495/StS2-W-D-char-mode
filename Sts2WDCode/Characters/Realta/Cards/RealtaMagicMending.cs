using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaMagicMending() : RealtaCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<Steady>();
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prefs = new MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs(SelectionScreenPrompt, 1);
        var card = (await CardSelectCmd.FromCombatPile(choiceContext, Owner.PlayerCombatState!.DiscardPile, Owner, prefs)).FirstOrDefault();
        if (card == null) return;
        await CardPileCmd.Add(card, PileType.Hand);
        await RefineCmd.Upgrade(choiceContext, card);
        Steady steady = ModelDb.Enchantment<Steady>();
        if (!card.Keywords.Contains(CardKeyword.Retain) && steady.CanEnchant(card))
            RealtaEnchantments.EnchantTemporarily(card, steady, 1);
    }
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
