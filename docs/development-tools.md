# 개발 검사와 조사 명령

## 정적 검증 명령

프리커밋, CI 및 기계적 자동 검증 스크립트는 이식성을 위해 기본적으로 Python(표준 라이브러리 우선, 픽셀 디코딩이 필요한 경우 기존 Pillow·NumPy·OpenCV 사용)으로 작성합니다. `dev.ps1 verify`, `build`, `check`, `publish`, `deploy`와 GitHub Actions(`ubuntu-latest`, Python 3.12)는 아래 Python 정적 검증 5종을 순서대로 실행합니다(게임 DLL·.NET SDK 불필요).

1. `python -X utf8 scripts/check_tracked_references.py` — Git 추적 마크다운·코드·리소스 파일의 링크, `res://`, `.import` 짝 파일, `..` 상위 경로 금지, 로컬·무시 경로 참조 검사(표준 라이브러리 전용).
2. `python -X utf8 scripts/check_docs_artifacts.py` — `docs/`↔`Sts2WD/` 바이트 동일본, `docs/` 내부 중복 이미지, `docs/characters/` 후보·오버뷰 산출물, 인게임 적용이 끝난 `refs/<topic>/` 러프 및 고아 마크다운 검사(표준 라이브러리 전용).
3. `python -X utf8 scripts/check_install_package.py` — `Sts2WD/` 3단 `.import` 압축 티어(Tier 1 BPTC / Tier 2 Lossy WebP `0.85` / Tier 3 Lossless WebP), 소스 PNG 해상도 상한, 비PNG 래스터 금지, 비런타임 `.gdignore`·[export_presets.cfg](/export_presets.cfg)·[project.godot](/project.godot) 경계, `Sts2WD/` 미참조 PNG 정적 검사(표준 라이브러리 전용). 새 PNG 추가 또는 티어 전환 시 `.\dev.ps1 import` → `python -X utf8 scripts/check_install_package.py --sync-params` → `.\dev.ps1 import` 순으로 갱신한다.
4. `python -X utf8 scripts/check_card_portraits.py` — `Sts2WD/images/card_portraits/` 아래 모든 카드 PNG의 [card-portrait-layouts.json](/scripts/card-portrait-layouts.json) 기준 규격·PNG 인코딩·전 픽셀 불투명도 검사(기존 `.agents/skills/character-asset-pipeline/scripts/asset-pipeline.ps1 check-cards`도 이 스크립트로 위임).
5. `python -X utf8 scripts/check_png_alpha.py` — 아래 PNG 투명도 검사.

## PNG 투명도 검사

Git이 추적하는 모든 PNG에서 8비트 알파가 0~8인 픽셀은 RGBA 네 채널을 모두 0으로 저장한다. 알파 9~255의 픽셀은 색과 투명도를 보존한다. 16비트 PNG에는 같은 비율인 0~2056을 적용한다.

`dev.ps1 verify`, `build`, `check`, `publish`, `deploy`, 커밋 훅과 GitHub CI에서 같은 기준을 검사한다. 새 PNG는 Git에 추가하기 전에도 `python scripts/check_png_alpha.py <파일>`로 확인할 수 있다. 파일 인수를 생략하면 저장소 전체를 검사한다. Python에 Pillow와 NumPy가 필요하며, 투명도를 가진 16비트 PNG에는 OpenCV도 필요하다.

정리할 파일과 픽셀 수는 `python scripts/normalize_png_alpha.py`로 미리 본다. 파일 경로를 지정하면 해당 PNG만 대상으로 삼는다. C2PA 매니페스트가 있는 변경 대상은 따로 표시한다. 픽셀을 바꾸면 기존 서명의 대상 데이터도 달라진다.

`python scripts/normalize_png_alpha.py --apply <파일>`은 임시 파일을 다시 읽어 각 프레임의 저알파 픽셀과 기준보다 높은 픽셀의 RGBA를 원본에서 계산한 기대값과 대조한다. 확인을 마친 파일부터 교체하며, 파일 인수를 생략하면 Git 추적 PNG 전체에 적용한다.

## 패키징 출력과 로그

`dev.ps1 publish`와 `deploy`는 기본적으로 `-Configuration Release`(3파일: DLL·PCK·JSON manifest, PDB 제외 및 잔존 PDB 제거)로 실행되며, 로컬 스택트레이스 심볼이 필요할 때만 `-Configuration Debug`(4파일: PDB 포함)를 명시한다. 익스포트 직후 [scripts/check_pck_package.py](/scripts/check_pck_package.py)(`-Step pck-check`)가 PCK 경로·확장자·`.godot/imported/<file>` 1:1 출처 및 번들 파일 계약을 검증하고, 카테고리·텍스처 포맷별 용량 요약을 콘솔과 `artifacts/runtime/packaging-.../pck-report.json`에 기록한다. 전체 정책은 [설치 패키지 최소화 컨벤션](/docs/conventions/packaging/install-package.md)을 따른다.

각 명령의 종료 결과와 전체 로그 경로를 표시하며, 표준 출력·오류 전체와 단계별 JSON 기록은 실행마다 생성하는 `artifacts/runtime/packaging-...` 폴더에 보존한다. 화면에는 진단 메시지를 최대 5줄, 실패 시 마지막 로그를 최대 8줄 표시하며 한 줄은 최대 400자로 줄인다. 종료 코드가 0이어도 인증서 저장소 접근 오류 같은 엔진 진단은 표시한다. 이는 출력 요약이며 기존 성공·실패 판정은 유지한다.

실패를 조사할 때는 `run.json`과 해당 단계의 JSON부터 읽고, 그 단계의 로그에서 필요한 부분만 조회한다. SDK·MegaDot 버전도 각각의 로그에 남는다. 설치와 재시도 절차는 [빌드·설치 스킬](/.agents/skills/build-install/SKILL.md)을 따른다.

## 카드 미리보기 검사

```powershell
.\dev.ps1 card-tests
.\dev.ps1 card-tests -Configuration Release
```

게임 DLL과 PCK, MegaDot을 사용하는 로컬 검사다. 참조 카드 종류·강화·자기 참조의 55개 모델 상태와 화면 넘침·겹침·크기의 36개 배치 조건을 확인한다. 이어서 현재 리소스를 `dev.ps1 publish`로 패키징하고, 원작 카드·툴팁 씬을 실제로 초기화하는 18개 통합 조건을 검사한다. 전사의 타격은 자신의 강화 상태에 맞는 시아르쿤 한 장, 대상 미정인 재편은 결과 4종을 표시해야 한다. [테스트 명세와 실행 결과 판정](/tests/card-previews/README.md)에 사용자 승인 범위와 검증하지 않는 항목을 정리했다.

통합 검사는 창 없이 실행하며 저장 I/O는 메모리로 격리한다. 원작 카드의 `_Ready`와 `Reload`는 그대로 실행하므로 MegaDot의 카드 노드 이름 변경 후에도 크기·화면 범위·겹침을 검사한다. 패키지 생성은 로컬 개발 산출물을 갱신하며 게임 설치를 수행하지 않는다.

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

명령은 [공통 프로세스 기록 함수](/scripts/dev-workflow.ps1)의 `Invoke-RecordedCommand`를 재사용한다. 디컴파일이 실패하면 해당 단계 로그 경로와 함께 실패한다. 타입 조회는 성공했으나 멤버가 없을 때도 실패를 알리고 전체 소스 경로를 남긴다. 제한된 출력은 조사 편의를 위한 것이며 원문을 삭제하지 않는다. 공유 문서에는 이 로컬 산출물 경로 대신 DLL 버전, 타입·멤버, 확인한 동작을 기록한다.

### 멤버 조회 자동 검사

```powershell
.\dev.ps1 api-tests
```

[사용자가 요청한 자동 검사](/tests/api-member-selection.ps1)는 설치된 게임의 `MegaCrit.Sts2.Core.HoverTips.HoverTipFactory`를 실제로 조회한다. 주변 문맥을 제외한 선언 출력에 `FromCard<T>(bool upgrade = false)`와 `FromCard(CardModel card, bool upgrade = false)`가 모두 있는지 확인한다. 위 API 조사와 같은 SDK·ILSpy·게임 DLL이 필요하며, 이 명령은 별도 로컬 검사다.

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

임시 관찰 결과는 작업 보고에 남긴다. 공유 문서에는 현재 적용된 동작과 재현 절차를 기록한다. [공유 문서 작성 규칙](/AGENTS.md#shared-documentation)

### Godot 경로 조회와 오프스크린 캡처

- **MegaDot 경로 조회**: MegaDot 실행 파일 경로는 로컬 설정 파일([local.props.example](/local.props.example) 참고)이나 고정 경로를 직접 가정하지 않고, [dev-workflow.ps1](/scripts/dev-workflow.ps1)의 `Initialize-DevelopmentDotNet` + `Get-DevelopmentPaths`(`$paths.GodotPath`, MSBuild 프로퍼티 조회) 또는 `.\dev.ps1 doctor`로 확인한다.
- **오프스크린 프레임 캡처**: `SubViewport`와 `RenderingServer.frame_post_draw`를 사용하는 일회성 프레임 캡처 스크립트는 `--headless`로 실행하면 더미 렌더러에서 `frame_post_draw`가 발생하지 않아 무한 대기한다. `dev.ps1 anim`과 동일하게 `--rendering-method gl_compatibility --position -10000,-10000 --log-file <워크스페이스 로그>`로 실행한다(`--log-file`로 워크스페이스 로그를 지정해야 권한 제한 환경에서 기본 AppData 로그 순환 중 발생하는 시작 크래시를 피할 수 있다).
- **좁은 화면비(`16:10` / `3:2`)·Z-order 및 초기 로드 히치 관찰**: 커스텀 전투 UI·오버레이·대형 리그/VFX 씬을 추가하거나 변경할 때는 `dev.ps1 observe` 캡처 스크립트에서 기본 `1920x1080`(`16:9`) 외에 `1920x1200`(`16:10`) 및 `1440x960`(`3:2`) `SubViewport` 해상도를 함께 렌더해 좌·우측 적 체력바, 손패, 상점 매대와의 겹침(Z-order) 여부를 확인한다. 같은 스크립트에서 `Time.get_ticks_usec()`로 첫 `PackedScene.instantiate()` + 첫 프레임 드로우 소요 시간(ms)과 이후 평균 프레임 처리 시간을 분리 기록해 셰이더/텍스처 재로드 히치가 없는지 점검한다.

## 인게임 개발자 콘솔 QA 도우미 (`wd`)

모드가 로드된 상태에서 백틱(`` ` ``) 키로 여는 기본 개발자 콘솔(`DevConsole`)은 [WdConsoleCmd.cs](/Sts2WDCode/DevConsole/WdConsoleCmd.cs)(`AbstractConsoleCmd` 파생)를 자동 등록한다. 일반 런이나 게임플레이 수치에는 개입하지 않으며 탭 자동완성을 지원한다.

```text
wd cards [char] [all|attack|skill|power|rare|ancient|missing-art] [upgraded]
wd relics [char]
wd status [char]
```

- `wd cards`: 지정 캐릭터(생략 시 현재 플레이 중인 모드 캐릭터)의 조건에 맞는 카드를 손패 여유분만큼 `Hand`에, 나머지는 `Draw`(비전투 중에는 `Deck`)에 일괄 생성한다. `missing-art` 필터는 공용 폴백 이미지를 사용 중인 미적용 카드만 선별하며, `upgraded`를 붙이면 강화 상태로 생성한다.
- `wd relics`: 지정 캐릭터 전용 유물 중 미보유 유물을 일괄 획득해 상단 유물 바와 툴팁·외곽선을 즉시 검수한다.
- `wd status`: 지정 캐릭터(또는 전체 모드 캐릭터)의 카드 전용 원화 적용 수, 유물·포션 수, 4대 씬 리소스 존재 여부를 콘솔에 요약 출력한다.

## PR 자료 보관

PR에만 사용하는 캡처·비교 렌더·설명 메모·일회성 렌더 스크립트는 생성할 때부터 `artifacts/pr-<작업명>/`에 둔다. 이 폴더는 기존 `.gitignore`의 `artifacts/` 규칙으로 제외된다. 제품에 필요한 그림과 재현에 필요한 공유 명세는 기존 제품·문서 경로에 둔다.

스테이징 전에 PR 자료마다 다음 두 명령으로 확인한다. 예시 경로를 실제 자료 경로로 바꾼다. 첫 명령은 무시된 경로를 출력하고, 두 번째 명령은 아무것도 출력하지 않아야 한다. 커밋할 제품·문서 파일은 경로를 명시해 스테이징한다.

```powershell
$prEvidence = '<artifacts 아래의 PR 자료 파일 경로>'
git check-ignore -- $prEvidence
git ls-files -- $prEvidence
```

이미지는 GitHub 첨부로 PR에 연결한다. 업로드 권한이나 도구 제한으로 첨부할 수 없으면 로컬 결과를 완료 보고에 제시하고 제한을 설명한다. 첨부를 대신하려고 PR 전용 자료를 커밋하지 않는다.
