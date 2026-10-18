using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public sealed class RealtaImprovisedWeapon() : RealtaCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{

    // Reuse existing placeholder artwork until this card receives its own asset.
    public override string PortraitPath => ModelDb.Card<RealtaMagicWeapon>().PortraitPath;
    public override string CustomPortraitPath => ModelDb.Card<RealtaMagicWeapon>().CustomPortraitPath;
    public override string BetaPortraitPath => ModelDb.Card<RealtaMagicWeapon>().BetaPortraitPath;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move), new DynamicVar("Upgraded", 0m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Refine), .. HoverTipFactory.FromEnchantment<Adroit>(RefineCmd.GetAmount(this))];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target).Execute(choiceContext);
        var skills = ModelDb.CardPool<RealtaCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(card => card.Type == CardType.Skill);
        await RealtaDiscoveryCmd.Choose(choiceContext, Owner, skills, IsUpgraded, RefineCmd.GetAmount(this));
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Upgraded"].UpgradeValueBy(1m);
    }
}
