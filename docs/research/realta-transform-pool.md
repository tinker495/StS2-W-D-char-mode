# 레아르타 시작 카드 변환 풀 조사

조회 기준일: **2026-10-18, Asia/Seoul**. 변성체의 숲에서 레아르타의 시작 타격·수비를 변환하면 Ironclad 카드가 나오는 문제를 조사하고, 사용자 합의에 따라 전용 시작 카드로 해결했다.

## 원본에서 확인한 동작

조사한 설치 `sts2.dll`의 SHA256은 `A1F9E653F1E28E4076558FEE1E60D218619CB7E057B887C6417F62C62C6D7A52`다. `.tools/ilspy/ilspycmd.exe`로 설치 DLL의 `CardFactory`를 다시 역컴파일하여 아래 로컬 역컴파일본의 핵심 분기와 대조했다. `.tools` 링크는 로컬 개발 환경의 조사 자료다.

- 수정 전 시작 덱은 `StrikeIronclad` 4장과 `DefendIronclad` 4장을 사용했다. 캐릭터의 `CardPool`이 `RealtaCardPool`이어도 이 두 카드 자체의 풀은 Ironclad다. [수정 전 시작 덱](https://github.com/tinker495/StS2-W-D-char-mode/blob/71a4f05982271d819754dbf0d39f678d22ca4293/Sts2WDCode/Characters/Realta/Realta.cs), [카드의 풀 조회](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Models/CardModel.cs), [Ironclad 풀](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Models.CardPools/IroncladCardPool.cs).
- `MorphicGrove.Group`은 두 카드를 선택하고 각각 `CardCmd.TransformToRandom`을 호출한다. `CardTransformation.GetReplacement`는 교체 카드나 후보가 지정되지 않았을 때 기본 factory를 호출한다. [이벤트](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Models.Events/MorphicGrove.cs), [변환 명령](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Commands/CardCmd.cs), [CardTransformation](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Entities.Cards/CardTransformation.cs).
- `CardFactory.GetDefaultTransformationOptions`는 소유 캐릭터의 풀이 아니라 `original.Pool`에서 후보를 얻는다. Quest 또는 Event·Ancient·Token 카드는 Colorless 풀을 사용한다. 해금·멀티 조건을 적용하고, 일반 변환에서는 Common·Uncommon·Rare만 허용하며, 같은 ID와 전투 생성 불가 카드도 해당 조건에 따라 제외한다. [factory](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Factories/CardFactory.cs), [해금·멀티 필터](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Models/CardPoolModel.cs).
- 변환 미리보기와 Astrolabe·PandorasBox도 같은 기본 후보 API를 사용한다. 따라서 시작 카드의 소속을 바로잡으면 이벤트마다 별도 패치를 넣을 필요가 없다. [미리보기](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Nodes.Cards/NTransformPreview.cs), [Astrolabe](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Models.Relics/Astrolabe.cs), [PandorasBox](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Models.Relics/PandorasBox.cs).

**수정 전 런타임 재현**: 동일 seed로 두 번 실행했을 때 캐릭터는 `Realta`이고 전용 풀에는 해금 카드 66장이 있었지만, 타격·수비의 후보는 Ironclad 카드 80장이었다. 실제 `CardTransformation.GetReplacement` 결과도 `Dark Embrace`였으며 `RED failures=2`였다. 캐릭터 등록이나 비어 있는 보상 풀이 원인은 아니었다. [수정 전 관측 로그](../../artifacts/scratch/transform-debug/before.log).

## 구현과 저장 호환

`RealtaStrike`와 `RealtaDefend`를 `RealtaCard`의 구체 카드로 만들고 시작 덱에 각각 4장씩 넣었다. 상속된 `[Pool(typeof(RealtaCardPool))]` 등록을 따르며, 두 카드 모두 Basic이므로 일반 카드 보상·무작위 변환 결과에는 포함되지 않는다. 타격은 비용 1·피해 6(강화 9), 수비는 비용 1·방어도 5(강화 8)이고 원본 Strike/Defend 태그와 `GainsBlock`을 유지한다. 임시 그림만 원본을 재사용한다. [시작 덱](../../Sts2WDCode/Characters/Realta/Realta.cs), [타격](../../Sts2WDCode/Characters/Realta/Cards/RealtaStrike.cs), [수비](../../Sts2WDCode/Characters/Realta/Cards/RealtaDefend.cs), [풀 등록](../../Sts2WDCode/Characters/Realta/Cards/RealtaCard.cs), [기본 변환 필터](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Factories/CardFactory.cs).

게임의 변환 메서드는 패치하지 않는다. 레아르타가 실제로 획득한 다른 캐릭터 카드와 무색·저주·상태 카드, 명시 결과·후보 변환도 게임 규칙을 따른다. 사용자와 합의한 카드 소유권 결정은 [ADR-0006](../adr/0006-realta-owned-starting-cards.md)에 기록했다.

기존 런은 `Player.FromSerializable` 전에 레아르타의 덱에 한해 `StrikeIronclad`·`DefendIronclad` ID를 새 ID로 바꾼다. 같은 `SerializableCard`의 강화 횟수·인챈트·속성·획득 층은 유지하고 게임의 기본 복원을 사용한다. 다른 캐릭터의 저장은 건드리지 않는다. 예전 레아르타 저장에는 공유 모델을 사용하는 타격·수비의 출처가 구별되지 않으므로, 덱에 남아 있는 해당 두 ID는 모두 이관한다. [이관 구현](../../Sts2WDCode/Characters/Realta/RealtaStartingDeckMigration.cs), [원본 카드 저장 형식](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Saves.Runs/SerializableCard.cs), [기본 카드 복원](../../.tools/sts2-decompiled/MegaCrit.Sts2.Core.Models/CardModel.cs).

## 실행한 검증

임시 재현 코드는 Git과 제품 컴파일에서 제외된 `artifacts/scratch/transform-debug/`에 있다. 실제 설치 DLL과 같은 hash의 게임을 별도 headless 프로세스로 실행하고 별도 프로필에 BaseLib와 빌드된 모드를 로드했다. 사용자 게임 설치·저장 데이터에는 쓰지 않았다. 정식 테스트·검증 자산은 작성하거나 변경하지 않았다. [임시 실행기](../../artifacts/scratch/transform-debug/run.ps1), [관측 코드](../../artifacts/scratch/transform-debug/Observation.cs).

실행 명령: `powershell -ExecutionPolicy Bypass -File .\artifacts\scratch\transform-debug\run.ps1`. 최종 결과는 `GREEN failures=0`이다. 다음은 임시 관측의 범위이며 새 제품 인수 기준이 아니다. [최종 실행 로그](../../artifacts/runtime/transform-debug-20261018/diagnostic.stdout.log).

- 시작 덱 10장이 전부 레아르타 풀이고, 전용 타격·수비가 각각 4장임을 확인했다. 실제 두 카드의 변환 후보는 레아르타 카드 62장이며 추첨 결과도 레아르타 카드였다.
- 기본/강화 수치·Strike/Defend 태그·한국어 이름과 임시 초상화 리소스 로드를 확인했다. 실제 전투에서 타격 6/9 피해, 수비 5/8 방어도, 매회 에너지 1 소모와 버린 카드 더미 이동을 확인했다.
- 강화된 시작 카드와 전투 후보, 일반 레아르타 카드, 획득한 `Anger`, 무색·저주·상태 카드, 고정 결과와 명시 후보 변환을 확인했다.
- 구형 ID로 된 저장 입력을 `Player.FromSerializable`로 복원하여 새 카드로 바뀌고 강화·Sharp 2·획득 층이 유지됨을 확인했다. 새 저장 재로딩의 멱등성과 Ironclad 저장이 이관되지 않는 것도 확인했다.
- 2인 `RunState`에서 레아르타와 Ironclad의 시작 카드가 각자 풀을 사용하고 singleplayer-only 후보가 제외됨을 확인했다. 네트워크 동기화 검증은 아니다.
- `MorphicGrove.BeginEvent`와 `GROUP` 선택지를 실행하고 자동 선택기로 타격·수비 한 장씩을 선택했다. 골드 100→0, 덱 10장 유지, 이벤트 완료를 확인했다. 두 교체 카드는 `RealtaSteelPincers`·`RealtaFathersAdvice`였다.

`dev.ps1 build`는 경고·오류 0개로 통과했다. `dev.ps1 publish`는 최초 sandbox 실행에서 Windows 인증서 저장소 접근 오류로 실패했으며, 권한을 확장해 재실행한 로컬 패키징은 성공했다. [패키징 로그](../../artifacts/realta-starters-publish-retry.log).

미실행 범위는 이벤트 UI·미리보기의 시각 확인, Astrolabe·PandorasBox 개별 실행, 실제 구형 저장 파일 재개, 네트워크 멀티 동기화다. 로컬 게임 설치에는 배포하지 않았다.
