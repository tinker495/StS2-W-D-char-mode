# 메르쿠리우스 Branch B UI·전신 적용 상태

2026-10-31 사용자가 **선택 타일 B + 차분한 눈매 수정본**을 채택했다. 승인된 9개 PNG를 정식 리소스에 복사하고 모델·장면을 연결했다. 외형과 제작 방향은 [제작 명세](art.md), 배경은 [선택 배경 상태](../selection/assets.md), 패키징·설치·검증은 [공통 배포 기록](../../../../deployment-status.md#2026-10-31-메르쿠리우스-branch-b-패키징)을 따른다.

## 파일과 연결

모든 PNG의 정식 경로는 `Sts2WD/images/charui/mercurius/`다. 채택한 검토 출력과 바이트가 같은 파일을 적용했으며, 적용 단계에서 재생성·크롭·색 보정을 하지 않았다. 현재 정식 파일과 아래 명세가 기준이다. 로컬 후보·프롬프트 없이도 적용 범위를 확인할 수 있다.

| 파일 | 규격 | 연결 |
|---|---|---|
| `character_icon.png` | 128 × 128 RGBA | `Mercurius.CustomIconTexturePath` |
| `character_icon_outline.png` | 128 × 128 RGBA | `CustomIconOutlineTexturePath`; 최종 초상화 알파에서 흰색 2 px 확장 |
| `map_marker.png` | 128 × 128 RGBA | `CustomMapMarkerPath`; 머리·모자만 보이는 수정본 |
| `big_energy.png` | 74 × 74 RGBA | `MercuriusCardPool`, `MercuriusRelicPool`, `MercuriusPotionPool`의 `BigEnergyIconPath` |
| `text_energy.png` | 24 × 24 RGBA | 위 세 풀의 `TextEnergyIconPath`; 같은 원본에서 축소 |
| `char_select.png` | 132 × 195 불투명 | `CustomCharacterSelectIconPath`; 차분한 눈매의 B안 |
| `char_select_locked.png` | 132 × 195 불투명 | `CustomCharacterSelectLockedIconPath`; B안과 같은 알파 |
| `combat.png` | 384 × 576 RGBA | 전투·휴식·상점 공용 전신 |
| `char_select_bg.png` | 1920 × 1080 불투명 | `CustomCharacterSelectBg`; [선택 배경](../selection/assets.md) |

타일 A와 수정 전 후보는 로컬 비교 자료로만 남겼다. 전신은 승인된 [공식 베이스](../../base.png) 전체를 비례 Fit 축소한 파일이다. 베이스의 포즈·비율·여백을 보존하며, 이번 눈매 수정으로 베이스를 교체하지 않았다. 에너지 두 파일은 카드 비용·본문 표시에 사용된다. 전투 화면의 큰 에너지 카운터는 기존 네이티브 표시를 유지한다.

## 장면과 네이티브 연동

`Mercurius`는 기존 `PlaceholderCharacterModel` 상속을 유지하고 BaseLib의 시각 리소스 확장 지점을 사용한다. 신규 Harmony 패치나 별도 리소스 로더는 없다.

| 모델 속성 | `Sts2WD/scenes/mercurius/` 아래 장면 |
|---|---|
| `CustomVisualPath` | `combat.tscn` |
| `CustomRestSiteAnimPath` | `rest_site.tscn` |
| `CustomMerchantAnimPath` | `merchant.tscn` |
| `CustomCharacterSelectBg` | `char_select_bg_mercurius.tscn` |

전투·상점은 384 × 576 전신을 0.625배(캔버스 240 × 360)로 표시하고 중심 위치를 (0, -152)에 둔다. 불투명 발끝이 원점 부근에 오도록 정한 정적 배치다. 전투는 `Visuals`, `Bounds`, `CenterPos`, `IntentPos`, `OrbPos`, `TalkPos`, `FormVfx`를 제공한다. Bounds는 (-120, -320)부터 (120, 8), IntentPos는 (20, -330), TalkPos는 (50, -245)다. 휴식은 `ControlRoot/Visuals`에 0.85배 전신과 Hitbox·생각 말풍선 기준점을 둔다. 새 애니메이션은 추가하지 않았다.

관찰한 StS2 DLL은 `0.1.0+59260271157f76a2896f0eab5bc6ea1245d8b314`, BaseLib은 `3.4.7+255b1531a5cb6ba7e4e4b76dc3dd2afbc4a42b23`다. `CustomCharacterModel.RegisterSceneConversions()`는 전투·휴식·상점 경로를 각각 `NCreatureVisuals`, `NRestSiteCharacter`, `NMerchantCharacter` 변환에 등록한다. 네이티브 `NCreatureVisuals._Ready()`는 Visuals·Bounds·IntentPos·CenterPos를 고유 이름으로 찾고 OrbPos·TalkPos를 선택적으로 사용한다. 이 계약과 기존 정적 장면 구성을 따라 연결했다.

## 확인 범위

초상화·마커는 48/85 px, 에너지는 24/74 px, 타일은 132 × 195, 전신은 240 × 360 비교판으로 검토했다. 타일과 배경은 전체 불투명이며 나머지 출력의 네 모서리는 투명하다. 눈매는 윗눈꺼풀과 눈썹의 긴장을 낮췄으며, 생성형 편집에 따른 미세한 선·색 차이를 포함한 수정본을 사용자가 채택했다.

빌드·패키지 리소스 로딩은 [공통 기록](../../../../deployment-status.md#2026-10-31-메르쿠리우스-branch-b-패키징)에 있다. 실제 게임의 버튼 마스크·UI 배율·바닥 위치·히트박스·멀티플레이는 검증하지 않았다. 정식 테스트·검증기·CI는 변경하지 않았다.
