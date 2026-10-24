# 레아르타 유물 시안

2026-10-24 제작. [전체 비교](overview.png)에서 위쪽은 확대 그림, 아래쪽은 94px·48px 표시를 확인할 수 있다.

| 유물 | 반영한 참고 형태 | 파일 이름 |
|---|---|---|
| 청동의 번견 | 오른쪽 켈트 문양의 정면 개 머리를 청동 부조로 단순화 | `realta_bronze_watchdog` |
| 쿠우 | 왼쪽의 사나운 늑대를 입체적인 금속 머리로 구성 | `realta_kuu` |
| 마녀 고깔모자 | 청록·남보라 고깔모자, 금색 곡선과 별 | `realta_witch_pointed_hat` |
| 용의 망토 | 붉은 왕실 망토, 흰 털 장식, 큰 금색 문양 | `realta_dragon_cloak` |
| 엘디르 그라므르 | 어두운 손잡이와 달궈진 붉은 검날 | `realta_eldir_gramr` |

각 이름의 `.png`는 94×94, `_outline.png`는 같은 크기의 흰 외곽선, `big/`의 `.png`는 256×256이다. 전체 그림을 비례 축소했으며 잘라내지 않았다. 작은 아이콘은 전체 캔버스를 90×90으로 줄인 뒤 사방에 투명 여백 2px을 두어 칼끝 외곽선이 잘리지 않게 했다. 15개 PNG의 크기와 투명 모서리를 확인했다. 아직 기존 게임용 이미지에 덮어쓰거나 설치하지 않은 시안이다.

## Production notes for agents

Tool: built-in image generation, one separate call per asset. New current-run hat artwork anchored the remaining exclusive objects' rendering; the newly generated Bronze Watchdog anchored Kuu's bronze family. No previous delivered relic was an input. The official `relics-potions-frame0.png` supplied contour weight, broad value grouping and small-icon density only. Original reference labels and backgrounds were excluded.

Prompt set: isolated square transparent object, bold near-black hand-painted contour, 2–3 matte connected values, readable at 48 px, entire subject with margin, no scenery, pedestal, floor shadow, lettering or frame. Subject-specific constraints are the five rows above and the canonical relic-refresh section in `realta-art.md`. The starter subjects follow the user's later correction, not the initial broom-glaive idea. Keep original master compositions intact; conversion uses the asset pipeline's Fit mode, followed by 2 px white alpha dilation for outlines. Runtime application is a separate step after the user selects the candidates.
