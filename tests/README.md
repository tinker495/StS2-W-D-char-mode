# 레아르타 인스턴스 간 상태 독립성 테스트

사용자 wjdrb가 2026-10-20 회고의 자동 검증 제안을 반영하도록 요청해 추가했고, 2026-11-21 통합 `rigc` 현대화에 이어 2026-11-27 사용자 KyuSeok Jung(wjdrb)의 명시적 승인(이슈 `#46` / `Q6=A`)을 받아 네이티브 `SpineSprite` 기준으로 현대화한 테스트다.

[spine_rig_mount.gd](/Sts2WD/scenes/shared/spine_rig_mount.gd)가 동일 아틀라스·스켈레톤 경로에 대해 `SpineSkeletonDataResource`를 정적 캐시로 공유하더라도, 각 `SpineSprite` 인스턴스의 런타임 `SpineSkeleton`·`SpineAnimationState`·슬롯 어태치먼트·이벤트 시그널은 완전히 격리되어야 한다. 같은 씬(`combat.tscn` 또는 `merchant.tscn`)의 인스턴스 두 개를 생성하고 한쪽에서 `cast`(`0.25초`, `cast_release` 이벤트 및 `sigil_core` 어태치먼트·알파 `> 0.5`), `guard`(`0.18초`, `guard_brace` 이벤트 및 `sigil_inner` 비전 방어막 활성), `attack`(`0.54초`, `attack_hit` 이벤트 및 `weapon_head` 반전 창날 교체)을 진행한 뒤 다른 쪽에서 `idle_loop`(`combat`) 또는 `relaxed_loop`(`merchant`)를 여러 스텝(`0.25초`, `0.50초`) 재생한다. 대기 인스턴스의 재생 전후로 행동 중인 인스턴스의 슬롯 어태치먼트·알파와 주요 `SpineBone`(`pelvis`, `chest`, `upper_arm_r`)의 월드 좌표·회전이 변하지 않고, 대기 인스턴스는 이벤트를 방출하거나 시길·공격 창날을 노출하지 않아야 한다.

`combat`과 `merchant` 각각에서 두 인스턴스의 역할을 바꿔 총 네 경우를 확인한다. 특정 렌더 결과를 정답으로 저장하지 않는다.

## 실행

[개발 환경](/README.md#새-pc에서-준비하기)을 준비한 뒤 저장소 루트의 PowerShell에서 [dev-workflow.ps1](/scripts/dev-workflow.ps1)의 `Invoke-WithLocalSpineExtension`과 `Get-SpineRuntimeExecutable`(`<Sts2Path>/SlayTheSpire2.exe`)을 통해 실행한다. `MegaDot` 에디터 바이너리와 게임 동봉 `template_release` `libspine_godot` DLL은 GDExtension ABI가 다르므로, 리소스 임포트는 `MegaDot`으로 수행하고 런타임 `SpineSprite` 검증은 게임 실행 바이너리로 구동한다.

```powershell
$ErrorActionPreference = 'Stop'
$testRepo = (Get-Location).ProviderPath
. (Join-Path $testRepo 'scripts/dev-workflow.ps1')
$dotnet = Initialize-DevelopmentDotNet $testRepo
$paths = Get-DevelopmentPaths $testRepo $dotnet
$spineRuntime = Get-SpineRuntimeExecutable $paths.Sts2Path
$logDir = Join-Path ([IO.Path]::GetTempPath()) 'sts2wd-rig-test'
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

& $paths.GodotPath --log-file (Join-Path $logDir 'godot-import') --headless --editor --path $testRepo --import --quit
if ($LASTEXITCODE -ne 0) { throw 'Resource import failed.' }

Invoke-WithLocalSpineExtension -TargetProjectDir $testRepo -Sts2Path $paths.Sts2Path -Action {
    & $spineRuntime --log-file (Join-Path $logDir 'spine-test') --headless --path $testRepo `
        --script (Join-Path $testRepo 'tests/realta_instance_isolation.gd') 2>&1 | ForEach-Object { $_.ToString() }
    if ($LASTEXITCODE -ne 0) { throw "Instance isolation failed (exit $LASTEXITCODE)." }
}
```

종료 코드 `0`과 `PASS: Realta instance isolation (4 cases)`가 모두 출력되어야 성공이다. 실패하면 인스턴스별 변경 전후 값과 종료 코드 `1`이 출력된다. 초기 상태를 만들지 못했거나 스크립트·리소스 오류가 발생한 실행은 성공으로 취급하지 않는다.

기존 pre-commit 훅은 Python 정적 게이트와 C# 빌드를 수행한다. 이 테스트는 위 명령 또는 `.\dev.ps1 anim`에서 실행하며, `tests/.gdignore`가 테스트 스크립트를 게임 리소스 임포트와 PCK에서 제외한다.
