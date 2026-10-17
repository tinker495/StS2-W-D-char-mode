# StS2-W&D-char-mode

Slay the Spire 2에 캐릭터 4명을 추가하기 위한 개발 프로젝트입니다.
모드 ID와 C# 어셈블리 이름은 `Sts2WD`입니다. GitHub 저장소 이름은 특수문자 제약 때문에 `StS2-W-D-char-mode`를 사용합니다.

## 현재 상태

- `Realta`, `CharacterTwo`, `CharacterThree`, `CharacterFour` 캐릭터 뼈대
- 캐릭터별 카드·유물·포션 풀과 추상 기본 클래스
- 캐릭터별 UI 이미지 경로, 영어·한국어 캐릭터 텍스트 및 고대인 대사
- .NET 9 / MegaDot 4.5.1 / BaseLib 기반 빌드·패키징

`CharacterTwo`·`CharacterThree`·`CharacterFour`의 이름, 능력, 아트와 전용 카드는 아직 정하지 않았습니다. 이 셋은 템플릿의 아이언클래드 시각 자료, 타격 5장·수비 5장, 불타는 혈액, 체력 70을 사용하며 전용 보상 풀은 비어 있습니다.

레아르타(`Realta`)는 설계 합의([`docs/characters/realta.md`](docs/characters/realta.md))에 따라 카드 27장(시작 카드 2장·수호 토큰 1장·보상 카드 24장)과 기본 유물 쿠우를 갖췄습니다. 수호는 카드별 방어도를 누적해서 높이고, 개선·유산·주문·역작·결투·용맹 효과를 지원합니다. 시각 자료와 타격·수비는 아직 임시 자료를 씁니다. 용어는 [`GLOSSARY.md`](GLOSSARY.md), 설계 결정은 [`docs/adr/`](docs/adr/)에 있습니다.

## 이 PC에서 사용하기

PowerShell에서 이 프로젝트 폴더를 열고 실행합니다. `dev.ps1`은 프로젝트 안의 `.tools`에 있는 SDK를 자동으로 선택합니다.

```powershell
.\dev.ps1 doctor             # SDK, 게임 DLL, MegaDot 경로 확인
.\dev.ps1 build              # C# 빌드
.\dev.ps1 publish            # DLL + PCK + manifest 생성
.\dev.ps1 deploy             # 패키징 후 게임 mods/Sts2WD에 복사
.\dev.ps1 editor             # MegaDot 편집기 열기
```

PowerShell의 스크립트 실행 정책 때문에 실행되지 않는다면 해당 명령만 다음처럼 실행할 수 있습니다. 시스템 정책은 변경하지 않습니다.

```powershell
powershell -ExecutionPolicy Bypass -File .\dev.ps1 build
```

`build`와 `publish` 결과는 `artifacts/mods/Sts2WD/`에 저장됩니다. `deploy`는 게임 설치 폴더에 씁니다. 게임을 닫고 실행한 뒤, 게임의 모드 설정에서 BaseLib와 이 모드를 활성화하세요.
코드만 바꿨으면 `build`, 텍스트·이미지 등을 바꿨으면 `publish`가 필요합니다. 게임에 반영하려면 `deploy`를 사용합니다.

IDE에서는 `Sts2WD.sln`을 엽니다. SDK 경로를 직접 지정해야 한다면 `.tools/dotnet/dotnet.exe`를 선택합니다. Godot용 솔루션은 `.sln` 형식을 유지합니다.

## 새 PC에서 준비하기

1. Slay the Spire 2와 [BaseLib Workshop 모드](https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127)를 설치합니다.
2. [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)를 설치합니다. `global.json`은 9.0.318과 이후 9.0.3xx 패치를 허용합니다.
3. [MegaDot 4.5.1](https://megadot.megacrit.com/)을 설치합니다. 현재 사용 버전은 `4.5.1-m.14`입니다.
4. `local.props.example`을 `local.props`로 복사하고 게임·MegaDot 경로를 지정합니다.
5. `.\dev.ps1 restore`와 `.\dev.ps1 publish`를 실행합니다.

이 PC의 `.tools/`에는 SDK, MegaDot, NuGet 캐시, 원본 템플릿 및 참고 소스가 들어 있습니다. 이 폴더, 빌드 결과, 게임 DLL, `local.props`는 Git에 포함하지 않습니다. MegaDot은 `_sc_` 파일을 사용하여 편집기 설정도 `.tools/megadot/editor_data`에 저장합니다.

## 캐릭터 작업 위치

```text
Sts2WDCode/
  MainFile.cs
  Characters/
    Realta/                # 캐릭터 모델, 풀, Cards/Relics/Potions 기본 클래스
    CharacterTwo/
    CharacterThree/
    CharacterFour/
  Cards/                   # 공통 카드 이미지 경로 처리
  Relics/
  Potions/
  Powers/
Sts2WD/
  images/charui/realta/           # 캐릭터별 선택 아이콘, 지도 아이콘, 에너지
  images/charui/character_two/
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
