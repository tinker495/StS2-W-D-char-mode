using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Assets;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public class AltheaCardPool : CustomCardPoolModel
{
    public override string Title => Althea.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/althea/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/althea/text_energy.png".ImagePath();


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    public override float H => 1f;
    public override float S => 1f; //Saturation
    public override float V => 1f; //Brightness

    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load Sts2WD/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => Althea.Color;
    public override Texture2D CustomFrame(CustomCardModel card) =>
        PreloadManager.Cache.GetTexture2D("cards/althea/frame.png".ImagePath());

    public override bool IsColorless => false;
}
