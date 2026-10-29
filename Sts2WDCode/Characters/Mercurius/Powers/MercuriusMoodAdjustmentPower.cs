using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusMoodAdjustmentPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<ReboundPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<ReboundPower>().ResolvedBigIconPath;
    public override async Task BeforeSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Owner.Player is not { } player) return;
        for (int i = 0; i < Amount && player.PlayerCombatState!.Hand.Cards.Count > 0; i++)
        {
            var selected = (await CardSelectCmd.FromHand(context, player,
                new CardSelectorPrefs(new MegaCrit.Sts2.Core.Localization.LocString("cards", "STS2WD-MERCURIUS_TUNE_FROM_HAND.selectionScreenPrompt"), 1), null, this)).FirstOrDefault();
            if (selected != null) await CardPileCmd.Add(selected, PileType.Draw, CardPilePosition.Top);
        }
    }
}
