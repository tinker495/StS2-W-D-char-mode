using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.CharacterFour;

public class CharacterFourRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => CharacterFour.Color;

    public override string BigEnergyIconPath => "charui/character_four/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/character_four/text_energy.png".ImagePath();
}
