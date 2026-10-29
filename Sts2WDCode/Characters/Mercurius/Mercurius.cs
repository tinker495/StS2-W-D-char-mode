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
    public override string CustomIconTexturePath => "mercurius/character_icon.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "mercurius/char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "mercurius/char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "mercurius/map_marker.png".CharacterUiPath();
}
