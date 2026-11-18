# 아셀리게·알테아 변경 재구성

PR #29의 아셀리게 조우·유물, 알테아 카드 85종 및 캐릭터 유물 이미지를 `origin/main`의 `f48f6da` 위에 재구성했다. 기존 병합 커밋은 가져오지 않고 최종 변경을 새 브랜치에 적용했다. 캐릭터 설계의 기준 문서는 [알테아](/docs/characters/althea/althea.md)와 [아셀리게](aselige-design.md)다.

## 리뷰 반영

- 강화 상태 복원은 `CardModel.UpgradeInternal`과 `FinalizeUpgradeInternal`을 사용한다. 재편 토큰이나 불실 복귀본에 기존 강화 단계를 옮기는 일은 새로운 강화가 아니므로 짙푸른 비늘을 소모하지 않는다.
- 마검의 숙명은 복귀본에 기존 단계와 전투 인챈트를 복원한 다음 실제 신규 강화 명령을 한 번 실행한다. 짙푸른 비늘의 보존은 손으로 돌아오는 카드에 남고 영구 덱은 변경되지 않는다. 이미 인챈트된 카드에는 기존 인챈트 제한을 적용한다.
- 이성의 의지·저력·악전고투의 한·영 설명을 JSON 파싱 후 실제 개행이 되도록 수정했다.
- 사용되지 않는 `images/cards/character_two`와 `materials/character_two` 복제본을 제거했다. 런타임은 `althea` 경로만 사용한다.
- 이전 식별자·카드 수를 기록한 중복 `docs/althea-design.md`를 제거했다. 최신 규칙과 변경 이력은 `docs/characters/althea/`의 기존 문서가 소유한다.

## 네이티브 구현과 다른 리뷰 전제

현재 게임 DLL의 다음 경로를 확인했다. 초기 셔플과 다음 런 누출 지적은 아래 호출 관계와 구분해야 한다.

- `CardPile.RandomizeOrderInternal`은 `Hook.ModifyShuffleOrder(..., isInitialShuffle: true)`만 호출한다. `AfterShuffle`은 호출하지 않는다. `CardPileCmd.Shuffle`이 실제 재셔플을 마친 뒤 `Hook.AfterShuffle`을 호출한다. 정화의 숨결은 이 후자만 처리한다. `PlayerCombatState.TurnNumber`는 1에서 시작하므로 `> 0` 조건으로 초기 셔플을 구분하지 않는다.
- `ActModel.DeepCloneFields`는 새 `RoomSet`을 만든다. `NGame`과 `NCharacterSelectScreen`의 새 런 진입점은 `RunState.CreateForNewRun`에 각 막을 `ToMutable`로 복제해 전달한다. 저장 복원은 `ActModel.FromSave`에서 별도 `RoomSet`을 만든다. 아셀리게 조우 훅은 `run.Acts[0]`에 `AssertMutable`을 호출한 뒤 현재 런의 첫 막을 수정한다. 기본 캐릭터 런에서는 원래 고대 선택을 유지한다.
