# 리그 기계 검사

사용자 wjdrb가 2026-10-20 회고 2번에서 아래 검사와 publish 전 실행을 요청했다. `check_rig.gd`는 이 요청과 현재 애니메이션 명세를 검사한다. 기존 `realta_instance_isolation.gd`의 기준은 변경하지 않는다.

| 검사 | 기준 |
|---|---|
| 레이어 속성 | 전투·상점 리그의 모든 CanvasItem은 `z_index=0`, `top_level=false` |
| 앞뒤 순서 | 무기·리본을 담은 Skeleton2D는 신체와 사망 셀보다 앞, 잡고 있는 손만 무기 위, 해제된 손은 아래, Slash/CastFx는 무기와 손 위 |
| 이름과 전이 | 기존 10개 디스패치 이름과 에디터용 RESET, 변형별 idle 별칭·autoplay, action의 next와 실제 자동 복귀, 사망의 비반복·최종 자세 유지 |
| 기본값 | 애니메이션 전체에서 사용되는 속성마다 각 클립에 중복 없는 트랙과 시간 0 키가 존재 |
| 지면 | 불투명도 임계값으로 가장자리를 버리지 않고, alpha가 0보다 큰 모든 무기·리본 픽셀의 모서리를 변환해 source Y527을 넘지 않는지 확인 |
| 사망 시각 | `Realta.DeathAnimTime`과 `death-sequence.json`의 `settled_time`이 같고, 그 시점부터 종료까지 표시되는 최종 몸·무기·리본 자세가 유지 |

지면과 레이어 검사는 각 클립의 모든 키 시각 및 키 사이 1/4·1/2·3/4 지점을 관찰한다. 연속 시간 전체나 실제 게임의 모든 전이를 수학적으로 증명하는 검사는 아니다. 그림의 깊이감, 날 방향, IK 동작의 설득력, 숨은 면의 완성도는 [동작 검토 기준표](../docs/agents/cutout-motion-review.md)에 따라 별도로 검토한다.

저장소 루트에서 `powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 check`를 실행한다. C# 빌드와 import 후 기계 검사 및 기존 인스턴스 독립성 검사를 실행한다. 종료 코드 0과 두 PASS 메시지를 모두 확인한다. 각 프로세스 종료 코드와 로그는 새 `artifacts/rig-check-*` 폴더에 저장한다.

2026-10-22 사용자 wjdrb는 “RESET 추가와 해당 검사 수정 지시”를 명시했다. 이 요청에 따라 정확한 이름 목록에 RESET만 추가했으며 기존 10개 호출 이름과 나머지 검사 기준은 유지한다. RESET에도 기존의 비반복·시간 0 기본값·트랙 중복/완전성 검사를 적용한다.

`dev.ps1 anim`은 저장된 씬 import 후, `dev.ps1 publish`와 `deploy`는 PCK export 전에 같은 검사를 실행한다. 검사 실패 시 이후 단계는 실행하지 않는다. 게임 DLL과 MegaDot이 필요한 로컬 검사이며, CI 연결은 포함하지 않는다.
