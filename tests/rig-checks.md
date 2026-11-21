# 리그 기계 검사

사용자 wjdrb가 2026-10-20 회고 2번에서 리그 자동 검사를 요청했고, 2026-11-21 명시적 승인을 거쳐 통합 7-클립 rigc 전투·상점 리그 계약에 맞게 `check_rig.gd`를 현대화했다. 기존 `realta_instance_isolation.gd`의 기준은 변경하지 않는다.

| 검사 | 기준 |
|---|---|
| 레이어 속성 | 전투·상점 리그(`realta_combat_rig.tscn`, `realta_merchant_rig.tscn`)의 모든 `CanvasItem`은 `z_index=0`, `top_level=false` |
| 슬롯 그리기 순서 | `Visuals/Skeleton2D`는 항상 자식 인덱스 `0`을 유지하고, `Visuals.draw_order`는 `Visuals.orders`의 유효한 인덱스이며 `Skeleton2D` 뒤의 슬롯 자식 순서가 `orders[draw_order]`와 정확히 일치 |
| 이름과 전이 | 10개 디스패치 이름(`attack`, `cast`, `die`, `hurt`, `idle`, `idle_loop`, `powerup`, `relaxed`, `relaxed_loop`, `revive`), 변형별 `autoplay`·별칭(`idle`, `relaxed`, `powerup`), 반복 모드, 1회성 액션(`attack`, `hurt`, `cast`, `powerup`, `revive`)의 `next` 지정과 실제 자동 복귀 |
| 값 트랙 기본값 | `Animation.TYPE_VALUE` 트랙에 한해 전체 클립의 속성 합집합이 모든 클립에 중복 없이 존재하며, 각 트랙에 시간 `0.0` 기본 키가 존재(`Animation.TYPE_METHOD` 트랙은 제외) |
| 메서드 큐 계약 | `.` 대상 `Animation.TYPE_METHOD` 트랙에서 `attack`은 `0.54s`(`Realta.AttackAnimDelay`)에 `notify_cue(&"attack_hit")`, `cast`·`powerup`은 `0.25s`에 `notify_cue(&"cast_release")`, `die`는 `0.12s`에 `notify_cue(&"weapon_release")`를 호출 |
| 타이밍과 사망 정지 | `Realta.cs`의 `AttackAnimDelay == 0.54`와 `DeathAnimTime == 1.66`이 일치하고, `die`는 길이 `2.20s`·비반복·`next` 없음이며 `1.66s`~`2.20s` 구간 동안 핵심 몸통 본(`Visuals/Skeleton2D/root/pelvis` 완전 정지, `Visuals/Skeleton2D/root/pelvis/chest` 위치 완전 정지 및 회전 변화 `0.5°` 이내)이 정지 상태를 유지하고 종료 후 빈 큐로 정지 |

레이어 및 슬롯 순서 검사는 각 클립의 모든 키 시각과 키 사이 1/4·1/2·3/4 지점을 샘플링해 확인한다. 연속 시간 전체나 실제 게임의 모든 전이를 수학적으로 증명하는 검사는 아니다. 시각적 품질과 동작 완성도는 별도의 시각 검토 절차에 따라 확인한다.

사용자 wjdrb의 2026-10-23 요청에 따라 이 검사와 기존 인스턴스 독립성 검사는 `dev.ps1 anim`의 bake 뒤에서만 실행한다. `check`, `publish`, `deploy`는 실행하지 않는다. 종료 코드 0과 두 PASS 메시지를 모두 확인하며, 검사 실패 시 이후 렌더는 실행하지 않는다. 각 프로세스 종료 코드와 로그는 `anim` 출력 폴더에 저장한다. 게임 DLL과 MegaDot이 필요한 로컬 검사이며, CI 연결은 포함하지 않는다.
