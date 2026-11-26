using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Sts2WD.Sts2WDCode.Characters.Erenil;

public class Erenil : PlaceholderCharacterModel
{
    public const string CharacterId = "Erenil";

    public static readonly Color Color = new("9b72cf");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<BurningBlood>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<ErenilCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<ErenilRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<ErenilPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    public override string CustomRestSiteAnimPath => "res://Sts2WD/scenes/erenil/rest_site.tscn";
    public override string CustomIconTexturePath => "erenil/character_icon.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "erenil/char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "erenil/char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "erenil/map_marker.png".CharacterUiPath();
}
