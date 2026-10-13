using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.CharacterTwo;

public class CharacterTwoRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => CharacterTwo.Color;

    public override string BigEnergyIconPath => "charui/character_two/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/character_two/text_energy.png".ImagePath();
}
