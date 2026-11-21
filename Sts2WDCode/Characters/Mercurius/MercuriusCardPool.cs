using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;

namespace Sts2WD.Sts2WDCode.Characters.Mercurius;

public class MercuriusCardPool : CustomCardPoolModel
{
    public override string Title => Mercurius.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/mercurius/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/mercurius/text_energy.png".ImagePath();


    public override float H => 0.13f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => new("e9c46a");

    public override bool IsColorless => false;
}
