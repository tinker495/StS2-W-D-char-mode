using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public interface MercuriusIllusionLossListener
{
    Task AfterIllusionLost(PlayerChoiceContext context, int amount);
}
