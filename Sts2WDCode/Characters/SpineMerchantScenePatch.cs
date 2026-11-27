using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Sts2WD.Sts2WDCode.Characters;

/// <summary>
/// Unwraps %Visuals SpineSprite to NMerchantCharacter.GetChild(0) right after BaseLib's
/// NMerchantCharacterFactory wraps merchant.tscn (outside the SceneTree, blocked == 0),
/// while leaving Sprite2D merchant scenes untouched.
/// </summary>
[HarmonyPatch(typeof(CustomCharacterModel), nameof(CustomCharacterModel.RegisterSceneConversions))]
internal static class SpineMerchantScenePatch
{
    private static void Postfix(CustomCharacterModel __instance)
    {
        if (__instance.GetType().Assembly != typeof(MainFile).Assembly)
            return;

        __instance.CustomMerchantAnimPath?.RegisterSceneForConversion<NMerchantCharacter>(PromoteSpineMerchantChild);
    }

    internal static void PromoteSpineMerchantChild(NMerchantCharacter merchant)
    {
        if (merchant.GetChildCount() == 0)
            return;

        var wrapper = merchant.GetChild(0);
        var spine = wrapper.GetNodeOrNull("%Visuals");
        if (spine == null || spine.GetClass() != "SpineSprite")
            return;

        spine.UniqueNameInOwner = false;
        wrapper.RemoveChild(spine);
        merchant.RemoveChild(wrapper);
        merchant.AddChild(spine);
        spine.Owner = merchant;
        foreach (var sub in spine.GetChildren())
            sub.Owner = merchant;
        wrapper.QueueFree();
    }
}
