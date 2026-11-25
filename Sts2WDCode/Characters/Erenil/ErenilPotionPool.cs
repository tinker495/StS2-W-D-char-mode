using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.Erenil;

public class ErenilPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Erenil.Color;


    public override string BigEnergyIconPath => "charui/erenil/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/erenil/text_energy.png".ImagePath();
}
