# 레아르타의 장면과 애니메이션 리소스

전투와 상점은 [기준 원화](base.png)를 파팅한 공통 네이티브 `SpineSprite`(Spine `4.2`) 리그를 사용한다. 전투 대기는 서서 한 손으로 창을 든 자세이고, 상점 대기는 턱을 괴고 쪼그려 앉은 자세다. 장면마다 초기 재생 클립과 액션 종료 후 복귀할 대기 루프를 연결한다. [클립별 동작](animation-art.md) · [제작 소스](animation-sources.md)

<a id="current-scene-and-property-connections"></a>

## 게임에서 불러오는 장면

| 모델 속성 | 장면 | 화면에 표시되는 내용 |
|---|---|---|
| `CustomVisualPath` | [combat.tscn](/Sts2WD/scenes/realta/combat.tscn) | [spine_rig_mount.gd](/Sts2WD/scenes/shared/spine_rig_mount.gd)가 `%Visuals` 자리에 네이티브 `SpineSprite`와 발밑 공용 접지 그림자([ground_shadow.tscn](/Sts2WD/scenes/shared/ground_shadow.tscn))를 마운트한다. Track 0 `idle_loop` + Track 1 `blink_loop` 대기와 `attack`, `guard`, `hurt`, `cast`, `die`를 재생한다. |
| `CustomMerchantAnimPath` | [merchant.tscn](/Sts2WD/scenes/realta/merchant.tscn) | 같은 Spine 골격·아틀라스와 접지 그림자를 쓰는 상점 변형. `relaxed_loop`를 자동 재생하며 `NMerchantCharacter` 아래에서는 루트 자식으로 승격된다. |
| `CustomRestSiteAnimPath` | [rest_site.tscn](/Sts2WD/scenes/realta/rest_site.tscn) | `ControlRoot/Visuals`에 앉은 그림을 표시한다. 막별 불빛·실시간 모닥불 일렁임과 소등, 좌우 대화 위치는 휴식 장면이 담당한다. |
| `CustomCharacterSelectBg` | [char_select_bg_realta.tscn](/Sts2WD/scenes/realta/char_select_bg_realta.tscn) | 전체 화면 `Background`([char_select_bg.png](/Sts2WD/images/charui/realta/char_select_bg.png)) 위에 단일 `"animation"` 루프를 갖는 최소형 `SpineSprite` 오버레이(`NSpineAutoPlayer` 연동)를 얹어 리본 흔들림, 칼날 광휘, 눈 깜박임, 마력 입자를 재생한다. |

[Realta 모델](/Sts2WDCode/Characters/Realta/Realta.cs)이 이 경로와 `CreatureAnimator` 상태 기계를 정의한다. 전투의 `Bounds`는 x `-120`~`120`, y `-310`~`0`이며 `CenterPos`, `IntentPos`, `OrbPos`, `TalkPos`, `FormVfx`는 게임의 이펙트·의도·대화 위치를 제공한다. 원화의 접지선 중앙 `(576, 1581)`은 게임 좌표 `(0, -7.97)`에 놓인다(8비트 `α > 128` 발끝 하단 `y ≈ -9`). 게임 바닥 기준은 `y = -7.285`이고 원화 1픽셀은 게임의 `137/600`(약 `0.2283`)픽셀에 해당한다. 앞발의 보폭과 창끝은 전투 `Bounds` 밖으로 뻗을 수 있으며, 방별 `Visuals` 스케일·오프셋과 앵커는 위 링크된 `.tscn` 장면이 직접 소유한다. [장면 변환 API](animation-api.md) · [방별 스케일 계약](/docs/research/sts2-cutout-runtime-contract.md#room-scale-and-ground-line-contract)

## 런타임 리소스

전투와 상점 장면은 Spine `4.2.43` 스켈레톤 데이터([realta_skeleton.spjson](/Sts2WD/animations/realta/combat/realta_skeleton.spjson)), 아틀라스 디스크립터([realta_skeleton.spatlas](/Sts2WD/animations/realta/combat/realta_skeleton.spatlas)), 그리고 `2048×2048` Tier 1 BPTC 텍스처([realta_skeleton.png](/Sts2WD/animations/realta/combat/realta_skeleton.png))를 공유한다. 스켈레톤 시트에는 몸과 장비, 여섯 표정의 눈·눈썹과 입, 세 겹의 시전·방어 마법진, 찌르기 기류·섬광 파츠(58개)만 들어 있으며 전신 합성 스프라이트는 포함하지 않는다. 표정별 눈과 입은 원화 1픽셀당 3텍셀로 보관한다.

캐릭터 선택 화면 오버레이는 [realta_select_atlas.spjson](/Sts2WD/animations/realta/select/realta_select_atlas.spjson), [realta_select_atlas.spatlas](/Sts2WD/animations/realta/select/realta_select_atlas.spatlas), 그리고 `512×512` Tier 2 Lossy WebP 아틀라스([realta_select_atlas.png](/Sts2WD/animations/realta/select/realta_select_atlas.png))를 사용하여 배경 원화 위에 페더링된 리본 패치, 축소된 칼날 날끝 광휘, 눈 깜박임 패치, 마력 입자만 최소 구성으로 구동한다.

휴식 장면은 [기준 원화](base.png)의 색과 명암으로 그린 `500×846` 마스터를 원작 통나무 좌석에 배치한다. `84×46` 눈 깜박임 패치는 마스터의 `(238, 207)`에 겹친다. 후면 그림자와 모닥불 쪽 가장자리 빛은 인물 윤곽에 맞춘 `500×846` 마스크를 쓴다. 네 그림을 같은 `24×36` 메시로 움직여 숨을 쉴 때도 눈꺼풀과 빛이 제자리에 붙어 있고, 엉덩이와 부츠, 통나무를 짚은 손은 접점을 지킨다. 네 막은 각각 환경 색과 실시간 모닥불 일렁임을 입히며, 불이 꺼지면 가장자리 빛이 사라지고 후면 그림자가 남는다. [휴식 마스터](/Sts2WD/animations/realta/rest/rest_master.png) · [눈 깜박임 패치](/Sts2WD/animations/realta/rest/rest_blink_patch.png) · [그림자 마스크](/Sts2WD/animations/realta/rest/rest_cast_shadow.png) · [림 마스크](/Sts2WD/animations/realta/rest/rest_rim_highlight.png) · [휴식 스크립트](/Sts2WD/animations/realta/rest/realta_rest.gd)

초상화·에너지 카운터·선택 화면 리소스는 [UI 목록](ui-assets.md#canonical-base-and-runtime-images)에서, 로컬 설치와 게임 확인 범위는 [공통 배포 기록](/docs/deployment-status.md)에서 확인할 수 있다.
