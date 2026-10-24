using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public class Althea : PlaceholderCharacterModel
{
    public const string CharacterId = "Althea";

    public static readonly Color Color = new("a91f30");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 80;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<AltheaRageStrike>(),
        ModelDb.Card<AltheaStrike>(),
        ModelDb.Card<AltheaStrike>(),
        ModelDb.Card<AltheaStrike>(),
        ModelDb.Card<AltheaStrike>(),
        ModelDb.Card<AltheaStrike>(),
        ModelDb.Card<AltheaDefend>(),
        ModelDb.Card<AltheaDefend>(),
        ModelDb.Card<AltheaDefend>(),
        ModelDb.Card<AltheaDefend>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<AltheaWyrd>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<AltheaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<AltheaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<AltheaPotionPool>();

    /*  PlaceholderCharacterModel will utilize placeholder basegame assets for most of your character assets until you
        override all the other methods that define those assets.
        These are just some of the simplest assets, given some placeholders to differentiate your character with.
        You don't have to, but you're suggested to rename these images. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    public override string CustomIconTexturePath => "althea/character_icon.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "althea/char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "althea/char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "althea/map_marker.png".CharacterUiPath();

    public override Task BeforeCardPlayed(CardPlay play)
    {
        AltheaRules.BeginPlay(play);
        return Task.CompletedTask;
    }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (AltheaRules.Applies(player))
        {
            var state = AltheaRules.For(player); state.Attacks = 0; state.Streak = 0; state.CardsPlayed = 0;
            state.ReformedThisTurn.Clear();
        }
        return Task.CompletedTask;
    }
    public override Task AfterPowerAmountChanged(PlayerChoiceContext context, PowerModel power, decimal amount,
        Creature? applier, CardModel? source)
    {
        if (power.Owner.Player is { } player && AltheaRules.Applies(player) && applier?.Side == CombatSide.Enemy
            && AltheaRules.IsHarmfulApplication(power, amount, applier))
            AltheaRules.For(player).EnemyEffects.Add(power);
        return Task.CompletedTask;
    }
    public override async Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        if (!AltheaRules.Applies(card.Owner)) return;
        if (AltheaRules.IsCurse(card)) AltheaRules.For(card.Owner).CursesExhausted++;
        await AltheaRules.Unfading(context, card, exhausted: true);
    }
    public override Task AfterShuffle(PlayerChoiceContext context, Player player)
    {
        if (AltheaRules.Applies(player))
        {
            AltheaRules.For(player).Shuffles++;
            if (player.PlayerCombatState!.TurnNumber > 0)
                foreach (var card in player.PlayerCombatState.DrawPile.Cards.OfType<AltheaCard>()
                    .Where(c => c.Kind is AltheaCardKind.Feint or AltheaCardKind.Regroup)) card.EnergyCost.AddThisCombat(1);
        }
        return Task.CompletedTask;
    }
}
