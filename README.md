# StS2-W&D-char-mode

Slay the Spire 2에 캐릭터 4명을 추가하기 위한 개발 프로젝트입니다.
모드 ID와 C# 어셈블리 이름은 `Sts2WD`입니다. GitHub 저장소 이름은 특수문자 제약 때문에 `StS2-W-D-char-mode`를 사용합니다.

## 현재 상태

- `Realta`, `Althea`, `CharacterThree`, `CharacterFour` 캐릭터 뼈대
- 캐릭터별 카드·유물·포션 풀과 추상 기본 클래스
- 캐릭터별 UI 이미지 경로, 영어·한국어 캐릭터 텍스트 및 고대인 대사
- .NET 9 / MegaDot 4.5.1 / BaseLib 기반 빌드·패키징

`CharacterThree`·`CharacterFour`의 이름, 능력, 아트와 전용 카드는 아직 정하지 않았습니다. 이 둘은 템플릿의 아이언클래드 시각 자료, 타격 5장·수비 5장, 불타는 혈액, 체력 70을 사용하며 전용 보상 풀은 비어 있습니다.

레아르타(`Realta`)의 카드·유물 구성은 설계 합의([`docs/characters/realta/realta.md`](docs/characters/realta/realta.md))를 따릅니다. 타격·수비는 레아르타 전용 카드이며, 시작 덱은 타격 4장·수비 4장·비행 글레이브·담금질의 총 10장입니다. 수호는 카드별 방어도를 누적해서 높이고, 개선·유산·주문·역작·결투·용맹 효과를 지원합니다. 1단계 캐릭터 UI와 정적 전신 스탠딩은 정본 베이스([`docs/characters/realta/base.png`](docs/characters/realta/base.png))를 기준으로 적용되어 있으며, 카드·유물·파워 개별 아이콘은 순차 교체합니다. 용어는 [`GLOSSARY.md`](GLOSSARY.md), 설계 결정은 [`docs/adr/`](docs/adr/)에 있습니다.

Althea의 카드·유물 구성과 내부 식별자 결정은 설계 합의([`docs/characters/althea/althea.md`](docs/characters/althea/althea.md))를 따릅니다.

## 소개문·대화 작성

모든 모드 캐릭터의 소개문·고대 존재 대화·이벤트 문구·멀티플레이 핑·전용 유물 이야기는 [캐릭터 문장 작성 안내](docs/characters/character-text.md)를 따릅니다. `Sts2WD/character_text/`의 캐릭터별 프리셋에서 한국어·영어 원고를 함께 작성하며, 빈칸은 기존 동작을 유지합니다. 말투·가치관·관계는 [문장 설계 양식](docs/characters/character-voice-template.md)에 정리할 수 있습니다. 원고 수정 후 `publish` 또는 `deploy`와 게임 재시작이 필요합니다.

## 아트 제작

공통 화풍·치비 적용 범위는 [ARTS.md](ARTS.md), `refs → base → assets` 제작 절차는 [`character-asset-pipeline`](.agents/skills/character-asset-pipeline/SKILL.md)을 따릅니다. 캐릭터별 서술은 `docs/characters/<char>/`에 둡니다. 레아르타의 외형·팔레트·연출·전용 프롬프트는 [아트 명세](docs/characters/realta/realta-art.md), 파일 규격·런타임 연결·제작 이력은 [에셋 가이드](docs/characters/realta/realta-assets.md)에서 확인합니다. 공통 근거는 [공식 출처 조사](docs/research/sts2-art-direction-sources.md)에 있습니다.

## 이 PC에서 사용하기

PowerShell에서 이 프로젝트 폴더를 열고 실행합니다. `dev.ps1`은 프로젝트 안의 `.tools`에 있는 SDK를 자동으로 선택합니다.

```powershell
.\dev.ps1 hooks              # 이 checkout의 커밋 전 빌드 검사 활성화
.\dev.ps1 doctor             # SDK, 게임 DLL, MegaDot 경로 확인
.\dev.ps1 build              # C# 빌드
.\dev.ps1 check              # 빌드, import, 리그 기계·인스턴스 검사
.\dev.ps1 anim attack        # 작성 → import → bake → 검사 → 렌더 → 비교 시트
.\dev.ps1 publish            # DLL + PCK + manifest + PDB 생성
.\dev.ps1 deploy             # 패키징 후 백업·설치·SHA256 확인
.\dev.ps1 editor             # MegaDot 편집기 열기
```

PowerShell의 스크립트 실행 정책 때문에 실행되지 않는다면 해당 명령만 다음처럼 실행할 수 있습니다. 시스템 정책은 변경하지 않습니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\dev.ps1 build
```

`hooks`는 이 checkout의 Git `core.hooksPath`를 `.githooks`로 설정합니다. 이후 커밋할 때마다 현재 작업 트리에 대해 `dev.ps1 build`를 실행하며, 컴파일이나 기존 ModAnalyzers의 오류로 빌드가 실패하면 커밋을 중단합니다. 게임 DLL이 필요한 빌드이므로 로컬 Git hook에서 실행합니다. 새로 clone한 checkout에서도 한 번 실행하세요.

레아르타의 반복 작업은 [애니메이션 작업 절차](docs/characters/realta/animation/reproduction.md)를 따릅니다. `anim`, `check`, `publish`, `deploy`는 [리그 기계 검사](tests/rig-checks.md)와 [인스턴스 간 상태 독립성 테스트](tests/README.md)를 실행하며, 실패하면 이후 렌더·export·설치를 중단합니다. `anim`의 패키지 모드는 이미 생성한 PCK를 독립적으로 렌더합니다.

MegaDot 로그는 실행마다 `artifacts/` 아래에 따로 저장됩니다. 메모리 접근 오류 창이나 인증서 오류가 나타나면 [빌드 진단 절차](docs/agents/build-troubleshooting.md)에 따라 해당 실행의 로그와 종료 코드를 확인합니다.

`build`와 `publish` 결과는 `artifacts/mods/Sts2WD/`에 저장됩니다. `deploy`는 패키징 성공과 네 파일의 존재를 확인하고 게임 실행 중에는 설치를 중단합니다. 기존 네 파일은 `artifacts/runtime/local-install-backup-<UTC 시각>-<고유 ID>/`에 백업하고, 게임의 `mods/Sts2WD`에 설치한 뒤 SHA256을 비교합니다. 다른 모드 파일은 복사 대상이 아닙니다. 설치 도중 실패하면 출력된 백업 경로와 현재 설치 파일을 확인하세요. 자동 롤백은 하지 않습니다.
코드만 바꿨으면 `build`, 텍스트·이미지 등을 바꿨으면 `publish`가 필요합니다. 게임에 반영하려면 `deploy`를 사용합니다.

`publish`와 `deploy`는 실행마다 `artifacts/runtime/packaging-<UTC 시각>-<고유 ID>/`에 진단 자료를 남깁니다. `run.json`에는 브랜치·커밋·작업 트리 변경 목록·경로·실행 결과가, 단계별 JSON에는 실행 파일·인수·작업 디렉터리·종료 코드가 저장됩니다. 단계별 UTF-8 로그와 버전 로그로 SDK 및 MegaDot 출력도 확인할 수 있습니다. 설치 성공 기록에는 백업 경로와 네 파일의 해시가 포함됩니다.

리그 검사 두 단계는 같은 진단 폴더에 `*.stdout.log`, `*.stderr.log`, `*.engine.log`, `*.exitcode`를 남깁니다. 두 PASS 메시지와 종료 코드 0을 모두 확인해야 검사 성공입니다.

실행 환경을 알고 있다면 `-ExecutionContextLabel local`, `sandbox`, `escalated`로 기록할 수 있습니다. 기본값은 `unspecified`이며 이 옵션은 권한을 부여하거나 자동 판별하지 않습니다. 진단 자료에는 로컬 경로와 도구 출력이 포함되므로 공유 전에 내용을 확인하세요.

패키징 실패 시 해당 실행의 파일은 설치하지 않고 실패 단계 로그부터 확인합니다. 다시 성공했더라도 원인 수정이 입증된 것은 아닙니다. 작업 복구 여부와 원인 확인 여부를 구분해 기록하세요. 설치 후 게임에서 BaseLib와 이 모드를 활성화해야 하며, 파일 해시 일치는 게임 내 동작 확인을 대신하지 않습니다.

IDE에서는 `Sts2WD.sln`을 엽니다. SDK 경로를 직접 지정해야 한다면 `.tools/dotnet/dotnet.exe`를 선택합니다. Godot용 솔루션은 `.sln` 형식을 유지합니다.

## 새 PC에서 준비하기

1. Slay the Spire 2와 [BaseLib Workshop 모드](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127)를 설치합니다.
2. [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)를 설치합니다. `global.json`은 9.0.318과 이후 9.0.3xx 패치를 허용합니다.
3. [MegaDot 4.5.1](https://megadot.megacrit.com/)을 설치합니다. 현재 사용 버전은 `4.5.1-m.14`입니다.
4. `local.props.example`을 `local.props`로 복사하고 게임·MegaDot 경로를 지정합니다.
5. `.\dev.ps1 hooks`로 커밋 전 빌드 검사를 활성화하고, `.\dev.ps1 restore`와 `.\dev.ps1 publish`를 실행합니다.

이 PC의 `.tools/`에는 SDK, MegaDot, NuGet 캐시, 원본 템플릿 및 참고 소스가 들어 있습니다. 이 폴더, 빌드 결과, 게임 DLL, `local.props`는 Git에 포함하지 않습니다. MegaDot은 `_sc_` 파일을 사용하여 편집기 설정도 `.tools/megadot/editor_data`에 저장합니다.

## 캐릭터 작업 위치

```text
Sts2WDCode/
  MainFile.cs
  Characters/
    Realta/                # 캐릭터 모델, 풀, Cards/Relics/Potions 기본 클래스
    Althea/
    CharacterThree/
    CharacterFour/
  Cards/                   # 공통 카드 이미지 경로 처리
  Relics/
  Potions/
  Powers/
Sts2WD/
  images/charui/realta/           # 캐릭터별 선택 아이콘, 지도 아이콘, 에너지
  images/charui/althea/
  images/charui/character_three/
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
- 구성 기준 게임 버전: 0.107.1

NuGet 버전은 프로젝트와 `packages.lock.json`에 고정합니다. 게임 또는 BaseLib를 업데이트할 때 컴파일과 게임 실행을 다시 확인하세요. 게임 DLL은 각 개발자의 로컬 설치를 참조하므로 일반 GitHub 호스팅 러너에서는 바로 빌드할 수 없습니다.

이 프로젝트는 [Alchyr의 캐릭터 템플릿](https://github.com/Alchyr/ModTemplate-StS2)에서 시작했습니다. [설치 가이드](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup)와 [BaseLib 소스](https://github.com/Alchyr/BaseLib-StS2)를 참고합니다. 포함된 템플릿 코드와 예제 이미지의 출처는 Alchyr의 템플릿이며, 별도의 프로젝트 배포 라이선스는 아직 정하지 않았습니다.
