using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Sts2WD.Sts2WDCode.Characters.Realta;

public class RealtaCardPool : CustomCardPoolModel
{
    public override string Title => Realta.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/realta/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/realta/text_energy.png".ImagePath();


    // The painted frame already contains the approved black-to-vermilion palette.
    public override float H => 1f;
    public override float S => 1f;
    public override float V => 1f;

    public override Texture2D CustomFrame(CustomCardModel card) =>
        PreloadManager.Cache.GetTexture2D("cards/realta/frame.png".ImagePath());

    //Color of small card icons
    public override Color DeckEntryCardColor => new("e76f51");

    public override bool IsColorless => false;
}
