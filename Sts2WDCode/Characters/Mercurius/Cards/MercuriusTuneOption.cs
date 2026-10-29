using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public abstract class MercuriusTuneOption() : MercuriusCard(-1, CardType.Skill, CardRarity.Token, TargetType.None)
{
    public override int MaxUpgradeLevel => 0;
    public override bool CanBeGeneratedInCombat => false;
    protected abstract CardModel PortraitSource { get; }
    public override string PortraitPath => PortraitSource.PortraitPath;
    public override string CustomPortraitPath => PortraitPath;
    public override string BetaPortraitPath => PortraitPath;
    public abstract Task Resolve(PlayerChoiceContext context);
}
