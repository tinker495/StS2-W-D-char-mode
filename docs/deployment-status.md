# 공통 배포 상태

현재 로컬 게임에 설치된 배포의 범위와 확인한 사항을 기록한다. 새 설치를 완료하면 이전 항목을 누적하지 않고 현재 상태를 갱신한다. 캐릭터별 에셋 경로와 적용 범위는 각 에셋 가이드에서 관리한다. [빌드·설치 절차](/.agents/skills/build-install/SKILL.md)

## 현재 로컬 설치

- **소스:** `codex/update-rig-c`의 `24895a7` 위에 있는 미커밋 작업 포함 (로컬 설치본 기준이며 원격 `main`이나 Git 업로드 버전을 뜻하지 않음)
- **적용 범위:** 레아르타 `rig-c` 기반 전투 컷아웃 리그(단일 `2048x2048` 58파츠 `Sts2WD/animations/realta/combat/realta_skeleton.png`, 라이브 `CanvasGroup` `revive`/`revive_merchant`, 캐릭터 시점 `_r`/`_l` 명명, 풀바디 기준 `hand_l-casting`·`hand_r-ground`, 가속 찌르기 스플라인과 관통 FX `thrust_streak`·`thrust_flash`) 및 기존 레아르타·알테아·메르쿠리우스·아셀리게 반영분 포함
- **설치 대상 및 검증:** `dev.ps1 deploy`로 Steam 설치 폴더의 `mods/Sts2WD/`에 DLL·PCK·JSON manifest·PDB를 설치했고, 네 파일의 SHA-256이 배포 패키지와 일치함을 확인
- **환경:** .NET SDK 9.0.318, MegaDot 4.5.1.m.14 (`4.5.1.m.14.mono.custom_build`), BaseLib 3.4.7
- **확인 범위:** `dev.ps1 verify`(추적 참조·카드 초상화·PNG 알파), `dev.ps1 build`(경고 0·오류 0), `dev.ps1 anim attack -Variant both`(Godot `tests/check_rig.gd` 및 `tests/realta_instance_isolation.gd` 통과, `rigc render --folds` 0/16), `dev.ps1 deploy` 설치 파일 및 SHA-256 일치까지. 실제 게임 내 런 실행 및 육안 확인은 별도 수행 필요.

현재 캐릭터별 리소스는 각 가이드에서 확인할 수 있다. [레아르타](characters/realta/realta-assets.md), [알테아](characters/althea/althea-assets.md), [메르쿠리우스](characters/mercurius/mercurius-assets.md), [아셀리게](characters/aselige/aselige-assets.md)
