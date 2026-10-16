using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public class Realta : PlaceholderCharacterModel
{
    public const string CharacterId = "Realta";

    public static readonly Color Color = new("e76f51");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    // Strike/Defend are still the Ironclad placeholders.
    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<RealtaFlyingGlaive>(),
        ModelDb.Card<RealtaTempering>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<RealtaKuu>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<RealtaCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<RealtaRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<RealtaPotionPool>();

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
    public override string CustomIconTexturePath => "realta/character_icon.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "realta/char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "realta/char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "realta/map_marker.png".CharacterUiPath();
}
