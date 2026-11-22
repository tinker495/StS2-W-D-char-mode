# 공통 배포 상태

현재 로컬 게임에 설치된 배포의 범위와 확인한 사항을 기록한다. 새 설치를 완료하면 이전 항목을 누적하지 않고 현재 상태를 갱신한다. 캐릭터별 에셋 경로와 적용 범위는 각 에셋 가이드에서 관리한다. [빌드·설치 절차](/.agents/skills/build-install/SKILL.md)

## 현재 로컬 설치

- **소스:** `main` 브랜치(`e97d995` 이후 `docs/` 아티팩트 정리·Python 정적 검증 이관 및 설치 패키지·PCK 최소화 정책 반영본)
- **적용 범위:** 3단 텍스처 `.import` 압축 정책(Tier 1 VRAM BPTC/BC7 `realta_skeleton.png` 1건, Tier 2 Lossy WebP `0.85` 18건, Tier 3 Lossless WebP 177건), 비런타임 PCK 경계 차단(`scripts/.gdignore`, `export_presets.cfg` 보강) 및 미참조 PNG 8건 제거, `Release` 기본 3파일 번들 전환(`11.58 MB`, PCK `11.08 MB`, 누수·고아 0건) 및 기존 레아르타·알테아·메르쿠리우스·아셀리게 반영분 포함
- **설치 대상 및 검증:** `dev.ps1 deploy -ExecutionContextLabel local`로 Steam 설치 폴더의 `mods/Sts2WD/`에 `Release` 3파일(DLL·PCK·JSON manifest)을 설치하고 기존 잔존 PDB를 백업 후 제거했으며, 세 파일의 SHA-256이 배포 패키지와 일치함을 확인
- **환경:** .NET SDK 9.0.318, MegaDot 4.5.1.m.14 (`4.5.1.m.14.mono.custom_build`), BaseLib 3.4.7
- **확인 범위:** `dev.ps1 verify`(Python 정적 검증 5종), `dev.ps1 build`(경고 0·오류 0), `dev.ps1 deploy`(Stage 2 PCK 검사 통과·3파일 SHA-256 일치·잔존 PDB 제거), `asset-pipeline.ps1 observe-texture`(Lossy WebP·BPTC PCK 로드), `--headless` `tests/check_rig.gd` 및 `tests/realta_instance_isolation.gd` 통과, `dev.ps1 anim idle -Variant both -PackagePath` BPTC vs 무손실 255프레임 비교(최소 PSNR 55.31 dB)까지. `dev.ps1 card-tests`는 설치 게임 `sts2.dll`의 `HoverTip.Id` 읽기 전용 변경(`CS0200`)으로 보호 테스트 수정 없이 미통과이며, 실제 게임 내 런 실행 및 육안 확인은 별도 수행 필요.

현재 캐릭터별 리소스는 각 가이드에서 확인할 수 있다. [레아르타](characters/realta/realta-assets.md), [알테아](characters/althea/althea-assets.md), [메르쿠리우스](characters/mercurius/mercurius-assets.md), [아셀리게](characters/aselige/aselige-assets.md)
