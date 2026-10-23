using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public class AltheaPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Althea.Color;


    public override string BigEnergyIconPath => "charui/althea/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/althea/text_energy.png".ImagePath();
}
