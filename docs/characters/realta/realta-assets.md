# Realta Asset Production and Integration Guide

This guide explains production and integration for character UI, 81 card illustrations, five relic icons, and power artwork. The delivered power set contains 24 mod-supplied icons (21 custom powers and three native-power presentations); one additional custom power retains native fallback art. Its evidence is the shared [mod code](../../../Sts2WDCode/), [runtime images](../../../Sts2WD/images/), [project dependencies](../../../Sts2WD.csproj), and the StS2 / BaseLib 3.4.7 API observations recorded below. Identify external game APIs by assembly, type, and member; private decompilation-cache paths and decompiler line numbers are not required evidence.

[realta-art.md](realta-art.md) owns appearance, palette, equipment, theme visualization, asset staging, and character prompt material. This guide owns file mappings, code/scene integration, and current runtime status. [realta-history.md](realta-history.md) records past production decisions and verification. Follow [ARTS.md](../../../ARTS.md) for shared style and chibi scope.

## 0. Current Application Status

**Relic refresh candidates, 2026-10-24:** five new illustrations are staged in `relic-candidates/2026-10-24/` beside this guide. The folder mirrors the five existing model filenames in section 2.3.2: 94 x 94 small PNGs, matching white 2 px alpha-derived outlines, and 256 x 256 large PNGs in `big/` (15 assets total). `overview.png` shows the full compositions and 94/48 px readability on light/dark backgrounds. All images were generated separately using the built-in image tool and proportionally Fit-resized without cropping. Small icons fit the entire source canvas into 90 x 90 with 2 px transparent padding on all sides before outline generation, preserving clearance around the sword tip. All 15 delivered PNGs have the intended dimensions and transparent corners. Original-game relic/potion pixels supplied rendering density; the user's five subject references supplied the updated forms. The canine starter correction replaces the initial weapon proposal. Existing runtime relic PNGs, gameplay code, installed game and PCK are unchanged pending selection of the new artwork. No package build or gameplay test applies to this candidate-only delivery.

As of 2026-10-20, the nine Branch B runtime PNGs, heated-iron card body, flame-heart combat energy counter, and dedicated power icons coexist with the four animated Realta scenes. `combat.png` remains the canonical standing still and fallback; the combat, rest, and merchant scenes use their animation resources. Sections 0.1–0.2 map the images and scenes; sections 0.6–0.7 and 3 describe the newer presentation resources.

The 2026-10-20 combat/merchant rig places the weapon and ribbon in front of all body and death cels. Only the gripping hand and VFX appear above them; the released hand moves behind the weapon during death. Existing PNGs, meshes, and bone paths are preserved. The tree order and death ribbon angle are recorded in [animation/README.md](animation/README.md).

Merchant `relaxed_loop` and its `idle`/`relaxed` aliases place the entire free arm behind the body. Clip keys switch the tree order of `upper_arm_r` and `forearm_r`; the wrist moves slightly outward so the glove edge remains visible beside the coat. Return-to-combat and death ordering changes occur when the arm and coat no longer overlap. Existing PNGs, meshes, and rest/selection resources are preserved. See Merchant free arm order in [animation/README.md](animation/README.md) for key timing and blend details.

Merchant refinement on 2026-10-19 replaces the native eight-second `relaxed_loop` with independently authored planted-foot IK, a lower fixed shaft grip, free glove by the belt and viewer-left leg support. The same resource serves merchant autoplay/idle and combat relaxed aliases. `author_clips.py` owns the pose; `build_combat_rig.gd` owns live-phase blends to idle/death; the runtime controller suppresses phase randomization when changing loops. Alpha-only cleanup of `combat/parts/weapon-rigid.png` and `foot_l.png` preserves their canvas/offset registration and all RGB. No new paintings or rest/selection edits are part of this refinement. Reproduction and verification scope are documented in [animation/README.md](animation/README.md).

As of 2026-10-19, **all four Realta scenes are animated** (combat/merchant cutout rig, seated rest mesh, animated selection painting). Their runtime files, ownership and reproduction live in [animation/README.md](animation/README.md); decisions and comparisons are in [the iteration log](realta-animation-iteration-log.md) and [the runtime dispatch research](../../research/sts2-cutout-runtime-contract.md). `combat.png` remains the canonical standing still (portrait source, fallback), but no scene references it any more. The repository build/PCK export and packaged-scene rendering were verified; in-game play was not.

The 2026-10-18 standing delivery reverted the separate combat pose and removed the extra `standing.png`. The base-derived `combat.png` retains the youthful appearance, open forehead, both pauldrons, continuous collar, and upright one-handed stance of the approved base. The selection illustration retains the blade-inspection scene. The tables and conversion procedures below explain the current state without local generation masters or historical logs.

### 0.1 Canonical Base and Runtime Images

The canonical base is `docs/characters/realta/base.png`. Runtime PNGs share the directory `Sts2WD/images/charui/realta/`.

| Target / delivery file | Production basis / conversion | PNG dimensions | Applied state |
|---|---|---|---|
| [Canonical base.png](base.png) | Approved complete figure and transparent margins | 1152×1728 RGBA | Approved and promoted; identity input for subsequent production |
| `combat.png` | Fit the whole approved base, including margins | 384×576 RGBA | Canonical standing still and fallback; animated scenes use separate resources |
| `character_icon.png` | Generate the base's face and clothing as a chibi bust | 128×128 RGBA | Chibi portrait applied |
| `character_icon_outline.png` | Dilate the final portrait alpha by 2 px | 128×128 RGBA | Connected with the same portrait silhouette |
| `map_marker.png` | Dedicated face-and-hat master with the same chibi appearance | 128×128 RGBA | Fit the whole master within 110×110, then add 9 px transparent padding on each side |
| `char_select.png` | Non-SD face close-up generated from the base and original-game tiles | 132×195, opaque | Fit the whole master and composite over blue |
| `char_select_locked.png` | Reuse the same downscaled alpha | 132×195, opaque | Black figure on gray |
| `big_energy.png` | User-selected red-orange flame-heart emblem, whole-image Fit | 74×74 RGBA | Empty dark burgundy heart inset for the cost number; no small inner heart |
| `text_energy.png` | Same flame-heart master, whole-image Fit | 24×24 RGBA | Small inline-text output applied |
| `char_select_bg.png` | Separately generated blade-inspection scene | 1920×1080, opaque | Fit the whole master; no post-generation crop |

The base, portrait, marker, energy icons, and standing figure have transparent backgrounds. Selection tiles, the locked variant, and the selection background are opaque. Chibi scope follows the shared rule in [ARTS.md](../../../ARTS.md). Use [realta-art.md](realta-art.md) when carrying the latest approved appearance into new assets.

### 0.2 Current Scene and Property Connections

`Realta.cs` returns these `.tscn` paths under `res://Sts2WD/scenes/realta/`. Animated content lives under `res://Sts2WD/animations/realta/` (see [animation/README.md](animation/README.md)).

| Property | Scene | Current structure / display placement |
|---|---|---|
| `CustomVisualPath` | `combat.tscn` | `Node2D` + `%Visuals` (empty `Sprite2D` at `(0,-155)`, scale 0.625) → `Rig` instance of `realta_combat_rig.tscn` at `(-192,-288)`; `Bounds`, dialogue/intent/orb anchors and `%FormVfx` unchanged. Displayed 240×360 |
| `CustomRestSiteAnimPath` | `rest_site.tscn` | `Node2D` → `ControlRoot` (flip/seat point) → `Visuals` (`realta_rest.gd`, position `(-258,-285)`, scale 0.4), `%Hitbox`, `%ThoughtBubbleLeft/Right` |
| `CustomMerchantAnimPath` | `merchant.tscn` | `Node2D` + `%Visuals` → `Rig` instance of `realta_merchant_rig.tscn` (autoplay and `idle` = `relaxed_loop`) |
| `CustomCharacterSelectBg` | `char_select_bg_realta.tscn` | `Control` (FullRect, clips, ignores input) → `Illustration` (`realta_select.gd`) covering the rect like `KEEP_ASPECT_COVERED`, using `char_select_bg.png` |

Combat `Bounds` span `(-120,-360)~(120,0)`; the rest `Hitbox` spans `(-195,-280)~(195,200)`. `CustomIconOutlineTexturePath` connects the portrait-outline PNG. The combat energy counter uses the flame-heart treatment below. The four scenes use the animations listed above; multiplayer hands retain their existing defaults.

### 0.3 Verification Scope and Reading Guide

2026-10-19 전투/상인 리그 보완본은 가려진 관절 면, 앞/뒤 코트, 공격 전용 그립, 피격·사망 다리 동작과 착지 중간 셀을 포함한다. runtime PNG와 parts.json/meshes.json의 source mapping이 빌드 입력이며, 구체적인 파일 소유권과 재현 절차는 [animation/README.md](animation/README.md)에 있다. 7개 클립 396프레임 렌더와 publish가 완료됐고 새 PCK에서 두 장면·10개 재생 이름·추가 텍스처·행동 복귀·사망 유지·revive reset을 관찰했다. 게임 설치와 실제 게임 검증은 하지 않았으며 rest/select 원본 12개는 작업 시작 해시와 같다.

The combat expression textures `Sts2WD/animations/realta/combat/parts/head-closed.png`, `head-hurt.png`, and `head-attack.png` were regenerated separately and registered by scale/translation against unchanged hair, hat and ear regions. Each remains 188×126 RGBA at source offset `(140,99)`, with alpha identical to the untouched `head.png`. One connected facial-skin mask preserves hair/hat outside the face and the original dark jaw junction. Matching 384×576 source layers place each complete runtime rectangle at `(140,99)` in `drafts/cutout-v12/parts/` for future export consistency. Three visual review passes included 8× face views on grey/dark and head-over-torso at scale 0.625. This PNG-only update did not run Godot import, packaging, installation or game verification; existing packaged assets may therefore predate these textures.

- Historical rollback verification checked that the restored `combat.png` exactly matched the former base-derived `standing.png` and that all three scenes referenced `combat.png` before animation integration. The extra standing PNG and its import descriptor are removed. Packaging/resource-loading results for that restored state are recorded in section 0.5. These observations are not formal tests or product acceptance; actual gameplay and other aspect ratios remain unverified.
- The approved `base.png` was unchanged during downstream production. Section 0.4 describes delivery conversion.
- An approximate selection-UI overlay obscured part of the working hand behind the left description panel. The face and blade remained visible, and the selected composition was applied unchanged. This was neither a game capture nor a formal test; text and action readability still need an actual gameplay check.
- For PNG creation/editing, read section 0.1 and the art specification. For scenes, read sections 0.2 and 2.1. For card/relic/power delivery, read sections 2.2–2.4 and 4. Historical rationale is in [realta-history.md](realta-history.md).

### 0.4 New Production and Delivery Conversion

The game and packaging use the runtime PNGs and can build without generation intermediates. For new images, design the composition from the approved base, [art specification](realta-art.md), and original-game research for the use case. Do not reuse old deliveries or local drafts as generation inputs. The following procedures export new masters; they do not reproduce the exact pixels of unavailable historical masters.

- **Canonical standing figure:** Fit the whole `base.png` to 384×576, including transparent margins, without cropping. The command below reproduces the `combat.png` still; the animated scenes use the resources in section 0.2. The user reverted the separate combat pose; do not regenerate or reintroduce a separate runtime standing file without a new request.
- **Portrait and marker:** Generate each for a square composition. Export the whole portrait to 128×128 and dilate its final alpha by a 2 px radius for a white outline. Fit the whole marker master within 110×110, then place it at `(9,9)` on a 128×128 transparent canvas.
- **Two selection tiles:** Fit the whole transparent face-close-up master to 132×195. Reuse its downscaled RGB and alpha `a` (0–1). Composite the normal tile as `a × figure RGB + (1−a) × #2476A6`, and the locked tile as `(1−a) × #3B3B3B` to produce a black figure silhouette. Both final outputs have alpha 255.
- **Two energy icons:** Proportionally resize the complete flame-heart master, with an empty dark inset for the number, to 74×74 and 24×24. Keep the outside background transparent. Do not reintroduce the earlier small inner heart or bronze/blue medallion.
- **Selection background:** Generate a 16:9 scene from the start and Fit the whole image to 1920×1080. Fill edge margins caused by aspect-ratio rounding with the background's dark blue `#0C273C`; set final alpha to 255. Do not use post-generation cropping or Cover extraction.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 export -Source docs/characters/realta/base.png -Target Sts2WD/images/charui/realta/combat.png -Width 384 -Height 576 -CropMode Fit
```

After export, inspect the whole image at actual display size on light and dark backgrounds. Check output dimensions and scene connections, then refresh imports and the PCK with `powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 -Task publish`. Installation requires a separate request.

### 0.5 Standing and Combat Split

The separate runtime combat pose was reverted at the user's request. The former base-derived `standing.png` now occupies `Sts2WD/images/charui/realta/combat.png`; `standing.png` and `standing.png.import` have been removed. At that stage, combat, rest, and merchant scenes all referenced the restored `combat.png`, with their previous anchors and scales unchanged. No C# property changes were needed for this rollback. Section 0.2 describes the subsequent animation integration.

The restored 384×576 RGBA image has SHA-256 `0372A0C097D3EF52CF90858889BAB0DAFE068D3B3171027902E6C0A9C1A7AE6B`, matching the former standing file exactly. This is the current character-specific delivery even though the generic Branch B workflow supports separate poses.

Rollback packaging completed through `dev.ps1 publish` with zero C# warnings/errors and successful Godot import/PCK export. Temporary resource observations loaded nine textures and instantiated four scenes. All three character scenes resolved `combat.png`, and `ResourceLoader.exists` returned false for `standing.png`. Subsequent local installations include this rollback; actual gameplay remains unverified.

### 0.6 Heated-Iron Frame and Flame-Heart Energy Application

The 2026-10-20 user-selected frame concept was adapted into a separate body layer, using the game's `card_frame_attack_s` layout as a composition reference. The body has no baked cost icon, title banner, or type plaque. The original complete generated body is proportionally Fit resized to 598×844; the selected flame-heart master is Fit resized to 74×74 and 24×24. No generation region is extracted or enlarged. The applied PNGs and the descriptions here are sufficient for runtime use; optional drafts are not build inputs.

| Resource | Runtime path | Contract |
|---|---|---|
| Card body | `Sts2WD/images/cards/realta/frame.png` | 598×844 RGBA, transparent outer corners and illustration aperture, dark description panel |
| Cost icon | `Sts2WD/images/charui/realta/big_energy.png` | 74×74 RGBA, flame outline and empty dark heart |
| Inline icon | `Sts2WD/images/charui/realta/text_energy.png` | 24×24 RGBA, same whole-image conversion |
| Title / portrait-border materials | Native `CardModel.BannerMaterial` | Retain rarity-dependent gray/cyan/gold materials and visible border |
| Type material | `Sts2WD/materials/realta/card_type_plaque.tres` | Warm iron shading over the native nine-patch plaque |
| Shared shader | `Sts2WD/materials/realta/heated_metal.gdshader` | Retains each original layer's alpha and broad luminance variation |

`RealtaCardPool.CustomFrame` supplies the body; H/S/V are all 1 to preserve the painted palette. `RealtaGuard` keeps TokenCardPool ownership, overrides `VisualCardPool` for the Realta cost icon, and explicitly supplies the same custom frame because BaseLib's pool frame hook reads `Pool`, not `VisualCardPool`.

`RealtaCardAppearance` runs after `NCard.Reload`. Standard Realta cards clear the body HSV material and replace only the type-plaque material; the native title banner and portrait border retain `CardModel.BannerMaterial` and visible opacity so rarity remains readable. The type label becomes ivory with a dark outline for readability on the dark plaque. Ancient cards keep the native full-art layers. `%EnergyLabel` uses offsets left/right −17/+17 and top/bottom −8/+28 (34×36px), 14–22px live autosizing, 3px outline/shadow outline, and 1px shadow offsets. The existing Korean/locale font, cost colors, and cost values are retained. The hook explicitly refreshes `MegaLabel.AdjustFontSize` because `SetTextAutoSize` skips unchanged text. For another character on a pooled node, it restores native ±23 / −26,+30 offsets, 22–32px autosizing, 16px outlines, and 2px shadows. Portrait-border opacity and type-label overrides are also restored. Existing portrait geometry, title/description positions, upgrade coloring, and rarity-dependent title outlines remain native.

The 128×128 combat energy counter uses its native scene with the dedicated treatment described below; its texture is separate from card and inline energy icons. A temporary Godot renderer loaded the actual game/mod PCK resources and rendered Common/Uncommon/Rare layers with native banner/border materials, heated-iron body, and the game's Korean `gyeonggi_cheonnyeon_batang_bold_shared.tres` font. The rarity colors remain distinct and 0–3, X, 10, and ? fit the 64px emblem with the capped font/outline layout. This resource preview does not execute NCard or replace an in-game screenshot. Mod Harmony patches registered successfully and the temporary audio-path observations still passed. Packaging/installation verification is separate from actual gameplay, which remains unperformed.

The final `dev.ps1 publish` completed successfully with zero C# warnings/errors. The local game installation was updated after backing up its previous four mod files. SHA256 comparisons matched for the installed DLL, PCK, JSON manifest, and PDB. This installation includes the working-tree visual changes on top of gameplay commit `5c64ff8`; it does not imply those visual changes have been committed or published to GitHub.
---

### 0.7 Lower-Left Combat Energy Counter

`RealtaEnergyCounterAppearance` runs after the native `NEnergyCounter._Ready`, scoped to a Realta player. It replaces `Layers/Layer1.Texture` with `Sts2WD/images/charui/realta/combat_energy.png`: the entire approved flame-heart painting proportionally resized to 256px, displayed inside the native 128px counter. It hides `Layers/RotationLayers`, `Layers/Layer4`, and `Layers/Layer5` so old orb decorations cannot cover or rotate the emblem. The original counter scene, current/max energy model, subscriptions, hover tip, energy-gain particle VFX, and entrance/exit animation remain active. Native RefreshLabel continues to darken the texture and turn the text red at zero energy.

The sibling MegaLabel uses anchors 0.5/0.5 with offsets -24,-2,+30,+42: the same 54x44px box, optically shifted 3px right at the user's request. It uses live 16-26px autosizing, 3px outline/shadow outline, and 1px shadows. Realta overrides EnergyLabelOutlineColor with #4a160f. This keeps current/max energy inside the lower heart inset and shrinks longer values. Actual game textures/materials and the Korean locale font were rendered for 3/3, 0/3, 4/4, 10/3, 10/10, and 0/10 in a temporary resource preview. This does not claim an in-game playthrough.

## 1. Repository Asset Structure and Code Status

### 1.1 Resource Root and Build/Packaging Pipeline

- **Resource root (`Sts2WDCode/MainFile.cs:12-13`):**
  - `MainFile.ModId = "Sts2WD"`
  - `MainFile.ResPath = "res://Sts2WD"`
  - All mod assets live under `Sts2WD/` on disk and correspond to runtime Godot paths under `res://Sts2WD/...`.
- **Godot import and `.pck` generation (`dev.ps1:62-68`, `Sts2WD.csproj:120-125`, `export_presets.cfg:9-11`):**
  - After adding a `.png` under `Sts2WD/images/`, running `.\dev.ps1 -Task publish` or `.\dev.ps1 -Task deploy` invokes MegaDot's headless import (`--headless --editor --path $projectRoot --import`). This creates an adjacent `<filename>.png.import` and `.godot/imported/<filename>-<hash>.ctex`.
  - `Godot.ResourceLoader.Exists(path)` and `ResourceLoader.Load<Texture2D>(path)` require the imported `CompressedTexture2D` in the PCK (`.ctex` plus `.import` remapping). **A standalone `dotnet build` does not package new images. Run Godot import and PCK export through publish or deploy.**
- **Default `.import` parameters (`Sts2WD/images/**/*.png.import`):**
  - Existing `.png.import` files use the same lossless 2D texture settings:
    - `importer="texture"`, `type="CompressedTexture2D"`
    - `compress/mode=0` (Lossless)
    - `mipmaps/generate=false`
    - `process/fix_alpha_border=true`, `process/premult_alpha=false`
    - `detect_3d/compress_to=1`

### 1.2 Path Helpers (`Sts2WDCode/Extensions/StringExtensions.cs:6-88`)

`Sts2WD.Sts2WDCode.Extensions.StringExtensions` builds paths relative to `MainFile.ResPath` (`res://Sts2WD`). For checked resource helpers, a missing file (`!ResourceLoader.Exists(path)`) produces a log message and falls back to the shared placeholder PNG.

| Extension method (`StringExtensions.cs`) | Lines | Resolved path when `ResourceLoader.Exists` succeeds | Fallback when `Exists` fails |
|---|---|---|---|
| `ImagePath(this string path)` | `8-11` | `res://Sts2WD/images/{path}` | No fallback; returns the path unchanged |
| `CardImagePath(this string path)` | `13-20` | `res://Sts2WD/images/card_portraits/{path}` | `res://Sts2WD/images/card_portraits/card.png` |
| `BigCardImagePath(this string path)` | `22-29` | `res://Sts2WD/images/card_portraits/big/{path}` | `res://Sts2WD/images/card_portraits/big/card.png` |
| `PowerImagePath(this string path)` | `31-38` | `res://Sts2WD/images/powers/{path}` | `res://Sts2WD/images/powers/power.png` |
| `BigPowerImagePath(this string path)` | `40-47` | `res://Sts2WD/images/powers/big/{path}` | `res://Sts2WD/images/powers/big/power.png` |
| `RelicImagePath(this string path)` | `49-56` | `res://Sts2WD/images/relics/{path}` | `res://Sts2WD/images/relics/relic.png` |
| `BigRelicImagePath(this string path)` | `58-65` | `res://Sts2WD/images/relics/big/{path}` | `res://Sts2WD/images/relics/big/relic.png` |
| `PotionImagePath(this string path)` | `67-74` | `res://Sts2WD/images/potions/{path}` | `res://Sts2WD/images/potions/potion.png` |
| `PotionOutlineImagePath(this string path)` | `76-83` | `res://Sts2WD/images/potions/{path}` | `res://Sts2WD/images/potions/outline/potion.png` |
| `CharacterUiPath(this string path)` | `85-88` | `res://Sts2WD/images/charui/{path}` | No fallback; returns the path unchanged |

### 1.3 ID Generation (`ModelDb` + `BaseLib`)

1. `ModelDb.GetEntry(Type type)` (`sts2.dll: MegaCrit.Sts2.Core.Models.ModelDb`) calls `StringHelper.Slugify(type.Name)` (`StringHelper.cs:90-95`) to convert the C# class name to `UPPER_SNAKE_CASE`.
   - Replace regex `([A-Za-z0-9]|\G(?!^))([A-Z])` with `$1_$2`, then uppercase.
   - Example: `RealtaFlyingGlaive` -> `REALTA_FLYING_GLAIVE`.
2. `BaseLib.Patches.Content.PrefixIdPatch` (`BaseLib.dll`, `[HarmonyPatch(typeof(ModelDb), "GetEntry")]`) adds the root-namespace prefix to types implementing `ICustomModel` (`TypePrefix.GetPrefix(type)` -> `"STS2WD-"`).
   - Resulting `Id.Entry`: `"STS2WD-REALTA_FLYING_GLAIVE"`.
3. `BaseLib.Extensions.StringExtensions.RemovePrefix(this string id)` (`BaseLib.dll`) takes the substring after the first `'-'`; `.ToLowerInvariant()` then produces the filename stem.
   - Result: `"realta_flying_glaive"`.

### 1.4 Base Model Class Status

- **Cards (`Sts2WDCode/Cards/Sts2WDCard.cs:11-26`, `Sts2WDCode/Characters/Realta/Cards/RealtaCard.cs:10-19`):**
  - `Sts2WDCard : CustomCardModel` overrides three properties:
    - `public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();`
    - `public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();`
    - `public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();`
  - `RealtaCard : Sts2WDCard` declares `[Pool(typeof(RealtaCardPool))]` and provides Duel/Valor helpers (`IsDuel`, `IsValor`).
  - **Token exception (`Sts2WDCode/Characters/Realta/Cards/RealtaGuard.cs:15-16`):** `RealtaGuard` (`강철의 수호자`) must belong to `[Pool(typeof(TokenCardPool))]`, so it inherits directly from `Sts2WDCard`, bypassing `RealtaCard`.
- **Relics (`Sts2WDCode/Relics/Sts2WDRelic.cs:10-15`, `Sts2WDCode/Characters/Realta/Relics/RealtaRelic.cs:6-7`):**
  - `Sts2WDRelic : CustomRelicModel` overrides three properties:
    - `public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();`
    - `protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();`
    - `protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();`
  - All five Realta relics inherit from `RealtaRelic` (`: Sts2WDRelic`).
  - **Outline fallback:** If `<name>_outline.png` is missing, `PackedIconOutlinePath` returns the `RelicImagePath()` fallback `res://Sts2WD/images/relics/relic.png`, rather than `relic_outline.png`.
- **Powers (`Sts2WDCode/Powers/Sts2WDPower.cs:14-31`, `Sts2WDCode/Characters/Realta/Powers/*.cs`):**
  - `Sts2WDPower : CustomPowerModel` provides overrides for `CustomPackedIconPath` (`PowerImagePath()`) and `CustomBigIconPath` (`BigPowerImagePath()`).
  - **Current status:** Twenty of the 22 power classes under `Sts2WDCode/Characters/Realta/Powers/` inherit `Sts2WDPower` and use dedicated 64px/256px PNGs. `RealtaForgeHeatPower` and `RealtaUndyingLoyaltyPower` retain their native fallbacks. Three additional native powers receive presentation-only icons through `RealtaPowerPresentation`; see sections 2.4 and 3.
- **Character and card/relic/potion pools (`Sts2WDCode/Characters/Realta/Realta.cs:11-61`, `RealtaCardPool.cs:7-34`, `RealtaRelicPool.cs:7-13`, `RealtaPotionPool.cs:7-14`):**
  - `Realta : PlaceholderCharacterModel` overrides the name and energy-label outline colors, portrait/outline, selection tiles, map marker, and the combat/rest/merchant/selection scenes in section 0.2. `RealtaSceneBridge.cs` forwards rest act/light-off and non-combat game-over death to the Godot scenes. `RealtaEnergyCounterAppearance` supplies the counter treatment in section 0.7, and `RealtaCardAudio` reuses native upgrade and enchantment sounds. Other character visuals and sounds use `PlaceholderCharacterModel` defaults.
  - `RealtaCardPool : CustomCardPoolModel` defines `BigEnergyIconPath` (`"charui/realta/big_energy.png".ImagePath()`), `TextEnergyIconPath` (`"charui/realta/text_energy.png".ImagePath()`), a custom heated-iron frame with identity HSV parameters (`H = 1f, S = 1f, V = 1f`), and `DeckEntryCardColor = new("e76f51")`.
  - `RealtaRelicPool` and `RealtaPotionPool` use the same `BigEnergyIconPath`, `TextEnergyIconPath`, and `LabOutlineColor => Realta.Color`.

### 1.5 How to Read the Image Inventory

Use [section 0.1](#01-canonical-base-and-runtime-images) for the current base/runtime production basis and dimensions. Sections 2 and 4 map files and dimensions for assets still to be produced. Work from current delivery paths and integration contracts rather than historical file inventories.

---

## 2. BaseLib 3.4.7 and StS2 Primary API Reference

### 2.1 Character Models (`CharacterModel` -> `CustomCharacterModel` -> `PlaceholderCharacterModel`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.CharacterModel`
  - `sts2.dll: MegaCrit.Sts2.Core.Models.Characters.Ironclad`
  - `sts2.dll: MegaCrit.Sts2.Core.Models.Characters.Regent`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomCharacterModel`, `BaseLib.Abstracts.PlaceholderCharacterModel`, `BaseLib.Utils.NodeFactories.*`)

#### 2.1.1 Complete List of Overridable Character Asset, Visual, Sound, and Color Members

BaseLib applies Harmony Prefix patches to virtual and non-virtual `CharacterModel` properties (`BaseLib.Abstracts.CustomCharacterVisualPath`, `CustomCharacterVisuals`, `GenerateAnimatorPatch`, `TrailPath`, `Icon`, `IconPath`, `IconTexturePath`, `IconOutlineTexturePath`, `EnergyCounterPath`, `EnergyCounterPatch`, `EnergyCounterOutlineColorPatch`, `RestSiteAnimPath`, `MerchantAnimPath`, `ArmPointingTexturePath`, `ArmRockTexturePath`, `ArmPaperTexturePath`, `ArmScissorsTexturePath`, `CharacterSelectTransitionPath`, `CustomCharacterSelectBg`, `CharacterSelectIconPath`, `CharacterSelectLockedIconPath`, `MapMarkerPath`, `AttackSfx`, `CastSfx`, `DeathSfx`). They intercept the corresponding getter when the `CustomCharacterModel.Custom*` property is not `null`.

`PlaceholderCharacterModel` supplies the following defaults based on `public virtual string PlaceholderID => "ironclad";`.

| Category | `CustomCharacterModel` / `CharacterModel` member signature | Patched target (`CharacterModel`) | `PlaceholderCharacterModel` default (`PlaceholderID = "ironclad"`) | Current `Realta.cs` status | Vanilla resource dimensions/type (`SlayTheSpire2.pck`) |
|---|---|---|---|---|---|
| **Selection button icon** | `public virtual string? CustomCharacterSelectIconPath { get; }` | `CharacterSelectIconPath` -> `CharacterSelectIcon` (`CompressedTexture2D`) | `res://images/packed/character_select/char_select_ironclad.png` | **Overridden** (`"realta/char_select.png".CharacterUiPath()`) | `132 x 195` PNG |
| **Locked selection icon** | `public virtual string? CustomCharacterSelectLockedIconPath { get; }` | `CharacterSelectLockedIconPath` -> `CharacterSelectLockedIcon` (`CompressedTexture2D`) | `res://images/packed/character_select/char_select_ironclad_locked.png` | **Overridden** (`"realta/char_select_locked.png".CharacterUiPath()`) | `132 x 195` PNG |
| **Selection background scene** | `public virtual string? CustomCharacterSelectBg { get; }` | `CharacterSelectBg` (`NCharacterSelectScreen.cs:693`) | `res://scenes/screens/char_select/char_select_bg_ironclad.tscn` | **Overridden** (see section 0 for the current connection) | `Control` root `.tscn` (`1920x1080` screen) |
| **Screen-transition material** | `public virtual string? CustomCharacterSelectTransitionPath { get; }` | `CharacterSelectTransitionPath` (`NGame.Instance.Transition.FadeOut`) | `res://materials/transitions/ironclad_transition_mat.tres` | Not overridden (Ironclad transition) | `ShaderMaterial` (`.tres`) |
| **Map-marker icon** | `public virtual string? CustomMapMarkerPath { get; }` | `MapMarkerPath` -> `MapMarker` (`CompressedTexture2D`, `NMapMarker.cs:85`) | `res://images/packed/map/icons/map_marker_ironclad.png` | **Overridden** (`"realta/map_marker.png".CharacterUiPath()`) | Vanilla `49 x 64`, current template `128 x 128` PNG |
| **Top-bar / statistics / daily-challenge icon node** | `public virtual Control? CustomIcon { get; }` (priority 1)<br>`public virtual string? CustomIconPath { get; }` (priority 2) | `Icon` (`Control`, `NTopBarPortrait.cs:35`, `NCharacterStats.cs:124`) | `CustomIcon => null`, `CustomIconPath => "res://scenes/ui/character_icons/ironclad_icon.tscn"` | **`CustomIcon` Overridden** (`NodeFactory<Control>.CreateFromResource(CustomIconTexturePath)`) | `TextureRect` (`FullRect`) |
| **Run-history / multiplayer-lobby / Ancient-dialogue icon texture** | `public virtual string? CustomIconTexturePath { get; }` | `IconTexturePath` -> `IconTexture` (`Texture2D`) | `res://images/ui/top_panel/character_icon_ironclad.png` | **Overridden** (`"realta/character_icon.png".CharacterUiPath()`) | Vanilla `85 x 85`, current template `128 x 128` PNG |
| **Ancient-dialogue / multiplayer-vote icon-outline texture** | `public virtual string? CustomIconOutlineTexturePath { get; }` | `IconOutlineTexturePath` -> `IconOutlineTexture` (`Texture2D`, `NAncientDialogueLine.cs:181`, `NMultiplayerVoteContainer.cs:127`) | `res://images/ui/top_panel/character_icon_ironclad_outline.png` | **Overridden** (see section 0 for the current connection) | Vanilla `85 x 85` (or `128 x 128`, matching `CustomIconTexturePath`) PNG |
| **Combat energy-orb counter** | `public virtual string? CustomEnergyCounterPath { get; }` (recommended)<br>`public virtual CustomEnergyCounter? CustomEnergyCounter { get; }` (generated from five PNG layers) | `EnergyCounterPath` and `NEnergyCounter.Create` (`NEnergyCounter.cs:162-171`) | `res://scenes/combat/energy_counters/ironclad_energy_counter.tscn` | Native Ironclad scene retained; RealtaEnergyCounterAppearance supplies the flame-heart texture and label layout | `128 x 128` `Control` scene; each layer texture is `256 x 256` PNG × 5 (`Layer1`~`Layer5`) |
| **Combat energy-label outline color** | `public override Color EnergyLabelOutlineColor { get; }` | `CharacterModel.EnergyLabelOutlineColor` (`NEnergyCounter.cs:160, 222`) | `new Color("0000000D")` (`CharacterModel.cs:86`) | Overridden in Realta: dark red #4a160f | `Color` (e.g. Ironclad `801212FF`, Regent `784000FF`) |
| **Combat character visuals (sprite / Spine / scene)** | `public virtual string? CustomVisualPath { get; }`<br>`public virtual NCreatureVisuals? CreateCustomVisuals()` | `VisualsPath` and `CreateVisuals()` (`CharacterModel.cs:104, 210-213`) | `res://scenes/creature_visuals/ironclad.tscn` | **Overridden** (see section 0 for the current connection) | `NCreatureVisuals` (`Node2D` root) or a single PNG (`NodeFactory` conversion) |
| **Combat animation state machine** | `public virtual CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)` | `GenerateAnimator(MegaSprite)` (`CharacterModel.cs:220-241`) | `null` (`CharacterModel.GenerateAnimator` default states: `idle_loop`, `cast`, `attack`, `hurt`, `die`, `relaxed_loop`) | Not overridden | Called only for Spine animation |
| **Non-Spine death wait time** | `public virtual float DeathAnimTime => 1.5f;` | `NCreature.StartDeathAnim` (`CustomAnimationPatch.cs:97-129`) | `1.5f` (clamped to a maximum of `5f`) | Not overridden | Applies to Godot `AnimationPlayer` / `AnimatedSprite2D` |
| **Attack/cast animation delay** | `public override float AttackAnimDelay => 0.15f;`<br>`public override float CastAnimDelay => 0.25f;`<br>`public virtual float PowerUpAnimDelay => CastAnimDelay;` | `CharacterModel` properties with the same names | `0.15f` / `0.25f` | Not overridden | Seconds |
| **Rest-site (campfire) character** | `public virtual string? CustomRestSiteAnimPath { get; }` | `RestSiteAnimPath` (`NRestSiteCharacter.cs:191-197`) | `res://scenes/rest_site/characters/ironclad_rest_site.tscn` | **Overridden** (see section 0 for the current connection) | `NRestSiteCharacter` `.tscn` or a single `Sprite2D` root `.tscn` |
| **Merchant-room character** | `public virtual string? CustomMerchantAnimPath { get; }` | `MerchantAnimPath` (`NMerchantRoom`) | `res://scenes/merchant/characters/ironclad_merchant.tscn` | **Overridden** (see section 0 for the current connection) | `NMerchantCharacter` `.tscn` |
| **Card-trail VFX** | `public virtual string? CustomTrailPath { get; }` | `TrailPath` (`NCardTrailVfx.cs:94-100`) | `res://scenes/vfx/card_trail_ironclad.tscn` | Not overridden (Ironclad red trail) | `NCardTrailVfx` `.tscn` |
| **Four multiplayer-hand textures (pointer / rock-paper-scissors)** | `public virtual string? CustomArmPointingTexturePath { get; }`<br>`public virtual string? CustomArmRockTexturePath { get; }`<br>`public virtual string? CustomArmPaperTexturePath { get; }`<br>`public virtual string? CustomArmScissorsTexturePath { get; }` | `ArmPointingTexturePath`, `ArmRockTexturePath`, `ArmPaperTexturePath`, `ArmScissorsTexturePath` (`NHandImage.cs:243, 358-360`) | `res://images/ui/hands/multiplayer_hand_ironclad_{point,rock,paper,scissors}.png` | Not overridden (Ironclad hands) | `422 x 1200` PNG each |
| **Yummy Cookie relic icon** | `public virtual RelicIconData? CustomYummyCookie { get; }` | `RelicImageOverridePatch.AddOverride<YummyCookie>` (`CustomContentDictionary.cs:135-149`) | `null` | Not overridden | `new RelicIconData(BigIconPath, PackedIconPath, PackedIconOutlinePath)` |
| **Architect-event attack VFX** | `public override List<string> GetArchitectAttackVfx()` | `CharacterModel.GetArchitectAttackVfx()` |List of five effects, including blunt and slash (`PlaceholderCharacterModel`) | Not overridden | List of VFX scene-path strings |
| **Five sound effects (SFX)** | `public override string CharacterSelectSfx { get; }`<br>`public override string CharacterTransitionSfx { get; }`<br>`public virtual string? CustomAttackSfx { get; }`<br>`public virtual string? CustomCastSfx { get; }`<br>`public virtual string? CustomDeathSfx { get; }` | `CharacterSelectSfx`, `CharacterTransitionSfx`, `AttackSfx`, `CastSfx`, `DeathSfx` | `event:/sfx/characters/ironclad/ironclad_{select,attack,cast,die}` and `event:/sfx/ui/wipe_ironclad` | Not overridden | FMOD event path |
| **Dialogue / speech-bubble / map-drawing / targeting colors** | `NameColor`, `DialogueColor`, `SpeechBubbleColor`, `MapDrawingColor`, `RemoteTargetingLineColor`, `RemoteTargetingLineOutline` | `CharacterModel` (`CharacterModel.cs:51, 167-179`) | `DialogueColor = 28454f`, `SpeechBubbleColor = VfxColor.Cyan`, others: `Colors.Black` | Only `NameColor` is overridden (`#e76f51`) | `Color` and `VfxColor` enum |

#### 2.1.2 Overriding Combat, Rest, Merchant, and Energy-Counter Visuals

BaseLib's `PostModInitPatch.RegisterSceneConversions` (`BaseLib.dll`, `[HarmonyPatch(typeof(ModelDb), "Preload")]`) and `SceneConversionPatch` (`[HarmonyPatch(typeof(PackedScene), "Instantiate")]`) use `CustomCharacterModel.RegisterSceneConversions()` to convert ordinary Godot `.tscn` scenes at these four paths into game-specific node types at runtime:
- `CustomVisualPath` -> `NCreatureVisuals` (`NCreatureVisualsFactory`)
- `CustomRestSiteAnimPath` -> `NRestSiteCharacter` (`NRestSiteCharacterFactory`)
- `CustomMerchantAnimPath` -> `NMerchantCharacter` (`NMerchantCharacterFactory`)
- `CustomEnergyCounterPath` -> `NEnergyCounter` (`NEnergyCounterFactory`)

1. **Basic static-PNG combat visuals (`CreateCustomVisuals` override):**
   - To display a combat figure from one PNG without a `.tscn`, use `NCreatureVisualsFactory.CreateBareFromResource` (`NCreatureVisualsFactory.cs:25-66`).
   ```csharp
   public override NCreatureVisuals? CreateCustomVisuals() =>
       NodeFactory<NCreatureVisuals>.CreateFromResource("charui/realta/combat_sprite.png".ImagePath());
   ```
   - `NCreatureVisualsFactory` automatically creates `Visuals` (`Sprite2D`, position `(0, -height * 0.5)`), `Bounds` (`Control`, size `size * 1.1`), `CenterPos` (`Marker2D`), `IntentPos` (`Marker2D`), and `FormVfx` (`Control`).
2. **Godot `.tscn` combat visuals using Sprite2D / AnimatedSprite2D / AnimationPlayer / AnimationTree:**
   - Create a `Node2D`-rooted scene in Godot, such as `res://Sts2WD/scenes/realta/realta_visuals.tscn`, with these children:
     - `%Visuals` (`Node2D` / `Sprite2D` / `AnimatedSprite2D`)
     - `Bounds` (`Control`, default `Size = (240, 280), Position = (-120, -280)`)
     - `%CenterPos` (`Marker2D`, hit-VFX spawn position)
     - `IntentPos` (`Marker2D`, above the head)
     - Optional: `%OrbPos` (`Marker2D`), `%TalkPos` (`Marker2D`), `%FormVfx` (`Control`)
   - `CustomAnimation` (`BaseLib.Utils.CustomAnimation`, `CustomAnimationPatch.cs:142-170`) searches below the root in the order `AnimationTree` -> `AnimationPlayer` -> `AnimatedSprite2D`, then automatically plays animations with these names:
     - Idle: `"Idle"`, `"idle"`
     - Attack: `"Attack"`, `"attack"`
     - Skill/power cast: `"Cast"`, `"cast"`
     - Hit: `"Hit"`, `"Hurt"`, `"hit"`, `"hurt"`
     - Death: `"Dead"`, `"Die"`, `"dead"`, `"die"` (waits for `DeathAnimTime` during playback)
     - Revive: `"revive"`, `"Revive"`
   - Override `public override string CustomVisualPath => "res://Sts2WD/scenes/realta/realta_visuals.tscn";` in `Realta.cs`; `RegisterSceneConversions()` converts it into `NCreatureVisuals` automatically.
   - **Preloading constraint (`CharacterModel.cs:188`):** `CharacterModel.AssetPaths` preloads `VisualsPath` through `PreloadManager` when a run starts. If overriding only `CreateCustomVisuals()` while inheriting `PlaceholderCharacterModel`, retain the default Ironclad scene in `CustomVisualPath` to avoid a preload error.
3. **Spine-skeleton combat visuals:**
   - Point `CustomVisualPath` to a `.tscn` whose `%Visuals` node is a `SpineSprite`.
   - If animation names differ from the defaults (`idle_loop`, `cast`, `attack`, `hurt`, `die`, `relaxed_loop`), override `SetupCustomAnimationStates(MegaSprite controller)` and return `CustomCharacterModel.SetupAnimationState(controller, idleName: "...", deadName: "...", hitName: "...", attackName: "...", castName: "...", relaxedName: "...")`.
4. **Rest site (`CustomRestSiteAnimPath`) and merchant (`CustomMerchantAnimPath`):**
   - **Rest site:** Return a `.tscn` path with a single textured `Sprite2D` root from `CustomRestSiteAnimPath`. `NRestSiteCharacterFactory.ConvertScene` (`NRestSiteCharacterFactory.cs:79-92`) creates `ControlRoot`, `%Hitbox`, `%SelectionReticle`, `%ThoughtBubbleLeft`, `%ThoughtBubbleRight`, and `Visuals` automatically.
   - **Merchant:** `NMerchantCharacterFactory` (`NMerchantCharacterFactory.cs:16-45`) wraps a `Texture2D` in `Visuals` only when called through `NodeFactory<NMerchantCharacter>.CreateFromResource(texturePath)`. Because `CustomMerchantAnimPath` goes through `PreloadManager.Cache.GetScene`, point it to a `.tscn` with a `Node2D` root and `Sprite2D` (or `AnimationPlayer` / `AnimatedSprite2D`, using `"idle"` / `"Idle"` / `"relaxed_loop"` and `"Die"` / `"die"` animations); see `MerchantCharacterAnimPatch.cs:60-71`.
5. **Combat energy counter (`CustomEnergyCounter` or `CustomEnergyCounterPath`):**
   - With five `256 x 256` layer PNGs (`layer_1.png`–`layer_5.png`; layers 2 and 3 rotate), override the following in `Realta.cs` without a `.tscn` (`NEnergyCounterFactory.cs:235-295`):
     ```csharp
     public override CustomEnergyCounter? CustomEnergyCounter => new CustomEnergyCounter(
         layer => $"charui/realta/energy_orb/layer_{layer}.png".ImagePath(),
         outlineColor: new Color("5c2214"),
         burstColor: Realta.Color
     );
     ```

---

### 2.2 Card Portraits and Frames (`CardModel`, `CustomCardModel`, `CustomCardPoolModel`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.CardModel`
  - `sts2.dll: MegaCrit.Sts2.Core.Nodes.Cards.NCard`
  - `sts2.dll: MegaCrit.Sts2.Core.Nodes.Screens.NInspectCardScreen`
  - `sts2.dll: MegaCrit.Sts2.Core.Models.ModelDb`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomCardModel`, `CustomCardPortrait`, `CustomCardPortraitPath`, `CustomCardPortraitPngPath`, `CustomCardFrame`, `CustomCardPoolModel`, `CustomCardPoolMaterialPatch`)
  - `Sts2WDCode/Cards/Sts2WDCard.cs` (lines `11-26`)

#### 2.2.1 Portrait Resolution in `CardModel` and `CustomCardModel`

1. **Vanilla `CardModel` portrait properties (`CardModel.cs:142-156, 1047`):**
   - `public virtual string PortraitPath` -> `res://images/atlases/card_atlas.sprites/{pool}/{entry}.tres`
   - `public virtual string BetaPortraitPath` -> `res://images/atlases/card_atlas.sprites/{pool}/beta/{entry}.tres`
   - `private string PortraitPngPath` -> `res://images/packed/card_portraits/{pool}/{entry}.png`
   - `private string BetaPortraitPngPath` -> `res://images/packed/card_portraits/{pool}/beta/{entry}.png`
   - `public bool HasPortrait => ResourceLoader.Exists(PortraitPngPath);`
   - `public bool HasBetaPortrait => ResourceLoader.Exists(BetaPortraitPngPath);`
   - `public Texture2D Portrait => ResourceLoader.Load<Texture2D>(PortraitPath, null, ResourceLoader.CacheMode.Reuse);`
   - `public virtual IEnumerable<string> AllPortraitPaths => [PortraitPath];`
2. **Vanilla UI rendering (`NCard.cs:1181-1250`, `NInspectCardScreen.cs:222, 276`):**
   - Unlike StS1, StS2 has **no separate `BigPortrait` texture property**.
   - Normal combat/reward/compendium cards (`NCard`) and the inspect screen (`NInspectCardScreen`, which scales the same `NCard` to `Scale = Vector2.One * 2f`) both use **only `Model.Portrait`**.
   - Inside `res://scenes/cards/card.tscn`:
     - Standard cards (`Rarity != CardRarity.Ancient`) place `Model.Portrait` in `%Portrait` (`TextureRect`, UI area `250 x 190`, `offset_left=-125, offset_top=-168, offset_right=125, offset_bottom=22`). Vanilla source PNGs (`res://images/packed/card_portraits/<pool>/<card>.png`) are **`1000 x 760`**, exactly four times `250 x 190` at ratio `25:19`. `500 x 380` or `250 x 190` inputs share that ratio and scale automatically.
     - Ancient cards (`Rarity == CardRarity.Ancient`) place `Model.Portrait` in `%AncientPortrait`, a full-art `TextureRect`. Vanilla full-art PNGs, such as `res://images/packed/card_portraits/event/apotheosis.png`, are **`606 x 852`** (small-thumbnail reference: `250 x 350`).
3. **BaseLib 3.4.7 `CustomCardModel` patches (`BaseLib.Abstracts.CustomCard*`):**
   - `CustomCardModel` has no `CustomBigPortraitPath`. It provides:
     - `public virtual string? CustomPortraitPath => null;`
     - `public virtual Texture2D? CustomPortrait => null;`
   - Three BaseLib Harmony patches intercept `CardModel` getters:
     - `CustomCardPortrait` (`CardModel.Portrait` getter Prefix): returns `CustomPortrait` if set; otherwise, if `CustomPortraitPath` is not `null`, returns `ResourceLoader.Load<Texture2D>(CustomPortraitPath, null, CacheMode.Reuse)`. Only a `null` `CustomPortraitPath` falls through to vanilla `CardModel.Portrait`, which loads `PortraitPath`.
     - `CustomCardPortraitPngPath` (`CardModel.PortraitPngPath` getter Prefix): when `CustomPortraitPath != null`, replaces `PortraitPngPath` with `CustomPortraitPath`, making `HasPortrait` check `ResourceLoader.Exists(CustomPortraitPath)`.
     - `CustomCardPortraitPath` (`CardModel.PortraitPath` base getter Prefix): when `CustomPortraitPath != null`, makes the base `CardModel.PortraitPath` implementation return the `ResourcePath` of `CustomPortraitPath`.
4. **Current dual-path lookup in `Sts2WDCard.cs` and a proposed improvement:**
   - Template-derived code in `Sts2WDCode/Cards/Sts2WDCard.cs:17-25` currently overrides **both** `CustomPortraitPath` (`BigCardImagePath()` -> `images/card_portraits/big/<id>.png`) and `PortraitPath` (`CardImagePath()` -> `images/card_portraits/<id>.png`).
   - Consequences:
     1. At startup, `ModelDb.Preload()` (`ModelDb.cs:453`) calls `allCard.AllPortraitPaths` -> virtual `Sts2WDCard.PortraitPath`, looking for `images/card_portraits/<id>.png`. Missing files produce `"Could not find card image path: ..."` messages and return `images/card_portraits/card.png` (`250x190`). The eleven newer cards reuse the portrait properties listed in section 2.2.4.
     2. During rendering, `NCard.Reload()` (`NCard.cs:1192`) calls `Model.Portrait` -> `CustomCardPortrait` patch -> `Sts2WDCard.CustomPortraitPath`. It **loads only `images/card_portraits/big/<id>.png`**, falling back to `images/card_portraits/big/card.png` (`1000x760`). The `PortraitPath` override is not used for screen rendering.
   - **Proposed integration: automatic mapping and fallback under `Sts2WD/images/card_portraits/realta/<card_name>.png`:**
     - To manage all 81 Realta cards (79 standard, including the `RealtaGuard` token, plus two Ancient cards) in one directory, with unfinished artwork falling back to `card.png` or `big/card.png`, adapt `StringExtensions.cs` and `RealtaCard.cs` / `RealtaGuard.cs` (or `Sts2WDCard.cs`) as follows:
     ```csharp
     // Add to Sts2WDCode/Extensions/StringExtensions.cs, or adapt CardImagePath:
     public static string CharacterCardImagePath(this string path, string characterFolder)
     {
         string charPath = Path.Join(MainFile.ResPath, "images", "card_portraits", characterFolder, path);
         if (ResourceLoader.Exists(charPath)) return charPath;

         string flatBigPath = Path.Join(MainFile.ResPath, "images", "card_portraits", "big", path);
         if (ResourceLoader.Exists(flatBigPath)) return flatBigPath;

         string flatPath = Path.Join(MainFile.ResPath, "images", "card_portraits", path);
         if (ResourceLoader.Exists(flatPath)) return flatPath;

         return Path.Join(MainFile.ResPath, "images", "card_portraits", "big", "card.png");
     }
     ```
     - Point `CustomPortraitPath` to the same resolved path in `RealtaCard.cs` and `RealtaGuard.cs` (or detect an `Id.Entry` beginning with `"STS2WD-REALTA_"` in `Sts2WDCard.cs`), then unify `PortraitPath => CustomPortraitPath;`.
       - One PNG per card (`Sts2WD/images/card_portraits/realta/<card_name>.png`, `1000 x 760` or `606 x 852` for Ancient cards) then serves both normal and inspect views (`NInspectCardScreen`).
       - Unfinished cards fall back to `res://Sts2WD/images/card_portraits/big/card.png` (`1000 x 760`) without per-card code changes.
       - `RealtaGuard` (`[Pool(typeof(TokenCardPool))]`) directly inherits `Sts2WDCard`, not `RealtaCard`. Apply shared behavior in `Sts2WDCard` or include overrides in `RealtaGuard` so the token is not missed.

#### 2.2.2 Card Frames and Energy Icons (`CustomCardPoolModel`, `RealtaCardPool.cs`)

- **Card-frame color / custom textures (`RealtaCardPool.cs:19-28`, `CustomCardPoolModel.cs`, `CustomCardFrame.cs`):**
  - Default approach: when `CardFrameMaterialPath` is `"card_frame_red"`, `CustomCardPoolMaterialPatch` generates a `ShaderUtils.GenerateHsv(H, S, V)` material (`res://shaders/hsv.gdshader`) and applies it to the vanilla attack/skill/power frames (`598 x 844`). Realta now supplies its own painted body texture with H/S/V all 1 and clears its frame material after reload; the default HSV route remains an alternative for other pools.
  - Custom frame images:
    - For the whole pool, override `public override Texture2D? CustomFrame(CustomCardModel card)` in `RealtaCardPool`, returning a `598 x 844` `Texture2D` for each `card.Type` (`Attack`, `Skill`, `Power`). Set `H = 1f, S = 1f, V = 1f` to avoid shader recoloring.
    - For one card, override `CustomCardModel.CustomFrame` (`Texture2D?`) or `CustomCardModel.CreateCustomFrameMaterial` (`Material?`).
    - **Exception (`CardModel.cs:179-183, 1235-1250`):** `CardRarity.Ancient` cards (`RealtaIordinaMacBethlehem`, `RealtaGeniusInspiration`) hide the regular `_frame` and use `AncientBorder` and `AncientTextBg`, so `CustomFrame` does not affect them.
  - **Token frame color: `RealtaGuard` (`CardModel.cs:252, 286, 324`):**
    - Because `RealtaGuard` belongs to `[Pool(typeof(TokenCardPool))]`, its default `VisualCardPool` is `TokenCardPool`: gray `card_frame_colorless` frame and default colorless energy icon.
    - The applied `RealtaGuard.VisualCardPool` override selects Realta's frame material and energy icon (`big_energy.png`):
      ```csharp
      public override CardPoolModel VisualCardPool => ModelDb.CardPool<RealtaCardPool>();
      ```

#### 2.2.3 Filename Mapping for the Original 70 Realta Cards

This table and section 2.2.4 cover the 81 cards in the design specification. `RealtaStrike` and `RealtaDefend` additionally reuse the corresponding vanilla starting-card portraits and are outside this 81-card illustration inventory.
- Recommended resolution for the 79 standard-layout Basic, Token, Common, Uncommon, and Rare cards: **`1000 x 760`** (or `500 x 380` / `250 x 190`, ratio `25:19`).
- Recommended resolution for the two `Ancient` cards (`realta_iordina_mac_bethlehem.png`, `realta_genius_inspiration.png`): **`606 x 852`** (full art; small-thumbnail reference `250 x 350`).

| # | C# class (`Sts2WDCode/Characters/Realta/Cards/`) | Korean card name | Rarity / type | `ModelId.Entry` | Automatically mapped filename (`<card_name>.png`) | Recommended resolution |
|---|---|---|---|---|---|---|
| 1 | `RealtaTempering` | 담금질 | Basic / Skill | `STS2WD-REALTA_TEMPERING` | `realta_tempering.png` | `1000 x 760` |
| 2 | `RealtaFlyingGlaive` | 비행 글레이브 | Basic / Attack | `STS2WD-REALTA_FLYING_GLAIVE` | `realta_flying_glaive.png` | `1000 x 760` |
| 3 | `RealtaGuard` *(inherits directly from `Sts2WDCard`)* | 강철의 수호자 | Token / Skill | `STS2WD-REALTA_GUARD` | `realta_guard.png` | `1000 x 760` |
| 4 | `RealtaWhetstone` | 숯돌 | Common / Attack | `STS2WD-REALTA_WHETSTONE` | `realta_whetstone.png` | `1000 x 760` |
| 5 | `RealtaArcaneShock` | 비전 충격 | Common / Attack | `STS2WD-REALTA_ARCANE_SHOCK` | `realta_arcane_shock.png` | `1000 x 760` |
| 6 | `RealtaBroom` | 싸릿대 | Common / Attack | `STS2WD-REALTA_BROOM` | `realta_broom.png` | `1000 x 760` |
| 7 | `RealtaMastersWeapon` | 대가의 무기 | Common / Attack | `STS2WD-REALTA_MASTERS_WEAPON` | `realta_masters_weapon.png` | `1000 x 760` |
| 8 | `RealtaRushingBlades` | 몰아치는 칼날 | Common / Attack | `STS2WD-REALTA_RUSHING_BLADES` | `realta_rushing_blades.png` | `1000 x 760` |
| 9 | `RealtaShockingTouch` | 충격의 접촉 | Common / Attack | `STS2WD-REALTA_SHOCKING_TOUCH` | `realta_shocking_touch.png` | `1000 x 760` |
| 10 | `RealtaSteelPincers` | 강철 집게 | Common / Attack | `STS2WD-REALTA_STEEL_PINCERS` | `realta_steel_pincers.png` | `1000 x 760` |
| 11 | `RealtaSparring` | 대련 | Common / Attack | `STS2WD-REALTA_SPARRING` | `realta_sparring.png` | `1000 x 760` |
| 12 | `RealtaFlyingCharge` | 비행 돌격 | Common / Attack | `STS2WD-REALTA_FLYING_CHARGE` | `realta_flying_charge.png` | `1000 x 760` |
| 13 | `RealtaThunderBlade` | 천둥 칼날 | Common / Attack | `STS2WD-REALTA_THUNDER_BLADE` | `realta_thunder_blade.png` | `1000 x 760` |
| 14 | `RealtaHoning` | 연마 | Common / Attack | `STS2WD-REALTA_HONING` | `realta_honing.png` | `1000 x 760` |
| 15 | `RealtaReclaim` | 되돌리기 | Common / Skill | `STS2WD-REALTA_RECLAIM` | `realta_reclaim.png` | `1000 x 760` |
| 16 | `RealtaEmergencyRepairs` | 긴급 수리 | Common / Skill | `STS2WD-REALTA_EMERGENCY_REPAIRS` | `realta_emergency_repairs.png` | `1000 x 760` |
| 17 | `RealtaMagicMending` | 마법 땜질 | Common / Skill | `STS2WD-REALTA_MAGIC_MENDING` | `realta_magic_mending.png` | `1000 x 760` |
| 18 | `RealtaWellPrepared` | 준비만전 | Common / Skill | `STS2WD-REALTA_WELL_PREPARED` | `realta_well_prepared.png` | `1000 x 760` |
| 19 | `RealtaEquipmentCare` | 장비 손질 | Common / Skill | `STS2WD-REALTA_EQUIPMENT_CARE` | `realta_equipment_care.png` | `1000 x 760` |
| 20 | `RealtaBarrier` | 방어막 | Common / Skill | `STS2WD-REALTA_BARRIER` | `realta_barrier.png` | `1000 x 760` |
| 21 | `RealtaSnatch` | 물어채기 | Common / Skill | `STS2WD-REALTA_SNATCH` | `realta_snatch.png` | `1000 x 760` |
| 22 | `RealtaMyNameIs` | 내 이름은! | Common / Skill | `STS2WD-REALTA_MY_NAME_IS` | `realta_my_name_is.png` | `1000 x 760` |
| 23 | `RealtaFaerieFire` | 요정 불빛 | Common / Skill | `STS2WD-REALTA_FAERIE_FIRE` | `realta_faerie_fire.png` | `1000 x 760` |
| 24 | `RealtaFaerieBronze` | 요정의 청동 | Common / Skill | `STS2WD-REALTA_FAERIE_BRONZE` | `realta_faerie_bronze.png` | `1000 x 760` |
| 25 | `RealtaValor` | 용맹과감 | Uncommon / Attack | `STS2WD-REALTA_VALOR` | `realta_valor.png` | `1000 x 760` |
| 26 | `RealtaMagicWeapon` | 마법의 무기 | Uncommon / Attack | `STS2WD-REALTA_MAGIC_WEAPON` | `realta_magic_weapon.png` | `1000 x 760` |
| 27 | `RealtaOldChisel` | 오래된 끌 | Uncommon / Attack | `STS2WD-REALTA_OLD_CHISEL` | `realta_old_chisel.png` | `1000 x 760` |
| 28 | `RealtaDimensionalCharge` | 차원 돌격 | Uncommon / Attack | `STS2WD-REALTA_DIMENSIONAL_CHARGE` | `realta_dimensional_charge.png` | `1000 x 760` |
| 29 | `RealtaPincerAttack` | 협공 | Uncommon / Attack | `STS2WD-REALTA_PINCER_ATTACK` | `realta_pincer_attack.png` | `1000 x 760` |
| 30 | `RealtaDragonScale` | 용의 비늘 | Uncommon / Attack | `STS2WD-REALTA_DRAGON_SCALE` | `realta_dragon_scale.png` | `1000 x 760` |
| 31 | `RealtaSelfRepair` | 자가 수리 | Uncommon / Skill | `STS2WD-REALTA_SELF_REPAIR` | `realta_self_repair.png` | `1000 x 760` |
| 32 | `RealtaHammering` | 망치질 | Uncommon / Skill | `STS2WD-REALTA_HAMMERING` | `realta_hammering.png` | `1000 x 760` |
| 33 | `RealtaAnnealing` | 뜨임 | Uncommon / Skill | `STS2WD-REALTA_ANNEALING` | `realta_annealing.png` | `1000 x 760` |
| 34 | `RealtaMending` | 수선 | Uncommon / Skill | `STS2WD-REALTA_MENDING` | `realta_mending.png` | `1000 x 760` |
| 35 | `RealtaKnightsOath` | 기사의 맹세 | Uncommon / Skill | `STS2WD-REALTA_KNIGHTS_OATH` | `realta_knights_oath.png` | `1000 x 760` |
| 36 | `RealtaChivalry` | 기사도 | Uncommon / Skill | `STS2WD-REALTA_CHIVALRY` | `realta_chivalry.png` | `1000 x 760` |
| 37 | `RealtaSpatialWarp` | 공간 왜곡 | Uncommon / Skill | `STS2WD-REALTA_SPATIAL_WARP` | `realta_spatial_warp.png` | `1000 x 760` |
| 38 | `RealtaSmelting` | 제련 | Uncommon / Skill | `STS2WD-REALTA_SMELTING` | `realta_smelting.png` | `1000 x 760` |
| 39 | `RealtaDamageImmunity` | 파손 면역 | Uncommon / Skill | `STS2WD-REALTA_DAMAGE_IMMUNITY` | `realta_damage_immunity.png` | `1000 x 760` |
| 40 | `RealtaAuraOfVigor` | 활력의 오라 | Uncommon / Skill | `STS2WD-REALTA_AURA_OF_VIGOR` | `realta_aura_of_vigor.png` | `1000 x 760` |
| 41 | `RealtaFathersAdvice` | 아버지의 조언 | Uncommon / Power | `STS2WD-REALTA_FATHERS_ADVICE` | `realta_fathers_advice.png` | `1000 x 760` |
| 42 | `RealtaFirstDuel` | 첫번째 결투 | Uncommon / Power | `STS2WD-REALTA_FIRST_DUEL` | `realta_first_duel.png` | `1000 x 760` |
| 43 | `RealtaWitchsSecretArt` | 마녀의 비술 | Uncommon / Power | `STS2WD-REALTA_WITCHS_SECRET_ART` | `realta_witchs_secret_art.png` | `1000 x 760` |
| 44 | `RealtaJousting` | 마상 창시합 | Uncommon / Power | `STS2WD-REALTA_JOUSTING` | `realta_jousting.png` | `1000 x 760` |
| 45 | `RealtaOldWorkshop` | 낡은 작업소 | Uncommon / Power | `STS2WD-REALTA_OLD_WORKSHOP` | `realta_old_workshop.png` | `1000 x 760` |
| 46 | `RealtaWorthyRival` | 호적수 | Uncommon / Power | `STS2WD-REALTA_WORTHY_RIVAL` | `realta_worthy_rival.png` | `1000 x 760` |
| 47 | `RealtaHerosArmor` | 영웅의 갑주 | Uncommon / Power | `STS2WD-REALTA_HEROS_ARMOR` | `realta_heros_armor.png` | `1000 x 760` |
| 48 | `RealtaForgeHeat` | 대장간의 열기 | Uncommon / Power | `STS2WD-REALTA_FORGE_HEAT` | `realta_forge_heat.png` | `1000 x 760` |
| 49 | `RealtaLuckyNecklace` | 행운의 목걸이 | Uncommon / Power | `STS2WD-REALTA_LUCKY_NECKLACE` | `realta_lucky_necklace.png` | `1000 x 760` |
| 50 | `RealtaGreatFlame` | 위대한 불꽃 | Rare / Attack | `STS2WD-REALTA_GREAT_FLAME` | `realta_great_flame.png` | `1000 x 760` |
| 51 | `RealtaFireDragonsBlood` | 화룡의 피 | Rare / Attack | `STS2WD-REALTA_FIRE_DRAGONS_BLOOD` | `realta_fire_dragons_blood.png` | `1000 x 760` |
| 52 | `RealtaMoteOfFlame` | 불꽃의 티끌 | Rare / Attack | `STS2WD-REALTA_MOTE_OF_FLAME` | `realta_mote_of_flame.png` | `1000 x 760` |
| 53 | `RealtaMountainSplitter` | 산 쪼개기 | Rare / Attack | `STS2WD-REALTA_MOUNTAIN_SPLITTER` | `realta_mountain_splitter.png` | `1000 x 760` |
| 54 | `RealtaForceMaw` | 역장 아가리 | Rare / Attack | `STS2WD-REALTA_FORCE_MAW` | `realta_force_maw.png` | `1000 x 760` |
| 55 | `RealtaRinnalBodbDearg` | 린날 보브데그 | Rare / Skill | `STS2WD-REALTA_RINNAL_BODB_DEARG` | `realta_rinnal_bodb_dearg.png` | `1000 x 760` |
| 56 | `RealtaFinestKnight` | 최고의 기사 | Rare / Skill | `STS2WD-REALTA_FINEST_KNIGHT` | `realta_finest_knight.png` | `1000 x 760` |
| 57 | `RealtaBlessingOfFire` | 불의 가호 | Rare / Skill | `STS2WD-REALTA_BLESSING_OF_FIRE` | `realta_blessing_of_fire.png` | `1000 x 760` |
| 58 | `RealtaDragonWings` | 용의 날개 | Rare / Skill | `STS2WD-REALTA_DRAGON_WINGS` | `realta_dragon_wings.png` | `1000 x 760` |
| 59 | `RealtaUndyingLoyalty` | 불멸의 충성 | Rare / Skill | `STS2WD-REALTA_UNDYING_LOYALTY` | `realta_undying_loyalty.png` | `1000 x 760` |
| 60 | `RealtaBoundaryReforging` | 경계 재련 | Rare / Skill | `STS2WD-REALTA_BOUNDARY_REFORGING` | `realta_boundary_reforging.png` | `1000 x 760` |
| 61 | `RealtaDragonsFootsteps` | 용의 발자취 | Rare / Power | `STS2WD-REALTA_DRAGONS_FOOTSTEPS` | `realta_dragons_footsteps.png` | `1000 x 760` |
| 62 | `RealtaPrimalForm` | 원초의 형상 | Rare / Power | `STS2WD-REALTA_PRIMAL_FORM` | `realta_primal_form.png` | `1000 x 760` |
| 63 | `RealtaGrannsBlessing` | 그란의 축복 | Rare / Power | `STS2WD-REALTA_GRANNS_BLESSING` | `realta_granns_blessing.png` | `1000 x 760` |
| 64 | `RealtaGoibniusForging` | 고브누의 마법 재련 | Rare / Power | `STS2WD-REALTA_GOIBNIUS_FORGING` | `realta_goibnius_forging.png` | `1000 x 760` |
| 65 | `RealtaFaerieArtisan` | 요정 장인 | Rare / Power | `STS2WD-REALTA_FAERIE_ARTISAN` | `realta_faerie_artisan.png` | `1000 x 760` |
| 66 | `RealtaIndomitable` | 칠전팔기 | Rare / Power | `STS2WD-REALTA_INDOMITABLE` | `realta_indomitable.png` | `1000 x 760` |
| 67 | `RealtaAgdridsAnvil` | 아그드리드의 모루 | Rare / Power | `STS2WD-REALTA_AGDRIDS_ANVIL` | `realta_agdrids_anvil.png` | `1000 x 760` |
| 68 | `RealtaSteelPredator` | 강철 포식자 | Rare / Power | `STS2WD-REALTA_STEEL_PREDATOR` | `realta_steel_predator.png` | `1000 x 760` |
| 69 | `RealtaIordinaMacBethlehem` | 요르디나 막 들레헴 | **Ancient** / Attack | `STS2WD-REALTA_IORDINA_MAC_BETHLEHEM` | `realta_iordina_mac_bethlehem.png` | **`606 x 852`** |
| 70 | `RealtaGeniusInspiration` | 천재의 영감 | **Ancient** / Power | `STS2WD-REALTA_GENIUS_INSPIRATION` | `realta_genius_inspiration.png` | **`606 x 852`** |

#### 2.2.4 Additional Eleven Cards and Current Portrait Sources

The cards below currently override `PortraitPath`, `CustomPortraitPath`, and `BetaPortraitPath` to reuse the listed Realta card's existing placeholder artwork. No new illustration PNGs were created for this implementation. When dedicated art is delivered, use the intended filename and restore the normal `Sts2WDCard` path resolution for that card. All eleven use the standard `1000 x 760` composition.

| # | C# class | Korean card name | Rarity / type | Intended filename | Current portrait source |
|---|---|---|---|---|---|
| 71 | `RealtaHeroicCharge` | 영웅적인 돌격! | Common / Attack | `realta_heroic_charge.png` | `RealtaFlyingCharge` |
| 72 | `RealtaMaterialAnalysis` | 재료 분석 | Common / Skill | `realta_material_analysis.png` | `RealtaMagicMending` |
| 73 | `RealtaImprovisedWeapon` | 급조 무기 | Uncommon / Attack | `realta_improvised_weapon.png` | `RealtaMagicWeapon` |
| 74 | `RealtaHeroism` | 영웅심 | Uncommon / Skill | `realta_heroism.png` | `RealtaKnightsOath` |
| 75 | `RealtaMasterpiece` | 자신작 | Uncommon / Skill | `realta_masterpiece.png` | `RealtaAnnealing` |
| 76 | `RealtaMakeshift` | 임시변통 | Uncommon / Skill | `realta_makeshift.png` | `RealtaSmelting` |
| 77 | `RealtaMajestic` | 위풍당당 | Uncommon / Skill | `realta_majestic.png` | `RealtaDamageImmunity` |
| 78 | `RealtaPerfectDay` | 완벽한 날 | Uncommon / Power | `realta_perfect_day.png` | `RealtaForgeHeat` |
| 79 | `RealtaPortableForge` | 이동식 대장간 | Uncommon / Power | `realta_portable_forge.png` | `RealtaAgdridsAnvil` |
| 80 | `RealtaAlmenelsBulwark` | 알메넬의 철벽 | Rare / Power | `realta_almenels_bulwark.png` | `RealtaMending` |
| 81 | `RealtaHerosSilhouette` | 영웅의 뒷모습 | Rare / Power | `realta_heros_silhouette.png` | `RealtaHerosArmor` |

---

### 2.3 Relic Icons (`RelicModel`, `CustomRelicModel`, `Sts2WDRelic`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.RelicModel`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomRelicModel`, `BaseLib.Patches.UI.RelicImageOverridePatch`, `BaseLib.Patches.UI.RelicIconData`)
  - `Sts2WDCode/Relics/Sts2WDRelic.cs` (lines `10-15`)
  - `Sts2WDCode/Characters/Realta/Relics/RealtaRelic.cs` (lines `6-7`)

#### 2.3.1 Relic Icon Properties and Resolutions

`RelicModel` declares three path properties as `virtual`; `Sts2WDRelic` overrides them directly:

1. **`public virtual string PackedIconPath`** (`RelicModel.cs:130`, `Sts2WDRelic.cs:12`):
   - Loaded by `RelicModel.Icon` (`ResourceLoader.Load<Texture2D>(PackedIconPath)`).
   - Small icon for the top relic bar, shop, rewards, and character-selection screen.
   - **Resolution:** **`94 x 94`** (`Format32bppArgb`, RGBA PNG).
   - Current path: `Sts2WD/images/relics/<relic_name>.png` (fallback: `Sts2WD/images/relics/relic.png`).
2. **`protected virtual string PackedIconOutlinePath`** (`RelicModel.cs:132`, `Sts2WDRelic.cs:13`):
   - Loaded by `RelicModel.IconOutline` (`ResourceLoader.Load<Texture2D>(PackedIconOutlinePath)`).
   - Outline for relic hover/highlight and compendium display.
   - **Resolution:** **`94 x 94`** (`Format32bppArgb`, RGBA PNG).
   - Current path: `Sts2WD/images/relics/<relic_name>_outline.png`.
   - **Proposed code improvement (`StringExtensions.cs:49-56`):** `Sts2WDRelic.PackedIconOutlinePath` currently calls `RelicImagePath()`, so a missing `<relic_name>_outline.png` falls back to `relic.png` instead of `relic_outline.png`. A separate `RelicOutlineImagePath` in `StringExtensions.cs` would supply the appropriate `relic_outline.png` fallback.
3. **`protected virtual string BigIconPath`** (`RelicModel.cs:134, 148-170`, `Sts2WDRelic.cs:14`):
   - Loaded by `RelicModel.BigIcon` (`PreloadManager.Cache.GetTexture2D(ResolvedBigIconPath)`).
   - High-resolution icon for relic inspection popups and detail views.
   - **Resolution:** **`256 x 256`** (`Format32bppArgb`, RGBA PNG).
   - Current path: `Sts2WD/images/relics/big/<relic_name>.png` (fallback: `Sts2WD/images/relics/big/relic.png`).

#### 2.3.2 Filename Mapping for Five Realta Relics

| # | C# class (`Sts2WDCode/Characters/Realta/Relics/`) | Korean relic name | Rarity | `ModelId.Entry` | Small icon (`94 x 94`) | Outline icon (`94 x 94`) | Large icon (`256 x 256`) |
|---|---|---|---|---|---|---|---|
| 1 | `RealtaBronzeWatchdog` | 청동의 번견 | Starter | `STS2WD-REALTA_BRONZE_WATCHDOG` | `Sts2WD/images/relics/realta_bronze_watchdog.png` | `Sts2WD/images/relics/realta_bronze_watchdog_outline.png` | `Sts2WD/images/relics/big/realta_bronze_watchdog.png` |
| 2 | `RealtaKuu` | 쿠우 | Starter (upgraded) | `STS2WD-REALTA_KUU` | `Sts2WD/images/relics/realta_kuu.png` | `Sts2WD/images/relics/realta_kuu_outline.png` | `Sts2WD/images/relics/big/realta_kuu.png` |
| 3 | `RealtaWitchPointedHat` | 마녀 고깔모자 | Common | `STS2WD-REALTA_WITCH_POINTED_HAT` | `Sts2WD/images/relics/realta_witch_pointed_hat.png` | `Sts2WD/images/relics/realta_witch_pointed_hat_outline.png` | `Sts2WD/images/relics/big/realta_witch_pointed_hat.png` |
| 4 | `RealtaDragonCloak` | 용의 망토 | Uncommon | `STS2WD-REALTA_DRAGON_CLOAK` | `Sts2WD/images/relics/realta_dragon_cloak.png` | `Sts2WD/images/relics/realta_dragon_cloak_outline.png` | `Sts2WD/images/relics/big/realta_dragon_cloak.png` |
| 5 | `RealtaEldirGramr` | 엘디르 그라므르 | Rare | `STS2WD-REALTA_ELDIR_GRAMR` | `Sts2WD/images/relics/realta_eldir_gramr.png` | `Sts2WD/images/relics/realta_eldir_gramr_outline.png` | `Sts2WD/images/relics/big/realta_eldir_gramr.png` |

---

### 2.4 Power Icons (`PowerModel`, `CustomPowerModel`, `Sts2WDPower`)

- **Primary sources:**
  - `sts2.dll: MegaCrit.Sts2.Core.Models.PowerModel`
  - `BaseLib.dll` (`BaseLib.Abstracts.CustomPowerModel`, `BaseLib.Abstracts.ICustomPower`, `BaseLib.Abstracts.PackedIconPath`, `BaseLib.Abstracts.BigIconPath`, `BaseLib.Abstracts.BigBetaIconPath`)
  - `Sts2WDCode/Powers/Sts2WDPower.cs` (lines `14-31`)
  - `Sts2WDCode/Characters/Realta/Powers/*.cs` (22 classes)

#### 2.4.1 Power Icon Properties and Resolutions

`PowerModel.PackedIconPath`, `BigIconPath`, and `BigBetaIconPath` are non-virtual. BaseLib's Harmony Prefix patches (`BaseLib.Abstracts.PackedIconPath`, `BigIconPath`, `BigBetaIconPath`) replace them with the following virtual properties of `ICustomPower` (`CustomPowerModel`):

1. **`public virtual string? CustomPackedIconPath => null;`** (`CustomPowerModel.cs`, `Sts2WDPower.cs:17`):
   - Loaded by `PowerModel.Icon` (`ResourceLoader.Load<Texture2D>(PackedIconPath)`).
   - Small icon rendered in the power strip below the character/enemy health bar during combat.
   - **Resolution:** **`64 x 64`** (`Format32bppArgb`, RGBA PNG).
   - `Sts2WDPower` path: `Sts2WD/images/powers/<power_name>.png` (fallback: `Sts2WD/images/powers/power.png`).
2. **`public virtual string? CustomBigIconPath => null;`** (`CustomPowerModel.cs`, `Sts2WDPower.cs:18`):
   - Loaded by `PowerModel.BigIcon` (`PreloadManager.Cache.GetTexture2D(ResolvedBigIconPath)`). `ResolvedBigIconPath` is also cached during `ModelDb.Preload()` (`ModelDb.cs:462`).
   - High-resolution icon for hover tips and power-application VFX.
   - **Resolution:** **`256 x 256`** (`Format32bppArgb`, RGBA PNG).
   - `Sts2WDPower` path: `Sts2WD/images/powers/big/<power_name>.png` (fallback: `Sts2WD/images/powers/big/power.png`).
3. **`public virtual string? CustomBigBetaIconPath => null;`** (`CustomPowerModel.cs`):
   - Secondary beta-icon path checked by `PowerModel.ResolvedBigIconPath` (`PowerModel.cs:118-140`) when `ResourceLoader.Exists` cannot find the resource specified by `CustomBigIconPath`.

#### 2.4.2 Why 22 Power Cards Produce 22 `PowerModel` Classes, and How to Migrate Icons

- **Reason for the count difference:**
  - Realta has **22 cards of `CardType.Power`**.
  - **`RealtaAgdridsAnvil` (`아그드리드의 모루`)** immediately upgrades/improves the draw pile, and **`RealtaAlmenelsBulwark` (`알메넬의 철벽`)** immediately grants existing Plating. Neither needs a corresponding custom `PowerModel` (`22 - 2 = 20`).
  - The Skill cards **`RealtaUndyingLoyalty` (`불멸의 충성`)** and **`RealtaWellPrepared` (`준비만전`)** add two models for persistent/delayed effects: **`RealtaUndyingLoyaltyPower`** and **`RealtaVigorNextTurnPower` (displayed as `준비만전`)**, respectively (`20 + 2 = 22`).
  - `Sts2WDCode/Characters/Realta/Powers/` therefore contains **22 `PowerModel` classes**, all visible buffs (`IsVisibleInternal == true`).
- **Current resource ownership:** Twenty-one delivered custom powers inherit the existing Sts2WDPower base and resolve model-ID PNG filenames through the repository's image-path helpers. Only ForgeHeat retains its explicit native icon overrides. No extra fallback abstraction is needed.

#### 2.4.3 Filename and Original Native Icon References for 22 Realta `PowerModel` Classes

| # | C# class (`Sts2WDCode/Characters/Realta/Powers/`) | Korean power name | Granting card | Original native reference (active only for ForgeHeat) | Small icon (`64 x 64`) | Large icon (`256 x 256`) |
|---|---|---|---|---|---|---|
| 1 | `RealtaFathersAdvicePower` | 아버지의 조언 | `RealtaFathersAdvice` (Power) | `FurnacePower` | `Sts2WD/images/powers/realta_fathers_advice_power.png` | `Sts2WD/images/powers/big/realta_fathers_advice_power.png` |
| 2 | `RealtaFirstDuelPower` | 첫번째 결투 | `RealtaFirstDuel` (Power) | `StrengthPower` | `Sts2WD/images/powers/realta_first_duel_power.png` | `Sts2WD/images/powers/big/realta_first_duel_power.png` |
| 3 | `RealtaWitchsSecretArtPower` | 마녀의 비술 | `RealtaWitchsSecretArt` (Power) | `DrawCardsNextTurnPower` | `Sts2WD/images/powers/realta_witchs_secret_art_power.png` | `Sts2WD/images/powers/big/realta_witchs_secret_art_power.png` |
| 4 | `RealtaJoustingPower` | 마상 창시합 | `RealtaJousting` (Power) | `DexterityPower` | `Sts2WD/images/powers/realta_jousting_power.png` | `Sts2WD/images/powers/big/realta_jousting_power.png` |
| 5 | `RealtaOldWorkshopPower` | 낡은 작업소 | `RealtaOldWorkshop` (Power) | `PlatingPower` | `Sts2WD/images/powers/realta_old_workshop_power.png` | `Sts2WD/images/powers/big/realta_old_workshop_power.png` |
| 6 | `RealtaWorthyRivalPower` | 호적수 | `RealtaWorthyRival` (Power) | `MachineLearningPower` | `Sts2WD/images/powers/realta_worthy_rival_power.png` | `Sts2WD/images/powers/big/realta_worthy_rival_power.png` |
| 7 | `RealtaHerosArmorPower` | 영웅의 갑주 | `RealtaHerosArmor` (Power) | `DexterityPower` | `Sts2WD/images/powers/realta_heros_armor_power.png` | `Sts2WD/images/powers/big/realta_heros_armor_power.png` |
| 8 | `RealtaForgeHeatPower` | 대장간의 열기 | `RealtaForgeHeat` (Power) | `VigorPower` | `Sts2WD/images/powers/realta_forge_heat_power.png` | `Sts2WD/images/powers/big/realta_forge_heat_power.png` |
| 9 | `RealtaLuckyNecklacePower` | 행운의 목걸이 | `RealtaLuckyNecklace` (Power) | `RoyaltiesPower` | `Sts2WD/images/powers/realta_lucky_necklace_power.png` | `Sts2WD/images/powers/big/realta_lucky_necklace_power.png` |
| 10 | `RealtaDragonsFootstepsPower` | 용의 발자취 | `RealtaDragonsFootsteps` (Power) | `FlameBarrierPower` | `Sts2WD/images/powers/realta_dragons_footsteps_power.png` | `Sts2WD/images/powers/big/realta_dragons_footsteps_power.png` |
| 11 | `RealtaPrimalFormPower` | 원초의 형상 | `RealtaPrimalForm` (Power) | `FurnacePower` | `Sts2WD/images/powers/realta_primal_form_power.png` | `Sts2WD/images/powers/big/realta_primal_form_power.png` |
| 12 | `RealtaGrannsBlessingPower` | 그란의 축복 | `RealtaGrannsBlessing` (Power) | `FurnacePower` | `Sts2WD/images/powers/realta_granns_blessing_power.png` | `Sts2WD/images/powers/big/realta_granns_blessing_power.png` |
| 13 | `RealtaGoibniusForgingPower` | 고브누의 마법 재련 | `RealtaGoibniusForging` (Power) | `FurnacePower` | `Sts2WD/images/powers/realta_goibnius_forging_power.png` | `Sts2WD/images/powers/big/realta_goibnius_forging_power.png` |
| 14 | `RealtaFaerieArtisanPower` | 요정 장인 | `RealtaFaerieArtisan` (Power) | `WellLaidPlansPower` | `Sts2WD/images/powers/realta_faerie_artisan_power.png` | `Sts2WD/images/powers/big/realta_faerie_artisan_power.png` |
| 15 | `RealtaIndomitablePower` | 칠전팔기 | `RealtaIndomitable` (Power) | `PlatingPower` | `Sts2WD/images/powers/realta_indomitable_power.png` (not `realta_industrial_power`) | `Sts2WD/images/powers/big/realta_indomitable_power.png` |
| 16 | `RealtaSteelPredatorPower` | 강철 포식자 | `RealtaSteelPredator` (Power) | `HammerTimePower` | `Sts2WD/images/powers/realta_steel_predator_power.png` | `Sts2WD/images/powers/big/realta_steel_predator_power.png` |
| 17 | `RealtaGeniusInspirationPower` | 천재의 영감 | `RealtaGeniusInspiration` (Ancient Power) | `ImprovementPower` | `Sts2WD/images/powers/realta_genius_inspiration_power.png` | `Sts2WD/images/powers/big/realta_genius_inspiration_power.png` |
| 18 | `RealtaUndyingLoyaltyPower` | 불멸의 충성 | `RealtaUndyingLoyalty` (Skill) | `OneTwoPunchPower` | `Sts2WD/images/powers/realta_undying_loyalty_power.png` | `Sts2WD/images/powers/big/realta_undying_loyalty_power.png` |
| 19 | `RealtaVigorNextTurnPower` | 준비만전 | `RealtaWellPrepared` (Skill) | `VigorPower` | `Sts2WD/images/powers/realta_vigor_next_turn_power.png` | `Sts2WD/images/powers/big/realta_vigor_next_turn_power.png` |
| 20 | `RealtaPerfectDayPower` | 완벽한 날 | `RealtaPerfectDay` (Power) | `StrengthPower` | `Sts2WD/images/powers/realta_perfect_day_power.png` | `Sts2WD/images/powers/big/realta_perfect_day_power.png` |
| 21 | `RealtaPortableForgePower` | 이동식 대장간 | `RealtaPortableForge` (Power) | `PlatingPower` | `Sts2WD/images/powers/realta_portable_forge_power.png` | `Sts2WD/images/powers/big/realta_portable_forge_power.png` |
| 22 | `RealtaHerosSilhouettePower` | 영웅의 뒷모습 | `RealtaHerosSilhouette` (Power) | `MasterPlannerPower` | `Sts2WD/images/powers/realta_heros_silhouette_power.png` | `Sts2WD/images/powers/big/realta_heros_silhouette_power.png` |

---

## 3. Power Display Titles and Runtime Icon Status

`RealtaPowerPresentation` changes presentation while retaining native power instances, stacking, audio, and turn hooks. On Realta, applied `BlurPower` uses the canonical `RealtaRinnalBodbDearg` card title and `ReflectPower` uses `RealtaSpatialWarp`. The native power model does not preserve the granting card, so these aliases apply to those powers on Realta regardless of source; other characters keep native Blur/Reflect titles. The next-turn Vigor timer takes `RealtaWellPrepared.TitleLocString`. Its independent `DrawCardsNextTurnPower` keeps its shared functional label because other effects also use it.

`FlameBarrierPower.Title` uses the mod-localized 화염 / Flame label for both applied powers and canonical hover tips, including on other characters. The original Flame Barrier card title is unchanged. Other dedicated buff titles already agree with their granting cards; Vigor, Plating, and other shared generic powers keep their existing names.

The user approved runtime application of all 23 generated power icons on 2026-10-20. Complete compositions were proportionally converted with Fit to 256px/64px RGBA PNGs. Twenty-one custom powers reuse `Sts2WDPower`'s existing model-ID resource mapping. Native `FlameBarrierPower` uses `realta_flame_power.png` (small and `big/`), applied `BlurPower` on Realta uses `realta_rinnal_bodb_dearg_power.png`, and applied `ReflectPower` on Realta uses `realta_spatial_warp_power.png`. The prepared anvil/clock concept is mapped to `realta_vigor_next_turn_power.png`. Other characters keep native Blur/Reflect titles and art; Flame uses the renamed title and dedicated art globally, including canonical hover tips. Native power icon routing overrides `ResolvedBigIconPath` before its inherited canonical cache, so the owning character's tooltip and application VFX agree with the small icon. No power behavior or stacking was forked.

The 19 named user motif references and complete title-to-motif mapping are preserved in [refs/powers](refs/powers/README.md). Runtime artwork incorporates the requested follow-up revisions: 요정 장인 uses green leaf wings and a rectangular bronze hammer without the eye-like bead or circular wing spots; 강철 포식자 shows a visibly snapped blade and steel fragments against a straight, intact wall. Their final PNGs occupy the unsuffixed runtime model filenames, so neither a local draft nor a versioned generation file is needed by the game. 불멸의 충성 uses an exact horizontal reflection of 원초의 형상: flip each complete 64px/256px source PNG along its horizontal axis into the corresponding realta_undying_loyalty_power.png path, retaining size, colors and alpha without resampling. This temporary reuse was explicitly selected by the user. Only 대장간의 열기 retains native art. The table's original vanilla references are historical sources for the twenty-one migrated custom powers and an active fallback only for ForgeHeat.

## 4. Follow-Up Work Candidates

These are candidates after the current delivery, not an automatic execution sequence. Work only on the requested asset scope.

1. **UI, standing figure, and selection-scene revisions:** Section 0.1 describes the current delivery. If the base changes or the user requests more work, record the appearance/composition differences in the document and update only affected assets.
2. **Cards, relics, and powers:** Check actual file mappings and fallbacks in sections 2.2–2.4, then produce the requested items. An image-update request does not itself authorize changes to path helpers or gameplay behavior.
3. **Additional visuals:** The combat counter is connected through `RealtaEnergyCounterAppearance`. Combat, merchant, rest, and selection animations are connected as described in section 0.2. Multiplayer hands, card trails, and additional animations remain separate scope.

---

## 4. Complete Asset Specification Table

This table covers both current deliveries and future production targets. Section 0 owns current application status and production basis.

| Asset category | Recommended target path (`Sts2WD/...`) | Exact pixel dimensions (`W x H`) | Format / Godot import rules | C# class and property/method to override |
|---|---|---|---|---|
| **Card top-left energy icon** | `Sts2WD/images/charui/realta/big_energy.png` | `74 x 74` | RGBA PNG (`CompressedTexture2D`, Lossless, Mipmaps Off) | `RealtaCardPool.BigEnergyIconPath`<br>`RealtaRelicPool.BigEnergyIconPath`<br>`RealtaPotionPool.BigEnergyIconPath` |
| **Inline-text energy icon** | `Sts2WD/images/charui/realta/text_energy.png` | `24 x 24` | RGBA PNG (`CompressedTexture2D`, Lossless) | `RealtaCardPool.TextEnergyIconPath`<br>`RealtaRelicPool.TextEnergyIconPath`<br>`RealtaPotionPool.TextEnergyIconPath` |
| **Top-bar / statistics / run-history character icon** | `Sts2WD/images/charui/realta/character_icon.png` | `128 x 128` *(vanilla `85 x 85`)* | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomIconTexturePath` and `Realta.CustomIcon` |
| **Ancient-dialogue / multiplayer-vote character-icon outline** | `Sts2WD/images/charui/realta/character_icon_outline.png` | `128 x 128` *(vanilla `85 x 85`)* | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomIconOutlineTexturePath` |
| **Character-selection button icon** | `Sts2WD/images/charui/realta/char_select.png` | `132 x 195` | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomCharacterSelectIconPath` |
| **Locked character-selection icon** | `Sts2WD/images/charui/realta/char_select_locked.png` | `132 x 195` | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomCharacterSelectLockedIconPath` |
| **Map-marker icon** | `Sts2WD/images/charui/realta/map_marker.png` | `128 x 128` *(vanilla `49 x 64`)* | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomMapMarkerPath` |
| **Combat energy counter** | `Sts2WD/images/charui/realta/combat_energy.png` | `256 x 256` (UI display size `128 x 128`) | RGBA PNG (`CompressedTexture2D`, Lossless) | `RealtaEnergyCounterAppearance` replaces the native counter's first texture layer; native counter logic and VFX remain active |
| **Standard card portraits (79)** | `Sts2WD/images/card_portraits/realta/<card_name>.png` *(current default: `card_portraits/big/<card_name>.png`; newer-card aliases in section 2.2.4)* | **`1000 x 760`** *(smaller output: `500 x 380` / `250 x 190`, ratio `25:19`)* | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDCard.CustomPortraitPath` (and `PortraitPath`) |
| **Ancient full-art card portraits (two)** | `Sts2WD/images/card_portraits/realta/<card_name>.png` (`realta_iordina_mac_bethlehem.png`, `realta_genius_inspiration.png`) | **`606 x 852`** *(smaller output: `250 x 350`)* | RGB/RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDCard.CustomPortraitPath` (and `PortraitPath`) |
| **Applied shared card body** | `Sts2WD/images/cards/realta/frame.png` | `598 x 844` | RGBA PNG (`CompressedTexture2D`, Lossless) | `RealtaCardPool.CustomFrame(CustomCardModel)` or `CustomCardModel.CustomFrame` |
| **Small relic icons (five)** | `Sts2WD/images/relics/<relic_name>.png` | **`94 x 94`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDRelic.PackedIconPath` |
| **Relic-outline icons (five)** | `Sts2WD/images/relics/<relic_name>_outline.png` | **`94 x 94`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDRelic.PackedIconOutlinePath` |
| **Large relic icons (five)** | `Sts2WD/images/relics/big/<relic_name>.png` | **`256 x 256`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `Sts2WDRelic.BigIconPath` |
| **Small power icons (22)** | `Sts2WD/images/powers/<power_name>.png` | **`64 x 64`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `CustomPowerModel.CustomPackedIconPath` (`Sts2WDPower`) |
| **Large power icons (22)** | `Sts2WD/images/powers/big/<power_name>.png` | **`256 x 256`** | RGBA PNG (`CompressedTexture2D`, Lossless) | `CustomPowerModel.CustomBigIconPath` (`Sts2WDPower`) |
| **Character-selection background scene** | `Sts2WD/scenes/realta/char_select_bg_realta.tscn` (+ background PNG) | `1920 x 1080` (`Control` root scene) | Godot `.tscn` + RGBA PNG | `Realta.CustomCharacterSelectBg` |
| **Combat character visuals** | `Sts2WD/scenes/realta/combat.tscn` + `Sts2WD/animations/realta/combat/` | rig in 384×576 source px, displayed at `240 x 360`, Bounds `240 x 360` | Godot `.tscn` (`Node2D` root) with baked rig | `Realta.CustomVisualPath` |
| **Rest-site character scene** | `Sts2WD/scenes/realta/rest_site.tscn` + `Sts2WD/animations/realta/rest/` | 1254² painting at scale 0.4 (≈ 448 x 467 figure), Hitbox `390 x 480` | Godot `.tscn` (`Node2D` + `ControlRoot`) | `Realta.CustomRestSiteAnimPath` |
| **Merchant character scene** | `Sts2WD/scenes/realta/merchant.tscn` | same rig, displayed at `240 x 360` (`Node2D` root) | Godot `.tscn` (`Node2D` root) | `Realta.CustomMerchantAnimPath` |
| **Multiplayer-hand textures (four, optional)** | `Sts2WD/images/charui/realta/hand_{point,rock,paper,scissors}.png` | `422 x 1200` | RGBA PNG (`CompressedTexture2D`, Lossless) | `Realta.CustomArm{Pointing,Rock,Paper,Scissors}TexturePath` |

---

### 공격 궤적 범위 (2026-10-20)

공격 `Slash`는 기존 `slash-trail.png`를 유지하고, 240 Hz로 측정한 칼끝·칼날 윤곽을 사용하는 `Polygon2D`로 적용했다. `author_clips.py`, `build_combat_rig.gd`, `slash_reveal.gdshader`가 경로·메시·점진 표시를 담당한다. 전투와 상점 리그에 함께 적용하며 상세 재현 절차는 `animation/README.md`에 있다.
