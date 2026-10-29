using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public sealed class MercuriusIAlonePower : CustomPowerModel
{
    // All copies of this power share the provenance guard for generated echoes.
    private static readonly HashSet<CardModel> Echoes = new();
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override string CustomPackedIconPath => ModelDb.Power<CreativeAiPower>().PackedIconPath;
    public override string CustomBigIconPath => ModelDb.Power<CreativeAiPower>().ResolvedBigIconPath;
    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (Owner.Player is not { } player || (creator ?? card.Owner) == player || Echoes.Contains(card)) return;
        for (int i = 0; i < Amount; i++)
        {
            var copy = card.CreateClone();
            copy.Owner = null!;
            copy.Owner = player;
            copy.AddKeyword(MegaCrit.Sts2.Core.Entities.Cards.CardKeyword.Ethereal);
            Echoes.Add(copy);
            try { await MegaCrit.Sts2.Core.Commands.CardPileCmd.AddGeneratedCardToCombat(copy, MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand, player); }
            finally { Echoes.Remove(copy); }
        }
    }
}
