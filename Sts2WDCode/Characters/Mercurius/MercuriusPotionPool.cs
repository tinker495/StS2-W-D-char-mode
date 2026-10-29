using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public class MercuriusPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Mercurius.Color;


    public override string BigEnergyIconPath => "charui/mercurius/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/mercurius/text_energy.png".ImagePath();
}
