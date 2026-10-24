# 공식 애니메이션과 Realta cutout-v1 비교

조사일: 2026-10-19 (Asia/Seoul). 조사 대상은 Mega Crit가 공개한 영상 자료, 로컬 설치판의 동작 계약, Realta의 첫 분해 리깅 초안이다. **공식 발언**, **프레임 관찰**, **코드 관찰**, **제작 제안**을 구분한다. 이 문서는 품질 인증이나 새로운 테스트 명세가 아니다.

첫 초안은 관절 회전을 재생하는 기술 시연에는 성공했지만, 공식 캐릭터와 비교하면 전신의 무게 이동, 동작별 시간 배분, 천·장식의 후행 운동, 씬별 자세와 재생 계약이 부족하다. 파츠 수나 클립 수만 늘리는 것으로 해결되지 않는다.

## 1. 출처와 관찰 범위

| 자료 | 1차 출처 | 이번에 확인한 내용과 한계 |
|---|---|---|
| Silent 개발 영상 | [2024년 10월 공식 글](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/), [원본 GIF](https://www.megacrit.com/images/silent_anim.gif) | 저장된 600×495 GIF의 121프레임, 각 20ms, 전체 2.42초. 피격·대기·공격을 직접 프레임 비교. 출시 전 자료이므로 설치판과 동일하다고 간주하지 않음 |
| Necrobinder/Osty 영상 | [2025년 10월 공식 글](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/), [원본 GIF](https://www.megacrit.com/images/osty_poke3.gif) | 700×432, 130프레임, 각 30ms, 전체 3.90초. 카드 선택·공격·피격 효과가 함께 들어간 화면이며 클립 단독 길이가 아님 |
| 공식 선택 화면 | [공식 PNG](https://www.megacrit.com/images/steam_screenshot3_new.png), [공식 press kit](https://www.megacrit.com/press-kits/slay-the-spire-2/) | Necrobinder와 Osty의 대형 구도, UI와의 관계. 정지 화면으로 애니메이션 길이나 변형 방식은 판단하지 않음 |
| 설치판 코드·애니메이션 | `Slay the Spire 2/release_info.json`, `sts2.dll`의 아래 형식·멤버, `SlayTheSpire2.pck`의 아래 리소스 | 메타데이터 직접 확인: **v0.107.1, commit 59260271**, 배포 날짜 `2026-06-18T15:43:56-07:00`. 코드 재생 계약, Spine 리소스의 클립 길이, Ironclad/Silent의 씬별 표본 렌더 확인. 최신 공개판 여부와 완성된 게임 화면 재생은 별개 |
| Realta v1 | [시도 보고서](../characters/realta/realta-cutout-trial.md)의 관절 manifest·전용 조립 변환·합성 GIF | manifest와 프리뷰 프레임 직접 확인. 104개 인코딩 프레임, 전체 5.25초. 원래 생성기의 105개 입력 프레임 중 동일 프레임 병합으로 한 프레임이 100ms가 됨. Godot 실시간 녹화가 아닌 별도 합성 프리뷰 |

공개 GIF의 저장본·해시는 [출처 목록](sts2-style-refs/README.md)에 있다. 이번 관찰용 contact sheet는 각 GIF를 Pillow로 순회하며 누적 `duration`을 기록하고 6프레임 간격으로 **전체 프레임**을 축소해 배치했다. 공식 파일을 고치거나 가려진 부위를 생성하지 않았다. 로컬 비교 시트는 선택적 작업 자료이며, 공유 문서의 판단 근거는 아래에 시간과 관찰로 남겼다. 설치판 원본 자산을 새로 배포하지 않았다.

## 2. 공개 영상에서 보이는 차이

**공식 발언:** Mega Crit는 작은 화면에서 자세·손잡이 잡는 방식·표정이 읽히는 것과 반응의 빠른 느낌을 중시하며, 후속작에서 더 많은 애니메이션과 전환을 지향한다고 설명한다. 특정 관절 수·프레임 수·보간 곡선을 공식 표준으로 제시한 것은 아니다. [2024년 공식 글](https://www.megacrit.com/news/2024-10-02-neowsletter-issue-3/)

### Silent: 몸 전체가 동작의 뜻을 바꾼다

**프레임 관찰:** GIF 0.00~0.36초는 피격 숫자와 효과가 보이는 구간이다. 몸통·머리·손의 위치가 변하며, 약 0.48~0.60초부터 넓게 선 기본 자세로 돌아온다. GIF가 피격 도중 시작하므로 이 구간을 `hurt` 전체 길이라고 부를 수 없다. 0.72~1.20초에는 대기 자세가 이어진다. [공식 GIF](https://www.megacrit.com/images/silent_anim.gif)

**프레임 관찰:** 1.20초에는 기본 자세, 1.32초에는 전방 팔과 몸통이 크게 뻗고 무릎이 접힌 자세, 1.44~1.56초에는 망토와 뒤쪽 땋은 머리가 펼쳐진 다른 외곽선, 1.68초에는 기본 자세가 보인다. 공격은 팔의 회전만으로 표현되지 않는다. 다리와 몸통이 같은 행동에 참여하고, 망토·머리가 전후 프레임에서 다른 곡선을 만든다. 화면의 칼 궤적 효과도 공격을 읽는 데 기여한다. 6프레임 간격 표본이므로 정확한 시작·접촉 프레임을 확정한 수치는 아니다. [공식 GIF](https://www.megacrit.com/images/silent_anim.gif)

**Realta 비교:** v1의 `hip`, 양쪽 `thigh`, 양쪽 `shin`에는 동작 트랙이 없다. 공격·피격·사망에서도 하체와 중심 위치가 같은 상태로 남는다. 상체 회전 범위를 늘려도 몸의 지지와 무게 이동이 추가되지는 않는다. 사망 마지막 프레임은 선 채로 상체만 기울어진 모습이다. 무릎·어깨 이음새와 과도하게 큰 빗자루도 계속 노출된다. 근거는 v1 `rig.json`과 0.00~5.20초 프리뷰 관찰이다.

### Necrobinder/Osty: 대기, 행위 주체, 타격 반응을 분리한다

**프레임 관찰:** 0.00~2.34초 대부분은 카드 선택과 조준이다. 이를 긴 공격 예비 동작으로 측정하면 잘못이다. 약 2.52초에 Osty가 앞으로 뻗고, 2.70초에는 적 위치에 타격 효과와 피해 숫자가 보인다. 2.88초에는 Osty가 다시 가까이 돌아오며, 이후 숫자와 효과가 남아 있다. Necrobinder 본체와 Osty는 같은 크기의 운동을 동시에 하지 않는다. [공식 GIF](https://www.megacrit.com/images/osty_poke3.gif)

**공식 발언:** Osty는 Necrobinder와 독립적으로 공격하는 동료다. 영상의 분리된 행위 주체는 이 설정·게임 동작과 맞는다. [2025년 공식 글](https://www.megacrit.com/news/2025-10-16-neowsletter-issue-15/)

**제작 제안:** Realta도 창을 휘두르는 손·몸통을 주요 동작으로, 모자 끝·짧은 머리·리본·코트 끝을 따라오는 동작으로 구분한다. 다만 Osty나 별도 동료를 새로 만들어 넣는 근거로 쓰지 않는다. v1의 머리+모자, 무기+리본, 코트 전체는 각각 하나의 강체라 내부의 후행 움직임을 만들 수 없다.

## 3. 설치판에서 확인한 씬·상태 계약

아래 코드 관찰의 1차 자료는 설치판 `sts2.dll`이다. 이름과 멤버를 그대로 기록해 같은 버전에서 다시 확인할 수 있게 했다. 디컴파일된 코드는 구현을 읽는 수단이며 Mega Crit가 공개 배포한 소스 저장소를 뜻하지 않는다.

| 범위 | 관찰한 형식·멤버 | 확인한 동작 | Realta에 주는 의미 |
|---|---|---|---|
| 기본 전투 | `MegaCrit.Sts2.Core.Models.CharacterModel.GenerateAnimator` | `Idle→idle_loop`, `Attack→attack`, `Hit→hurt`, `Dead→die`, `Cast/PowerUp→cast`, `Relaxed→relaxed_loop`. attack/cast/hurt 뒤 idle 큐 등록 | v1의 네 이름만으로 공식 전투 범위를 덮지 못함. 자체 이름을 써도 trigger와의 명시적 연결 필요 |
| 캐릭터별 행동 | `Ironclad`, `Silent`, `Defect`, `Necrobinder`, `Regent`의 `GenerateAnimator` | 각각 `attack_heavy`, `shiv`, `process`, `cast_mighty`, `attack_sovereign` 같은 별도 상태가 있음 | 모든 공식 클립을 Realta에 복제할 필요는 없음. Realta의 실제 카드 행위가 필요한 구분을 소유해야 함 |
| 상태 전환 | `MegaCrit.Sts2.Core.Animation.CreatureAnimator.SetNextState`, `AddNextState`, `OnAnimationCompleted` | 실제 애니메이션 존재 여부 확인, 다음 상태 큐, 완료 신호 처리, 경우에 따라 bounds 갱신 | 단독 GIF 순서 재생은 게임 이벤트와 중단·복귀 동작을 입증하지 않음 |
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

표본 120개와 두 contact sheet를 직접 확인했다. 씬 배경·UI·외부 VFX 스크립트는 로드하지 않았다. 회색은 관찰용 배경이다. 클립끼리는 bounds가 달라 표시 배율도 다르므로 절대 크기를 비교하면 안 된다. 특히 Ironclad 공격의 하얀 궤적은 외부 VFX 재질을 적용하지 않은 원본 attachment이며 최종 게임의 효과 색깔로 해석하지 않는다. 실행 시 OS 인증서 저장소 오류가 출력되었지만 모든 이미지 저장은 `OK(0)`를 반환했다.

| 범위 | 원본 resource 경로 (`<c>` = `ironclad` 또는 `silent`) | 직접 관찰 |
|---|---|---|
| 전투 | `res://animations/characters/<c>/<c>_skel_data.tres` | Ironclad 공격은 발 간격·무릎·몸통과 검이 함께 전진하며, 피격은 무릎을 굽히고 상체를 뒤로 젖힘. Silent 공격은 앞쪽 무릎이 굽고 망토가 대각선으로 펼쳐짐. 각 cast는 공격과 다른 팔 동작 |
| 전투 사망 | 위 리소스의 `die` | Ironclad는 검을 떨어뜨리고 무릎을 꿇은 뒤 앞으로 쓰러져 누운 자세. Silent는 서 있는 몸이 낮아지고 옆으로 길게 누운 자세로 정착. v1의 서서 상체만 기울이는 결말과 분명히 다름 |
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

**v1 관찰:** `build_trial.py`는 클립별 키들을 균등 분할하며 모든 track에 선형 보간을 쓴다.

| 클립 | v1 시간 구조 | 차이와 수정 방향 |
|---|---|---|
| idle | 2.4초, 키 0 / 1.2 / 2.4초 | 머리·팔·코트의 꺾임이 같은 순간에 발생. 작은 진폭에 둔감한 것이 아니라 속도·위상 구조 자체가 단순함 |
| attack | 0.9초, 키 0 / 0.3 / 0.6 / 0.9초 | 예비 동작·전방 동작·복귀가 동일 0.3초. 현재 전방 pose가 0.6초이므로 0.15초 효과 계약과 곧바로 연결하면 효과가 먼저 나올 수 있음 |
| hit | 0.55초, 키 0 / 0.275 / 0.55초 | 밀려남과 복귀의 시간이 같음. 피해 순간의 짧고 분명한 반응과 이후 감속 복귀를 따로 조정할 필요 |
| die | 1.2초, 키 0 / 0.6 / 1.2초 | 이동·무릎 굽힘·바닥 접촉이 없고 상체 회전만 증가. 완성된 사망 동작의 근거가 아님 |

**제작 제안:** 먼저 공식 구현에서 확인한 이벤트 시점에 맞춰 Realta 공격의 읽히는 순간을 배치하고, 앞쪽에 짧은 준비, 뒤쪽에 더 긴 정리 구간을 둔다. 0.15초가 모든 카드를 지배한다는 가정은 피하고 Realta의 카드·기반 라이브러리 실제 경로도 확인한다. 관절마다 시간 키를 명시하고 동작 속도 변화를 설계한다. 공격을 느리게 늘리는 대신 짧은 접촉과 읽히는 정리 자세를 확보한다. 이는 제안이며 공식의 보간 곡선을 복원했다고 주장하지 않는다.

## 5. 다음 반복의 순서

아래는 관찰을 개선 작업으로 연결하는 순서다. 자동 합격 검사나 사용자 소유 테스트를 새로 정의하지 않는다.

1. **정지 조립 먼저 수정.** base/combat의 얼굴·실루엣·무기 길이를 함께 비교한다. 어깨판·커프·칼라의 소유 파츠를 하나로 정하고, 무릎 이음과 그립을 실제 표시 크기에서 확인한다. 틀린 형상은 이미지 생성 툴로 해당 파츠를 재생성한다.
2. **동작이 요구하는 파츠 구성으로 변경.** 발/다리/골반의 지지 운동과 모자 끝·리본·코트 끝의 후행 운동에 필요한 분리를 정한다. 모든 것을 잘게 분해하지 않는다. 하나의 강체 회전으로 만들 수 없는 앞뒤 자세에는 전용 포즈나 attachment가 필요하다.
3. **동작별 주요 자세를 비교.** idle, 준비, 타격, 정리, recoil, settle, death final을 나란히 확인한다. 몸통과 팔만 움직인 상태에서 진폭만 늘리는 수정을 피한다.
4. **시간·접촉·복귀를 조정.** event와 전체 길이를 분리하고 곡선·위상을 설정한다. 발이 이동하면 딛는 목적이 있어야 하며, 무기와 손은 전체 구간에서 연결되어야 한다. 사망 마지막 자세는 충분히 보여 주고 루프를 끈다.
5. **씬별 후보를 별도로 비교.** 전투 긴장 자세, 상점 relaxed 및 die, 휴식 세 Act loop와 불빛, 선택 화면 대형 구도·UI·상호작용을 구분한다. 같은 이미지를 확대·축소해 이름만 바꾸는 것을 씬 완성으로 기록하지 않는다.
6. **프리뷰와 실제 재생을 연결.** 후보마다 버전·입력 이미지·재생기·클립·시간을 기록한다. Godot 실제 렌더, 정상 속도 playback, 중단 후 idle 복귀와 사망 고정, 게임 이벤트 전달, 실제 화면의 UI·clipping을 각각 확인한다.
7. **다음 수정의 원인을 남김.** 예를 들어 “공격 0.15초에 아직 준비 자세”이면 timing 소유 모듈을, “팔 회전 시 어깨가 두 겹”이면 파츠 생성과 소유 규칙을 고친다. 개선된 것과 여전히 관찰되는 결함을 함께 남긴다.

## 6. 아직 단정할 수 없는 것

- 공개 GIF가 설치판과 같은 skeleton·키프레임·VFX를 쓰는지 확인하지 않았다.
- 공개 GIF의 모든 death·cast·relaxed 상태를 확보한 것은 아니다. Silent 영상에 death가 없는데 공식 사망 자세를 본 것처럼 설명하지 않는다.
- 설치판의 Ironclad/Silent 10개 클립씩은 시간 표본을 직접 렌더했지만 전체 프레임·실시간 playback·다인 배치·UI·외부 VFX까지 확인한 것은 아니다. 나머지 세 캐릭터의 모든 씬도 이번 조사 범위에 포함하지 않았다.
- 공식 제작사가 내부적으로 어떤 리그·메시·보간·레이어 제작 규칙을 적용했는지 영상만으로 확정할 수 없다.
- v1이 공식 수준에 도달했다는 근거는 없다. 이후 후보도 남은 결함과 미확인 범위를 공개하며 비교해야 하고, 에이전트 자체 점수만으로 동등 품질을 인증할 수 없다.

이번 조사는 파일 읽기·공식 웹 자료 확인·기존 GIF 프레임 관찰·설치판의 원본 Spine 시간 표본 렌더를 수행했다. 테스트, 검증 기준, 런타임 코드, 캐릭터 원화는 수정하지 않았다.

## 7. 외부 제작 사례를 통한 v2 워크플로우 비교

2026-10-19 추가 조사. 사용자 요청에 따라 **매 검증 반복에서 유사 사례의 외부 1차 자료를 확인하고, 결과물과 작업 방식의 개선을 함께 기록**한다. 아래는 Godot·Spine 제작사의 문서와 연구자 본인의 논문을 현재 문제에 대입한 비교다. Spine 기능을 Godot Sprite2D에 이미 구현했다거나 연구 모델이 Codex 이미지 툴과 같은 기능이라고 주장하지 않는다.

### 보이는 원본과 새로 그리는 가려진 면을 분리

**외부 근거:** Esoteric의 [파츠 준비 가이드](https://eu.esotericsoftware.com/blog/How-to-cut-your-assets-for-animation)는 움직이면 드러나는 가려진 부분까지 그려야 한다고 설명한다. 관절의 겹침 끝을 둥글게 만들고, 앞뒤로 지나가는 천은 별도 레이어로 준비하며, 분할 정도는 계획한 움직임에 맞춘다. 모든 조각을 많이 만드는 것이 목적은 아니다.

**v2 선택:** 기존 `combat.png`에서 보이는 픽셀과 그 파츠의 소유 마스크를 보존한다. 생성 툴에는 관절 안쪽·팔 뒤 몸통·코트 안쪽처럼 새로 드러나는 면을 보완하도록 요청하고, 결과를 **별도 보완 레이어**로 보관한다. 반환된 전체 캐릭터를 원본으로 교체하지 않는다. 최종 조립은 원본 소유 영역과 검토한 보완 영역을 명시적으로 결합한다. 이 방법의 원본 보존 성질은 이미지 모델의 약속이 아니라 합성 단계에서 어느 픽셀을 선택했는지에 달려 있다. 반투명 경계는 단순 덮어쓰기만으로 동일하게 합성된다고 가정하지 않고 별도로 관찰한다.

**연구 사례와 한계:** [See-through, 2026년 2월 preprint](https://arxiv.org/abs/2602.03749)는 단일 anime 그림의 부위 분해·가려진 면 복원·draw order 추정을 함께 다룬다. [Adobe의 LayerDecomp, CVPR 2025](https://research.adobe.com/publication/generative-image-layer-decomposition-with-visual-effects/)는 투명 효과를 가진 전경과 깨끗한 배경의 생성적 분해를 다룬다. 두 연구는 분할·가림·합성 일관성이 독립된 제작 문제임을 보여 준다. 원래 보이지 않던 부분의 정답이 단일 이미지에 존재한다는 뜻이 아니며, Codex의 일반 이미지 편집이 해당 전용 모델이나 픽셀 보존 보장을 제공한다는 증거도 아니다. 이 작업에서 연구 모델 설치·학습은 채택하지 않는다.

### 반복 polygon 수정에 앞서 원인을 구분

| 관찰한 문제 | 외부 1차 자료의 관련 기법 | 현재 v2에서 채택할 선택 | 보류·기각할 선택과 이유 |
|---|---|---|---|
| 같은 파츠를 매번 잘라 위치를 다시 맞춤 | [Spine PSD import](https://esotericsoftware.com/spine-import-psd)는 layer, origin, tag를 통해 attachment·bone·slot 구조를 재사용 | Godot에서도 원본 좌표계의 마스크·pivot·draw order를 하나의 manifest에 고정하고 수정한 파츠만 다시 출력. PSD를 쓴다면 origin을 유지 | 매번 새로운 전체 atlas를 생성하고 모든 조각을 재정렬하는 방식은 identity drift와 배치 작업을 반복시킴. PSD/Spine 도입 자체를 필수로 만들 필요는 없음 |
| 팔꿈치 회전 시 틈 또는 두 겹 어깨 | [파츠 준비 가이드](https://eu.esotericsoftware.com/blog/How-to-cut-your-assets-for-animation)의 둥근 겹침과 앞뒤 레이어 분리 | 관절 안쪽의 숨은 면과 겹침 방향, 어깨판의 단일 소유를 먼저 고침. 사용하려는 최소·최대 회전 자세를 함께 봄 | 외곽 polygon 정점만 계속 이동해 없는 관절 안쪽을 숨기는 방식은 새 면을 만들지 못함 |
| 천·관절이 종이판처럼 꺾임 | [Godot 2D skeletons](https://docs.godotengine.org/en/4.5/tutorials/animation/2d_skeletons.html)는 자동 삼각분할의 한계와 굽힘 부위 내부 정점·weight를 설명 | 연속 곡면이 필요한 코트 끝이나 관절만 sparse `Polygon2D`/bone weight 후보로 전환. 먼저 큰 동작에서 삼각형이 접히는 위치를 확인 | 몸 전체를 조밀한 mesh로 바꾸면 weight와 보수 비용이 커짐. 이 문서는 4.5 미갱신 경고가 있으므로 메뉴 절차를 그대로 믿지 말고 현재 MegaDot에서 확인 |
| 정점 수만 늘고 수정이 계속됨 | [Spine mesh 가이드](https://esotericsoftware.com/spine-meshes)는 Trace/Generate, 중요한 특징 주변 정점, 적은 vertex, weight 중심 변형을 설명 | alpha 외곽으로 초기 hull을 만들고, 실제로 휘는 부분에만 내부 edge를 추가하는 접근을 참고. 재사용할 topology를 확정한 뒤 animation 작업 | Spine의 Trace/Generate는 기존 weight·deform key를 지우므로 이미 작업한 mesh에 반복 실행하지 않음. 자동 외곽 추적은 내부 부위 의미·관절 위치까지 알아내는 도구가 아님 |
| 몸이 내려가면 발이 미끄러짐 | [Spine IK](https://esotericsoftware.com/spine-ik-constraints)는 손발 target으로 두 관절을 제어해 많은 FK 보정 키를 줄임 | 골반 이동과 발 target을 분리하고, 닿아 있는 발은 target을 고정. 무릎 굽힘 방향과 도달 범위를 명시 | 프레임마다 허벅지·정강이 각도를 손으로 보정하는 반복을 피함. 지지하지 않는 발까지 항상 고정하면 사망·발걸음 동작을 제한함 |
| 기본 Node2D 리그에 IK 기능이 없음 | [Godot TwoBoneIK 4.5](https://docs.godotengine.org/en/4.5/classes/class_skeletonmodification2dtwoboneik.html)는 두 Bone2D, target, 굽힘 방향, 거리 한계를 제공 | 발 접지 개선 효과가 필요한 지점에서 `Skeleton2D` 도입 비용과 비교. 기존 Node2D 시연에선 먼저 두 링크 해를 임시로 관찰할 수 있음 | Godot 클래스는 experimental. Sprite2D 계층에 해당 리소스 이름만 추가하면 작동한다고 가정하지 않음. 범용 IK 프레임워크를 새로 만들지 않음 |
| 얼굴 방향·손의 그립·눕는 자세를 회전만으로 만들 수 없음 | [Godot cutout 가이드](https://docs.godotengine.org/en/4.5/tutorials/animation/cutout_animation.html)는 손·발·표정의 별도 cel과 cutout 혼합을 지원 | 읽히는 공격 손·사망 끝 자세 등 필요한 부분만 pose-specific attachment/그림 교체 후보로 둠. 전신이 눕는 사망은 별도 그림 또는 추가 가림 구조가 필요할 수 있음 | 정면 얼굴을 과도하게 mesh 왜곡하거나 서 있는 팔을 늘려 모든 방향을 흉내 내는 방식은 형태 오류가 커짐 |
| 장식이 몸과 동시에 멈춤 | [Spine physics constraints](https://esotericsoftware.com/spine-physics-constraints)는 머리·천 등의 후행 운동, damping, reset과 loop warm-up을 설명 | 현 단계는 모자 끝·리본·코트 끝의 지연과 정착을 적은 키로 먼저 조정. runtime physics를 택하면 clip 전환·재시작·loop warm-up까지 함께 관찰 | 물리만 추가하면 동작 연출이 완성되는 것은 아님. 현재 확인한 문서는 최신 Spine 문서라 설치판 4.2 리소스와 모든 기능이 같다고 가정하지 않음 |
| 복잡한 리그가 파츠별로 중복됨 | [Spine Mix-and-match 예제](https://esotericsoftware.com/spine-examples-mix-and-match)는 모자·가방·천의 control bone과 변형을 분리 | 하나의 주요 control이 여러 보조 파츠를 움직이도록 하고, 그립·모자·코트의 책임을 분리. 특정 물체에 필요한 작은 제어만 추가 | 얼굴 2.5D·가방 원근 등의 전체 예제를 Realta에 복제할 필요는 없음. 동작이 요구하지 않는 제어는 보수 비용만 늘림 |

### 현재 반복에서 먼저 줄일 작업

**제작 판단:** 가장 먼저 고정할 것은 전체 파츠의 재생성 횟수가 아니라 **보이는 픽셀의 소유, 변하지 않는 좌표, 동작을 주도하는 소수의 control**이다. 분할 마스크를 한 번 기록하면 같은 입력에서 다시 추출할 수 있고, 문제 파츠만 보완하면 나머지 관절 배치를 보존할 수 있다. 손으로 좌표를 여러 스크립트에 복사하는 대신 manifest 하나에서 씬과 관찰용 렌더를 파생한다. 미리보기 렌더러가 Godot와 다른 변형을 쓰면, 그 차이를 메우기 위한 별도 수작업이 늘어나므로 최종 비교는 실제 Godot에서 수행한다.

**작업 순서 제안:** 정지 원본 복원 → 관절 극값에서 가려진 면 확인 → 골반·발 target으로 주요 자세 → 접촉 시점과 복귀 → 필요한 파츠만 변형·교체 → 후행 운동 → 씬별 변형 순으로 진행한다. 한 단계의 결함을 다른 단계로 감추지 않는다. 예를 들어 관절 안쪽이 없는 문제를 부드러운 easing으로 완화해도 파츠 제작 문제는 남는다.

### 매 검증 반복에 남길 외부 비교와 비용 기록

다음은 작업 관찰 기록 항목이며 테스트 기준·정량 합격선을 추가하는 것이 아니다. 매 반복마다 관련 외부 자료를 실제로 다시 열어 현재 결함과 연결한다. 같은 자료를 다시 쓰는 경우에도 이번에 확인한 절과 새로 판단한 내용을 남긴다. 검색 횟수만 늘리고 동일한 설명을 복사하는 것은 개선 활동으로 세지 않는다.

| 기록 항목 | 남길 내용 |
|---|---|
| 반복과 입력 | 후보 버전, 원본 이미지·마스크·rig 버전, 검사한 clip·시점·표시 크기 |
| 직접 관찰한 결함 | 예: `attack 0.15s`에서 어깨판 중복. 보이지 않은 결함이나 느낌 점수로 대체하지 않음 |
| 외부 유사 사례 | 당일 확인한 1차 URL, 관련 절·예제, 이 결함과 같은 점 및 다른 점 |
| 채택·보류·기각 | 채택할 원리와 수정 대상, 기각한 대안 및 예상 재작업 원인. 문서 내용을 그대로 구현할 필요는 없음 |
| 실제 작업량 | 이미지 툴 호출 수, 다시 그린/추출한 파츠 수, 수정한 mask·pivot·mesh·clip 수, 렌더 횟수, 확인 가능한 소요 시간. 측정하지 못한 시간·비용은 미측정으로 표시 |
| 관찰 결과 | 같은 카메라·크기·시점에서 이전 후보와 비교해 해소·잔존·새 발생 결함을 기록. 성공한 한 프레임만 고르지 않음 |
| 효율 판단 | 같은 결함을 다시 고쳤는지, 유지한 파츠·키를 재활용했는지, 다음 반복에서도 쓸 수 있는 구조가 생겼는지. 횟수가 줄었어도 품질이 나빠지면 개선으로 주장하지 않음 |
| 다음 행동 | 잔존 결함이 asset/segmentation/rig/timing/dispatch/scene 중 어디에 속하는지와 다음 외부 비교 주제 |

예를 들어 “정점 10개를 더 옮겼다”는 결과보다 “가려진 팔꿈치 면이 원인이어서 해당 보완 파츠 하나만 재생성했고, 기존 마스크·pivot·나머지 클립은 재사용했으며 공격 극값에서 틈이 사라졌다”가 작업 방식의 개선을 설명한다. 이때 마지막 시각적 결과와 실제 호출·수정 횟수는 관찰한 경우에만 기록한다.
