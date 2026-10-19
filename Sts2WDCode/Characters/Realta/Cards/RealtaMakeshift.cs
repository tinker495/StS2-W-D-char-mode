using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaMakeshift() : RealtaCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaSmelting>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaSmelting>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaSmelting>().BetaPortraitPath;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Upgraded", 0m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardSelectorPrefs prefs = new(CardSelectorPrefs.ExhaustSelectionPrompt, 1);
        CardModel? selected = (await CardSelectCmd.FromHand(choiceContext, Owner, prefs, null, this)).FirstOrDefault();
        if (selected != null) await CardCmd.Exhaust(choiceContext, selected);

        var pool = ModelDb.CardPool<ColorlessCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint);
        await RealtaDiscoveryCmd.Choose(choiceContext, Owner, pool, IsUpgraded);
    }

    protected override void OnUpgrade() => DynamicVars["Upgraded"].UpgradeValueBy(1m);
}
