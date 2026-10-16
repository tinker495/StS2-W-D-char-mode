---
name: build-install
description: Windows에서 이 저장소의 모드를 빌드하고 로컬 Slay the Spire 2에 설치한다. 현재 브랜치를 게임에 패치하거나 재설치하는 요청에 사용한다.
---

# 빌드 및 로컬 설치

현재 checkout을 패키징하고 게임의 `mods/Sts2WD`에 설치한다. 빌드 절차는 저장소 루트의 `dev.ps1`을 사용한다.

## 대상과 환경 확인

저장소 루트에서 시작한다. `git status --short --branch`와 `git log -1 --oneline`으로 설치할 브랜치·커밋과 기존 변경을 기록한다. 사용자가 선택한 checkout을 유지하고, 커밋되지 않은 변경도 빌드에 포함된다는 점을 완료 보고에 반영한다.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 doctor
```

`doctor`가 성공하면 출력의 `Sts2Path`, `ModsPath`, `GodotPath`를 사용한다. SDK 선택은 `dev.ps1`에 맡긴다. 게임 경로, SDK 버전, BaseLib 버전을 이 스킬에 고정하지 않는다. 의존성이 없으면 `README.md`의 준비 절차를 확인하고, 설치를 막는 항목을 해결한다.

`Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue`로 실행 여부를 확인한다. 게임이 실행 중이어도 패키징은 진행할 수 있다. 파일 교체 전에는 사용자에게 게임 종료를 요청하고, 종료될 때까지 설치를 보류한다.

## 패키징

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 publish
```

사용자가 빌드 구성을 지정했다면 `-Configuration Debug` 또는 `-Configuration Release`를 전달한다. `publish`가 C# 빌드, MegaDot 리소스 import, PCK export를 수행하므로 별도의 `build`를 반복할 필요는 없다.

프로세스의 종료 코드가 0이고, 출력 경로인 `<ModsPath>/Sts2WD`에 아래 네 파일이 모두 존재할 때만 설치로 진행한다. 실패한 실행이 남긴 파일이나 이전 패키지로 설치를 대신하지 않는다.

- `Sts2WD.dll`
- `Sts2WD.pck`
- `Sts2WD.json`
- `Sts2WD.pdb`

로그가 길면 저장소의 무시되는 `artifacts/` 아래에 저장하고, 실패 시 해당 오류 부분을 확인한다. 리소스 export가 끝났다는 메시지만으로 성공을 판단하지 않는다.

### 샌드박스에서 실패할 때

이 PC에서는 MegaDot이 샌드박스 안에서 `Failed to read the root certificate store`를 출력하고 export 종료 코드가 실패한 사례가 있다. 이 오류나 명시적인 접근 거부가 발생하면 해당 명령을 도구의 권한 상승 기능으로 한 번 재시도한다. 재시도도 실패하면 오류를 보고하고 설치를 중단한다. 시스템 실행 정책이나 인증서 설정을 바꾸어 해결하지 않는다.

## 백업 후 설치

게임 설치 경로에 쓰기 전에 패키징을 완료한다. 설치 요청은 파일 교체 권한을 포함하므로 추가 사용자 확인을 반복하지 않는다. 도구가 요구하는 샌드박스 권한은 실제 복사 명령에 적용한다.

PowerShell에서 `$ErrorActionPreference = 'Stop'`으로 작업하고 경로에는 `-LiteralPath`를 사용한다.

1. `doctor`에서 확인한 실제 경로로 원본 `<ModsPath>/Sts2WD`와 대상 `<Sts2Path>/mods/Sts2WD`를 정한다. 경로에 공백과 `&`가 있으므로 문자열로 전달한다. 파일 교체 직전에 게임이 종료되어 있는지 다시 확인한다.
2. 네 원본 파일의 존재를 모두 확인한 뒤, 기존 대상 파일 중 같은 이름의 파일을 저장소의 `artifacts/runtime/local-install-backup-<timestamp>/`에 복사한다. 매 실행마다 새 백업 디렉터리를 사용하고, 백업 경로를 출력한다. 백업이 실패하면 대상 파일을 교체하지 않는다.
3. 대상 디렉터리를 만들고 네 파일만 `Copy-Item -Force`로 설치한다. 게임 DLL, BaseLib 또는 다른 모드 파일은 이 패키지의 설치 대상이 아니다.
4. 네 파일 각각에 `Get-FileHash -Algorithm SHA256`을 실행하여 원본과 설치본의 해시를 비교한다. 누락이나 불일치가 있으면 설치 실패로 보고하고 백업 경로를 함께 알린다. 네 파일이 모두 일치할 때 설치가 완료된다.

빌드 후 `git status --short`를 확인한다. MegaDot이 이번 실행에서 새로 생성한 `.cs.uid`를 정리할 때는 시작 시점에 없었던 파일인지 먼저 확인하고 해당 파일만 지정한다. 기존 변경과 추적 중인 리소스는 보존한다.

## 완료 보고

설치한 브랜치·커밋, 게임의 설치 대상 경로, 빌드·패키징 결과, 네 파일의 해시 검증 결과와 백업 경로를 간결하게 보고한다. 실행한 테스트와 실행하지 않은 검증을 구분한다. 빌드나 파일 해시 확인을 게임 내 플레이 검증으로 표현하지 않는다.

게임 실행이나 모드 설정 변경은 요청받았을 때 수행한다. 활성화 상태를 확인하지 못했다면 게임에서 BaseLib와 Sts2WD의 활성화가 필요하다고 안내한다.
