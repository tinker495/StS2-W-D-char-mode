# Realta UI Runtime Images and Energy Counter

[Asset index](../../realta-assets.md).

## Canonical Base and Runtime Images

The canonical base is `docs/characters/realta/base.png`. Runtime PNGs share the directory `Sts2WD/images/charui/realta/`.

| Target / delivery file | Production basis / conversion | PNG dimensions | Applied state |
|---|---|---|---|
| [Canonical base.png](../../base.png) | Approved complete figure and transparent margins | 1152×1728 RGBA | Approved and promoted; identity input for subsequent production |
| `combat.png` | Fit the whole approved base, including margins | 384×576 RGBA | Canonical standing still and fallback; animated scenes use separate resources |
| `character_icon.png` | Generate the base's face and clothing as a chibi bust | 128×128 RGBA | Chibi portrait applied |
| `character_icon_outline.png` | Dilate the final portrait alpha by 2 px | 128×128 RGBA | Connected with the same portrait silhouette |
| `map_marker.png` | Dedicated face-and-hat master with the same chibi appearance | 128×128 RGBA | Fit the whole master within 110×110, then add 9 px transparent padding on each side |
| `char_select.png` | Non-SD face close-up generated from the base and original-game tiles | 132×195, opaque | Fit the whole master and composite over blue |
| `char_select_locked.png` | Reuse the same downscaled alpha | 132×195, opaque | Black figure on gray |
| `big_energy.png` | User-selected red-orange flame-heart emblem, whole-image Fit | 74×74 RGBA | Empty dark burgundy heart inset for the cost number; no small inner heart |
| `text_energy.png` | Same flame-heart master, whole-image Fit | 24×24 RGBA | Small inline-text output applied |
| `char_select_bg.png` | Separately generated blade-inspection scene | 1920×1080, opaque | Fit the whole master; no post-generation crop |

The base, portrait, marker, energy icons, and standing figure have transparent backgrounds. Selection tiles, the locked variant, and the selection background are opaque. Chibi scope follows the shared rule in [ARTS.md](../../../../../ARTS.md). Use [realta-art.md](../../realta-art.md) when carrying the latest approved appearance into new assets.

## Lower-Left Combat Energy Counter

`RealtaEnergyCounterAppearance` runs after the native `NEnergyCounter._Ready`, scoped to a Realta player. It replaces `Layers/Layer1.Texture` with `Sts2WD/images/charui/realta/combat_energy.png`: the entire approved flame-heart painting proportionally resized to 256px, displayed inside the native 128px counter. It hides `Layers/RotationLayers`, `Layers/Layer4`, and `Layers/Layer5` so old orb decorations cannot cover or rotate the emblem. The original counter scene, current/max energy model, subscriptions, hover tip, energy-gain particle VFX, and entrance/exit animation remain active. Native RefreshLabel continues to darken the texture and turn the text red at zero energy.

The sibling MegaLabel uses anchors 0.5/0.5 with offsets -24,-2,+30,+42: the same 54x44px box, optically shifted 3px right at the user's request. It uses live 16-26px autosizing, 3px outline/shadow outline, and 1px shadows. Realta overrides EnergyLabelOutlineColor with #4a160f. This keeps current/max energy inside the lower heart inset and shrinks longer values. Actual game textures/materials and the Korean locale font were rendered for 3/3, 0/3, 4/4, 10/3, 10/10, and 0/10 in a temporary resource preview. This does not claim an in-game playthrough.
