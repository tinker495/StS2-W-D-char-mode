# 공통 배포 상태

현재 로컬 게임에 설치된 배포의 범위와 확인한 사항을 기록한다. 새 설치를 완료하면 이전 항목을 누적하지 않고 현재 상태를 갱신한다. 캐릭터별 에셋 경로와 적용 범위는 각 에셋 가이드에서 관리한다. [빌드·설치 절차](/.agents/skills/build-install/SKILL.md)

## 현재 로컬 설치

- **소스:** `main` 브랜치(`d6daba6`, 아셀리게 블리츠·모디파이어 니오우 동등화 및 일러스트 전체화면 레이아웃 수정 반영본)
- **적용 범위:** 아셀리게(`STS2WD-ASELIGE`) 블리츠 모드 단일 선택지(`정록의 인도` / `Verdant Guidance`) 및 `BlitzRunModel.GiveNeowRewards`·모디파이어 순차 실행(`SetEventFinished` 직접 종료로 `AncientChoices` 중복 방지), `AseligeLifeSource` 블리츠 제외 및 일반 런 생성 지도(`StandardActMap`·`SpoilsActMap`·`GoldenPathActMap`) 양성 허용 목록, `aselige.tscn` `KeepAspectCovered` + `±64 px` 오버스캔 및 `Aselige.ConfigureBackgroundScene` 정적 역변환 전체화면 보정, 한/영 블리츠 로컬라이제이션 3키 추가 및 기존 캐릭터 씬 스케일 표준화·3단 텍스처 압축·PCK 최소화 정책(`Release` 3파일 번들 `11.65 MB`, PCK `11.14 MB`, 491 entries, 누수·고아 0건) 포함
- **설치 대상 및 검증:** `dev.ps1 deploy -ExecutionContextLabel local`로 Steam 설치 폴더의 `mods/Sts2WD/`에 `Release` 3파일(DLL·PCK·JSON manifest)을 설치했으며, 세 파일의 SHA-256이 배포 패키지와 일치함을 확인
- **환경:** .NET SDK 9.0.318, MegaDot 4.5.1.m.14 (`4.5.1.m.14.mono.custom_build`), BaseLib 3.4.7
- **확인 범위:** `dev.ps1 verify`(Python 정적 검증 5종), `dev.ps1 build`(Debug 경고 0·오류 0), `dev.ps1 deploy`(Release 경고 0·오류 0, MegaDot `--import`·PCK export, Stage 2 PCK 검사 통과·3파일 SHA-256 일치), MegaDot `--headless` `aselige.tscn` 4:3·16:10·16:9·21:9 전체화면 커버리지·±64 px 오버스캔 검증까지. 실제 게임 내 블리츠·모디파이어·일반 런 실행, 창 리사이즈, 화면 흔들림 육안 확인 및 BaseLib·Sts2WD 활성화 여부는 별도 수행 필요.

현재 캐릭터별 리소스는 각 가이드에서 확인할 수 있다. [레아르타](characters/realta/realta-assets.md), [알테아](characters/althea/althea-assets.md), [메르쿠리우스](characters/mercurius/mercurius-assets.md), [에레닐](characters/erenil/erenil-assets.md), [아셀리게](characters/aselige/aselige-assets.md)
