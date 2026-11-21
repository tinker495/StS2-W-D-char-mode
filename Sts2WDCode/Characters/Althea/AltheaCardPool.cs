using BaseLib.Abstracts;
using Sts2WD.Sts2WDCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Sts2WD.Sts2WDCode.Cards;

namespace Sts2WD.Sts2WDCode.Characters.Althea;

public class AltheaCardPool : CustomCardPoolModel, ICardPoolAppearance
{
    public override string Title => Althea.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/althea/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/althea/text_energy.png".ImagePath();


    public override float H => 1f;
    public override float S => 1f;
    public override float V => 1f;

    public override Color DeckEntryCardColor => Althea.Color;
    public override Texture2D CustomFrame(CustomCardModel card) =>
        PreloadManager.Cache.GetTexture2D("cards/althea/frame.png".ImagePath());

    public override bool IsColorless => false;

    void ICardPoolAppearance.ApplyAppearance(NCard card) => AltheaCardAppearance.ApplyTo(card);
}
