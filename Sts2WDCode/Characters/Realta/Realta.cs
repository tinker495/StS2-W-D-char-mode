using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public class Realta : PlaceholderCharacterModel
{
    public const string CharacterId = "Realta";

    public static readonly Color Color = new("e76f51");

    public override Color NameColor => Color;
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

    // Scene wrappers share one static sprite and provide each screen's layout and required nodes.
    public override string CustomVisualPath => "res://Sts2WD/scenes/realta/combat.tscn";
    public override string CustomRestSiteAnimPath => "res://Sts2WD/scenes/realta/rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://Sts2WD/scenes/realta/merchant.tscn";
    public override string CustomCharacterSelectBg => "res://Sts2WD/scenes/realta/char_select_bg_realta.tscn";

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
