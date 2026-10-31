# 메르쿠리우스 UI·전신 적용 상태

B 타일·차분한 눈매를 적용했고, 선택 타일은 목도리 아래 어깨·상체까지 이어지도록 보완했다. [외형 명세](art.md), [선택 배경](../selection/assets.md), [배포·검증 기록](../../../../deployment-status.md)을 따른다.

## 파일과 연결

PNG 경로는 `Sts2WD/images/charui/mercurius/`다.

| 파일 | 규격 | 연결 |
|---|---|---|
| `character_icon.png` | 128 × 128 RGBA | `Mercurius.CustomIconTexturePath` |
| `character_icon_outline.png` | 128 × 128 RGBA | `CustomIconOutlineTexturePath`; 최종 초상화 알파의 흰색 2 px 확장 |
| `map_marker.png` | 128 × 128 RGBA | `CustomMapMarkerPath` |
| `big_energy.png` | 74 × 74 RGBA | 카드·유물·포션 풀의 `BigEnergyIconPath` |
| `text_energy.png` | 24 × 24 RGBA | 위 풀의 `TextEnergyIconPath`; 같은 원본에서 축소 |
| `char_select.png` | 132 × 195 불투명 | `CustomCharacterSelectIconPath`; 목도리 아래 상체 포함 |
| `char_select_locked.png` | 132 × 195 불투명 | `CustomCharacterSelectLockedIconPath`; 일반 타일과 같은 알파 |
| `combat.png` | 384 × 576 RGBA | 전투·휴식·상점 공용 전신 |
| `char_select_bg.png` | 1920 × 1080 불투명 | `CustomCharacterSelectBg` |

선택·잠금 타일은 전체 마스터를 `tile-pair`로 Fit하고 각각 `#254A53`·`#777777`에 합성한다. 전신은 [base.png](../../base.png) 전체의 비례 축소본이다. 에너지 이미지는 카드 비용·본문용이며, 전투의 큰 에너지 카운터는 네이티브 표시를 사용한다.

## 장면과 네이티브 연동

`Mercurius`는 `PlaceholderCharacterModel`과 BaseLib 시각 확장 지점을 사용한다. 장면 경로는 `Sts2WD/scenes/mercurius/`다.

| 모델 속성 | 장면 |
|---|---|
| `CustomVisualPath` | `combat.tscn` |
| `CustomRestSiteAnimPath` | `rest_site.tscn` |
| `CustomMerchantAnimPath` | `merchant.tscn` |
| `CustomCharacterSelectBg` | `char_select_bg_mercurius.tscn` |

전투·상점 전신은 0.625배(240 × 360), 중심 (0, -152)로 발끝을 원점에 맞춘다. 휴식은 `ControlRoot/Visuals`에서 0.85배다. 히트박스와 각 기준점은 해당 장면이 정의한다.

StS2 `0.1.0+59260271`, BaseLib `3.4.7+255b1531`에서 `CustomCharacterModel.RegisterSceneConversions()`가 전투·휴식·상점을 `NCreatureVisuals`·`NRestSiteCharacter`·`NMerchantCharacter`로 연결함을 확인했다. 전투는 `NCreatureVisuals._Ready()`의 필수 `Visuals`·`Bounds`·`IntentPos`·`CenterPos` 계약을 따른다.

실제 크기 비교와 [선택 UI 합성 검토](../selection/assets.md#ui-검토)를 완료했다. 실제 게임 입력·바닥 배치·히트박스·멀티플레이는 미검증이다.