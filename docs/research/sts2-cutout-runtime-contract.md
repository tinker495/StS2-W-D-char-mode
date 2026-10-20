# Realta 컷아웃 애니메이션의 런타임 연결 계약

조사일: 2026-10-19. 이 문서는 소스·씬 리소스를 읽어 확인한 연결 계약과 적용 후보 설계를 구분한다. 런타임 파일, 테스트, 설치본은 변경하지 않았다. 독립 Godot 미리보기에서 움직였다는 사실은 게임 이벤트 연결의 검증이 아니다.

## 결론

Realta의 `Sprite2D`·`AnimationPlayer` 경로는 `MegaSprite`나 `CreatureAnimator`를 통과하지 않는다. BaseLib가 전투 트리거를 별도로 가로채 Godot 애니메이터에 전달한다. 따라서 공식 Spine의 클립 이름 여섯 개만 복제하면 `Idle`, `Relaxed`, `PowerUp`과 자동 idle 복귀가 빠진다. 휴식의 act 선택과 불빛 끄기는 현재 BaseLib의 비-Spine 대응 범위 밖이다.

최소 적용 후보는 **자식 `Visuals` 아래의 컷아웃 리그 + 하나의 주 `AnimationPlayer` + 이름 alias와 완료 전이 + 휴식 전용 이벤트 연결**이다. 기존 scene root에 프로토타입 스크립트를 붙이는 방식은 combat/rest의 BaseLib 변환 과정에서 루트가 사라지므로 피해야 한다. GDScript 자체를 금지하는 근거는 없다. 실제 게임의 선택 화면과 오디오에도 GDScript 리소스가 사용된다.

## 조사 대상과 근거 식별

| 대상 | 확인한 버전·식별자 | 재확인할 파일·멤버 |
|---|---|---|
| 설치 게임 | `v0.107.1`, commit `59260271`, 빌드 일시 `2026-06-18T15:43:56-07:00` | 설치 폴더 `release_info.json`, `sts2.dll`, `SlayTheSpire2.pck` |
| BaseLib | 프로젝트 의존성 `Alchyr.Sts2.BaseLib 3.4.7`; 조사 소스 commit `c070755d11ec062f6c97c4dda1a205d975ed066a` | [프로젝트 의존성](../../Sts2WD.csproj), [BaseLib 저장소](https://github.com/Alchyr/BaseLib-StS2/tree/c070755d11ec062f6c97c4dda1a205d975ed066a) |
| 렌더러·SDK | MegaDot `4.5.1.m.14.mono.custom_build`, `Godot.NET.Sdk/4.5.1`, `net9.0` | 프로젝트 SDK와 실행 로그 |
| 현재 모델 | `Realta : PlaceholderCharacterModel : CustomCharacterModel` | [Realta.cs](../../Sts2WDCode/Characters/Realta/Realta.cs) |

아래 게임 API의 namespace는 `MegaCrit.Sts2.Core` 기준이다. 로컬 분석 소스는 `.tools/sts2-decompiled/<namespace>/<type>.cs`, BaseLib 소스는 `.tools/BaseLib-StS2/`에 있었다. 이 캐시가 없어도 표의 버전, 타입, 멤버 및 PCK 리소스 경로로 같은 관찰을 재현할 수 있다. 설치된 BaseLib DLL의 바이너리와 조사 소스의 완전 일치까지 검증한 것은 아니므로 실제 패키지 로딩 시 의존성 버전도 다시 확인해야 한다.

## 전투: 서로 다른 두 실행 경로

공식 Spine 경로는 다음과 같다.

```text
NCreatureVisuals._Ready
  %Visuals.GetClass() == "SpineSprite"
  → MegaSprite(%Visuals) → SpineBody
NCreature._Ready
  HasSpineAnimation → CharacterModel.GenerateAnimator(SpineBody)
NCreature.SetAnimationTrigger(trigger)
  → CreatureAnimator.SetTrigger → AnimState → MegaAnimationState.SetAnimation
```

`Bindings.MegaSpine.MegaSpineBinding.ValidateBoundObject`는 실제 native 클래스 이름, 메서드, signal을 검사한다. `MegaSprite`는 `SpineSprite` 전용이며 `Sprite2D`·`AnimationPlayer`를 감싸는 범용 어댑터가 아니다. `Animation.CreatureAnimator`는 Spine animation signal에 연결하고, `AnimState.NextState`를 queue하며, loop에 속도·시점 변화를 준다. `CustomCharacterModel.SetupCustomAnimationStates(MegaSprite)` 역시 이 경로에만 적용된다.

Realta 같은 비-Spine 경로는 `BaseLib.Patches.UI.CustomAnimationPatch.SendTriggerToOtherAnimators`가 `NCreature.SetAnimationTrigger`의 prefix에서 실행된다. `HasSpineAnimation == false`일 때 `BaseLib.Utils.CustomAnimation.PlayCustomAnimation`을 호출한다. source: `Patches/UI/CustomAnimationPatch.cs`, `Utils/CustomAnimation.cs`.

| 게임 트리거 | 실제 검색 순서 | 공식 이름을 유지하는 후보의 대응 |
|---|---|---|
| `Idle` | `Idle`, `idle` | 같은 리소스를 `idle_loop`와 `idle`로 등록 |
| `Attack` | `Attack`, `attack` | `attack` 사용 |
| `Hit` | `Hit`, `Hurt`, `hit`, `hurt` | `hurt` 사용 |
| `Cast` | `Cast`, `cast` | `cast` 사용 |
| `PowerUp` | `PowerUp`, `powerup` | `cast` 리소스를 `powerup` alias로 등록 |
| `Relaxed` | `Relaxed`, `relaxed` | `relaxed_loop` 리소스를 `relaxed` alias로 등록 |
| `Dead` | `Dead`, `Die`, `dead`, `die` | `die` 사용 |
| 임시 부활 | `revive`, `Revive` | 별도 reset/부활 clip과 idle 복귀 |

`CustomAnimation`은 첫 발견한 소스를 캐시한다. 실제 우선순위는 직접/지정 경로 `AnimationTree`, 재귀 `AnimationTree`, 직접/지정 경로 `AnimationPlayer`, 직접/지정 경로 `AnimatedSprite2D`, 재귀 `AnimationPlayer`, 재귀 `AnimatedSprite2D`다. `AnimationTree`는 `AnimationNodeStateMachine` root만 지원하고 `parameters/playback`의 `Travel(name)`을 호출한다. 주 리그와 VFX에 플레이어를 무작정 추가하면 의도한 플레이어를 고르지 않을 수 있다.

`AnimationPlayer`가 발견되면 `HasAnimation(name)` 후 `Play(name)`만 한다. 현재 이름이 같으면 `Stop()`으로 처음부터 다시 재생한다. 기본 경로는 다음 clip 예약, idle 복귀, 루프 속도 변주를 추가하지 않는다. 또한 `PlayCustomAnimation`의 반환값은 **클립을 찾았는가가 아니라 커스텀 소스가 존재하는가**다. 소스가 있어도 이름이 없으면 원래 메서드로 되돌아가지 않으므로 누락된 alias는 조용한 무반응이 된다.

후보는 기본 `AnimationLibrary`에 이름을 노출하고 `attack`, `hurt`, `cast`, `powerup`, `revive` 완료 시 `idle_loop`로 전이시킨다. `die`에는 다음 애니메이션을 지정하지 않는다. 모든 상태에 초기 transform·visible 값과 `RESET`을 명시해 사망 자세가 공격에 남지 않도록 한다. Godot의 `animation_set_next`, `autoplay`, 기본 라이브러리의 이름 규칙으로 구현할 수 있다. 재생 완료 검증은 `seek(end)`만 사용하면 완료 signal이 발생하지 않으므로 실제 진행 또는 `advance`가 필요하다. [Godot 4.5 AnimationPlayer](https://docs.godotengine.org/en/4.5/classes/class_animationplayer.html)

사망 대기는 `CustomAnimationPatch.AdjustTime`과 `WaitCustomAnim`이 `CustomCharacterModel.DeathAnimTime`을 사용하며 상한은 5초, 기본은 1.5초다. 별도의 `WaitCustomAnim` 검색 목록은 `Dead`, `Die`, `die`여서 표의 일반 trigger 경로보다 하나 적다. 비-Spine `StartDeathAnim`은 자체 Spine 분기에서 `SetAnimationTrigger`를 호출하지 않으므로 소문자 `dead`만 등록해서는 이 사망 경로를 충족하지 못한다. 후보의 `die`는 두 경로에서 모두 유효하다. `NCreature.GetCurrentAnimationLength/TimeRemaining` 자체는 여전히 Spine 조회이며 비-Spine 길이 전달 패치는 이 조사 범위에서 발견하지 못했다. 긴 죽음 clip을 만들면서 기본 대기 1.5초를 그대로 두면 시각 연출과 화면 진행이 어긋날 수 있다.

`Commands.CreatureCmd.TriggerAnim`은 트리거 전달 후 `Cmd.CustomScaledWait(min(waitTime * .5, .25), waitTime)`을 기다린다. `AttackCommand`의 캐릭터 지연은 clip 전체 길이가 아니다. Realta의 상속 기본값은 `AttackAnimDelay=.15`, `CastAnimDelay=.25`다. 타격 포즈를 이 이벤트 시점과 대조하고 빠른 모드도 따로 관찰해야 한다.

## 씬 변환과 자식 리그 배치

`CustomCharacterModel.RegisterSceneConversions()`는 combat/rest/merchant 경로를 각각 `NCreatureVisuals`, `NRestSiteCharacter`, `NMerchantCharacter`로 등록한다. `NodeFactory.TryAutoConvert`는 `PackedScene.Instantiate` 이후 등록된 root를 변환한다. source: `Abstracts/CustomCharacterModel.cs`, `Utils/NodeFactories/NodeFactory.cs`.

`NodeFactory.TransferAndCreateNodes`에서 `FlexibleStructure == false`면 자식을 새 root에 이전하고 이전 root를 `QueueFree`한다. combat은 고정 경로 `Bounds`, `IntentPos`, rest는 `ControlRoot` 때문에 이 분기다. 원래 root script, custom 상태, root에 연결한 signal을 보존한다고 가정하면 안 된다. merchant factory는 named node 요구가 없어 원래 root를 자식으로 보존하는 분기다. 장면마다 변환 결과가 다르므로 자식 리그를 일관된 경계로 두는 편이 안전하다.

| 씬 | 현재 파일·구조 | 후보에서 유지할 계약 |
|---|---|---|
| 전투 | [combat.tscn](../../Sts2WD/scenes/realta/combat.tscn), `%Visuals`는 정적 Sprite2D | `%Visuals`를 Node2D 리그로 사용. root 직계 `Bounds`·`IntentPos`, unique `%CenterPos`·`%OrbPos`·`%TalkPos`·`%FormVfx` 유지 |
| 휴식 | [rest_site.tscn](../../Sts2WD/scenes/realta/rest_site.tscn), `ControlRoot/Visuals`와 `%Hitbox` | 리그를 `ControlRoot` 아래에 두어 공식 `FlipX`를 상속. reticle·thought anchor는 factory 생성 가능하나 최종 좌표 확인 |
| 상점 | [merchant.tscn](../../Sts2WD/scenes/realta/merchant.tscn), 정적 Sprite2D | 재귀로 찾을 단일 주 AnimationPlayer, relaxed autoplay, die 유지 |
| 선택 | [char_select_bg_realta.tscn](../../Sts2WD/scenes/realta/char_select_bg_realta.tscn), Control/TextureRect | root `Control`, mouse ignore, 화면 전체 레이아웃 유지. 별도 selection autoplay |

현재 combat/rest/merchant는 모두 `combat.png`를 참조하며 클립이 없다. 공식은 전투 준비, 상점 이완, 앉은 휴식을 각각 표현한다. 리그가 같아도 자세·anchor·별도 원화의 역할은 구분되어야 한다.

## 휴식·상점·게임오버의 별도 계약

`Nodes.RestSite.NRestSiteCharacter._Ready`는 `Player.RunState.CurrentActIndex` 0/1/2를 각각 `overgrowth_loop`, `hive_loop`, `glory_loop`에 대응시킨다. `GetChildSpineNodes()`가 수집하는 **직계 SpineSprite**에만 재생하고 시작 phase를 무작위화한다. `HideFlameGlow` 역시 그 목록에서 `_tracks/light_off`를 확인해 Spine track 1에 재생한다. BaseLib의 현재 패치에서 이를 AnimationPlayer로 전달하는 코드는 발견하지 못했다.

따라서 휴식 후보의 세 클립과 `set_light_off(true)` API가 존재하는 것만으로는 게임 연동이 끝나지 않는다. 적용 시에는 Realta에 한정한 `_Ready` 후처리에서 act clip을 선택하고, `HideFlameGlow` 후처리에서 별도의 glow 속성을 끄는 작은 C# 연결부가 필요하다. 지속 호흡과 불빛을 같은 주 플레이어에서 바꾸면 기존 loop가 끊기므로 불빛은 독립 속성·material·별도 제어 계층으로 유지한다. `AnimationMixer.animation_finished`는 loop에서 발생하지 않는 점도 고려한다. [Godot 4.5 AnimationMixer](https://docs.godotengine.org/en/4.5/classes/class_animationmixer.html)

`Nodes.Screens.Shops.NMerchantCharacter.PlayAnimation`에는 `MerchantCharacterAnimPatch`가 있다. factory에서 만든 객체에 한해 `relaxed_loop` 요청을 `idle`, `Idle`, `relaxed_loop` 순서로, `die` 요청을 `Die`, `die` 순서로 검색한다. `loop` 인자는 커스텀 핸들러로 전달하지 않으므로 Animation 리소스의 loop 설정이 실제 동작을 결정한다. `_Ready`는 커스텀 플레이어가 있으면 생략된다. 시작을 보장하려면 후보 자체의 `autoplay="relaxed_loop"`가 필요하다. 이름 `idle`도 함께 있으면 상점 요청에서는 그 이름이 우선하므로 relaxed에 대응시켜야 한다.

게임오버에는 두 다른 경로가 있다. `Nodes.Screens.GameOverScreen.NGameOverScreen.MoveCreaturesToDifferentLayerAndDisableUi`는 상점에서 `NMerchantCharacter.PlayAnimation("die")` 후 reparent하므로 위 패치가 적용된다. 휴식에서는 **새 combat visuals를 생성**하고 `SpineAnimation.SetAnimation("die", false)`를 직접 호출한다. 비-Spine 후보에 대한 이 경로의 dispatch는 보완 대상이다. 상점 사망 검증만으로 휴식 게임오버까지 된다고 보고하면 안 된다.

## 선택 화면: 현재 그림과 공식 애니메이션의 차이

현재 Realta 선택 그림은 1920×1080의 완성된 한 장이다. 오른쪽에 얼굴과 갑옷, 중앙에 들린 글레이브와 작업 손, 왼쪽에 넓은 망토·광선이 있다. [선택 원화](../../Sts2WD/images/charui/realta/char_select_bg.png)를 직접 열어 확인했다. 씬에는 `AnimationPlayer`, `SpineSprite`, particle, script가 없고 `TextureRect.stretch_mode=6`을 사용한다. 즉 그림 안의 바람·마법 선은 움직임의 묘사이며 실제 시간 변화는 없다. 기존 Cover 배치의 다른 화면 비율 검증은 별도다.

설치 PCK의 `res://scenes/screens/char_select/char_select_bg_ironclad.tscn`, `..._silent.tscn`은 `SpineSprite` 아래 `Nodes.Animation.NSpineAutoPlayer`를 두고 별도의 `CPUParticles2D` 조명·재 입자를 배치한다. `NSpineAutoPlayer._Ready`는 부모 Spine이 준비되면 **정확히 하나인** animation 이름을 가져와 재생한다. 이 두 캐릭터에서는 이름이 `animation`이며 관찰한 길이는 Ironclad 5.333333초, Silent 11.666667초다. scene의 `preview_animation` 값을 런타임 autoplay의 근거로 삼으면 안 된다.

`NCharacterSelectScreen`은 배경을 `Instantiate<Control>`하여 컨테이너에 붙일 뿐 combat trigger를 보내지 않는다. 후보는 자신의 selection loop를 autoplay하면 되며 clip 이름을 `Idle`로 맞출 필요가 없다. 공식 `NCharacterSelectScreenBg.OnWindowChange`에는 종횡비에 따른 scale 코드가 있지만 조사한 두 선택 씬의 root에는 이 script가 없었다. 모든 공식 씬이 그 scale 동작을 사용한다고 일반화할 수 없다.

공식 Spine 단독 렌더에서 확인한 것은 미세한 본체 움직임이며, 전체 게임 화면의 입자 합성까지 재현한 것은 아니다. Realta의 개선 후보는 얼굴·손잡이 접촉을 고정한 채 모자 끝·망토·리본과 마법 광선을 분리 제어하고, 필요하면 배경 깊이를 보완하는 것이다. 단일 완성 그림을 분해하면 뒤의 배경이 존재하지 않으므로 분리 경계 뒤만 별도 보충해야 한다. 전신 스프라이트를 확대해서 선택 원화를 대체하는 방식은 현재 구도와 공식의 별도 hero composition에 맞지 않는다. 비교 맥락은 [공식 애니메이션 비교 연구](sts2-official-animation-comparison.md)에 기록했다.

## GDScript 프로토타입의 승격 범위

GDScript 사용 자체는 가능성을 배제할 이유가 없다. 설치 PCK의 위 선택 씬은 `res://scenes/backgrounds/little_light_script.gd`를 참조하고 해당 `.gdc`가 있다. `src/gdscript/audio_manager_proxy.gdc`, `music_controller_proxy.gdc`도 확인했다. Godot는 C#과 GDScript의 호출·signal 연결을 지원한다. 이는 특정 후보가 패키징되고 이벤트를 받는다는 보증과는 구분한다. [Godot 4.5 언어 간 호출](https://docs.godotengine.org/en/4.5/tutorials/scripting/cross_language_scripting.html)

이 저장소의 제약은 다음과 같다.

- [export_presets.cfg](../../export_presets.cfg)의 `all_resources` export는 `docs/*`, `artifacts/*`, `Sts2WDCode/*` 등을 제외한다. `drafts`의 `.gd`·PNG를 그대로 참조하는 후보는 배포 리소스가 아니다. 승인 후 리소스와 상대 경로를 `res://Sts2WD/...`로 옮겨야 한다. `Image.load_from_file`로 원본 PNG를 찾는 draft 로더도 export된 texture resource를 `load/preload`하는 방식으로 바꾸는 편이 안전하다.
- 같은 preset의 `script_export_mode=2`, `dotnet/include_scripts_content=false`는 GDScript 전면 금지 규칙이 아니다. `.NET` 스크립트 소스 포함 여부와 GDScript export는 서로 다른 설정이다.
- [Sts2WD.csproj](../../Sts2WD.csproj)는 `Sts2WD/**`의 C# 컴파일을 제외한다. C# controller를 자산 폴더에 넣어도 DLL에 포함되지 않는다. `docs`는 C# compile 제외 목록에 없으므로 임시 `.cs` 파일을 무심코 draft 아래 추가하지 않는다.
- [MainFile.Initialize](../../Sts2WDCode/MainFile.cs)는 현재 `ScriptManagerBridge.LookupScriptsInAssembly(assembly)`가 주석 처리되어 있다. 씬에 모드 C# script를 붙이는 설계라면 script 등록 경로를 함께 준비해야 한다. Harmony 연결 코드만 작성하는 경우와 씬 script를 연결하는 경우를 구별한다.
- `Modding.ModManager`는 DLL을 `AssemblyLoadContext.LoadFromAssemblyPath`로, PCK를 `ProjectSettings.LoadResourcePack`으로 읽는다. GDScript 파일 자체가 모드 초기화 DLL을 대체하지는 않는다. 게임 내부 타입에 대한 Harmony 후처리는 기존 C# 모듈에 두고 시각 계산은 하위 GDScript controller에 남길 수 있다.

Godot의 PCK 방식은 리소스·스크립트 묶음을 로드할 수 있지만 필요한 dependency와 로드 순서를 준비해야 한다. 독립 프로젝트가 가진 설정·autoload가 자동으로 게임에 통합된다고 가정하지 않는다. [Godot 4.5 PCK와 모드 배포](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_pcks.html)

## 적용 전 관찰 순서와 비용 관리

1. 독립 후보에서 실제 dispatch 이름을 순서대로 호출한다. 같은 공격 연속 호출, 공격 중 피격, 피격 중 사망, 부활, PowerUp, Relaxed→Idle을 기록한다. clip 재생 성공과 포즈 복구를 각각 확인한다.
2. BaseLib 변환 전후 노드 구조와 AnimationPlayer 검색 대상을 관찰한다. root script 보존을 가정하지 않는다. 이미지 생성 없이 alias·전이·노드 배치로 해결할 문제다.
3. 휴식 act 0/1/2와 불빛 끄기를 각각 호출하고 호흡 위상 유지·좌우 flip·thought anchor를 확인한다. 상점 relaxed 시작·게임오버 die, 휴식 게임오버의 새 combat visuals를 별개 항목으로 둔다.
4. 선택 장면은 실제 UI 가림, 16:9와 좁은 화면, 반복 진입·제거를 관찰한다. 공식 전체 장면의 입자와 본체 motion을 함께 비교한다.
5. 승인된 적용 범위에서만 저장소 publish/import/PCK 로딩을 진행하고 실제 게임 이벤트를 확인한다. 이 문서 작성에서는 빌드·설치·게임 조작을 하지 않았다.

이번 검토는 BaseLib·게임 소스 읽기, 기존 선택 원화 열람, 공식 PCK 두 선택 씬의 읽기 전용 구조 조사, Godot 4.5의 네 문서 비교로 수행했다. 이미지 생성 0회, runtime 편집 0회, 테스트 변경·실행 0회다. 외부 유사 구현에서 채택한 것은 native AnimationPlayer의 autoplay·완료 전이·언어 간 경계이며, 전체 리그를 Spine API처럼 가장하는 어댑터는 class 검사와 중복 state machine 비용 때문에 채택하지 않았다. 다음 검토에서는 사용한 외부 근거, 발견 결함, 채택/기각 기법, 변경한 모듈 수, 생성·렌더·패키징 횟수, 실제 해결 범위를 함께 기록한다. 이 관찰 절차는 사용자 소유 acceptance test나 공식 수준 인증을 대체하지 않는다.

## 적용 결과와 실제 BaseLib 관찰 (2026-10-19, v12)

위 계약을 따라 네 씬을 런타임에 적용했다. 구조와 재현은 [animation/README.md](../characters/realta/animation/README.md)에 있다. 전투·상점 rig는 `Visuals/Rig` 아래에 있고, 휴식 Act·`HideFlameGlow`·전투/상점 외 게임오버 `die`는 `Sts2WDCode/Characters/Realta/RealtaSceneBridge.cs`의 Harmony postfix가 연결한다.

`artifacts/realta-factory-observation/`의 독립 C# 호스트로 실제 `BaseLib.dll` 3.4.7(패키지 파일과 SHA256 일치)과 설치판 `sts2.dll`을 불러, 배포 PCK(`artifacts/mods/Sts2WD/Sts2WD.pck`)만 mount한 상태에서 관찰했다. `BaseLibMain.Initialize`는 호출하지 않았다.

| 항목 | 관찰 |
|---|---|
| `NodeFactory<NCreatureVisuals>.CreateFromScene` | 원래 Node2D root가 제거되고 이름만 이어받은 `NCreatureVisuals`가 됨. `Visuals/Rig/AnimationPlayer`와 모든 marker가 유지. `IntentPos`는 변환 중 unique가 됨. BaseLib `TransferAndCreateNodes`가 owner 불일치 경고 7건을 냄(동작 영향 없음) |
| 실제 `NCreatureVisuals._Ready` | 예외 없음. `HasSpineAnimation=False`, Bounds·Intent·CenterPos·Orb·Talk 모두 채워짐 |
| `CustomAnimation.PlayCustomAnimation` | Idle/Attack/Hit/Cast/PowerUp/Relaxed/Dead/revive 이름 목록 모두 의도한 clip 재생. 행동 clip은 끝난 뒤 `idle_loop`로 자동 복귀, 재호출은 처음부터 재시작, 공격 중 Hit는 끊고 재생, die는 마지막 자세 유지, revive는 alpha 1→0→1 후 idle. 없는 이름에도 `True`를 반환(소스 존재 여부)하므로 alias 누락은 조용한 무반응이 된다 |
| 상점 (`MerchantCharacterAnimPatch`만 적용) | flexible 분기로 원래 root를 자식으로 보존. 시작부터 `relaxed_loop` 무작위 위상 재생, `idle`=relaxed 리소스, `PlayAnimation("die")` 재생 |
| 휴식 변환 | mod PCK만으로는 `res://scenes/ui/selection_reticle.tscn`이 없어 실패. 게임 PCK와 게임 script 등록 시 성공. 생성된 `SelectionReticle`은 root 바로 아래(ControlRoot 밖)에 Hitbox 사각형으로 놓이므로, 반전 시 어긋나지 않게 Hitbox를 좌우 대칭(-195~195)으로 바꿨다 |

관찰 후 반영: BaseLib의 첫 `Idle` 요청이 idle을 0초부터 다시 시작하므로, rig는 행동 뒤가 아닌 loop 시작에서 위상을 무작위화한다.

여전히 확인하지 못한 것: 실제 `NCreature` 생성과 `CustomAnimationPatch`의 Harmony 진입(`SetAnimationTrigger`, `AnimDie`, `AnimTempRevive`), 실제 게임의 이벤트 타이밍(빠른 모드 포함), 휴식 `_Ready`와 브리지 postfix 실행, 게임오버 화면, 다인 배치. 이 관찰은 headless 더미 렌더러에서 수행했으며 화면 출력은 별도의 PCK 렌더 호스트로 확인했다.
