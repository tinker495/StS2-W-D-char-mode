using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public class Mercurius : PlaceholderCharacterModel
{
    public const string CharacterId = "Mercurius";

    public static readonly Color Color = new("e9c46a");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    public override IEnumerable<CardModel> StartingDeck => [
        ModelDb.Card<MercuriusOldMandolin>(),
        ModelDb.Card<MercuriusStrike>(),
        ModelDb.Card<MercuriusStrike>(),
        ModelDb.Card<MercuriusStrike>(),
        ModelDb.Card<MercuriusStrike>(),
        ModelDb.Card<MercuriusIllusoryStandIn>(),
        ModelDb.Card<MercuriusDefend>(),
        ModelDb.Card<MercuriusDefend>(),
        ModelDb.Card<MercuriusDefend>(),
        ModelDb.Card<MercuriusDefend>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<MercuriusDragonsWaymark>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<MercuriusCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<MercuriusRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<MercuriusPotionPool>();

    public override string CustomVisualPath => "res://Sts2WD/scenes/mercurius/combat.tscn";
    public override string CustomRestSiteAnimPath => "res://Sts2WD/scenes/mercurius/rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://Sts2WD/scenes/mercurius/merchant.tscn";
    public override string CustomCharacterSelectBg => "res://Sts2WD/scenes/mercurius/char_select_bg_mercurius.tscn";

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }
    public override string CustomIconTexturePath => "mercurius/character_icon.png".CharacterUiPath();
    public override string CustomIconOutlineTexturePath => "mercurius/character_icon_outline.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "mercurius/char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "mercurius/char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "mercurius/map_marker.png".CharacterUiPath();
}
