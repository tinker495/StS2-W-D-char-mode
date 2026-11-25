# 에레닐 (Erenil)

에레닐(Erenil)의 확정 식별자, 현재 런타임 기본 구성 및 저장 호환성 기준을 정리한 문서다. 공통 용어는 [`GLOSSARY.md`](/GLOSSARY.md), 공통 설계 결정은 [`docs/adr/`](/docs/adr/)를 따른다. 시각 명세 인덱스는 [에레닐 아트 명세](erenil-art.md), 런타임 리소스 매핑은 [에셋 상태](erenil-assets.md), 문장 원고 작성 방법은 [캐릭터 문장 작성 안내](/docs/characters/character-text.md), 로컬 설치 범위는 [배포 상태](/docs/deployment-status.md)에서 확인한다.

## 기본 정보

- **표시 이름:** 한국어 **에레닐**, 영문 **Erenil** (확정)
- **현재 구현 상태:** 자리표시 기본 모델(`PlaceholderCharacterModel`) 기반. 고유 카드·유물·포션·파워·전투 메커니즘은 아직 정하지 않았으며, 전용 보상 풀은 비어 있다. 에레닐의 복장과 마법은 [전신 원화](base-art.md#외형)에 담았다.
- **임시 런타임 기본값:**
  - **시작 체력:** 70
  - **성별 설정:** `CharacterGender.Neutral`
  - **임시 대표 색상:** `#9b72cf`
  - **임시 시작 덱 (10장):** 원작 `StrikeIronclad` 5장, `DefendIronclad` 5장
  - **임시 시작 유물:** 원작 `BurningBlood`
  - **임시 시각 자료:** 전투·상점·휴식·캐릭터 선택 배경은 `PlaceholderCharacterModel` 기본(아이언클래드) 시각 자료를 사용하며, UI 아이콘은 템플릿 임시 텍스처를 사용한다.
- **고대의 존재 연동:** 1막 고대의 존재 아셀리게의 모드 캐릭터 판정에 `Erenil` 타입이 포함된다. ([아셀리게 설계](/docs/ancients/aselige-design.md))

## 식별자와 저장 호환성

- **모델·풀·기본 타입:**
  - 캐릭터 모델: `Erenil` (모델 ID `STS2WD-ERENIL`)
  - 풀: `ErenilCardPool`, `ErenilRelicPool`, `ErenilPotionPool` (ID `STS2WD-ERENIL_CARD_POOL` 등)
  - 추상 기본 타입: `ErenilCard`, `ErenilRelic`, `ErenilPotion`
- **소유 경로:**
  - 코드: [`Sts2WDCode/Characters/Erenil/`](/Sts2WDCode/Characters/Erenil/Erenil.cs) (네임스페이스 `Sts2WD.Sts2WDCode.Characters.Erenil`)
  - UI 에셋: `Sts2WD/images/charui/erenil/`
  - 캐릭터 문장 프리셋: [erenil.json](/Sts2WD/character_text/erenil.json) (`characterId`: `STS2WD-ERENIL`)
  - 기본 현지화 키: `STS2WD-ERENIL.*` (`characters.json`), `THE_ARCHITECT.talk.STS2WD-ERENIL.*` (`ancients.json`)
- **저장 호환성:**
  - 이전 자리표시 모델 ID(`STS2WD-CHARACTER_FOUR` / `CharacterFour`)로 저장된 진행 중인 런과 기록은 새 ID(`STS2WD-ERENIL`)로 자동 마이그레이션하지 않는다.
  - UI 텍스처 경로를 `character_four`에서 `erenil`로 이전하면서 각 `.import`의 `source_file`과 `.ctex` 해시를 갱신했다.
