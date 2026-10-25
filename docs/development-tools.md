# 개발 검사와 조사 명령

## 카드 미리보기 검사

```powershell
.\dev.ps1 card-tests
.\dev.ps1 card-tests -Configuration Release
```

게임 DLL과 PCK, MegaDot을 사용하는 로컬 검사다. 참조 카드 종류·강화·자기 참조의 55개 모델 상태와 화면 넘침·겹침·크기의 36개 배치 조건을 확인한다. [테스트 명세와 실행 결과 판정](../tests/card-previews/README.md)에 사용자 승인 범위와 검증하지 않는 항목을 정리했다.

## 원작 API 조사

타입의 전체 이름과 필요한 멤버를 지정한다. 기본 대상은 프로젝트에서 발견한 설치 게임의 `sts2.dll`이다. 다른 DLL은 `-AssemblyPath`, 별도로 설치한 ILSpy는 `-IlSpyPath`로 지정할 수 있다.

```powershell
.\dev.ps1 api -Type MegaCrit.Sts2.Core.Nodes.HoverTips.NHoverTipCardContainer -Member LayoutResizeAndReposition
.\dev.ps1 api -Type MegaCrit.Sts2.Core.Models.CardModel -Member HoverTips -ContextLines 12 -MaxLines 60
.\dev.ps1 api -Type MegaCrit.Sts2.Core.Models.CardModel -StartLine 120 -MaxLines 40
```

ILSpy는 저장소의 도구 폴더를 먼저 찾고, 없으면 PATH의 `ilspycmd`를 사용한다. 설치되지 않았다면 SDK의 `dotnet tool install --global ilspycmd --version 9.1.0.7988`로 준비할 수 있다. 명령은 도구를 자동 설치하지 않는다. 현재 확인한 조합은 ILSpy 9.1.0.7988, .NET SDK 9.0.318이다.

저장소의 SDK 초기화를 재사용하고, .NET 8 대상 ILSpy를 .NET 9에서 실행할 수 있도록 자식 프로세스에 `DOTNET_ROLL_FORWARD=Major`를 적용한다. 종료 후 기존 값을 복원하며, ILSpy의 자동 업데이트 조회는 끈다. 시스템 런타임이나 설정 파일을 수정하지 않는다.

`-Member`는 정규식이 아닌 멤버 이름이다. 선언을 우선 찾고, 없으면 동일 식별자의 출현 위치를 찾는다. `-Member FromCard`처럼 이름만 지정하면 `FromCard<T>()`를 포함한 제네릭·일반 선언을 함께 찾는다. 기본 출력은 각 위치 앞뒤 8줄, 전체 최대 80줄이며 `-ContextLines`와 `-MaxLines`로 조절한다. 출력 한도는 최대 200줄이다. 긴 멤버의 나머지는 `-Member`를 빼고 `-StartLine`부터 읽거나 저장된 전체 소스를 열면 된다. 이는 텍스트 탐색이며 C# 구문 분석이나 모든 오버로드의 전체 본문 추출을 보장하지 않는다.

매 실행은 새 `artifacts/api-<고유 ID>/` 폴더를 만든다. 다른 위치는 존재하지 않는 새 폴더를 `-OutputDirectory`로 지정한다. 이전 결과를 덮어쓰지 않는다. 폴더에는 다음 자료가 남는다.

- `source/`: 타입 전체의 UTF-8 C# 소스.
- `request.json`: DLL 경로·제품 버전·SHA256, 타입·멤버·출력 범위.
- 단계별 로그와 JSON: SDK·ILSpy 버전, 실제 인수, 작업 디렉터리, 종료 코드와 실패 정보.

명령은 [공통 프로세스 기록 함수](../scripts/dev-workflow.ps1)의 `Invoke-RecordedCommand`를 재사용한다. 디컴파일이 실패하면 해당 단계 로그 경로와 함께 실패한다. 타입 조회는 성공했으나 멤버가 없을 때도 실패를 알리고 전체 소스 경로를 남긴다. 제한된 출력은 조사 편의를 위한 것이며 원문을 삭제하지 않는다. 공유 문서에는 이 로컬 산출물 경로 대신 DLL 버전, 타입·멤버, 확인한 동작을 기록한다.

### 멤버 조회 자동 검사

```powershell
.\dev.ps1 api-tests
```

[사용자가 요청한 자동 검사](../tests/api-member-selection.ps1)는 설치된 게임의 `MegaCrit.Sts2.Core.HoverTips.HoverTipFactory`를 실제로 조회한다. 주변 문맥을 제외한 선언 출력에 `FromCard<T>(bool upgrade = false)`와 `FromCard(CardModel card, bool upgrade = false)`가 모두 있는지 확인한다. 위 API 조사와 같은 SDK·ILSpy·게임 DLL이 필요하며, 이 명령은 별도 로컬 검사다.

## 임시 검증 실행

XML 설정 생성 등 준비 단계가 있는 일회성 관찰은 임시 PowerShell 스크립트로 작성하고, 저장소 루트에서 다음 명령으로 실행한다. 임시 코드는 `artifacts/` 등 정식 테스트 디렉터리 밖에 두며 커밋하지 않는다.

```powershell
.\dev.ps1 observe -ScriptPath <임시 스크립트 경로>
```

스크립트는 다음처럼 `param($Context)`를 받는다. `ProjectRoot`는 저장소 루트, `OutputDirectory`는 이번 실행의 산출물 폴더, `DotNet`은 저장소 설정으로 선택한 SDK 실행 파일이다. 작업 디렉터리는 저장소 루트다.

```powershell
param($Context)

# XML 생성 등 준비 작업도 이 스크립트 안에서 수행한다.
Invoke-RecordedCommand -Executable $Context.DotNet -Arguments @('--version') `
    -LogDirectory $Context.OutputDirectory -Step 'sdk'
```

외부 프로그램은 반드시 `Invoke-RecordedCommand`로 실행한다. 각 단계에는 서로 다른 `-Step` 이름을 사용하고, 실행기가 사용하는 `observation`, `dotnet-version`, `run`은 피한다. PowerShell의 오류 중단 설정만으로는 외부 프로그램의 0이 아닌 종료 코드를 잡을 수 없기 때문이다.

실행기는 별도 PowerShell 프로세스에서 `$ErrorActionPreference = 'Stop'`을 적용한다. 처리하지 않은 XML 설정 오류·PowerShell 오류와 공통 함수가 감지한 외부 프로그램 실패는 즉시 실행을 중단한다. 프로세스 종료 코드가 0이고 완료 기록도 성공일 때만 성공을 보고한다. 오류를 숨기거나 오류 중단 설정을 완화하지 않는다. 의도적으로 실패를 관찰할 때는 예상한 오류만 처리하고, 실제 결과가 기대와 다르면 예외를 발생시킨다.

기본 출력은 새 `artifacts/observation-<고유 ID>/` 폴더다. `-OutputDirectory`로 다른 새 폴더를 지정할 수 있으며, 기존 폴더는 덮어쓰지 않는다. 프로세스 로그와 `observation.json`에 전체 출력·실행 인수·프로세스 종료 코드가, `run.json`에 입력 스크립트 경로·SHA256·SDK 경로·실행 상태·오류 위치가 남는다. 외부 명령의 단계별 로그와 JSON도 같은 폴더에 저장된다.

임시 관찰의 성공은 정식 테스트 통과나 사용자 승인으로 간주하지 않는다. 공유 문서에는 재현 조건과 확인한 결과를 기록하며, 이 로컬 실행 폴더를 필수 입력으로 삼지 않는다.
