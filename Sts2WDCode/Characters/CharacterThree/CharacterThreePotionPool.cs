using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.CharacterThree;

public class CharacterThreePotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => CharacterThree.Color;


    public override string BigEnergyIconPath => "charui/character_three/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/character_three/text_energy.png".ImagePath();
}
