using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2WD.Sts2WDCode.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// Each token owns its accumulated block, following Sovereign Blade's growth rules.
// Tokens belong to the base-game TokenCardPool (as Sovereign Blade does), so this skips RealtaCard.
[Pool(typeof(TokenCardPool))]
public sealed class RealtaGuard() : Sts2WDCard(1, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    // Guard growth survives downgrade; upgrade-added Block does not.
    private decimal _guardBlock;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [RealtaKeywords.Legacy, CardKeyword.Retain];

    public override int MaxUpgradeLevel => int.MaxValue;

    // Token ownership stays with TokenCardPool; only its presentation belongs to Realta.
    public override CardPoolModel VisualCardPool => ModelDb.CardPool<RealtaCardPool>();
    public override Texture2D? CustomFrame => ModelDb.CardPool<RealtaCardPool>().CustomFrame(this);

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(RealtaKeywords.Guard), HoverTipFactory.FromKeyword(RealtaKeywords.Legacy)];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(0m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public void AddBlock(decimal amount)
    {
        AssertMutable();
        DynamicVars.Block.BaseValue += amount;
        _guardBlock += amount;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue = _guardBlock;
    }
}
