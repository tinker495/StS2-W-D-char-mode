using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.CharacterFour;

public class CharacterFourCardPool : CustomCardPoolModel
{
    public override string Title => CharacterFour.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/character_four/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/character_four/text_energy.png".ImagePath();


    public override float H => 0.75f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => new("9b72cf");

    public override bool IsColorless => false;
}
