# 메르쿠리우스 선택 배경 적용 상태

사용자가 채택한 차분한 눈매 수정본을 적용했다. [외형 명세](selection-art.md), [UI 파일 매핑](ui-assets.md), [배포 기록](/docs/deployment-status.md)을 따른다.

| 항목 | 상태 |
|---|---|
| 파일 | `Sts2WD/images/charui/mercurius/char_select_bg.png` |
| 규격 | 1920 × 1080, 전체 불투명 |
| 모델 | `Mercurius.CustomCharacterSelectBg` |
| 장면 | `Sts2WD/scenes/mercurius/char_select_bg_mercurius.tscn` |
| 표시 | 전체 영역 Control/TextureRect, `KEEP_ASPECT_CENTERED` (`stretch_mode = 5`) |
| 변환 | 1672 × 941 전체 그림을 Fit. 가장자리 `#142633`, 크롭 없음 |

## UI 검토

2026-11-12 StS2 `0.1.0+59260271`의 `res://scenes/screens/character_select_screen.tscn`을 MegaDot `4.5.1.m.14`로 렌더링했다. 1920 × 1080에서 현재 한국어 이름·소개·시작 유물 설명, 실제 타일·확인 버튼과 원작 폰트·정보판·마스크를 사용했다.

얼굴과 연주 접점은 글자에 가리지 않는다. 플루트 끝과 밝은 소매가 제목·유물 설명 뒤에 일부 오지만 정보판과 글자 윤곽선으로 읽을 수 있었고, 선택 타일과 확인 표시도 식별 가능했다. 배경 수정은 필요하지 않았다.

이는 로비·입력 콜백을 제외한 네이티브 장면 합성이며 실제 게임 캡처는 아니다. 다른 해상도·UI 설정·멀티플레이는 미검증이다.