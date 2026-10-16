using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// 개선 N: choose a card in hand; attacks get temporary Sharp N, everything else temporary Adroit N.
// Eligibility is the shared enchantment rule (RealtaEnchantments): an already enchanted normal card cannot be
// chosen, while 유산 can be refined repeatedly. With no valid target only the refine fizzles.
public static class RefineCmd
{
    public static async Task Refine(PlayerChoiceContext choiceContext, Player player, int amount, AbstractModel source)
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.EnchantSelectionPrompt, 1);
        CardModel? card = (await CardSelectCmd.FromHand(choiceContext, player, prefs, CanRefine, source)).FirstOrDefault();
        if (card == null) return;

        RealtaEnchantments.EnchantTemporarily(card, EnchantmentFor(card), amount);
    }

    private static EnchantmentModel EnchantmentFor(CardModel card) =>
        card.Type == CardType.Attack ? ModelDb.Enchantment<Sharp>() : ModelDb.Enchantment<Adroit>();

    private static bool CanRefine(CardModel card) => EnchantmentFor(card).CanEnchant(card);
}
