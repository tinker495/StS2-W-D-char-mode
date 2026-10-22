# Godot 네이티브 애니메이션 작업 방식과 Realta 비교

조사일: 2026-10-22. 대상은 Realta 전투·상점 리그다. 공식 Godot 4.5 문서와 `4.5-stable` 엔진 소스로 기능의 의미를 확인했다. 저장소의 `project.godot`에는 현재 `4.7` 기능 표기가 있으므로, 이 보고서는 실행 엔진의 버전을 4.5라고 단정하지 않는다. 실제 편집·패키징에는 저장소가 지정한 MegaDot을 사용하고 그 환경에서 검증한다. Cutout/2D skeleton 튜토리얼에는 4.5 갱신 미완료 표시가 있어, 세부 API 동작은 클래스 문서와 엔진 소스를 함께 확인했다.

## 일반적인 작업 방식

Godot가 제공하는 기본 흐름은 씬에 부품과 뼈대를 구성하고 `AnimationPlayer`의 타임라인에서 속성 키를 편집하는 것이다. Inspector의 키 버튼과 타임라인 키 선택으로 값을 기록·수정하며, Onion Skinning으로 앞뒤 포즈를 비교할 수 있다. 모든 속성을 애니메이션으로 만들 수 있다는 것은 모든 속성에 키가 필요하다는 뜻은 아니다. [공식 애니메이션 입문](https://docs.godotengine.org/en/4.5/tutorials/animation/introduction.html)

Cutout은 부품별 그림, 피벗, 계층을 구성하고 이동·회전을 기록한다. 변형이 필요하면 `Skeleton2D`/`Bone2D`와 `Polygon2D`를 결합한다. UV 편집기에서 뼈를 동기화하고 가중치를 칠한 뒤 뼈를 움직이는 것이 공식 예제의 기본 흐름이다. Rest pose는 바인딩의 기준이며, 일반 동작 포즈를 바꾸는 작업과 구분한다. [공식 2D skeleton 가이드](https://docs.godotengine.org/en/4.5/tutorials/animation/2d_skeletons.html)

표정·손·복잡한 자세는 셀 교체와 Cutout을 함께 사용할 수 있다. 앞뒤 관계는 트리 순서, Z 또는 `RemoteTransform2D`로 표현할 수 있다. 이 저장소는 기존 합의대로 Z=0과 트리 순서를 사용한다. 이는 Godot의 유일한 표준이 아니라 프로젝트의 렌더링 계약이다. [공식 Cutout 가이드](https://docs.godotengine.org/en/4.5/tutorials/animation/cutout_animation.html)

## 현재 구조와 비교

아래의 ‘수정 전’은 이번 보강을 시작할 때 읽은 `realta_combat_rig.gd`와 전투·상점 `.tscn`의 관찰이다. 권고는 공식 기능의 의미와 이 프로젝트의 보존 요구를 함께 적용한 판단이다.

| 항목 | Godot에서의 일반적인 구성 | 수정 전 Realta | 이번 보강 방향 |
|---|---|---|---|
| 원본과 타임라인 | 씬과 Animation 리소스를 에디터에서 직접 편집 | 두 `.tscn`에 뼈대·메시·7개 실제 클립 저장 | 공통 base scene과 공유 `.tres`로 중복 제거. 기존 두 경로는 상속 씬으로 유지 |
| 정적 메시 | 노드의 polygon/UV/삼각형/가중치가 바인딩을 소유 | 모든 클립에 동일한 기본 메시 데이터를 반복 기록 | 전체 클립에서 같은 데이터만 노드에 남기고 반복 트랙 제거 |
| 실제 모양 교체 | 필요한 속성만 시간에 따라 키 설정 | 사망·표정 등의 교체 데이터와 정적 데이터가 섞임 | 실제로 달라지는 데이터와 그 복귀 키 유지 |
| 뼈 동작 | 뼈 transform을 키로 편집 | 촘촘한 기존 transform 키 존재 | 개수·시간·값을 보존. 키 단순화는 보류 |
| 표시·겹침 | 하나의 명확한 소유자가 최종 값을 결정 | `lower_mode`·`weapon_gripped` 등의 의미 상태와 `visible_slots`·`draw_order`가 같은 노드를 다시 제어 | 기존 의미 상태에 책임을 모으고 표시 배열·경로 사전·관련 트랙 제거 |
| 미리보기 | 에디터 재생·탐색 결과를 즉시 표시 | `@tool`에서 표시 배열 적용, 실행 중에는 별도 discrete 보정 | 표시 계산은 공유하고 무작위 반복 진입·전환 이력은 실행 중에 한정 |
| 저장 기본값 | `RESET`과 Reset On Save로 미리보기 포즈의 저장 유출 방지 | `RESET` 없음. 저장 전 기본 포즈 복귀 안내 | 각 씬에 `RESET` 추가, `reset_on_save=true` 명시. 정적 메시 속성은 제외 |
| 별칭 | 라이브러리 이름이 Animation 리소스를 참조 | 10개 게임 호출 이름이 7개 리소스를 공유 | 리소스 정체성과 게임 호출 이름 유지 |
| 인스턴스별 상태 | 실행 중 수정되는 리소스만 필요한 범위에서 분리 | Slash material의 `resource_local_to_scene=true` | 유지. 모든 Animation을 무조건 복제하지 않음 |

`Polygon2D`는 `polygon`, `uv`, `polygons`, `skeleton`과 뼈별 가중치를 직접 가진다. 따라서 전 클립에서 고정된 값을 노드에 두면 에디터의 메시 수정이 재생 때 옛 키로 복원되는 문제를 없앨 수 있다. 이번 이관은 7개 실제 클립의 모든 키를 비교해 공통값을 확인하고, 그 값을 노드 기본값으로 먼저 옮긴 뒤 대응 트랙을 제거했다. 저장된 기본값이 달랐던 경우에도 실제 재생 때 적용되던 공통 키 값을 채택해 재생 결과를 보존했다. 클립이나 시각에 따라 달라지는 값은 제거하지 않았다. 이는 해당 노드 API와 실제 리그를 바탕으로 한 프로젝트 적용 판단이다. [Polygon2D API](https://docs.godotengine.org/en/4.5/classes/class_polygon2d.html)

조사 대상 속성인 `polygon`, `uv`, `polygons`, `bones`, `texture` 중 변하는 값은 `head:texture`의 4개 그림과 `hand_l:texture`의 2개 그림뿐이었다. 이 두 텍스처 교체 트랙은 유지했다. 전투·상점 각각에서 정적 데이터와 두 표시 배열에 해당하는 트랙 749개를 제거했으며, 뼈와 다른 속성의 촘촘한 동작 키는 그대로 남겼다.

## 키·혼합·미리보기의 경계

`UPDATE_CONTINUOUS`는 키 사이를 갱신하고 값을 유지한다. `UPDATE_DISCRETE`는 키 시점에 갱신한다. `UPDATE_CAPTURE`는 일부 재생 API에서 현재 값을 포착해 보간하는 기능이며 기본 포즈 저장 기능이 아니다. 뼈 이동·회전의 기존 보간은 보존하고, 표시 상태·텍스처처럼 중간값의 의미가 없는 상태는 discrete로 유지한다. [Animation UpdateMode](https://docs.godotengine.org/en/4.5/classes/class_animation.html#enum-animation-updatemode)

공식 문서는 혼합 시 discrete 처리에 `FORCE_CONTINUOUS`를 권장하지만, 이것을 모든 리그에 일괄 적용해서는 안 된다. 현재 리그는 혼합 이후 현재 클립의 discrete 상태를 명시적으로 확정한다. 이 보정은 일반적인 Godot 필수 단계가 아니라, 기존 전환에서 나가는 클립의 상태가 들어오는 클립을 덮지 않게 하는 프로젝트의 호환 처리다. 이번에는 유지하고 전환 결과를 확인한다. `deterministic=true` 역시 누락 트랙을 초기값/`RESET` 기준으로 취급해 기존 결과를 바꿀 수 있으므로 자동 적용하지 않는다. [AnimationMixer의 discrete·deterministic 설명](https://docs.godotengine.org/en/4.5/classes/class_animationmixer.html#class-animationmixer-property-callback-mode-discrete)

`@tool`과 `Engine.is_editor_hint()`는 에디터에서도 표시 계산을 실행하고 실행 전용 코드를 분리하는 공식 수단이다. 에디터에서 트리 순서를 바꾸는 코드도 실제 씬을 다루므로, 매 프레임 값이 같은데도 노드를 계속 이동시키지 않아야 한다. 임의 시각으로 타임라인을 이동했을 때 표시·표정·무기 상태가 즉시 맞는지 확인하되, 실행 이력이 필요한 전환 겹침 유지와 무작위 진입은 게임 재생에서 별도로 확인한다. [에디터에서 코드 실행](https://docs.godotengine.org/en/4.5/tutorials/plugins/running_code_in_the_editor.html)

이번 구현은 `_apply_presentation()`에서 하체 표시, 자유 팔 깊이, 무기 팔 깊이, 잡은 손 깊이를 한 번씩 적용한다. `lower_mode`, `free_arm_back`, `weapon_arm_front`, `weapon_gripped` 등의 기존 의미 상태를 타임라인에서 편집한다. `visible_slots`, `draw_order`, `slot_paths`와 대응 배열 트랙은 제거해 같은 노드를 덮어쓰는 두 번째 표현을 없앴다. 에디터 탐색에서는 이전 재생 이력이 필요한 깊이 유지 조건을 무시하고 현재 포즈를 표시하며, 실행 중에는 기존 전환 유지 동작을 보존한다.

`AnimationLibrary`는 이름으로 Animation 리소스를 보관한다. 엔진의 `add_animation()`은 전달된 참조를 저장한다. 따라서 `idle`/`idle_loop`, `powerup`/`cast`가 같은 리소스를 가리키는 현재 구성은 키를 복제하지 않고 호출 이름을 유지하는 방식이다. 한 별칭에서 키를 수정하면 공유 대상에도 반영된다. [AnimationLibrary API](https://docs.godotengine.org/en/4.5/classes/class_animationlibrary.html), [4.5-stable 구현](https://github.com/godotengine/godot/blob/4.5-stable/scene/resources/animation_library.cpp)

`resource_local_to_scene=true`는 씬 인스턴스별로 리소스를 복제해 한 인스턴스의 실행 중 변경이 다른 인스턴스로 전파되지 않게 한다. 리소스 공유 자체가 문제인 것은 아니다. 실행 중 매개변수가 달라지는 Slash material의 분리는 유지하고, 공유하는 정적 텍스처와 편집용 Animation은 필요 없이 복제하지 않는다. [Resource API](https://docs.godotengine.org/en/4.5/classes/class_resource.html#class-resource-property-resource-local-to-scene)

## RESET과 기본 포즈 저장

정상적인 네이티브 편집 흐름에서는 `RESET`에 시간 0의 기본값을 기록하고 `reset_on_save=true`를 사용하는 방식을 권장한다. 엔진 `AnimationMixer::can_apply_reset()`은 정확히 `RESET` 이름을 찾고, 저장 보정은 그 리소스의 시간 0을 적용한다. 이름을 다르게 하거나 이름 있는 라이브러리로 숨겨도 같은 자동 초기화가 되는 것은 아니다. [4.5-stable AnimationMixer 구현](https://github.com/godotengine/godot/blob/4.5-stable/scene/animation/animation_mixer.cpp#L2005)

2026-10-22 사용자가 `RESET` 추가와 해당 검사 수정을 명시적으로 승인했다. 이에 따라 두 씬에 에디터용 `RESET`을 추가했다. 각 씬의 기준은 전투 `idle_loop`, 상점 `relaxed_loop`의 시간 0이며, 남은 애니메이션 속성 256개에 각각 시간 0의 키를 기록한다. `RESET`은 길이 0.001초의 반복하지 않는 클립이고 `reset_on_save=true`를 명시한다. 게임이 호출하는 기존 10개 이름과 7개 실제 동작은 그대로다.

`RESET`에는 정적 `polygon`, `uv`, `polygons`, `bones`, 고정 텍스처를 넣지 않는다. 따라서 노드에서 편집한 메시·UV·가중치는 저장 때 이전 데이터로 되돌아가지 않는다. 반대로 애니메이션 대상인 뼈 transform이나 표시 상태의 기본 포즈를 바꾸려면 `RESET`의 해당 시간 0 키도 수정해야 한다. 에디터에서 기본 포즈를 확인할 때는 Animation 패널의 Edit → Apply Reset을 사용하고, 저장 후 다시 열어 포즈와 메시 수정이 유지되는지 확인한다. Bone2D rest는 저장 기본값을 대신하는 기능이 아니므로 동작 포즈 저장을 위해 rest를 덮어쓰지 않는다. [RESET 편집 절차](https://docs.godotengine.org/en/4.5/tutorials/animation/introduction.html#using-reset-tracks)

기존 `tests/check_rig.gd`의 정확한 10개 이름 검사는 `RESET` 추가와 충돌했으므로 사용자 승인 범위에서 `NAMES`에 `RESET`을 포함하도록 수정했다. 목록은 이제 기존 호출 이름 10개와 `RESET`이다. 공통 라이브러리 검사는 11개 모두에 계속 적용하며 다른 검사를 완화하지 않는다.

## 채택 범위와 검증

이번에 채택하는 범위는 정적 메시 데이터의 단일 소유권, 표시·겹침 상태의 단일 소유권, `RESET`을 통한 저장 기본값 관리, 에디터에서 실제로 수정할 속성과 유지할 계약을 구분한 안내다. 7개 실제 동작의 촘촘한 키, 10개 호출 이름과 리소스 공유, 다음 동작 전환, Z=0, 기존 게임 wrapper를 보존한다. AnimationTree 전환, 키 단순화, 다른 편집 도구, 휴식·캐릭터 선택 화면 전환은 이번 범위에 추가하지 않는다. AnimationTree는 공식 선택지이지만 현재 게임이 사용하는 AnimationPlayer를 대체해야 할 근거는 확인되지 않았다.

검증은 `dev.ps1 check`와 저장소의 애니메이션 렌더 절차를 사용한다. 추가로 임시 관찰 코드로 두 씬 로드, 기본 메시를 바꾼 뒤 여러 동작 재생, 타임라인 역방향 탐색, 사망→부활과 무기 놓기 전환을 확인할 수 있다. 이는 기존 테스트의 대체나 사용자 시각 승인으로 간주하지 않는다. 테스트 변경은 사용자가 승인한 `RESET` 이름 추가에 한정한다. 실제 실행 결과와 수행하지 못한 GUI·게임 확인은 작업 완료 보고에서 구분한다.

보강 후 빌드·기계 검사 42,193개 샘플·인스턴스 독립성 4개 검사·PCK export가 통과했다. 최종 PCK의 전체 동작 792프레임은 시작 위상을 맞춘 기존 동작과 RGBA가 같았다. 별도 임시 관찰에서 3,388개 전환 시점의 표시·변환·순서가 같았으며, 메시·UV·가중치 수정 후 저장·재열기·일곱 동작 재생에서도 수정값이 유지됐다. 네이티브 에디터 저장 호출로 사망 미리보기 중 RESET 기본값과 메시 수정이 함께 저장되는 것도 확인했다. 마우스를 통한 수동 조작과 실제 게임 플레이는 확인하지 않았다. 실행 환경의 인증서·헤드리스 썸네일·종료 경고와 상세 범위는 [재현 기록](../characters/realta/animation/reproduction.md)에 구분했다.

## 후속 Standards 수정: 변형 간 공통 원본

생성기 제거 후 두 씬을 독립 편집하게 두면 공통 메시·동작을 두 번 수정해야 했다. 공통 뼈대·메시는 `realta_rig_base.tscn` 하나로 모으고, 기존 전투·상점 경로는 이를 상속하도록 변경했다. 같은 여섯 동작은 `clips/*.tres`로 외부화해 두 AnimationLibrary가 같은 Animation 객체를 참조한다. RESET·부활·초기 포즈의 차이·별칭·autoplay·next/blend 설정만 변형별로 유지한다. 이는 별도 도구가 아닌 Godot의 네이티브 외부 리소스 공유를 적용한 것이다. [공식 Resources 문서](https://docs.godotengine.org/en/4.5/tutorials/scripting/resources.html)

임시 수정본에서 공통 base의 머리 정점 하나를 바꾸자 두 상속 씬에 모두 반영됐고, 공통 공격 리소스의 키 수정도 양쪽에서 같은 값으로 관찰됐다. RESET과 부활 리소스는 서로 다른 객체로 유지됐다. 공통화 전후의 792프레임이 같았다. 이 비교는 원래 전투 씬의 Godot 4.7 `libraries/` 표기를 게임용 MegaDot 4.5가 읽는 `libraries` 사전으로 정규화한 기준과 비교했다. 현재 씬에서 애니메이션 라이브러리를 읽지 못한 실패와 수동 처리 설정도 수정했으며, 양쪽 변형이 실제 자동 프로세스 프레임에서 진행함을 확인했다.

현재 머리·표정 PNG의 크기는 196×140, source offset은 `(134,93.99999)`다. 과거 188×126 / `(140,99)` 안내는 현재 교체 규격에서 제거하고 `parts.json` 및 공통 base의 UV 등록을 기준으로 연결했다.
