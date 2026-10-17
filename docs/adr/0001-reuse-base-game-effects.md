# 판금·활력·화염과 인챈트는 기존 게임 효과를 재사용한다

레아르타의 판금·활력·화염은 각각 Eternal Armor의 `PlatingPower`, Regent Patter의 `VigorPower`, Ironclad Flame Barrier의 `FlameBarrierPower`를 그대로 사용하고, 예리·숙련·기민은 기존 인챈트 `Sharp`·`Adroit`·`Nimble`을 사용한다. 이름이 같은 전용 파워나 인챈트를 새로 만들지 않는다. 기존 효과는 다른 캐릭터·유물·포션과의 상호작용, 툴팁, 현지화가 이미 검증되어 있으므로, 같은 개념을 모드 안에서 한 번 더 소유하면 버그와 표시 차이만 늘어난다.

## Consequences

- 동작 세부는 기존 효과를 따른다. 설계 문서와 다르게 보이면 전용 효과로 우회하지 말고 차이를 보고한 뒤 결정한다. 2026-10-16에 게임 코드와 대조한 결과는 다음과 같다.
  - `FlameBarrierPower`는 소유자가 powered attack으로 피해를 받은 뒤 반격하고, 상대 진영의 턴이 끝날 때 제거된다. 설계의 "다음 내 턴 시작까지"와 같은 구간이다.
  - `VigorPower`는 한 `AttackCommand` 단위로 적용되고, 그 명령이 끝난 뒤 소모된다. 역작의 반복 공격은 이 성질에 의존한다. ([ADR-0005](0005-masterwork-repeats-effect-not-play.md))
- 일시적 예리·숙련도 기존 `Sharp`·`Adroit` 인스턴스를 그대로 쓴다. 인챈트 슬롯에 둘 이상이 필요할 때는 이들을 담는 묶음 인챈트를 슬롯에 둔다. ([ADR-0003](0003-combat-scoped-enchantments-and-legacy.md))
- 기존 효과를 이 모드의 이름(판금·활력·화염)으로 부르는 것은 카드 텍스트와 용어집의 일이다. 코드 식별자는 원래 클래스 이름을 쓴다. 판금은 2026-10-17 카드 표에 맞춰 철갑에서 이름을 바꿨다.
