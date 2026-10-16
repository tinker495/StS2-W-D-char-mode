using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public class RealtaRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Realta.Color;

    public override string BigEnergyIconPath => "charui/realta/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/realta/text_energy.png".ImagePath();
}
