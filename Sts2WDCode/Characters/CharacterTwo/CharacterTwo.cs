using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public partial class CharacterTwo : PlaceholderCharacterModel
{
    public const string CharacterId = "CharacterTwo";

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

    public override CardPoolModel CardPool => ModelDb.CardPool<CharacterTwoCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<CharacterTwoRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<CharacterTwoPotionPool>();

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
    public override string CustomIconTexturePath => "character_two/character_icon.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "character_two/char_select.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "character_two/char_select_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "character_two/map_marker.png".CharacterUiPath();
}
