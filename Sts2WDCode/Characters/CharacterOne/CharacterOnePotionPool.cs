using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.CharacterOne;

public class CharacterOnePotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => CharacterOne.Color;


    public override string BigEnergyIconPath => "charui/character_one/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/character_one/text_energy.png".ImagePath();
}
