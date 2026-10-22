# Godot에서 레아르타 전투·상점 편집하기

전투·상점은 하나의 공통 리그와 공유 동작을 사용하는 Godot 상속 씬입니다. 저장한 `.tscn`·`.tres`를 게임에서도 그대로 사용합니다. 기존 동작의 촘촘한 키를 유지했으며, 별도 변환이나 재생성은 필요 없습니다. 아래 경로는 모두 `Sts2WD/animations/realta/combat/` 기준입니다.

| 대상 | 편집 파일 |
|---|---|
| 공통 뼈대·메시·UV·가중치 | `realta_rig_base.tscn`에서 한 번 편집 |
| 공통 동작 6개 | `clips/idle_loop.tres`, `attack.tres`, `hurt.tres`, `cast.tres`, `die.tres`, `relaxed_loop.tres` |
| 전투 미리보기·개별 설정 | `realta_combat_rig.tscn` — 공통 씬 상속 |
| 상점 미리보기·개별 설정 | `realta_merchant_rig.tscn` — 공통 씬 상속 |
| 개별 부활·저장 기본값 | `clips/combat_revive.tres`, `combat_reset.tres`, `merchant_revive.tres`, `merchant_reset.tres` |
| 호출 이름·별칭 연결 | `combat_library.tres`, `merchant_library.tres` |

## 열기

저장소 루트에서 실행합니다. 프로젝트에 설정된 Godot 호환 MegaDot 에디터를 사용합니다.

```powershell
.\dev.ps1 editor -Variant both
```

`-Variant both`는 양쪽이 공유하는 base scene을 엽니다. 전투를 확인하려면 `-Variant combat`, 상점을 확인하려면 `-Variant merchant`로 해당 상속 씬을 엽니다. 다른 씬은 에디터의 FileSystem에서 더블클릭해 탭으로 추가할 수 있습니다. 직접 열 때는 루트 `project.godot`을 가져온 뒤 위 씬을 엽니다. 씬 트리에서 부위를 선택하고 `F`로 화면을 맞추세요.

게임용 MegaDot 4.5에서 저장·검사하세요. 일반 Godot 4.7로 저장한 씬의 `libraries/` 형식을 게임용 엔진에서 읽지 못하는 사례가 확인됐습니다. 최신 에디터에서 열렸다는 것만으로 게임 호환성을 판단할 수 없습니다. 저장된 AnimationPlayer의 Process Callback도 자동 Idle로 유지하세요. Manual은 임시 관찰용 인스턴스에만 사용합니다.

## 부위와 동작 수정

1. base scene 또는 변형 씬의 `AnimationPlayer`를 선택하고 아래 Animation 패널에서 동작을 고릅니다. 타임라인을 이동하거나 재생해 포즈를 확인합니다. 공통 동작 6개는 어느 변형에서 편집해도 같은 `.tres`를 수정하므로 양쪽에 반영됩니다. **Make Unique로 복제하지 마세요.**
2. 공통 구조·기준 뼈대·메시 편집은 **`realta_rig_base.tscn`에서** 합니다. `Skeleton2D` 아래 `Bone2D`와 `Polygon2D`의 Polygon/UV/가중치 도구를 사용합니다. 정적 메시·UV·삼각형·가중치는 이 노드에 한 번만 저장되므로 수정하면 두 변형의 모든 동작에 반영됩니다. 그림은 `parts/*.png`를 참조합니다. 표정과 쥔 손 그림은 texture 트랙에서 교체하세요.
3. 움직임을 고치려면 해당 관절의 `position`·`rotation` 트랙에서 기존 키를 수정하거나 키를 넣습니다. 애니메이션 트랙이 있는 속성은 기본 Inspector 값만 바꾸면 재생 중 키 값으로 돌아갑니다. 기존 키가 촘촘하므로 특정 구간 전체를 바꾸려면 그 구간의 키도 함께 선택해야 합니다.
4. 표시·겹침 순서는 리그 루트의 상태 트랙 하나로 제어합니다. `lower_mode`는 몸/사망 그림, `weapon_gripped`는 쥔 장갑의 앞뒤, `weapon_arm_front`는 무기 쪽 소매, `free_arm_back`은 반대 팔의 앞뒤를 결정합니다. `free_arm_depth_hold`는 전환 중 이전 팔 순서를 잠시 유지합니다. 컨트롤러가 이를 에디터와 실행 중에 적용하므로 노드의 Visible이나 트리 순서만 바꾸면 다음 갱신 때 되돌아올 수 있습니다. 같은 결정을 담는 별도 배열을 함께 수정할 필요는 없습니다.
5. 씬과 변경한 외부 `.tres` 리소스를 함께 저장합니다. `RESET`과 Reset On Save가 저장되는 기본값을 복원하므로 사망·공격을 미리보는 중에도 해당 포즈가 기본값으로 굳지 않습니다. 씬을 닫고 다시 열어 기본 포즈와 메시 수정이 유지되는지 확인하세요. 메시의 기준 자세나 Bone2D rest를 바꾸면 바인딩에도 영향을 주므로 단순한 동작 수정에는 애니메이션 키를 사용하세요.

`idle_loop`, `attack`, `hurt`, `cast`, `die`, `relaxed_loop`, `revive`가 실제 동작입니다. `idle`, `relaxed`, `powerup`은 같은 Animation 리소스를 공유하는 별칭이므로 한쪽을 바꾸면 연결된 별칭도 바뀝니다. 상점의 `idle`은 상점 자세를 사용합니다. `revive`와 `RESET`은 변형별 리소스입니다. 변형 씬에는 초기 포즈 차이·라이브러리·autoplay·next·blend만 남깁니다. 상속된 공통 속성을 변형에서 바꾸면 override가 생기므로, 공통 수정은 base scene에서 하고 잘못 만든 override는 Revert로 해제하세요.

에디터 미리보기에서는 반복 동작의 시작 시각을 무작위로 바꾸지 않습니다. 전환 중 팔의 이전 겹침을 잠시 유지하는 기능은 실행 중에만 적용하며, 타임라인을 직접 이동하면 선택한 포즈의 겹침을 보여줍니다. 게임의 실제 전환은 별도 재생 확인이 필요합니다.

앞뒤 포즈를 비교할 때는 Animation 패널의 Onion Skinning을 사용하세요. 이동·회전은 기존 연속 보간을 유지하고, 그림 교체·상태 전환은 Discrete 키로 편집합니다. 이번 정리에서는 동작 키를 줄이거나 새 보간으로 바꾸지 않았습니다.

`RESET`은 게임 동작이 아닌 **에디터 저장용 기본값**입니다. 전투의 `idle_loop`, 상점의 `relaxed_loop` 0초를 기준으로 만들었습니다. 움직이는 속성의 기본 자세를 의도적으로 바꿀 때는 해당 RESET 키도 함께 갱신하세요. 정적 메시·UV·가중치는 RESET에 넣지 않으므로 노드에서 수정한 값이 유지됩니다. 게임용 autoplay나 다음 동작을 RESET으로 연결하지 마세요. 자세한 근거와 현재 방식의 차이는 [공식 문서 조사·비교](../../../research/godot-native-animation-workflow.md)에 정리했습니다.

## 확인하고 게임에 반영하기

```powershell
.\dev.ps1 check
.\dev.ps1 anim all -Variant both
.\dev.ps1 publish
```

`check`는 빌드·리소스 import·기존 리그 검사를 실행합니다. `anim`은 저장된 씬을 렌더해 `artifacts/animation-*/`에 GIF와 비교 시트를 만듭니다. `publish`는 저장된 씬으로 게임 패키지를 만듭니다. 실제 로컬 게임 설치는 필요할 때 `.\dev.ps1 deploy`로 진행합니다. 상세 캡처·PCK 확인 절차는 [재현 안내](reproduction.md)를 참고하세요.

게임 wrapper의 `Visuals`, Bounds와 위치 마커, 기존 노드 경로, 애니메이션 별칭·전환, `Slash` 재질의 `resource_local_to_scene`는 유지해야 합니다. 휴식·캐릭터 선택 화면은 이번 편집 전환 범위에 포함하지 않습니다.
