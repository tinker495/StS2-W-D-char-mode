# Realta animation dispatch and scene factories

This contract uses BaseLib 3.4.7. `Realta.CustomVisualPath`, `CustomMerchantAnimPath` and `CustomRestSiteAnimPath` return the game scenes listed in the [resource mapping](animation-assets.md#current-scene-and-property-connections). The rig-c authoring source is compiled to a native Godot scene before packaging. The game presents the finished skeleton through `Visuals/Rig/AnimationPlayer`. [Runtime dispatch research](/docs/research/sts2-cutout-runtime-contract.md)

## Combat and merchant

`CustomCharacterModel.RegisterSceneConversions()` registers the preload paths. When `PackedScene.Instantiate` runs, `NCreatureVisualsFactory` converts the combat root and retains the `Visuals` child and its rig. Keep the root-level `Bounds`, `IntentPos` and the unique `CenterPos`, `OrbPos`, `TalkPos` and `FormVfx`. The merchant factory uses its own root conversion and looks for an idle animation in the retained children. Both scenes expose one primary native player. [Combat wrapper](/Sts2WD/scenes/realta/combat.tscn) · [Merchant wrapper](/Sts2WD/scenes/realta/merchant.tscn)

For a non-Spine character, `CustomAnimationPatch.SendTriggerToOtherAnimators` forwards `NCreature.SetAnimationTrigger` to `CustomAnimation.PlayCustomAnimation`. `CustomAnimation` searches for an `AnimationTree`, `AnimationPlayer` or `AnimatedSprite2D` in that order. The registered player supplies these names.

| Game event | Name resolved by BaseLib | Realta action |
|---|---|---|
| Idle | `idle` | Combat `idle_loop`; merchant `relaxed_loop`. |
| Attack | `attack` | Two-handed lunge with contact at 0.54 s. |
| Hit | `hurt` | Recoil with the hurt face. |
| Cast / PowerUp | `cast` / `powerup` | One cast clip, sigil release at 0.25 s. |
| Relaxed | `relaxed` | Crouching merchant loop. |
| Dead | `die` | The body reaches the floor at 1.66 s. The hair and clothing settle through the 2.20 s endpoint. |
| Revive | `revive` | 0.40 s pose fade and return to the variant's default loop. |

`CharacterModel.AttackAnimDelay` dates the game action relative to the trigger. Realta overrides it to 0.54 s. The inherited `CastAnimDelay` is 0.25 s. BaseLib's `AdjustTime` and `WaitCustomAnim` use `Realta.DeathAnimTime` (1.66 s) for the non-Spine death wait. The animation lengths and held endpoints are independent of those delays. [Realta.cs](/Sts2WDCode/Characters/Realta/Realta.cs) · [Authored clip timings](animation-sources.md#clips-and-dispatch)

## Other scene owners

The rest site uses its own seated figure under `ControlRoot/Visuals`, hitbox and thought-bubble anchors. `RealtaSceneBridge` dispatches `play_variant` (`overgrowth_loop`, `underdocks_loop`, `hive_loop`, `glory_loop`), `set_light_off` on `HideFlameGlow`, and rest game-over events. Character selection displays the full-screen `Illustration` in its own scene. The combat energy counter uses `RealtaEnergyCounterAppearance` against the native `NEnergyCounter`; its UI resource belongs to the energy-counter guide. [Scene connections](animation-assets.md#current-scene-and-property-connections) · [Energy counter](ui-assets.md#lower-left-combat-energy-counter)
