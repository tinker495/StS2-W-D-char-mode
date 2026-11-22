# 공통 배포 상태

현재 로컬 게임에 설치된 배포의 범위와 확인한 사항을 기록한다. 새 설치를 완료하면 이전 항목을 누적하지 않고 현재 상태를 갱신한다. 캐릭터별 에셋 경로와 적용 범위는 각 에셋 가이드에서 관리한다. [빌드·설치 절차](/.agents/skills/build-install/SKILL.md)

## 현재 로컬 설치

- **소스:** `main` 브랜치(`21393cc`, 전투·상점·휴식 캐릭터 씬 스케일·접지선 표준화 및 정지 전신 PNG `640 × 960` 상향 반영본)
- **적용 범위:** Realta·Althea·Mercurius의 `combat`/`merchant`/`rest_site` 씬 9종 스케일·접지선·`Bounds`/`Hitbox`·앵커 표준화, 상점 자식 `Visuals` 전용 스케일(`1.62×–1.84×`, 루트 항등 변환 유지), Althea(`standing.png`)·Mercurius(`combat.png`) 정지 전신 `640 × 960` Tier 2 Lossy WebP(`0.85`, `mipmaps/generate=false`) 갱신 및 기존 3단 텍스처 압축·PCK 최소화 정책(`Release` 3파일 번들 `11.65 MB`, PCK `11.14 MB`, 누수·고아 0건) 포함
- **설치 대상 및 검증:** `dev.ps1 deploy -ExecutionContextLabel local`로 Steam 설치 폴더의 `mods/Sts2WD/`에 `Release` 3파일(DLL·PCK·JSON manifest)을 설치했으며, 세 파일의 SHA-256이 배포 패키지와 일치함을 확인
- **환경:** .NET SDK 9.0.318, MegaDot 4.5.1.m.14 (`4.5.1.m.14.mono.custom_build`), BaseLib 3.4.7
- **확인 범위:** `dev.ps1 verify`(Python 정적 검증 5종), `dev.ps1 build`(경고 0·오류 0), `dev.ps1 deploy`(Stage 2 PCK 검사 통과·3파일 SHA-256 일치), `--headless` `tests/check_rig.gd`(21,343샘플) 및 `tests/realta_instance_isolation.gd`(4케이스) 통과, `dev.ps1 observe` 패키징 PCK 1:1 룸 공간 9씬 실측(`±2 px` 이내·상점 루트 항등·휴식 `FlipX()` 대칭·Realta `relaxed_loop`/`attack`/`cast` 피크 통과) 및 `#E8E2D5`/`#1B1924` 배경 시각 검수까지. 실제 게임 내 런 실행 및 전투·상점(일반/블리츠, `NGameOverScreen` 사망 리페어런팅)·휴식 육안 확인은 별도 수행 필요.

현재 캐릭터별 리소스는 각 가이드에서 확인할 수 있다. [레아르타](characters/realta/realta-assets.md), [알테아](characters/althea/althea-assets.md), [메르쿠리우스](characters/mercurius/mercurius-assets.md), [아셀리게](characters/aselige/aselige-assets.md)
