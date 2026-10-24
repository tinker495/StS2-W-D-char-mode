# 알테아 유물 시안

2026-10-24 제작. [전체 비교](overview.png)는 확대 그림과 94px·48px 표시를 함께 보여준다.

| 유물 | 형태 | 파일 이름 |
|---|---|---|
| 윌드 | 검은 손잡이와 은빛 검날의 단순한 장검 | `althea_wyrd` |
| 피의 광란 | 같은 장검의 검붉은 검날과 휘감기는 붉은 기운 | `althea_blood_frenzy` |
| 투사의 완갑 | 은빛 금속 테두리와 갈색 가죽의 완갑 | `althea_fighters_bracer` |
| 크레킨 막 두브 | 검은 판금 흉갑과 어깨 보호대 | `althea_crekin_mac_dubh` |
| 선조의 인정 | 에너지 아이콘을 닮은 푸른 육각 문장과 원형 빛 | `althea_ancestors_recognition` |

각 파일의 `.png`는 94×94, `_outline.png`는 같은 크기의 흰 외곽선, `big/`의 `.png`는 256×256이다. 전체 그림을 자르지 않고 비례 축소했다. 작은 파일에는 외곽선 공간을 위해 사방에 투명 여백 2px을 두었다. 이후 사용자가 전체 적용을 승인해 같은 그림을 게임용 경로에 복사했다. 설치·검증 상태는 캐릭터 에셋 문서를 따른다.

## Production notes for agents

Tool: built-in image generation, one call per asset. Prompt set: one complete isolated object in a transparent square, strong near-black hand-painted silhouette, 2–3 broad matte value families, sparse texture, readability at 48 px, no scenery, floor shadow, pedestal, fine engraving, writing or UI frame. Subject-specific prompts follow the five rows above and the character art guide's relic section. Armor/bracer/sword photographs guide object construction, the original-game relic/potion sheet guides rendering, and the accepted energy master explicitly guides the blue Rare emblem's shape. The current-run Wyrd master anchors the upgraded Blood Frenzy variant. No old delivered relic is an input.

Use whole-canvas Fit conversion: 256 px large; 90 px small centered in a transparent 94 px canvas; derive its 2 px white alpha outline afterward. Preserve gameplay and model IDs when applying a selected version. Candidate approval and runtime wiring are separate from this delivery.
