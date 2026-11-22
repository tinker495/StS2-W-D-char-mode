# 레아르타의 장면과 애니메이션 리소스

전투와 상점은 [기준 원화](base.png)를 파팅한 같은 리그를 사용한다. 전투 대기는 서서 한 손으로 창을 든 자세이고, 상점 대기는 턱을 괴고 쪼그려 앉은 자세다. 장면마다 기본으로 재생하는 클립과 행동이 끝난 뒤 돌아갈 클립을 따로 연결한다. [클립별 동작](animation-art.md) · [제작 소스](animation-sources.md)

<a id="current-scene-and-property-connections"></a>

## 게임에서 불러오는 장면

| 모델 속성 | 장면 | 화면에 표시되는 내용 |
|---|---|---|
| `CustomVisualPath` | [combat.tscn](/Sts2WD/scenes/realta/combat.tscn) | `Visuals/Rig`의 전투 변형. `idle_loop` 대기와 공격, 피격, 시전, 쓰러짐, 부활을 재생한다. |
| `CustomMerchantAnimPath` | [merchant.tscn](/Sts2WD/scenes/realta/merchant.tscn) | 같은 골격·그림을 쓰는 상점 변형. 자동 재생과 `idle` 별칭은 `relaxed_loop`에 연결된다. |
| `CustomRestSiteAnimPath` | [rest_site.tscn](/Sts2WD/scenes/realta/rest_site.tscn) | `ControlRoot/Visuals`에 앉은 그림을 표시한다. 막별 불빛과 소등, 좌우 대화 위치는 휴식 장면이 담당한다. |
| `CustomCharacterSelectBg` | [char_select_bg_realta.tscn](/Sts2WD/scenes/realta/char_select_bg_realta.tscn) | 전체 화면 `Control`의 `Illustration`이 칼날을 살펴보는 그림과 눈 깜박임을 표시한다. |

[Realta 모델](/Sts2WDCode/Characters/Realta/Realta.cs)이 이 경로를 반환한다. 전투의 `Bounds`는 x −120~120, y −360~0이며 `CenterPos`, `IntentPos`, `OrbPos`, `TalkPos`, `FormVfx`는 게임의 이펙트·의도·대화 위치를 제공한다. 원화의 접지선 중앙 (576, 1581)은 게임 좌표 (0, −6.25)에 놓인다. 게임 바닥 기준은 y=−5.625이고 원화 1픽셀은 게임의 5/24픽셀에 해당한다. 앞발의 보폭과 창끝은 전투 `Bounds` 밖으로 뻗을 수 있다. [장면 변환 API](animation-api.md)

## 런타임 그림

전투와 상점에 등록된 두 장면은 `realta_body.tscn`의 본과 2048×2048 `realta_skeleton.png`를 함께 쓴다. 스켈레톤 시트에는 몸과 장비, 여섯 표정의 눈·눈썹과 입, 세 겹의 시전 마법진 파츠(55개)만 들어 있다. 표정별 눈과 입은 원화 1픽셀당 3텍셀로 보관한다. 전투와 상점의 `revive`는 별도 전신 이미지 없이 실제 골격 전체를 `Visuals:group_alpha`의 Godot `CanvasGroup` 합성 페이드로 전환해 각각 선 자세와 쪼그려 앉은 자세를 복원한다. [공통 골격](/Sts2WD/animations/realta/combat/realta_body.tscn) · [스켈레톤 시트](/Sts2WD/animations/realta/combat/realta_skeleton.png) · [전투 리그](/Sts2WD/animations/realta/combat/realta_combat_rig.tscn) · [상점 리그](/Sts2WD/animations/realta/combat/realta_merchant_rig.tscn)

초상화·에너지 카운터·선택 화면 리소스는 [UI 목록](ui-assets.md#canonical-base-and-runtime-images)에서, 로컬 설치와 게임 확인 범위는 [공통 배포 기록](/docs/deployment-status.md)에서 확인할 수 있다.
