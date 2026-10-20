# 레아르타 인스턴스 간 상태 독립성 테스트

사용자 wjdrb가 2026-10-20 회고의 자동 검증 제안을 반영하도록 요청해 추가한 테스트다. 같은 씬의 인스턴스 두 개를 생성하고 한쪽의 공격을 0.15초까지 진행한 뒤 다른 쪽의 `idle`을 재생한다. 공격 궤적이 보이는 상태에서, 다른 인스턴스의 재생 전후 `reveal` 값이 같아야 한다. 특정 렌더 결과를 정답으로 저장하지 않는다.

`combat`과 `merchant` 각각에서 두 인스턴스의 역할을 바꿔 총 네 경우를 확인한다. 게임 실행, BaseLib의 캐릭터 변환, 실제 멀티플레이 및 화면 렌더링은 이 테스트의 범위에 포함되지 않는다.

## 실행

[개발 환경](../README.md#새-pc에서-준비하기)을 준비한 뒤 저장소 루트의 PowerShell에서 실행한다. 아래는 저장소 안에 SDK를 설치한 경우이며, 별도로 설치했다면 두 실행 경로를 해당 설치 위치로 지정한다. `dev.ps1 doctor`로 프로젝트가 사용하는 경로를 확인할 수 있다.

```powershell
$ErrorActionPreference = 'Stop'
$testRepo = (Get-Location).ProviderPath
$testDotnet = Join-Path $testRepo '.tools/dotnet/dotnet.exe'
$testGodot = Join-Path $testRepo '.tools/megadot/MegaDot_v4.5.1-stable_mono_win64_console.exe'
$env:DOTNET_ROOT = Split-Path -Parent $testDotnet
$env:PATH = "$env:DOTNET_ROOT;$env:PATH"

& $testGodot --headless --editor --path $testRepo --import --quit
if ($LASTEXITCODE -ne 0) { throw 'Resource import failed.' }
& $testGodot --headless --path $testRepo --script (Join-Path $testRepo 'tests/realta_instance_isolation.gd')
$testExit = $LASTEXITCODE
if ($testExit -ne 0) { throw "Instance isolation failed (exit $testExit)." }
```

종료 코드 `0`과 `PASS: Realta instance isolation (4 cases)`가 모두 출력되어야 성공이다. 실패하면 인스턴스별 변경 전후 값과 종료 코드 `1`이 출력된다. 초기 상태를 만들지 못했거나 스크립트·리소스 오류가 발생한 실행은 성공으로 취급하지 않는다.

기존 pre-commit 훅은 C# 빌드를 수행한다. 이 테스트는 위 명령으로 별도 실행하며, 리그 생성 절차에서도 실행하도록 연결되어 있다. `tests/.gdignore`는 테스트 스크립트를 게임 리소스 import와 PCK에서 제외한다. 명시적인 `--script` 실행은 가능하다.
