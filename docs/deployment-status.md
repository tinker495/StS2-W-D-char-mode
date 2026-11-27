# 공통 배포 상태

현재 로컬 게임에 설치된 배포의 범위와 확인한 사항을 기록한다. 새 설치를 완료하면 이전 항목을 누적하지 않고 현재 상태를 갱신한다. 캐릭터별 에셋 경로와 적용 범위는 각 에셋 가이드에서 관리한다. [빌드·설치 절차](/.agents/skills/build-install/SKILL.md)

## 현재 로컬 설치

- **소스**는 `main`의 `cbfc0fb`다.
- **설치 범위**는 레아르타 네이티브 `SpineSprite`(Spine `4.2` `.spjson`/`.spatlas`) 전투·상점 8클립(`guard` 및 Track 1 `blink_loop` 포함)과 `512×512` 최소 선택 배경 오버레이, 방어형 스킬 카드 `Guard` 모션 라우팅, 공용 접지 그림자, 휴식 모닥불 일렁임 동기화, 개발자 콘솔 `wd` 명령을 포함한 Release 3파일 번들(`12.07 MB`, PCK `11.55 MB`)이다. 기존 모드를 백업한 뒤 Steam 게임 폴더의 `mods/Sts2WD`에 설치했다.
- **확인 범위**는 Windows 로컬 환경(설치 게임 `v0.114.0`, BaseLib `3.4.7`, MegaDot `4.5.1.m.14.mono.custom_build`, .NET SDK `9.0.318`)에서 정적 검사 5종, `anim` 24 fps 메쉬 접힘 검사·리그 기계 검사(`998` 샘플)·인스턴스 격리 검사(`4` 케이스), Release 빌드·MegaDot 리소스 가져오기와 PCK 내보내기, Stage 2 패키지 검사, 오프스크린 다중 화면비(`16:9`·`16:10`·`3:2`)·`FlipX` 렌더, 설치된 DLL·PCK·JSON의 SHA-256 일치까지다. 실제 플레이 세션에서의 확인은 별도다.

현재 캐릭터별 리소스는 각 가이드에서 확인할 수 있다. [레아르타](characters/realta/realta-assets.md), [알테아](characters/althea/althea-assets.md), [메르쿠리우스](characters/mercurius/mercurius-assets.md), [에레닐](characters/erenil/erenil-assets.md), [아셀리게](characters/aselige/aselige-assets.md)
