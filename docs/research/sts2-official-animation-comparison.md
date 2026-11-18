# Slay the Spire 2 공식 애니메이션 비교

2026-10-19에 Mega Crit의 공개 영상과 게임 `v0.107.1`의 장면, Spine 리소스, 상태 전환 코드를 조사했다. 레아르타의 장면 연결과 클립 계약은 [런타임 연결 계약](sts2-cutout-runtime-contract.md)에서 확인할 수 있다.

## 1. 출처와 관찰 범위

| 자료 | 1차 출처 | 이번에 확인한 내용과 한계 |
|---|---|---|
| Silent 개발 영상 | [2024년 10월 공식 글](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/), [원본 GIF](https://www.megacrit.com/images/silent_anim.gif) | 저장된 600×495 GIF의 121프레임, 각 20ms, 전체 2.42초. 피격·대기·공격을 직접 프레임 비교. 출시 전 자료이므로 설치판과 동일하다고 간주하지 않음 |
| Necrobinder/Osty 영상 | [2025년 10월 공식 글](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/), [원본 GIF](https://www.megacrit.com/images/osty_poke3.gif) | 700×432, 130프레임, 각 30ms, 전체 3.90초. 카드 선택·공격·피격 효과가 함께 들어간 화면이며 클립 단독 길이가 아님 |
| 공식 선택 화면 | [공식 PNG](https://www.megacrit.com/images/steam_screenshot3_new.png), [공식 press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/) | Necrobinder와 Osty의 대형 구도, UI와의 관계. 정지 화면으로 애니메이션 길이나 변형 방식은 판단하지 않음 |
| 설치판 코드·애니메이션 | `Slay the Spire 2/release_info.json`, `sts2.dll`의 아래 형식·멤버, `SlayTheSpire2.pck`의 아래 리소스 | 메타데이터 직접 확인: **v0.107.1, commit 59260271**, 배포 날짜 `2026-06-18T15:43:56-07:00`. 코드 재생 계약, Spine 리소스의 클립 길이, Ironclad/Silent의 씬별 표본 렌더 확인. 최신 공개판 여부와 완성된 게임 화면 재생은 별개 |

공개 GIF의 관찰 시점은 원본 프레임의 `duration`을 누적해 계산했다. 아래 영상 비교는 여섯 프레임 간격의 시간 표본을 사용한다. [공식 파일 출처](sts2-style-refs/README.md)

## 2. 공개 영상에서 보이는 차이

**공식 발언:** Mega Crit는 작은 화면에서 자세·손잡이 잡는 방식·표정이 읽히는 것과 반응의 빠른 느낌을 중시하며, 후속작에서 더 많은 애니메이션과 전환을 지향한다고 설명한다. 특정 관절 수·프레임 수·보간 곡선을 공식 표준으로 제시한 것은 아니다. [2024년 공식 글](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/)

### Silent: 몸 전체가 동작의 뜻을 바꾼다

**프레임 관찰:** GIF 0.00~0.36초는 피격 숫자와 효과가 보이는 구간이다. 몸통·머리·손의 위치가 변하며, 약 0.48~0.60초부터 넓게 선 기본 자세로 돌아온다. GIF가 피격 도중 시작하므로 이 구간을 `hurt` 전체 길이라고 부를 수 없다. 0.72~1.20초에는 대기 자세가 이어진다. [공식 GIF](https://www.megacrit.com/images/silent_anim.gif)

**프레임 관찰:** 1.20초에는 기본 자세, 1.32초에는 전방 팔과 몸통이 크게 뻗고 무릎이 접힌 자세, 1.44~1.56초에는 망토와 뒤쪽 땋은 머리가 펼쳐진 다른 외곽선, 1.68초에는 기본 자세가 보인다. 공격은 팔의 회전만으로 표현되지 않는다. 다리와 몸통이 같은 행동에 참여하고, 망토·머리가 전후 프레임에서 다른 곡선을 만든다. 화면의 칼 궤적 효과도 공격을 읽는 데 기여한다. 6프레임 간격 표본이므로 정확한 시작·접촉 프레임을 확정한 수치는 아니다. [공식 GIF](https://www.megacrit.com/images/silent_anim.gif)

### Necrobinder/Osty: 대기, 행위 주체, 타격 반응을 분리한다

**프레임 관찰:** 0.00~2.34초 대부분은 카드 선택과 조준이다. 이를 긴 공격 예비 동작으로 측정하면 잘못이다. 약 2.52초에 Osty가 앞으로 뻗고, 2.70초에는 적 위치에 타격 효과와 피해 숫자가 보인다. 2.88초에는 Osty가 다시 가까이 돌아오며, 이후 숫자와 효과가 남아 있다. Necrobinder 본체와 Osty는 같은 크기의 운동을 동시에 하지 않는다. [공식 GIF](https://www.megacrit.com/images/osty_poke3.gif)

**공식 발언:** Osty는 Necrobinder와 독립적으로 공격하는 동료다. 영상의 분리된 행위 주체는 이 설정·게임 동작과 맞는다. [2025년 공식 글](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/)

## 3. 설치판에서 확인한 씬·상태 계약

아래 코드 관찰의 1차 자료는 설치판 `sts2.dll`이다. 이름과 멤버를 그대로 기록해 같은 버전에서 다시 확인할 수 있게 했다. 디컴파일된 코드는 구현을 읽는 수단이며 Mega Crit가 공개 배포한 소스 저장소를 뜻하지 않는다.

| 범위 | 관찰한 형식·멤버 | 확인한 동작 | Realta에 주는 의미 |
|---|---|---|---|
| 기본 전투 | `MegaCrit.Sts2.Core.Models.CharacterModel.GenerateAnimator` | `Idle→idle_loop`, `Attack→attack`, `Hit→hurt`, `Dead→die`, `Cast/PowerUp→cast`, `Relaxed→relaxed_loop`. attack/cast/hurt 뒤 idle 큐 등록 | 게임 트리거 이름과 동작 완료 뒤의 전이를 레아르타 전투 리그에서 연결 |
| 캐릭터별 행동 | `Ironclad`, `Silent`, `Defect`, `Necrobinder`, `Regent`의 `GenerateAnimator` | 각각 `attack_heavy`, `shiv`, `process`, `cast_mighty`, `attack_sovereign` 같은 별도 상태가 있음 | 모든 공식 클립을 Realta에 복제할 필요는 없음. Realta의 실제 카드 행위가 필요한 구분을 소유해야 함 |
| 상태 전환 | `MegaCrit.Sts2.Core.Animation.CreatureAnimator.SetNextState`, `AddNextState`, `OnAnimationCompleted` | 실제 애니메이션 존재 여부 확인, 다음 상태 큐, 완료 신호 처리, 경우에 따라 bounds 갱신 | 비-Spine 레아르타 리그에서는 `AnimationPlayer`가 행동 뒤 대기 전이를 담당 |
| 대기 위상 | 같은 형식의 생성자·`OffsetLoopingAnimation` | 초기 idle 위상 무작위화, 반복 시간 배율 0.9~1.1 및 작은 위상 변동 | 여러 캐릭터가 똑같은 박자로 움직이지 않음. 이 수치는 관찰한 구현이며 Realta 품질 합격선이 아님 |
| 상점 | `CharacterModel.MerchantAnimPath`, `NMerchantCharacter._Ready/PlayAnimation` | 별도 `merchant/characters/<id>_merchant` 씬; 첫 자식 MegaSprite에 `relaxed_loop` 반복·무작위 위상 | 상점은 전투 공격 대기를 그대로 호출하는 경로가 아님 |
| 상점 사망 | `NGameOverScreen.MoveCreaturesToDifferentLayerAndDisableUi` | 상점 플레이어 visuals에 `PlayAnimation("die")` 호출 후 옮김 | 상점 변형에도 사망 경로를 고려해야 함 |
| 휴식 | `CharacterModel.RestSiteAnimPath`, `NRestSiteCharacter._Ready` | 별도 rest 씬; Act index 0/1/2에 `overgrowth_loop` / `hive_loop` / `glory_loop`; 자식 SpineSprite들에 재생·무작위 위상 | 휴식은 단일 combat idle 복제가 아님. 이름만으로 세 자세가 얼마나 다른지는 알 수 없음 |
| 휴식의 환경·다인 배치 | `NRestSiteCharacter.HideFlameGlow/FlipX/GetRestSiteOptionAnchor` | 선택적 `_tracks/light_off`를 track 1에 재생; 좌우 반전; 생각풍선 anchor 선택 | 불빛 레이어·반전·UI 위치도 씬 바리에이션 범위 |
| 휴식 장소 사망 | `NGameOverScreen.MoveCreaturesToDifferentLayerAndDisableUi` | 전투 visuals를 새로 만들고 `SpineAnimation.SetAnimation("die", false)` 재생 | 휴식 리그를 억지로 쓰러뜨린다고 실제 경로가 완성되지 않음 |
| 선택 화면 | `CharacterModel.CharacterSelectBg`, `NCharacterSelectScreenBg.OnWindowChange` | 별도 `screens/char_select/char_select_bg_<id>` 씬과 화면비별 배율 처리 | 전투 리그 확대와 별도로 구도·UI·화면비 확인 필요 |
| 선택 화면의 개별 상호작용 | `NRegentCharacterSelectBg._Ready/SetSkin` | 특정 hover 영역과 skin 전환을 연결 | 선택 화면의 모든 변형이 본체 애니메이션 하나라는 가정은 틀림. 다른 캐릭터에 동일 기능이 필수라는 뜻은 아님 |

위 표는 **코드 계약**이다. 이를 시각적 관찰과 연결하기 위해 Ironclad와 Silent의 실제 설치판 리소스를 아래 방법으로 렌더했다. 기존 [선택 구도 조사](sts2-character-select-composition-study.md)의 다섯 캐릭터 정지 렌더와 이번 시간 표본을 구분한다.

### 설치판을 직접 렌더한 씬 비교

**재현 절차:** 설치판의 Windows x86_64 template-release용 `libspine_godot` DLL을 로컬 GDExtension으로 로드하고 `ProjectSettings.load_resource_pack`으로 PCK를 읽었다. MegaDot `4.5.1.m.14.mono.custom_build`, OpenGL compatibility renderer를 사용했다. 각 SpineSprite에 원본 skeleton data를 연결한 뒤 해당 애니메이션을 nonloop 상태로 설정하고 `update_skeleton(t)`로 시간을 진행했다. 표본 시간은 `0, 0.2D, 0.4D, 0.6D, 0.8D, D−0.001`이며 D는 원본 클립 길이다. 그 다음 time scale을 0으로 고정했다. 여섯 표본의 bounds 합집합을 640×640 viewport에 맞춰 **같은 클립의 카메라를 고정**했다. 2회 process frame을 기다리고 `RenderingServer.force_draw()` 후 PNG로 저장했다.

관찰 범위는 Spine 리소스의 자세와 색이다. 클립마다 배율이 다르며 장면의 배경·UI·외부 VFX는 포함되지 않는다. Ironclad 공격의 흰 궤적은 외부 VFX 재질을 적용하기 전의 attachment다.

| 범위 | 원본 resource 경로 (`<c>` = `ironclad` 또는 `silent`) | 직접 관찰 |
|---|---|---|
| 전투 | `res://animations/characters/<c>/<c>_skel_data.tres` | Ironclad 공격은 발 간격·무릎·몸통과 검이 함께 전진하며, 피격은 무릎을 굽히고 상체를 뒤로 젖힘. Silent 공격은 앞쪽 무릎이 굽고 망토가 대각선으로 펼쳐짐. 각 cast는 공격과 다른 팔 동작 |
| 전투 사망 | 위 리소스의 `die` | Ironclad는 검을 떨어뜨리고 무릎을 꿇은 뒤 앞으로 쓰러져 누움. Silent는 서 있는 몸이 낮아지고 옆으로 길게 누운 자세로 정착 |
| 상점 | `res://animations/merchant/<c>/<c>_merchant_skel_data.tres` | `relaxed_loop`: Ironclad는 검 끝을 아래로 낮추고 다른 손을 목·어깨 근처에 둠. Silent는 발 간격을 좁히고 팔을 몸 가까이 모음. 둘 다 전투의 넓은 준비 자세와 구별됨 |
| 휴식 | `res://animations/rest_site/<c>/rest_site_<c>_skel_data.tres` | Ironclad는 앉아서 머리를 숙이고 팔을 무릎 사이로 내림. Silent는 앉아서 한 손으로 얼굴·턱 근처를 받침. 전투 자세를 축소한 모습이 아님 |
| 휴식 Act 차이 | 위 리소스의 세 `*_loop` | 표본들에서 기본 앉은 자세는 유지되며 `overgrowth`는 녹색, `hive`는 따뜻한 황색, `glory`는 청록 계열 조명 차이가 뚜렷함. 세 개를 완전히 별개의 행동 세트로 설명할 근거는 없음 |
| 선택 화면 | `res://animations/character_select/<c>/characterselect_<c>_skel_data.tres` | Ironclad는 검과 큰 얼굴·어깨가 붉은 배경을 차지하고, Silent는 펼친 망토와 비튼 상체가 큰 구도를 이룸. 시간 표본 사이의 움직임은 전투보다 절제되어 대형 구도가 유지됨. 외부 배경·UI가 없어 완성 화면과는 다름 |

**리소스 관찰:** 두 캐릭터 모두 전투와 상점 리소스의 animation 이름·길이 및 bone/slot 수가 같다. 이는 동작 데이터 재사용의 가능성을 뒷받침하지만 파일 내용 전체의 동일성을 검증한 것은 아니다. 휴식과 선택 리소스는 다른 구조다. 관절 수가 품질 척도라는 결론도 성립하지 않는다.

| 원본 클립 | Ironclad D(초) | Silent D(초) |
|---|---:|---:|
| 전투 `idle_loop` | 2.000 | 1.067 |
| 전투 `attack` | 1.167 | 0.933 |
| 전투 `hurt` | 1.000 | 0.733 |
| 전투 `cast` | 1.567 | 1.300 |
| 전투 `die` | 2.333 | 1.867 |
| 상점 `relaxed_loop` | 12.000 | 7.000 |
| 휴식 `overgrowth_loop` / `hive_loop` / `glory_loop` 각각 | 6.667 | 8.000 |
| 선택 `animation` | 5.333 | 11.667 |

수치는 `SpineSkeletonDataResource.get_animations()`와 `SpineAnimation.get_duration()`의 반환값을 소수 셋째 자리로 반올림했다. 전투·상점 Spine 버전은 `4.2.43`, 휴식은 `4.2.37`, 선택은 `4.2.40`이다. 이 값들은 조사한 두 캐릭터·설치 버전의 관찰치이며, Realta에 동일 길이를 강제하는 요구사항이 아니다. 긴 relaxed/rest loop와 짧은 전투 loop가 함께 존재하므로 모든 씬을 동일한 2.4초 반복으로 맞추는 것은 공식과 같은 설계라고 볼 수 없다.

## 4. 동작 시간은 전체 길이와 효과 시점을 나눠야 한다

**코드 관찰:** 기본 다섯 캐릭터의 `AttackAnimDelay`는 0.15초다. `CastAnimDelay`는 Ironclad/Silent/Defect/Regent가 0.25초, Necrobinder가 0.4초다. `AttackCommand.FromCard`가 이 값을 선택하고, 실행 경로에서 `CreatureCmd.TriggerAnim`을 기다린 뒤 타격 효과 등의 처리를 진행한다. `CreatureCmd.TriggerAnim`의 `waitTime`은 재생 시작 뒤 기다리는 시간이다. 따라서 이 값을 전체 animation duration으로 해석해서는 안 된다. `AttackCommand.FromOsty`의 기본값은 별도 0.3초다.

레아르타 전투 리그에서 공격 접촉은 0.15초, 주문 문양이 나오는 시점은 0.25초다. 두 클립의 전체 길이는 각각 1.05초, 1.3초다. [레아르타 클립 계약](/docs/characters/realta/animation-sources.md#clips-and-dispatch)

## 5. 리그 제작 근거

Esoteric Software의 파츠 준비 가이드는 관절의 겹침을 둥글게 만들고, 움직일 때 드러나는 면을 그리며, 앞뒤로 움직이는 천을 별도 레이어로 두는 방식을 설명한다. Godot의 2D 스켈레톤 가이드는 굽히는 부분에 내부 정점과 가중치를 배치하는 방법을 다룬다. 레아르타의 현재 파츠 구성은 소매 아래 어깨와 무릎 뒤의 가림을 보완하고 관절과 천에 가중치를 둔다. [Spine 파츠 준비](https://eu.esotericsoftware.com/blog/How-to-cut-your-assets-for-animation), [Godot 2D 스켈레톤](https://docs.godotengine.org/en/4.5/tutorials/animation/2d_skeletons.html), [레아르타 파츠 구성](/docs/characters/realta/animation-sources.md#completed-parts-and-overlap-ownership)
