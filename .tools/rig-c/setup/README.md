# rig-c 설치와 실행

이 문서가 있는 폴더에서 다음 명령을 실행합니다. 설치 명령은 npm을 사용해 잠금 파일에 고정된 버전을 부모 폴더에 설치합니다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./run.ps1 install
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./run.ps1 rigc --version
```

`run.ps1`에 `rigc`, `rig-create`, `rig-parts`, `rig-pose`, `rig-review` 중 하나와 인수를 전달하면 설치된 Bun으로 해당 CLI를 실행합니다. 브라우저 재생용 `@rig-c/play`와 PixiJS·Phaser·Three.js 어댑터도 함께 설치됩니다.

공식 Spine 파서로 빌드 결과를 다시 읽으려면 `install-spine`을 사용합니다. 잠금 파일에 고정된 파서가 함께 설치됩니다.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./run.ps1 install-spine
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ./run.ps1 rigc validate <빌드-폴더> --profile rig-play
```

Codex는 저장소의 [프로젝트 스킬](/.agents/skills)에서 배포판에 포함된 스킬을 불러옵니다. 패키지를 갱신할 때는 `rigc skills install --copy --dir <대상 폴더>`로 빈 폴더에 새 사본을 생성하고 프로젝트 스킬에 반영합니다. [rig-c 공식 설치 문서](https://github.com/rig-c/core#install-it-into-your-agent)
