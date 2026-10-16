using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2WD.Sts2WDCode.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

// 수호 카드: created and grown only through GuardCmd, like Regent's Sovereign Blade.
// Tokens belong to the base-game TokenCardPool (as Sovereign Blade does), so this skips RealtaCard.
[Pool(typeof(TokenCardPool))]
public sealed class RealtaGuard() : Sts2WDCard(1, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    private decimal _currentBlock;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(0m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public void AddBlock(decimal amount)
    {
        AssertMutable();
        DynamicVars.Block.BaseValue += amount;
        _currentBlock = DynamicVars.Block.BaseValue;
    }

    // Same upgrade as Sovereign Blade (docs/adr/0002).
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    // Downgrading resets dynamic vars to canonical values; keep the accumulated block (as Sovereign Blade does).
    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Block.BaseValue = _currentBlock;
    }
}
