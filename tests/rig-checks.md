# 리그 기계 검사

사용자 wjdrb가 2026-10-20 회고 2번에서 리그 자동 검사를 요청했고, 2026-11-21 통합 `rigc` 리그 현대화에 이어 2026-11-27 사용자 KyuSeok Jung(wjdrb)의 명시적 승인(이슈 `#46` / `Q6=A`)을 거쳐 네이티브 `libspine_godot` `SpineSprite`(`.spjson` / `.spatlas`) 런타임·`guard` 클립·Track 1 `blink_loop`·`FlipX` 미러링·선택 배경 단일 클립 계약에 맞게 [check_rig.gd](check_rig.gd)를 현대화했다.

| 검사 | 기준 |
|---|---|
| `Realta.cs` 및 카드 방어 라우팅 | `AttackAnimDelay == 0.54`, `DeathAnimTime == 1.66`, `GenerateAnimator`의 8개 트리거 바인딩(`Idle`→`idle_loop`, `Relaxed`→`relaxed_loop`, `Dead`→`die`, `Attack`→`attack`, `Guard`→`guard`, `Hit`→`hurt`, `Cast`·`PowerUp`→`cast`, 미존재 `low_health_loop` 미참조), `CardPlayAnimationPatch`의 방어형 `Skill` `Guard` 라우팅 및 `CustomPlayAnimTrigger` 우선순위 |
| 런타임 마운트와 레이어 속성 | `combat.tscn`·`merchant.tscn`의 `%Visuals`가 `SpineSprite`로 마운트되고 `GroundShadow`(`show_behind_parent = true`)를 유지하며, 모든 `CanvasItem`이 `z_index=0`, `top_level=false`를 지킴. 상점 변환 뒤에는 `SpineMerchantScenePatch`가 `SpineSprite`를 `NMerchantCharacter`의 `child(0)`으로 승격 |
| 클립 목록과 Track 1 눈 깜박임 | 전투·상점 `SpineSkeletonDataResource`에 8개 클립(`attack`, `blink_loop`, `cast`, `die`, `guard`, `hurt`, `idle_loop`, `relaxed_loop`)이 로드되고, 전투 `idle_loop`에서만 Track 1 `blink_loop`가 중첩 재생되며 액션 전환 시 Track 1이 해제되고 복귀 시 자동 재개됨 |
| Spine 이벤트 큐와 본 유한성 | 24 fps 샘플링 동안 모든 `SpineBone` 월드 좌표에 `NaN`/`Inf`가 없으며, `attack`은 `0.54s`에 `attack_hit`, `guard`는 `0.18s`에 `guard_brace`(및 비전 방어막 `sigil_inner` 활성), `cast`는 `0.25s`에 `cast_release`, `die`는 `0.12s`에 `weapon_release` 이벤트를 방출 |
| 전이 복귀·사망 정지·`FlipX` | 1회성 액션(`attack`, `guard`, `hurt`, `cast`) 뒤 기본 대기 루프로 복귀하고, `die`(`2.20s`)는 `1.66s`~`2.20s` 동안 핵심 몸통 본(`pelvis`, `chest`)이 정지한 채 비반복 종단 자세를 유지하며, `FlipX`(`scale_x = -1`)에서 월드 X 좌표가 깨짐 없이 대칭 반전됨 |
| 캐릭터 선택 배경 계약 | `char_select_bg_realta.tscn`이 `Background`와 `SpineSprite` 오버레이를 마운트하고 `NSpineAutoPlayer` 계약에 맞춰 정확히 1개의 `"animation"`(`8.00s`) 클립만 보유하며, `16:9`(`1920×1080`)와 `3:2`(`1440×960`) 화면비 전환에 맞춰 스케일·위치를 갱신 |

레이어·본 좌표 검사는 각 클립을 24 fps 간격으로 샘플링해 확인한다. 연속 시간 전체나 실제 게임의 모든 전이를 수학적으로 증명하는 검사는 아니며, 시각적 품질과 동작 완성도는 별도의 시각 검토 절차를 따른다.

사용자 wjdrb의 2026-10-23 요청에 따라 이 검사와 [인스턴스 독립성 검사](README.md)는 `dev.ps1 anim`의 리소스 임포트 뒤에서만 실행하며, `<Sts2Path>/SlayTheSpire2.exe`(`template_release` GDExtension 호스트)에 임시 `*.gdextension` 매니페스트를 연결해 구동한다. `check`, `publish`, `deploy`는 이 검사를 실행하지 않는다. 종료 코드 `0`과 두 `PASS` 메시지를 모두 확인하며, 검사 실패 시 이후 렌더는 실행하지 않는다.
