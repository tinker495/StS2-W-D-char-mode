using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using Sts2WD.Sts2WDCode.Extensions;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public class Realta : PlaceholderCharacterModel
{
    public const string CharacterId = "Realta";

    public static readonly Color Color = new("e76f51");

    public override Color NameColor => Color;
    public override Color EnergyLabelOutlineColor => new("4a160f");
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<RealtaStrike>(),
        ModelDb.Card<RealtaStrike>(),
        ModelDb.Card<RealtaStrike>(),
        ModelDb.Card<RealtaStrike>(),
        ModelDb.Card<RealtaDefend>(),
        ModelDb.Card<RealtaDefend>(),
        ModelDb.Card<RealtaDefend>(),
        ModelDb.Card<RealtaDefend>(),
        ModelDb.Card<RealtaFlyingGlaive>(),
        ModelDb.Card<RealtaTempering>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<RealtaBronzeWatchdog>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<RealtaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<RealtaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<RealtaPotionPool>();

    // The glaive lands on the attack_hit Spine event.
    public override float AttackAnimDelay => 0.54f;

    // The body reaches its fallen pose by 1.66 s; hair and clothing settle through 2.20 s.
    public override float DeathAnimTime => 1.66f;

    public override string CustomVisualPath => "res://Sts2WD/scenes/realta/combat.tscn";
    public override string CustomRestSiteAnimPath => "res://Sts2WD/scenes/realta/rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://Sts2WD/scenes/realta/merchant.tscn";
    public override string CustomCharacterSelectBg => "res://Sts2WD/scenes/realta/char_select_bg_realta.tscn";

    public override CreatureAnimator GenerateAnimator(MegaSprite controller, Creature creature)
    {
        var idle = new AnimState("idle_loop", isLooping: true);
        var die = new AnimState("die");
        var relaxed = new AnimState("relaxed_loop", isLooping: true);
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Relaxed", relaxed);
        animator.AddAnyState("Dead", die);

        foreach (var (state, trigger) in new (AnimState State, string Trigger)[]
        {
            (new AnimState("attack"), "Attack"),
            (new AnimState("guard"), "Guard"),
            (new AnimState("hurt"), "Hit"),
            (new AnimState("cast"), "Cast"),
            (new AnimState("cast"), "PowerUp"),
        })
        {
            state.AddNextState(idle);
            animator.AddAnyState(trigger, state);
        }

        return animator;
    }

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    public override string CustomIconTexturePath => "realta/character_icon.png".CharacterUiPath();
    public override string CustomIconOutlineTexturePath => "realta/character_icon_outline.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "realta/char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "realta/char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "realta/map_marker.png".CharacterUiPath();
}
