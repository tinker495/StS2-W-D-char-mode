# Realta animation dispatch and scene factories

This contract uses BaseLib 3.4.7+ and the native `libspine_godot` `SpineSprite` runtime adopted in [ADR-0008](/docs/adr/0008-native-spine-rigs-and-artifact-refresh.md). `Realta.CustomVisualPath`, `CustomMerchantAnimPath`, `CustomRestSiteAnimPath`, and `CustomCharacterSelectBg` return the game scenes listed in the [resource mapping](animation-assets.md#current-scene-and-property-connections). Compiled `rig-c` (Spine 4.3) packages are downgraded by [export_spine42.py](/scripts/export_spine42.py) to Spine 4.2 `.spjson`, `.spatlas`, and 4px-aligned `.png` atlases and mounted at runtime by [spine_rig_mount.gd](/Sts2WD/scenes/shared/spine_rig_mount.gd) before parent `_Ready()` callbacks inspect the node tree.

## Combat and merchant

`CustomCharacterModel.RegisterSceneConversions()` registers the preload paths. Because Godot's `_ready()` executes bottom-up (children before parent), [spine_rig_mount.gd](/Sts2WD/scenes/shared/spine_rig_mount.gd) replaces the editor-clean `Node2D` placeholder with a native `SpineSprite` (transferring `GroundShadow` and unique-name metadata) before `NCreatureVisuals._Ready()` or `NMerchantCharacter._Ready()` runs:

- **Combat ([combat.tscn](/Sts2WD/scenes/realta/combat.tscn), `mount_mode = "replace_self"`):** `NCreatureVisualsFactory` converts the root to `NCreatureVisuals` while retaining `%Visuals`, `Bounds`, `IntentPos`, and unique `CenterPos`, `OrbPos`, `TalkPos`, and `FormVfx`. `%Visuals` becomes a `SpineSprite` (`HasSpineAnimation == true`), so `NCreature` calls `Realta.GenerateAnimator(MegaSprite, Creature)` directly.
- **Merchant ([merchant.tscn](/Sts2WD/scenes/realta/merchant.tscn), `mount_mode = "replace_self"` + [SpineMerchantScenePatch.cs](/Sts2WDCode/Characters/SpineMerchantScenePatch.cs)):** `spine_rig_mount.gd` mounts `%Visuals` as `SpineSprite` inside `PackedScene.instantiate()`, and `SpineMerchantScenePatch` (`CustomCharacterModel.RegisterSceneConversions` postfix) unwraps `%Visuals` to `NMerchantCharacter.GetChild(0)` right after `NMerchantCharacterFactory` conversion so `new MegaSprite(GetChild(0))` binds directly to the skeleton while standalone scenes (`render_clip.gd`, rig tests) retain `%Visuals` on the wrapper.

`Realta.GenerateAnimator` registers these state transitions (omitting vanilla's unused `low_health_loop` so `CreatureAnimator` never warns on missing clips):

| Trigger | Spine Track 0 clip | Loop / next state | Notes |
|---|---|---|---|
| `Idle` | `idle_loop` | Looping | Track 1 simultaneously loops `blink_loop` (`3.60 s`) in combat and clears Track 1 whenever Track 0 switches to another clip. |
| `Relaxed` | `relaxed_loop` | Looping | Default merchant crouch loop (`8.00 s`). |
| `Attack` | `attack` | Returns to `idle_loop` | Two-handed lunge; `attack_hit` event at `0.54 s` (`Realta.AttackAnimDelay`). |
| `Guard` | `guard` | Returns to `idle_loop` | Diagonal polearm parry + arcane ward (`sigil_inner`/`sigil_core`) on `guard_brace` at `0.18 s`. |
| `Hit` | `hurt` | Returns to `idle_loop` | Recoil with hurt expression (`0.80 s`). |
| `Cast` / `PowerUp` | `cast` | Returns to `idle_loop` | Left-hand spell release (`cast_release` event at `0.25 s`, matching `CastAnimDelay`). |
| `Dead` | `die` | Non-looping hold (`2.20 s`) | Releases weapon at `0.12 s` (`weapon_release`); core body settles on the floor by `1.66 s` (`Realta.DeathAnimTime`). |

Defensive card plays route to `"Guard"` through [CardPlayAnimationPatch.cs](/Sts2WDCode/Cards/CardPlayAnimationPatch.cs) (`Hook.BeforeCardPlayed` postfix): `CardType.Skill` cards that grant Block (`GainsBlock`, `CardTag.Defend`, `DynamicVars["Block"]`, or `DynamicVars["Guard"]`) emit `SetAnimationTrigger("Guard")`, `CardType.Attack` cards keep native `"Attack"` priority, and [Sts2WDCard.cs](/Sts2WDCode/Cards/Sts2WDCard.cs) exposes `CustomPlayAnimTrigger` for explicit per-card overrides. [Realta.cs](/Sts2WDCode/Characters/Realta/Realta.cs) · [Authored clip timings](animation-sources.md#clips-and-dispatch)

## Character select, rest site, and combat UI

- **Character select ([char_select_bg_realta.tscn](/Sts2WD/scenes/realta/char_select_bg_realta.tscn), `mount_mode = "aspect_cover"`):** Keeps `Background` (`char_select_bg.png`) and mounts a `SpineSprite` overlay (`realta_select_atlas`) with `NSpineAutoPlayer` attached when running inside the game. The select skeleton exposes exactly one looping `"animation"` clip (`8.00 s`) to satisfy `NSpineAutoPlayer._Ready()`, dynamically matching `AspectRatioContainer` cover scaling across `16:9`, `16:10`, and `3:2`.
- **Rest site ([rest_site.tscn](/Sts2WD/scenes/realta/rest_site.tscn)):** Retains the 4-layer Stage-Inpaint seated mesh (`realta_rest.gd`) under `ControlRoot/Visuals`. The shared [rest-site lighting bridge](/Sts2WDCode/Characters/RestSiteLightingBridge.cs) dispatches `play_variant` (`overgrowth_loop`, `underdocks_loop`, `hive_loop`, `glory_loop`) with synchronized campfire flicker and `set_light_off` on `HideFlameGlow`.
- **Energy counter:** [RealtaEnergyCounterAppearance.cs](/Sts2WDCode/Characters/Realta/RealtaEnergyCounterAppearance.cs) customizes `NEnergyCounter` for Realta; see [Energy counter](ui-assets.md#lower-left-combat-energy-counter).
