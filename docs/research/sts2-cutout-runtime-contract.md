# 비-Spine 컷아웃 애니메이션의 런타임 연결 계약

게임과 BaseLib의 비-Spine 애니메이션 호출 경로와 씬 변환 계약을 정리한다. 레아르타의 구체적인 씬 구성과 클립 타이밍은 [애니메이션 API](/docs/characters/realta/animation-api.md)와 [애니메이션 소스](/docs/characters/realta/animation-sources.md)가, 로컬 게임 설치 기록은 [공통 배포 상태](/docs/deployment-status.md)가 관리한다.

## 결론

BaseLib는 비-Spine 전투 트리거를 전투·상점의 `Visuals/Rig/AnimationPlayer`에 전달한다. 각 리그에서 BaseLib가 찾는 이름과 행동 이후의 대기 복귀를 설정해야 하며, 휴식의 막별 대기 동작·불빛과 비전투 게임오버 사망은 별도 브리지로 연결해야 한다. 레아르타의 적용 예시는 [애니메이션 API](/docs/characters/realta/animation-api.md)와 [애니메이션 소스](/docs/characters/realta/animation-sources.md)를 참고한다.

## 조사 대상과 근거 식별

| 대상 | 확인한 버전·식별자 | 재확인할 파일·멤버 |
|---|---|---|
| 설치 게임 | `v0.107.1`, commit `59260271`, 빌드 일시 `2026-06-18T15:43:56-07:00` (방별 스케일·접지선 계약은 `v0.113.1`, commit `d09d48e5` 기준 추가 확인) | 설치 폴더 `release_info.json`, `sts2.dll`, `SlayTheSpire2.pck` |
| BaseLib | 프로젝트 의존성 `Alchyr.Sts2.BaseLib 3.4.7`; 조사 소스 commit `c070755d11ec062f6c97c4dda1a205d975ed066a` | [프로젝트 의존성](/Sts2WD.csproj), [BaseLib 저장소](https://github.com/Alchyr/BaseLib-StS2/tree/c070755d11ec062f6c97c4dda1a205d975ed066a) |
| 렌더러·SDK | MegaDot `4.5.1.m.14.mono.custom_build`, `Godot.NET.Sdk/4.5.1`, `net9.0` | [프로젝트 SDK](/Sts2WD.csproj) |
| 기준 구현 예시 | `Realta : PlaceholderCharacterModel : CustomCharacterModel` | [Realta.cs](/Sts2WDCode/Characters/Realta/Realta.cs) |

아래 게임 API의 namespace는 `MegaCrit.Sts2.Core` 기준이다. 게임 동작은 설치판 `sts2.dll`에서, BaseLib 동작은 표에 명시한 소스 커밋에서 확인했다.

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

비-Spine 경로는 `BaseLib.Patches.UI.CustomAnimationPatch.SendTriggerToOtherAnimators`가 `NCreature.SetAnimationTrigger`의 prefix에서 실행된다. `HasSpineAnimation == false`일 때 `BaseLib.Utils.CustomAnimation.PlayCustomAnimation`을 호출한다. source: `Patches/UI/CustomAnimationPatch.cs`, `Utils/CustomAnimation.cs`.

| 게임 트리거 | BaseLib 검색 순서 | 비-Spine 전투 리그 대응 |
|---|---|---|
| `Idle` | `Idle`, `idle` | `idle_loop`와 같은 리소스를 `idle` alias로 등록 |
| `Attack` | `Attack`, `attack` | `attack` 사용 |
| `Hit` | `Hit`, `Hurt`, `hit`, `hurt` | `hurt` 사용 |
| `Cast` | `Cast`, `cast` | `cast` 사용 |
| `PowerUp` | `PowerUp`, `powerup` | `cast` 리소스를 `powerup` alias로 등록 |
| `Relaxed` | `Relaxed`, `relaxed` | `relaxed_loop` 리소스를 `relaxed` alias로 등록 |
| `Dead` | `Dead`, `Die`, `dead`, `die` | `die` 사용 |
| `AnimTempRevive` | `revive`, `Revive` | `revive` 후 `idle_loop` 복귀 |

`CustomAnimation`은 첫 발견한 소스를 캐시한다. 실제 우선순위는 직접/지정 경로 `AnimationTree`, 재귀 `AnimationTree`, 직접/지정 경로 `AnimationPlayer`, 직접/지정 경로 `AnimatedSprite2D`, 재귀 `AnimationPlayer`, 재귀 `AnimatedSprite2D`다. `AnimationTree`는 `AnimationNodeStateMachine` root만 지원하고 `parameters/playback`의 `Travel(name)`을 호출한다. 주 리그와 VFX에 플레이어를 무작정 추가하면 의도한 플레이어를 고르지 않을 수 있다.

`AnimationPlayer`가 발견되면 `HasAnimation(name)` 후 `Play(name)`만 한다. 현재 이름이 같으면 `Stop()`으로 처음부터 다시 재생한다. 기본 경로는 다음 clip 예약, idle 복귀, 루프 속도 변주를 추가하지 않는다. 또한 `PlayCustomAnimation`의 반환값은 **클립을 찾았는가가 아니라 커스텀 소스가 존재하는가**다. 소스가 있어도 이름이 없으면 원래 메서드로 되돌아가지 않으므로 누락된 alias는 조용한 무반응이 된다.

따라서 비-Spine 전투 리그는 기본 `AnimationLibrary`에 alias를 등록하고, 일회성 행동(`attack`, `hurt`, `cast`, `powerup`, `revive`) 완료 뒤 대기 클립 복귀를 명시해야 한다(`die`는 마지막 자세 정지). `seek(end)`는 완료 signal을 발생시키지 않으므로 전이 확인에는 실제 재생 진행이나 `advance`를 사용한다. [Godot 4.5 AnimationPlayer](https://docs.godotengine.org/en/4.5/classes/class_animationplayer.html) · [레아르타 클립 계약](/docs/characters/realta/animation-sources.md#clips-and-dispatch)

사망 대기는 `CustomAnimationPatch.AdjustTime`과 `WaitCustomAnim`이 `CustomCharacterModel.DeathAnimTime`을 사용한다. 상한은 5초, 기본은 1.5초다. `WaitCustomAnim`은 `Dead`, `Die`, `die`를 검색한다. 캐릭터별 `DeathAnimTime`과 클립 길이는 각 캐릭터의 [애니메이션 API](/docs/characters/realta/animation-api.md) 및 [애니메이션 소스](/docs/characters/realta/animation-sources.md#clips-and-dispatch)에서 관리한다.

기본 게임의 `NCreature.StartReviveAnim`은 등록된 Spine `Revive` 트리거를 사용하며, 플레이어에게 해당 트리거가 없으면 `AnimTempRevive`를 호출한다. `AnimTempRevive`는 시각 노드의 알파를 0.2초 동안 낮추고 `ImmediatelySetIdle`로 대기 자세를 적용한 뒤 0.2초 동안 복원한다. BaseLib의 `CustomAnimationPatch.UseCustomReviveAnim`은 비-Spine 시각 노드에서 `revive`, `Revive`를 찾아 기본 처리를 대체한다. 애니메이션 소스만 있고 두 이름이 모두 빠지면 기본 부활도 차단되므로, 비-Spine 리그는 `revive` 클립과 대기 복귀를 명시해야 한다. [레아르타 클립 계약](/docs/characters/realta/animation-sources.md#clips-and-dispatch)

`Commands.CreatureCmd.TriggerAnim`은 트리거 전달 후 `Cmd.CustomScaledWait(min(waitTime * .5, .25), waitTime)`을 기다린다. `AttackCommand`는 캐릭터의 `AttackAnimDelay`(상속 기본값 0.15초)를, 시전은 `CastAnimDelay`(기본값 0.25초)를 사용한다. 빠른 모드의 실제 표시 타이밍은 게임 재생으로 확인한다. 캐릭터별 지연 재정의는 각 캐릭터의 [애니메이션 API](/docs/characters/realta/animation-api.md)에서 관리한다.

## 씬 변환과 자식 리그 배치

`CustomCharacterModel.RegisterSceneConversions()`는 combat/rest/merchant 경로를 각각 `NCreatureVisuals`, `NRestSiteCharacter`, `NMerchantCharacter`로 등록한다. `NodeFactory.TryAutoConvert`는 `PackedScene.Instantiate` 이후 등록된 root를 변환한다. source: `Abstracts/CustomCharacterModel.cs`, `Utils/NodeFactories/NodeFactory.cs`.

`NodeFactory.TransferAndCreateNodes`에서 `FlexibleStructure == false`면 자식을 새 root에 이전하고 이전 root를 `QueueFree`한다. combat은 고정 경로 `Bounds`, `IntentPos`, rest는 `ControlRoot` 때문에 이 분기다. 원래 root script, custom 상태, root에 연결한 signal을 보존한다고 가정하면 안 된다. 반면 `NMerchantCharacterFactory`는 named node 요구가 없어 `FlexibleStructure == true` 분기를 탄다. 이 분기는 원본 root의 `Scale`·`Position`을 새 `NMerchantCharacter`에 복사한 뒤 원본 root를 자식으로 그대로 유지하므로, `merchant.tscn` 루트 노드에 스케일을 넣으면 최종 배율이 `s²`로 제곱되고 위치가 두 배로 적용된다. 따라서 모든 `merchant.tscn` 루트 노드는 항등 변환(`position = (0, 0)`, `scale = (1, 1)`)을 유지하고, 상점 스케일·접지선 오프셋은 반드시 자식 `Visuals` 노드에만 배치한다.

<a id="room-scale-and-ground-line-contract"></a>

## 방별 스케일 및 접지선 계약 (`v0.113.1`)

설치 게임 `v0.113.1`(commit `d09d48e5`)의 룸 씬과 `sts2.dll`에서 확인한 룸 컨테이너 변환 소유권은 다음과 같다. 기준 뷰포트는 `1920 × 1080`(`+Y` 아래 방향), 불투명 영역 측정 기준은 8비트 `α > 128`이다.

| 룸 | 씬 / 컨테이너 | 컨테이너 변환 | 런타임 배치 동작 |
|---|---|---|---|
| 전투 | `res://scenes/rooms/combat_room.tscn` → `AllyContainer` | `scale = (1, 1)` | 각 캐릭터 씬의 `Visuals`가 전투 스케일·접지선 소유 |
| 상점 (일반·블리츠) | `res://scenes/rooms/standard_merchant_room.tscn`, `blitz_merchant_room.tscn` → `%CharacterContainer` | `scale = (1, 1)`, 중앙 기준 `offset = (-396, +226)` (화면 `(564, 766)`) | `NMerchantRoom.AfterRoomIsLoaded`는 1–4인 슬롯 위치(앞줄 `(0, 0)`·`(-275, 0)`, 뒷줄 `(-140, -50)`·`(-415, -50)`)와 뒷줄 `Modulate`만 설정. 원작 캐릭터는 각 `*_merchant.tscn`에서 `SpineSprite`를 전투 대비 `1.62×–1.84×`로 확대 |
| 휴식 | `res://scenes/rooms/rest_site_room.tscn` → `BgContainer/Character_1..4` | `scale = (0.5, 0.5)` | `NRestSiteRoom._Ready`가 0-기반 홀수 인덱스(`i % 2 == 1` → 우측 `Character_2`, `Character_4`)에 `FlipX()` 호출. `ControlRoot`는 `x = 0` 대칭 유지 |

원작 5캐릭터 실측을 기준으로 모드 캐릭터는 아래 공통 목표와 공식을 따른다. 캐릭터별 실제 수치는 각 캐릭터의 `.tscn` 씬 파일이 단일 진실 원천(SSOT)이다.

- **전투(`combat.tscn`):** 두개골–발끝(긴 무기·모자·깃털 돌출 허용) 기준 표준/성인 체형 `~275 px`, 소형 체형 `~245–250 px`, 접지 `y ≈ -9`, `Bounds` 높이 `244–335 px` · `offset_bottom = 0.0`.
- **상점(`merchant.tscn`):** 원작 배율 대역 `1.62×–1.84×`(기본 `~1.65×`, 웅크림·넓은 망토 예외 허용), 접지 `y ≈ +16`. 루트 항등 변환 유지, 자식 `Visuals` 전용 스케일.
- **휴식(`rest_site.tscn`):** `Character_1..4`의 `0.5×` 적용 후 최종 화면 높이 `225–235 px`(원작 접지 앉은 대역 `214–259 px` 내), 하단 `y ≈ +120`.
- **정지 전신 PNG(`640 × 960`, Tier 2 Lossy WebP `0.85`, `mipmaps/generate=false`, 기본 `LINEAR`, 중심 원점 `(320, 480)`) 공식:**
  - `dy_foot = y_feet - H / 2` (`y_top`, `y_feet`는 8비트 `α > 128` 상·하단, `y_skull`은 모자·깃털·무기 아래 두개골 상단)
  - 전투: `s_combat = H_body / (y_feet - y_skull)`, `Visuals.position.y = round(y_ground - s_combat * dy_foot)`
  - 상점: `s_merchant = round(M * s_combat)`, `Visuals.position.y = round(y_rug - s_merchant * dy_foot)`
  - 휴식: `s_rest = (2 * H_rest) / (y_feet - y_top)`, `ControlRoot/Visuals.position.y = round(2 * y_seat - s_rest * dy_foot)`

레아르타의 전투·휴식·상점·선택 4개 씬 연결과 필수 노드 구조는 [애니메이션 API](/docs/characters/realta/animation-api.md)와 [애니메이션 소스](/docs/characters/realta/animation-sources.md)를 참고한다.

## 휴식·상점·게임오버의 별도 계약

`Nodes.RestSite.NRestSiteCharacter._Ready`는 `Player.RunState.CurrentActIndex` 0/1/2를 각각 `overgrowth_loop`, `hive_loop`, `glory_loop`에 대응시킨다. `GetChildSpineNodes()`가 수집하는 **직계 SpineSprite**에만 재생하고 시작 phase를 무작위화한다. `HideFlameGlow` 역시 그 목록에서 `_tracks/light_off`를 확인해 Spine track 1에 재생한다. BaseLib의 현재 패치에서 이를 AnimationPlayer로 전달하는 코드는 발견하지 못했다. 따라서 비-Spine 휴식 씬은 별도 Harmony 브리지로 막별 동작과 불빛 끄기를 전달해야 한다([레아르타 애니메이션 API](/docs/characters/realta/animation-api.md)).

`Nodes.Screens.Shops.NMerchantCharacter.PlayAnimation`의 `MerchantCharacterAnimPatch`는 factory가 만든 객체에서 `relaxed_loop` 요청을 `idle`, `Idle`, `relaxed_loop` 순서로, `die`를 `Die`, `die` 순서로 검색한다. 커스텀 핸들러의 반복은 Animation 리소스가 결정한다. 커스텀 플레이어가 있으면 네이티브 `_Ready`가 생략되므로 상점 리그 자체가 `relaxed_loop`를 자동재생하고 상점의 `idle`도 같은 리소스에 연결해야 한다.

`Nodes.Screens.GameOverScreen.NGameOverScreen.MoveCreaturesToDifferentLayerAndDisableUi`는 상점에서 `NMerchantCharacter.PlayAnimation("die")`를 호출한 뒤 시각 노드를 옮긴다. 휴식 등에서는 전투 시각 노드를 새로 만들어 `SpineAnimation.SetAnimation("die", false)`를 직접 호출하므로, 비전투 게임오버에서 비-Spine 리그로 `die`를 전달하려면 별도 브리지가 필요하다([레아르타 애니메이션 API](/docs/characters/realta/animation-api.md)).

## 선택 화면

설치 PCK의 `res://scenes/screens/char_select/char_select_bg_ironclad.tscn`, `..._silent.tscn`은 `SpineSprite` 아래 `Nodes.Animation.NSpineAutoPlayer`를 두고 별도의 `CPUParticles2D` 조명·재 입자를 배치한다. `NSpineAutoPlayer._Ready`는 부모 Spine이 준비되면 **정확히 하나인** animation 이름을 가져와 재생한다. 이 두 캐릭터에서는 이름이 `animation`이며 관찰한 길이는 Ironclad 5.333333초, Silent 11.666667초다. scene의 `preview_animation` 값을 런타임 autoplay의 근거로 삼으면 안 된다.

`NCharacterSelectScreen`은 배경을 `Instantiate<Control>`로 만들어 컨테이너에 붙인다. 종횡비 배치와 선택 원화 연출은 선택 배경 씬의 책임이다([레아르타 애니메이션 API](/docs/characters/realta/animation-api.md), [공식 장면 비교](sts2-official-animation-comparison.md)).

## 배포 리소스

- 게임의 `Modding.ModManager`는 DLL을 `AssemblyLoadContext.LoadFromAssemblyPath`로, PCK를 `ProjectSettings.LoadResourcePack`으로 읽는다. Harmony 브리지는 `Sts2WDCode`에서 컴파일하고 장면과 GDScript는 `Sts2WD`의 PCK 리소스로 관리한다. [프로젝트 설정](/Sts2WD.csproj), [내보내기 설정](/export_presets.cfg)
- 전투·상점 리그는 애니메이션과 파츠 참조를 `.tscn`에 굽는다. 제작용 JSON은 리그를 재생성할 때 읽는다. 현재 PCK의 비리소스 JSON에는 `localization`과 `character_text` 경로가 포함된다. `FileAccess`로 읽는 임의의 JSON은 내보내기에 포함할 경로를 지정해야 한다. [리그 제작 소스](/docs/characters/realta/animation-sources.md), [내보내기 설정](/export_presets.cfg), [Godot 4.5 FileAccess](https://docs.godotengine.org/en/4.5/classes/class_fileaccess.html)
