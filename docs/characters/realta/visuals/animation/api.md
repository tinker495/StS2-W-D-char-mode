# Character Scene Factory Integration (BaseLib 3.4.7)

[Asset index](../../realta-assets.md).

## Overriding Combat, Rest, Merchant, and Energy-Counter Visuals

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
