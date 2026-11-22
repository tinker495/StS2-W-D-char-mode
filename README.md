# StS2-W&D-char-mode

Slay the Spire 2에 캐릭터 4명을 추가하기 위한 개발 프로젝트입니다.
모드 ID와 C# 어셈블리 이름은 `Sts2WD`입니다. GitHub 저장소 이름은 특수문자 제약 때문에 `StS2-W-D-char-mode`를 사용합니다.

## 현재 상태

- `Realta`, `Althea`, `Mercurius`, `CharacterFour` 캐릭터 모델
- 캐릭터별 카드·유물·포션 풀과 추상 기본 클래스
- 캐릭터별 UI 이미지 경로, 영어·한국어 캐릭터 텍스트 및 고대인 대사
- .NET 9 / MegaDot 4.5.1 / BaseLib 기반 빌드·패키징

`CharacterFour`의 이름, 능력, 아트와 전용 카드는 아직 정하지 않았습니다. 템플릿의 아이언클래드 시각 자료, 타격 5장·수비 5장, 불타는 혈액, 체력 70을 사용하며 전용 보상 풀은 비어 있습니다.

레아르타(`Realta`)의 카드·유물 설계와 리소스 적용 상태는 [설계 합의](docs/characters/realta/realta.md)와 [에셋 가이드](docs/characters/realta/realta-assets.md)를 따르며, 공통 용어는 [`GLOSSARY.md`](GLOSSARY.md), 설계 결정은 [`docs/adr/`](docs/adr/)에 있습니다.

알테아(`Althea`)의 설계 합의와 식별자 결정은 [`docs/characters/althea/althea.md`](docs/characters/althea/althea.md), 메르쿠리우스(`Mercurius`)의 설계 합의는 [`docs/characters/mercurius/mercurius.md`](docs/characters/mercurius/mercurius.md)를 따릅니다.

## 소개문·대화 작성

모든 모드 캐릭터의 소개문·고대 존재 대화·이벤트 문구·멀티플레이 핑·전용 유물 이야기는 [캐릭터 문장 작성 안내](docs/characters/character-text.md)를 따릅니다. `Sts2WD/character_text/`의 캐릭터별 프리셋에서 한국어·영어 원고를 함께 작성하며, 빈칸은 기존 동작을 유지합니다. 말투·가치관·관계는 [문장 설계 양식](docs/characters/character-voice-template.md)에 정리할 수 있습니다. 원고 수정 후 `publish` 또는 `deploy`와 게임 재시작이 필요합니다.

## 아트 제작

공통 화풍·치비 적용 범위는 [ARTS.md](ARTS.md), `refs → base → assets` 제작 절차는 [`character-asset-pipeline`](.agents/skills/character-asset-pipeline/SKILL.md)을 따릅니다. 캐릭터별 서술은 `docs/characters/<char>/`에 둡니다. 레아르타의 외형·팔레트·연출·전용 프롬프트는 [아트 명세](docs/characters/realta/realta-art.md), 파일 규격·런타임 연결·제작 이력은 [에셋 가이드](docs/characters/realta/realta-assets.md)에서 확인합니다. 공통 근거는 [공식 출처 조사](docs/research/sts2-art-direction-sources.md)에 있습니다.

## 이 PC에서 사용하기

PowerShell에서 이 프로젝트 폴더를 열고 실행합니다. `dev.ps1`은 프로젝트 안의 `.tools`에 있는 SDK를 자동으로 선택합니다.

```powershell
.\dev.ps1 hooks              # 이 checkout의 커밋 전 참조·빌드 검사 활성화
.\dev.ps1 doctor             # SDK, 게임 DLL, MegaDot 경로 확인
.\dev.ps1 import             # MegaDot 헤드리스 리소스 임포트 (.import·캐시 갱신)
.\dev.ps1 verify             # Python 정적 검증 5종(참조·docs 아티팩트·패키징 정책·카드 규격·PNG 투명도; SDK·게임 DLL 불필요)
.\dev.ps1 build              # Python 정적 검증 5종 후 C# 빌드 (기본 Debug)
.\dev.ps1 check              # Python 정적 검증 5종 후 잠긴 패키지 복원으로 C# 빌드 (기본 Debug)
.\dev.ps1 card-tests         # 참조 카드 55개 상태 + 화면 배치 36개 조건 검사 (기본 Debug)
.\dev.ps1 rebase             # origin/main 위로 리베이스한 뒤 기존 프리커밋 검사
.\dev.ps1 anim attack        # rig-c 소스 컴파일 → 게임 리그 반영 → import·검사·렌더
.\dev.ps1 publish            # 기본 Release 3파일(DLL + PCK + manifest) 생성 및 PCK 검사
.\dev.ps1 deploy             # Release 패키징·PCK 검사 후 백업·설치·잔존 PDB 정리·SHA256 확인
.\dev.ps1 editor             # MegaDot 편집기 열기
```

카드·API 자동 검사, 타입·멤버별 API 조사, 임시 검증 실행 방법은 [개발 검사와 조사 명령](docs/development-tools.md)에 정리했습니다. 카드 검사는 게임 DLL과 MegaDot을 사용하는 별도 로컬 명령입니다.

PowerShell의 스크립트 실행 정책 때문에 실행되지 않는다면 해당 명령만 다음처럼 실행할 수 있습니다. 시스템 정책은 변경하지 않습니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\dev.ps1 build
```

`hooks`는 이 checkout의 Git `core.hooksPath`를 `.githooks`로 설정합니다. 이후 일반 `git commit`을 실행할 때 현재 작업 트리에 대해 `dev.ps1 build`를 실행하며, Python 정적 검증 5종이나 C# 컴파일·기존 ModAnalyzers가 실패하면 커밋을 중단합니다. 게임 DLL이 필요한 빌드는 로컬 Git hook에서 실행합니다. 새로 clone한 checkout에서도 한 번 실행하세요.

리베이스는 작업 트리를 정리하고 `git fetch origin`으로 원격 정보를 갱신한 뒤 `.\dev.ps1 rebase`(다른 기준은 `-Upstream <대상>`)로 시작합니다. 리베이스 끝에 기존 `pre-commit` 검사가 자동 실행되며 자동 fetch·stash·push는 수행하지 않습니다. 일반 `git rebase`로 진행한 경우에는 완료 후 `git -c core.hooksPath=.githooks hook run pre-commit`으로 직접 검사하세요.

게임 설치 상태와 확인한 범위는 [배포 상태](docs/deployment-status.md)에 기록합니다. 각 캐릭터 에셋 문서에는 게임에서 사용하는 리소스와 연결 경로를 기록합니다.

`verify`는 Python 정적 검증 5종([참조 검사](scripts/check_tracked_references.py), [docs 아티팩트 검사](scripts/check_docs_artifacts.py), [설치 패키지·PCK 정책 검사](scripts/check_install_package.py), [카드 포트레이트 검사](scripts/check_card_portraits.py), [PNG 투명도 검사](scripts/check_png_alpha.py))을 순서대로 실행합니다. 문서 링크에는 `..` 대신 저장소 루트 기준 경로(`/docs/...`)나 같은 폴더·하위 경로를 사용하며, 세부 기준은 [문서 작성 규칙](AGENTS.md#shared-documentation)과 [설치 패키지 최소화 정책](docs/conventions/packaging/install-package.md)을 따릅니다. 새 PNG를 추가하거나 압축 티어를 바꿀 때는 `.\dev.ps1 import` → `python -X utf8 scripts/check_install_package.py --sync-params` → `.\dev.ps1 import` → `git add` 순서로 갱신한 뒤 검사합니다. GitHub Actions(`ubuntu-latest`, Python 3.12)도 push와 pull request마다 같은 5종 검사를 실행합니다.

레아르타의 반복 작업은 [애니메이션 작업 절차](docs/characters/realta/animation/reproduction.md)를 따릅니다. `anim`은 bake 후 [리그 기계 검사](tests/rig-checks.md)와 [인스턴스 간 상태 독립성 테스트](tests/README.md)를 실행하며, 실패하면 이후 렌더를 중단합니다. `check`, `publish`, `deploy`는 이 애니메이션 검사를 실행하지 않습니다. `anim`의 패키지 모드는 이미 생성한 PCK를 독립적으로 렌더합니다.

코드만 바꿨으면 `build`, 텍스트·이미지 등을 바꿨으면 `publish`가 필요하며 결과는 `artifacts/mods/Sts2WD/`에 저장됩니다. `publish`와 `deploy`는 기본적으로 `Release` 3파일(DLL·PCK·manifest, PDB 제외)을 생성·검증하고 [PCK 누수·용량 리포트](scripts/check_pck_package.py)를 출력합니다(로컬 크래시 줄번호 확인용 PDB가 필요할 때만 `-Configuration Debug` 4파일을 지정). 게임에 반영하려면 게임이 종료된 상태에서 `deploy`를 실행하세요. `deploy`는 패키징·PCK 검사 통과를 확인한 뒤 기존 설치본(기존 PDB 포함)을 `artifacts/runtime/local-install-backup-<UTC 시각>-<고유 ID>/`에 백업하고, `Release` 설치 시 잔존 PDB를 제거한 다음 `mods/Sts2WD`에 복사해 SHA256을 비교합니다(자동 롤백 없음). 실행 환경을 구분해 남길 때는 `-ExecutionContextLabel local|sandbox|escalated`를 지정할 수 있습니다.

패키징·설치 로그 구성과 진단 절차는 [패키징 출력과 로그](docs/development-tools.md#패키징-출력과-로그)와 [빌드 진단 절차](docs/agents/build-troubleshooting.md)를 참고하세요. 패키징에 실패한 산출물은 설치하지 않으며, 설치 후 게임에서 BaseLib와 Sts2WD를 활성화해야 합니다. 파일 해시 일치는 게임 내 동작 확인을 대신하지 않습니다.

IDE에서는 `Sts2WD.sln`을 엽니다. SDK 경로를 직접 지정해야 한다면 `.tools/dotnet/dotnet.exe`를 선택합니다. Godot용 솔루션은 `.sln` 형식을 유지합니다.

## 새 PC에서 준비하기

1. Slay the Spire 2와 [BaseLib Workshop 모드](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127)를 설치합니다.
2. [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)를 설치합니다. `global.json`은 9.0.318과 이후 9.0.3xx 패치를 허용합니다.
3. [MegaDot 4.5.1](https://megadot.megacrit.com/)을 설치합니다. 현재 사용 버전은 `4.5.1-m.14`입니다.
4. [`local.props.example`](local.props.example)을 복사해 `.example` 확장자를 제거하고 게임·MegaDot 경로를 지정합니다. 기본 게임 경로는 `SystemDrive` 환경변수, MegaDot 경로는 저장소의 도구 폴더를 기준으로 합니다.
5. `.\dev.ps1 hooks`로 커밋 전 참조·빌드 검사를 활성화하고, `.\dev.ps1 restore`와 `.\dev.ps1 publish`를 실행합니다.

이 PC의 `.tools/`에는 SDK, MegaDot, NuGet 캐시, 원본 템플릿 및 참고 소스가 들어 있습니다. 이 폴더, 빌드 결과, 게임 DLL, 복사한 개인 경로 설정 파일은 Git에 포함하지 않습니다. MegaDot은 `_sc_` 파일을 사용하여 편집기 설정도 `.tools/megadot/editor_data`에 저장합니다.

## 캐릭터 작업 위치

```text
Sts2WDCode/
  MainFile.cs
  Characters/
    Realta/                # 캐릭터 모델, 풀, Cards/Relics/Potions 기본 클래스
    Althea/
    Mercurius/
    CharacterFour/
  Cards/                   # 공통 카드 이미지 경로 처리
  Relics/
  Potions/
  Powers/
Sts2WD/
  images/charui/realta/           # 캐릭터별 선택 아이콘, 지도 아이콘, 에너지
  images/charui/althea/
  images/charui/mercurius/
  images/charui/character_four/
  images/card_portraits/        # 공통 카드 이미지, big/은 큰 이미지
  localization/eng/
  localization/kor/
```

예를 들어 레아르타의 카드는 `RealtaCard`를 상속하면 `RealtaCardPool`에 들어갑니다. 공유 이미지 처리가 필요해도 `Sts2WDCard`를 바로 상속하면 캐릭터 풀이 지정되지 않으므로 캐릭터별 기본 클래스를 사용합니다. 예외적으로 전투 중에만 생성되는 토큰 카드는 기본 게임처럼 `TokenCardPool`에 넣으므로 `Sts2WDCard`를 상속하고 `[Pool(typeof(TokenCardPool))]`을 붙입니다. 유물과 포션도 같은 방식입니다.

BaseLib는 구체적인 캐릭터 모델을 자동 등록합니다. 모델 ID는 네임스페이스 접두사와 클래스 이름에서 만들어집니다. 예: `Realta` → `STS2WD-REALTA`. 표시 이름은 `characters.json`의 `.title`에서 바꿉니다. 저장 데이터가 생긴 이후 모델 클래스 이름이나 모드 ID를 바꾸면 기존 저장과의 호환성에 영향을 줍니다.

카드·유물·포션 클래스 이름은 캐릭터 접두사를 붙여 모드 전체에서 고유하게 유지합니다. 해당 모델의 현지화 키를 `eng`와 `kor`에 함께 추가합니다. 예제 이미지는 임시 자료이므로 출시 전에 교체합니다.

## 의존성과 참고 자료

- `Godot.NET.Sdk`: 4.5.1
- `Alchyr.Sts2.BaseLib`: 3.4.7
- `Alchyr.Sts2.ModAnalyzers`: 0.2.1
- 구성 기준 게임 버전: 0.111.1

게임 DLL을 참조하는 프로젝트는 공통 `Directory.Build.targets`에서 설치 게임의 `Sentry`와 `Sentry.Godot`도 참조합니다. 게임 DLL의 초기화에 필요한 의존성이므로 모드와 별도 실행 호스트에 모두 적용합니다.

NuGet 버전은 프로젝트와 `packages.lock.json`에 고정합니다. 게임 또는 BaseLib를 업데이트할 때 컴파일과 게임 실행을 다시 확인하세요. 게임 DLL은 각 개발자의 로컬 설치를 참조하므로 일반 GitHub 호스팅 러너에서는 바로 빌드할 수 없습니다.

이 프로젝트는 [Alchyr의 캐릭터 템플릿](https://github.com/Alchyr/ModTemplate-StS2)에서 시작했습니다. [설치 가이드](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)와 [BaseLib 소스](https://github.com/Alchyr/BaseLib-StS2)를 참고합니다. 포함된 템플릿 코드와 예제 이미지의 출처는 Alchyr의 템플릿이며, 별도의 프로젝트 배포 라이선스는 아직 정하지 않았습니다.
