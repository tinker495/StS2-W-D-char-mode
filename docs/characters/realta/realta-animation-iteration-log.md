# Realta 애니메이션 개선 기록

2026-10-19. `base.png`와 현재 `combat.png`에서 출발해 공식 애니메이션과 직접 비교하며 반복했다. v12에서 전투·상점·휴식·선택 화면 네 씬을 **실제 런타임 씬에 적용**했고, 모드 빌드와 PCK export, 배포 PCK만으로 네 씬을 불러 렌더하는 관찰까지 마쳤다. 게임 안에서의 실제 전투·휴식·게임오버 재생은 아직 보지 못했고, 사용자 시각 승인도 받지 않았으므로 **공식 수준 달성을 이 문서가 판정하지는 않는다.** 설치본은 변경하지 않았다. 런타임 파일과 재현 절차는 [animation/README.md](animation/README.md)에 있다.

재사용 절차는 [Codex 이미지 툴 기반 지침](../../agents/codex-cutout-rig.md), 공식 관찰 근거는 [애니메이션 비교 연구](../../research/sts2-official-animation-comparison.md), 연결 계약은 [런타임 조사](../../research/sts2-cutout-runtime-contract.md)에 기록했다. 초기 결과는 [v1 기록](realta-cutout-trial.md)에 남겨 당시 관찰과 후속 개선을 구분한다.

## 비교 기준과 관찰 범위

설치판 `v0.107.1 / 59260271`의 Ironclad와 Silent를 MegaDot `4.5.1.m.14.mono.custom_build`의 실제 Spine 확장으로 렌더했다. 처음에는 캐릭터별 전투 5종·상점 1종·휴식 3종·선택 1종에서 6시점씩 총 120장을 관찰했다. 후속 비교에서는 전투 5종을 24fps로 총 340장(Ironclad 196장, Silent 144장) 렌더하고, 각 캐릭터의 idle 기준으로 고정한 카메라를 다른 동작에도 유지했다. 공격 도중 카메라를 맞춰 움직임을 크게 보이게 하지 않았다.

공식은 공격의 준비·타격·복귀가 비대칭이고, 피격은 빠른 반동 뒤 정리되며, 사망은 전신이 지면에 닿는다. 상점은 팔·무기·자세가 이완되고, 휴식은 전투와 다른 앉은 원화를 쓴다. 휴식의 세 Act는 같은 기본 자세에 조명이 달라진다. 선택 화면은 별도 대형 구도와 미세 움직임·입자를 갖는다. 이는 관찰한 두 캐릭터와 버전의 근거이며 모든 공식 캐릭터의 제작 규칙으로 일반화하지 않는다.

Realta는 같은 렌더러에서 6개 클립을 24fps, 런타임 전투 배율인 `0.625`로 총 386장 렌더했다. 별도 확대 관찰은 원본 해상도의 결함을 찾는 데만 사용했다. GIF는 이 native PNG를 인코딩한 것이고, 동작 뒤 0.4초 확인용 정지가 붙은 비반복 클립은 원래 클립 길이와 구분한다. 비교 이미지만으로 정상 속도의 감각·UI 배치·이벤트 전달까지 검증했다고 주장하지 않는다.

## 반복별 변경과 외부 사례

각 행은 외부 자료를 현재 결함에 대조한 제작 판단이다. 숫자는 작업량 관찰이며 새 합격 기준이나 자동 테스트가 아니다. 정밀한 총 소요 시간과 API 비용은 별도 계측하지 않았다.

| 후보 | 직접 관찰한 문제 | 확인한 외부 사례와 선택 | 실제 변경·재사용 | 결과와 남은 문제 |
|---|---|---|---|---|
| v1 | 생성 atlas에서 얼굴·무기 비율이 바뀌고 어깨·커프스가 중복. 네 관절 클립만으로 전체 동작 부족 | [Godot cutout](https://docs.godotengine.org/en/4.5/tutorials/animation/cutout_animation.html)의 관절·pivot·Z 구조를 적용 | 이미지 툴 1회, 12파츠, 4클립. API 키 없이 Codex 내장 툴 사용 | native 로딩은 확인했으나 원본 정체성과 동작 품질이 부족해 후속 제작의 원화로 채택하지 않음 |
| v2 | 큰 회전에서 관절·코트 빈 면 노출. source mask에 이웃 파츠 픽셀 포함 | [Esoteric 파츠 준비](https://eu.esotericsoftware.com/blog/How-to-cut-your-assets-for-animation)의 숨은 면·둥근 겹침, [Spine IK](https://esotericsoftware.com/spine-ik-constraints)의 발 target을 검토 | 원본 visible RGBA를 15파츠에 한 번씩 배정. 가려진 몸통·코트 2회, 휴식 원화 1회 생성. 골반·두 다리·독립 발, 6클립 및 0.15초 공격 시점 구성 | 정지 재조립의 RGBA 차이 bbox는 없음으로 관찰. 그러나 이것은 mask의 의미적 정확성을 보장하지 않았고 움직이면 털·짚 조각이 잘못 따라감 |
| v3 | 강체 조각 사이 틈과 급격한 접힘 | [Godot skeleton](https://docs.godotengine.org/en/4.5/tutorials/animation/2d_skeletons.html)의 내부 vertex·weight, [Spine mesh](https://esotericsoftware.com/spine-meshes)의 변형 구조 확인 | 기존 mask·pivot·클립 재사용, 이미지 생성 0회. 3,577점의 공통 변형장을 14 texture layer에 적용. native 36시점 렌더 | 이음새 일부 개선. 2D상 가까운 팔과 코트가 연결되어 팔을 들면 옷이 늘어나는 새 결함. 전면 grid 복제로 총 96,768 triangle |
| v4 | 코트에 남은 짚·다리에 붙은 털, 팔이 당기는 코트, 뒤엉킨 사망 끝 자세 | 위 파츠 가이드의 단일 소유와 [Godot cel 혼합](https://docs.godotengine.org/en/4.5/tutorials/animation/cutout_animation.html)을 재검토 | 원본 픽셀의 소유만 교정. 팔·다리 체인별 weight 제한, coat/head 독립. 실제 alpha 영역만 native Skeleton2D/Polygon2D로 출력. 사망 끝 원화 1회 생성 | 2,776 triangle까지 축소한 시점 관찰. cast의 큰 천 늘어짐은 제거. 코트 보충 원화의 접합 흔적과 사망 attachment의 순간 전환은 남음. triangle 감소가 FPS 개선을 증명하지는 않음 |
| v5 | 여러 보충 조각의 코트 이음, 약한 공격 방향, idle과 비슷한 상점, 모자 전체의 동시 운동 | [Spine mesh](https://esotericsoftware.com/spine-meshes)의 topology 재사용, [Spine secondary motion](https://esotericsoftware.com/spine-physics-constraints)의 관성·정리, [Godot AnimationPlayer](https://docs.godotengine.org/en/4.5/classes/class_animationplayer.html)의 실제 track 재생 확인 | 코트 하나만 1회 재생성하고 나머지 visible 원본 유지. 코트 배치 명시, 가까운 pauldron을 torso로 이동. 0.15초 전방 공격, 몸 가까운 상점 grip, 별도 hat-tip bone. 14 mesh 1,741점/2,798 triangle. 6클립 native AnimationPlayer, 36시점+386프레임 렌더 | 코트 외곽과 cast가 연결되고 상점 자세가 구분됨. 전체 atlas 재생성을 피함. 코트는 원본 픽셀 보존 대상에서 제외된 새 후보임. 사망의 무릎·attachment 전환, 표정 변화, 공격의 체중 이동은 추가 검토 필요 |
| v6 진행 | 좋은 사망 끝 그림만으로 서기→눕기 전환이 완성되지 않음. 손을 놓으면 원래 가려진 자루가 끊김 | Godot의 cutout/cel 혼합과 Esoteric의 숨은 면 준비를 전환 앞뒤 프레임에 다시 대조 | 중간 자세 1회, 자루 보충 1회 생성. 기존 rig·clip·최종 자세 재사용. 0.42초 무기 놓기, 0.5초 중간 pose, 1.13초 최종 pose. 첫 386프레임에서 떠 보이던 중간 자세를 낮추고 붕괴를 앞당겨 다시 386프레임 렌더. 자루 보충 후 사망 53프레임 재렌더 | 공중에 걸려 보이는 구간을 낮추고 자루 빈 면은 연결. 그러나 0.5초의 다리·코트 교체, 1.13초의 눕는 자세 교체는 여전히 차이가 보임. 공식 수준의 연속적인 사망으로 승인하지 않음 |
| v7 | 중간 그림과 마지막 그림 사이의 두 번째 교체가 튐 | [Spine weights](https://esotericsoftware.com/spine-weights)의 관절별 가중치·동작 범위 관찰을 적용 | 중간 그림 바이트 유지, 884점/1,554 triangle/6 bone으로 상체가 내려가게 구성. 두 번째 원화 교체 제거. 모자 지면 침범을 보고 머리 각도를 고쳐 사망 53장씩 두 번 렌더. 이미지 생성 0회 | 손은 약 1초부터 목표 바닥 좌표에 도달해 유지됨. 시작부터 고정된 손이라고 해석하지 않음. 첫 0.5초 전신 교체는 남음 |
| v8 | 매 프레임 seek하는 미리보기 구조는 게임의 외부 Play 호출을 덮어쓸 수 있음 | [AnimationPlayer](https://docs.godotengine.org/en/4.5/classes/class_animationplayer.html)의 시간·완료·다음 clip 계약과 실제 BaseLib 경로 대조 | native player가 관절과 pose_kind/pose_time을 소유. idle/relaxed/powerup alias와 액션 후 자동 복귀. controller를 Visuals 아래로 이동하고 script-relative 리소스 경로 사용. 이미지 생성 0회, 386장 렌더, 실제 엔진 시계와 중첩 폴더 재배치 관찰 | 전투·상점 시작, 외부 공격, 복귀, 사망 정지, 명시적 idle reset을 관찰. 실제 revive 요청은 이 단계에서 구현하지 않았으며 게임 연동 완료로 보지 않음 |
| v9 기각 | 첫 교체 전후 머리·어깨·골반 위치가 다름 | [Spine keys](https://esotericsoftware.com/spine-keys)와 weights를 교체 직전 좌표에 대조 | 두 원화의 landmark를 맞추도록 67개 사망 관절 표본 재작성. 이미지 생성 0회, 사망 53장 렌더 | 머리 위치는 가까워졌지만 몸통 틈·늘어난 팔·다리 분리가 발생. 기존 리깅이 지원하지 않는 자세를 좌표 보간만으로 강제한 결과라 채택하지 않음 |
| v10 진행 | 전신 그림 교체로 얼굴까지 바뀌고, 큰 하강에서는 손목·기본 미소가 부자연스러움 | [Spine slots](https://esotericsoftware.com/spine-slots)의 부분 attachment와 독립 draw order, weights의 국소 제어를 비교. 위 AnimationPlayer 문서와 공식 기본 부활 경로도 재검토 | 원본 머리·상체를 사망 끝까지 유지하고 기존 중간 원화에서 접힌 하체만 추출. 하체는 0.35초 교체. 팔 IK·독립 오른쪽 손목·비대칭 하강 적용. Codex 이미지 툴 1회로 감은 눈·닫힌 입 생성 후 얼굴 mask 안만 합성하여 모자·머리카락·원본 alpha 유지. 첫 사망 53장, 수정 후 6클립 386장 렌더 | 전신과 얼굴의 순간 교체 제거. 하체 전환은 여전히 튀고 끝 자세의 체중 지지·몸통 연결을 더 검토해야 함. 공식 수준 달성으로 판정하지 않음. 기존 그림·메시 재사용으로 전체 atlas 재생성을 피함 |
| v10 패키지·부활 | source 폴더에서의 성공은 JSON 누락·import 경로·실제 revive 이름을 확인하지 못함 | [Godot export filters](https://docs.godotengine.org/en/4.5/tutorials/export/exporting_projects.html)와 [패키징 조사](../../research/sts2-cutout-v8-packaging-review.md)의 로컬 타입/멤버 근거 | 0.4초 revive clip, 0.2초에서 숨김 상태로 idle 교체, 이후 fade-in 및 자동 복귀. 모든 action은 alpha=1을 복구. 세 JSON·24개 동적 리소스 입력을 명시한 별도 PCK 생성. 소스 없는 새 host에서 mount 및 전투/상점 12시점 렌더. 첫 관찰의 같은 clip 재호출이 시간을 초기화하지 않는 문제를 고쳐 12장 재관찰 | PCK mount·JSON 3종·imported texture·script·clip 이름 로딩과 부활 alpha 1→0.5→0→0.5→1 관찰. 중도 hurt도 alpha=1로 복구. 실제 BaseLib factory·게임 이벤트·다인 Z는 별도 미검증. 메인 export 설정과 설치본은 변경하지 않음 |
| rest-v1 | 선 전투 그림 재사용으로 공식의 앉은 실루엣이 없음 | 공식 rest 원본과 [Spine control 예제](https://esotericsoftware.com/spine-examples-mix-and-match), Godot UV mesh를 두 번 비교 | v2에서 생성한 앉은 원화 재사용, 추가 이미지 생성 0회. 호흡·머리와 받치는 손·모자 끝·양쪽 옷 끝 5 control, 8초 Act loop 3개와 light_off. 288+3장씩 두 차례 native 렌더 | 손-뺨 접촉 유지, 바위에 번지던 coat 영향 범위 1곳 수정. 원본 PNG 바이트 보존. 바위·왼발 표본 이동 0, 오른발 표본 미세 이동 0.069 source px. 전체 발의 무이동 증명은 아니며 실제 다인 배치·불·말풍선은 미검증 |
| selection-v1 | 현재 완성 그림은 정적이며 공식은 별도 미세 움직임·입자가 있음 | 공식 selection 샘플 및 [Spine Windmill](https://esotericsoftware.com/spine-examples-windmill)의 반복·위상, Godot Polygon2D/AnimationPlayer를 두 차례 비교 | 기존 1920×1080 원화 바이트 보존, 이미지 생성 0회. 8초 loop에 망토·리본·모자 끝의 국소 변형, 기존 청록 마법 밝기와 작은 입자 5개. 96프레임씩 두 번 native 렌더 | 얼굴·손·칼날 좌표 표본은 고정. 별도 몸통 articulation·표정·실제 선택 UI와 종횡비 배치는 미검증. 전체 그림의 pan이나 squash는 사용하지 않음 |
| v11 | 공격이 얼굴 근처 팔·자루 회전 위주라 체중 이동이 약함 | [Spine IK](https://esotericsoftware.com/spine-ik-constraints), [Spine graph](https://esotericsoftware.com/spine-graph), [Spineboy 예제](https://esotericsoftware.com/spine-examples-spineboy): 골반과 발 target 분리, 준비·타격·정리 곡선 분리 | v10 rig.json의 공격만 60Hz 64표본으로 재작성. 전진 스텝, 골반 이동·하강, 몸통 기울기, 머리 역회전, grip target, 0.15초 정확한 수평 타격. 이미지 생성 0회, 공격 26장 여러 번 + 386장 렌더 | 전신 공격으로 개선. 팔꿈치 경로를 바꿔 어깨 틈 해소. 0.083초·회복 구간에서 빗자루가 발 아래로 파고드는 문제 발견 |
| v12-1 지면 | 빗자루 털이 0.083초와 0.65~0.8초에 지면 아래 | Spine IK의 도달 범위·target 제어를 다시 확인. 원인을 각도 보정이 아니라 **grip 경로**로 판단 | 무기 alpha 픽셀을 FK로 변환해 최저점을 계산. 첫 보정은 0.05초 예비 동작과 0.15초 수평 타격까지 번져 **기각**. 와인드업에서 빗자루를 들어 올리고, 내려치기 grip을 높이고, 회복에서 들어 올렸다 세워 짚는 곡선으로 재설계. 남은 보정은 2키 0.19rad. 0.15초 키는 보정 금지 assert | key·4분할 보간 모두 원본 지면 +1px 이내(관찰치). 타격 자세 불변 |
| v12-2 표정·VFX | 얼굴이 모든 동작에서 같은 미소, 공격·cast에 읽히는 효과가 없음 | 공식 Ironclad 전투 씬에 별도 slash VFX가 있음을 PCK에서 확인. Spine slots의 부분 attachment 교체 | Codex 이미지 툴 3회: 피격(찡그림), 공격(기합) 표정, 붓터치 slash. 생성 머리가 재프레이밍되어 와 머리카락·챙 링으로 배율·이동 탐색 후 얼굴 mask만 합성. slash는 원 맞춤 후 FK 칼날 끝 경로에 등록. 표정·VFX를 AnimationPlayer 키로 이동, 모든 clip에 기본값 키 | 공격 0.167초 비교에서 공식처럼 궤적이 동작을 읽히게 함. idle·상점에 눈 깜빡임 추가 |
| v12-3 피격·cast | 피격은 상체만, cast는 빈손만 들어 공식과 차이 | 공식 hurt 24fps 프레임 직접 재관찰(무릎 굽힘·웅크림). Godot AnimationPlayer 문서 | 피격: 발 고정 IK로 골반 하강·몸통 말기·팔 튕김. cast: 손을 당겼다가 0.25초에 전신으로 내지름. Codex 이미지 툴 1회로 시길 생성, 팔에 수직인 원반으로 납작하게 배치 | 두 동작 모두 하체 참여. 시길이 0.25초 발동을 표시 |
| v12-4 사망 | v10 하체 교체에서 발이 80~100px 순간이동 | [Spine keys](https://esotericsoftware.com/spine-keys)의 "몇 프레임마다 attachment를 키로 바꾸는 frame-by-frame" | 코트가 hip 강체라 rig만으로 무릎 꿇기 불가(hem이 지면 60px 아래)를 계산으로 확인. 실제 파츠를 FK로 배치한 2패널 가이드를 만들고 Codex 이미지 툴 1회로 버클·무릎 cel을 한 장에 그리게 함(장 간 일관성). 지면 고정 cel A(0.2초)→B(0.31초), 놓친 빗자루는 바닥에 떨어져 한 번 튐 | 무릎·부츠가 이어지는 주저앉기. 마지막 자세는 무릎 꿇고 앞으로 쓰러져 손으로 받친 형태이며 공식 두 캐릭터처럼 완전히 엎드리지는 않음 |
| v12-4b 사망 끝 | 사용자 검토: v6의 누운 마지막 자세가 가장 자연스러움 | Spine keys의 frame-by-frame attachment 교체를 마지막 단계에도 적용 | v4에서 생성한 누운 전신 원화를 v6 위치 그대로 재사용(이미지 생성 0회). 무릎 cel 뒤 상체 낙하를 3.5배 가속해 0.62초 바닥 충돌 프레임에 교체하고 9px 낙하·회전 반동으로 착지. 빗자루는 v6처럼 몸 뒤 바닥에 놓임. 2.0배 가속은 반쯤 숙인 자세에서 바뀌어 차이가 커서 조정 | 마지막 프레임이 v6과 같은 구성. 교체 직전 상체가 거의 바닥에 닿음 |
| v12-5 휴식·선택 | 정적인 얼굴. 사용자 검토 후 선택 화면 patch가 원래 뜬 눈의 위 속눈썹을 덮지 못함 | Spine mix-and-match의 국소 control 재확인 | Codex 이미지 툴 2회로 눈만 감은 편집. 눈 밖 영역으로 정렬 확인 후 작은 eye patch로 만들어 같은 mesh 정점 위에 2프레임 표시(전체 텍스처 복제 회피). 선택 화면 mask 상단을 28px 올려 재생성 | 두 장면 모두 이음새 없는 깜빡임 |
| 런타임 적용 | 초안이 JSON을 런타임에 읽고, 절대 z와 top_level 무기를 사용 | [Godot CanvasItem](https://docs.godotengine.org/en/4.5/classes/class_canvasitem.html): 같은 layer는 트리 순서로 그리며 z는 부모와 합산되어 형제 UI 위로 올라감, top_level은 bare Node 자식처럼 동작. [AnimationPlayer](https://docs.godotengine.org/en/4.5/classes/class_animationplayer.html): autoplay·animation_set_next | rig를 .tscn으로 bake(JSON 불필요), z_index 전부 0·Skeleton2D 위치로 무기 순서 지정, 무기는 bone-local 변환으로 계산. 투명 여백만 잘라 파츠 1.78MB→570KB, 과대 VFX는 통째 축소. 휴식 Act·light_off·비전투 게임오버 die를 C# Harmony로 연결, DeathAnimTime 1.8초 | `dev.ps1 publish` 성공. 배포 PCK만 mount한 새 호스트에서 네 씬 렌더, 디스패치 이름·자동 복귀·revive·상점 alias 관찰 |
| v13 | 사용자 검토: 날이 뒤로 향하는 공격, 끊기는 사망, 표정 턱·볼 마감, 잘라낸 파츠, 피격·사망 초반 다리 벌어짐, 어색한 상점 자세. Gemini 외부 리뷰 17항목 | Spine physics의 감쇠 후행, Spine keys의 frame-by-frame 교체, 공식 Ironclad/Silent hurt·die·merchant 동시각 재비교 | Codex 작업 7건(이미지 툴 19회, 재시도 1회). 자루 축 반전으로 날 방향 전환과 손등 그립 손, 모자·코트·리본 스프링 bake, 연속 원화 사망과 0.1초 무기 해제, 표정 3종 재생성, 18개 파츠 숨은 면 재생성·코트 앞뒤 분리·메시 확장, 피격 골반 하강 23→4px·무릎 간격 축소, 상점 자세 재설계, 휴식·선택 깜빡임 재생성. 외부 리뷰는 항목별 재현 후 판정(cutout-v13/NOTES.md) | 7클립 396프레임, `dev.ps1 publish` 성공, 배포 PCK 렌더와 디스패치 재확인. 남은 것: 자루 회전 프레임의 가는 무기, 사망 원화 사이의 작은 윤곽 변화, 실제 게임 확인 |

v12까지 이미지 툴은 누적 16회(v1~v10 9회, v12 7회, 재시도 0회)다. 생성 원화는 매번 캔버스 위치·배율을 바꿔 돌려주므로, 프롬프트로 배치를 보장하지 않고 **변하지 않은 영역으로 정렬을 측정**한 뒤 필요한 영역만 합성했다. 정밀한 총 소요 시간과 비용은 계측하지 않았다.

메시 반복에서 얻은 중요한 구분은 **같이 움직여야 하는 관절 경계**와 **화면에서 겹쳐 보일 뿐인 물체 경계**다. 모든 경계를 같은 weight로 부드럽게 연결하면 팔을 들 때 코트가 늘어난다. 반대로 모든 파츠를 강체로만 처리하면 팔꿈치와 천이 끊긴다. 관절 체인 안에서만 연속성을 만들고, 가려진 면은 별도 그림으로 보충해야 한다.

이미지 툴은 지시한 캔버스 위치·배율을 여러 번 바꿨다. 따라서 “정확한 원본 분해”를 프롬프트만으로 보장하지 않는다. 보이는 원본 픽셀은 mask로 관리하고, 생성된 부분은 별도 파일과 등록 좌표를 남긴다. 코트처럼 조각별 보충이 계속 어긋난 부위는 해당 파츠 하나를 다시 그린 뒤 원본과 달라진 범위를 공개하는 편이 재작업을 줄였다.

## 현재 런타임 클립

| clip | 길이 | 내용 |
|---|---:|---|
| idle_loop (`idle`) | 2.6초 | 발을 지지한 호흡·머리·모자·리본, 1.9초에 2프레임 눈 깜빡임. 시작 위상 무작위 |
| attack | 1.05초 | 빗자루를 들어 올리는 와인드업 → 0.15초 정확한 수평 타격 + 청록 궤적 → 들어 올렸다 세워 짚는 회복. 기합 표정 |
| hurt | 0.8초 | 골반이 내려앉고 몸통을 말며 팔이 튕김. 찡그린 표정 |
| cast (`powerup`) | 1.3초 | 손을 당겼다가 0.25초에 전신으로 내지르며 시길 발동. 집중한 표정 |
| die | 2.2초 | 피격 표정 → 감은 눈, 다리 버클 → cel A(0.2초) → 무릎 cel B(0.31초) → 앞으로 쏠리며 0.62초 바닥 충돌 순간 옆으로 누운 전신 원화(v6 마지막 자세)로 교체·착지 반동, 빗자루는 몸 뒤에 떨어져 튐. 1.13초에 정착, 다음 clip 없음 |
| relaxed_loop (`relaxed`) | 8초 | 상점 자세. 2.4초와 6.1초(두 번)에 깜빡임 |
| revive | 0.4초 | 공식 기본: 0.2초 fade-out → 숨긴 상태에서 idle 복구 → fade-in |
| 휴식 3 Act | 각 8초 | 같은 앉은 동작에 Act별 조명. 2.6초·6.2초 깜빡임. `light_off`는 모닥불 온기만 제거 |
| 선택 화면 | 8초 | 망토·리본·모자 끝, 마법 맥동과 입자, 3.1초 깜빡임. 화면비에 맞춰 덮기 배치 |

## 게임 연결

BaseLib 3.4.7은 비-Spine 전투 트리거에서 첫 `AnimationPlayer`를 찾아 이름 목록 순서대로 `Play`한다. 그래서 rig 라이브러리는 `idle`·`relaxed`·`powerup` alias를 함께 노출하고, 행동 clip은 `next`로 idle에 돌아간다. 상점 rig는 `idle`이 `relaxed_loop`를 가리키는 별도 씬이다. 공식 Spine 전용 경로 세 곳은 `RealtaSceneBridge.cs`가 잇는다: 휴식 `_Ready` 뒤 Act loop 선택, `HideFlameGlow` 뒤 불빛 끄기, 전투·상점이 아닌 화면의 게임오버에서 새로 만든 전투 visuals에 `die` 재생. 전투 게임오버는 이미 사망 clip을 재생한 상태라 다시 시작하지 않는다. `DeathAnimTime`은 빗자루까지 정착하는 시각에 맞춰 1.25초로 지정했다.

게임 화면 안에서의 안전성을 위해 모든 파츠를 z_index 0으로 두고 트리 순서로만 그린다. z를 올리면 같은 layer의 체력바 같은 UI 위로 그려질 수 있기 때문이다. 놓친 무기도 top_level을 쓰지 않아 게임의 fade·가시성을 그대로 상속한다.

## 검증한 것과 남은 것

- **확인:** 모든 clip의 native 렌더(0.625 배율), 0.15초 타격 자세, 빗자루 지면 관찰, `dev.ps1 publish`(C# 빌드·import·PCK export, 경고는 오프라인 NuGet 조회 NU1900뿐), 배포 PCK만 mount한 새 호스트에서 네 씬 생성·렌더, BaseLib식 이름 요청과 자동 복귀·revive·상점 alias, 휴식 Act·light_off·좌우 반전, 선택 화면 16:9/4:3 배치. 또한 실제 BaseLib 3.4.7 `NodeFactory` 변환과 `CustomAnimation` 디스패치를 독립 C# 호스트에서 관찰했다(세부는 [런타임 조사](../../research/sts2-cutout-runtime-contract.md#적용-결과와-실제-baselib-관찰-2026-10-19-v12)).
- **미확인:** 실제 게임 안의 NCreature 생명주기와 Harmony 진입 경로, 이벤트 타이밍, 다인 배치와 겹침, 게임오버 화면, 휴식 모닥불·생각풍선과의 실제 배치, 선택 화면 UI 가림. 설치는 요청받지 않아 수행하지 않았다.
- **남은 시각 차이:** 사망은 무릎 자세에서 누운 원화로 바뀌는 0.62초 한 프레임에 형태 차이가 있다(바닥 충돌 순간에 두어 가렸지만 연속 변형은 아님). 피격 웅크림은 공식 Ironclad보다 작다. 휴식·선택 화면은 단일 원화 mesh라 큰 관절 동작은 없다.

## 로컬 검토 자료와 재현

공식 두 캐릭터와 Realta(v6·v8·v10·v12)는 같은 시각의 24fps 표본과 씬별 GIF로 비교했다. 당시 검토 페이지·중간 스크립트·생성 원본·프롬프트는 Git에서 제외되는 선택적 작업 자료이며, 관찰과 판단은 위 반복 이력에 기록했다. 공식 추출 원본은 연구용이며 배포 에셋에 포함하지 않는다.

런타임 재현은 추적되는 [animation/README.md](animation/README.md)가 소유한다. `rig-base.json`과 런타임 파츠·`parts.json`만으로 `author_clips.py` → `build_combat_rig.gd` bake → `dev.ps1 publish`를 반복할 수 있어, 이전 후보 간 참조를 하나의 입력 manifest로 정리하는 과제를 해결했다. 생성 원화는 확률적이므로 프롬프트만으로 같은 픽셀을 재현할 수 없고, 채택한 결과는 파츠 파일 자체가 기록이다.

새 테스트나 보호된 검증 자산은 추가·수정하지 않았다(저장소에 실행할 테스트가 없다). 위 좌표·이미지 관찰은 임시 탐색이며 사용자 승인이나 제품 합격 기준이 아니다.
