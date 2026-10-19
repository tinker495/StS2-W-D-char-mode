using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaMasterpiece() : RealtaCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaAnnealing>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaAnnealing>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaAnnealing>().BetaPortraitPath;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Refine", 9m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(RealtaKeywords.Refine)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(SelectionScreenPrompt, 1);
        CardModel? selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).FirstOrDefault();
        if (selected == null) return;

        var discarded = Owner.PlayerCombatState!.Hand.Cards.Where(card => card != selected).ToList();
        await CardCmd.Discard(choiceContext, discarded);
        await RefineCmd.Refine(choiceContext, selected, DynamicVars["Refine"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Refine"].UpgradeValueBy(3m);
}
