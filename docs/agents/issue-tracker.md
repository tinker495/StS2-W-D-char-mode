# Issue tracker: GitHub

이슈와 명세는 `tinker495/StS2-W-D-char-mode`의 GitHub Issues에서 관리한다. 작업에는 GitHub CLI를 사용한다.

## 실행 환경

- 저장소 루트에서 실행한다. `gh`가 PATH에 없으면 PowerShell에서 `& ./.tools/gh/bin/gh.exe`를 사용한다.
- 아래 명령의 `gh`는 위 실행 파일을 뜻한다. 이슈·라벨·PR 명령에는 `--repo tinker495/StS2-W-D-char-mode`를 지정한다.
- 인증은 기존 GitHub CLI 로그인 또는 프로세스 환경변수 `GH_TOKEN`을 사용한다. 이 PC에서는 Git Credential Manager에 저장된 해당 GitHub 저장소의 자격 증명을 사용할 수 있다. 토큰은 프로세스 안에서만 전달하며 출력하거나 파일에 저장하지 않는다.
- 여러 줄 본문은 UTF-8 임시 파일에 작성하고 `--body-file <path>`로 전달한다.

## 이슈 작업

- 생성: `gh issue create --title "..." --body-file <path>`.
- 조회: `gh issue view <number> --json number,title,body,labels,comments`.
- 목록: `gh issue list --state open --json number,title,body,labels,comments`. 필요에 따라 `--label`과 `--state`를 지정한다.
- 본문 수정: `gh issue edit <number> --body-file <path>`.
- 댓글: `gh issue comment <number> --body-file <path>`.
- 라벨 추가·제거: `gh issue edit <number> --add-label "..."` 또는 `--remove-label "..."`. 역할별 이름은 `triage-labels.md`를 따른다.
- 종료: 필요한 결과를 댓글로 기록한 뒤 `gh issue close <number>`.

스킬에서 “publish to the issue tracker”라고 하면 GitHub 이슈를 생성한다. “fetch the relevant ticket”은 해당 이슈를 조회한다는 뜻이다.

## 부모 이슈와 의존성

- 하위 이슈: `gh issue create --parent <parent> ...` 또는 `gh issue edit <parent> --add-sub-issue <child>`.
- CLI에서 하위 이슈를 지원하지 않으면 `gh api --method POST repos/tinker495/StS2-W-D-char-mode/issues/<parent>/sub_issues -F sub_issue_id=<child-db-id>`를 사용한다.
- 선행 작업 등록: `gh api --method POST repos/tinker495/StS2-W-D-char-mode/issues/<child>/dependencies/blocked_by -F issue_id=<blocker-db-id>`.
- API의 `*-db-id`에는 이슈 번호나 `node_id`가 아닌 숫자 `id`를 쓴다. `gh api repos/tinker495/StS2-W-D-char-mode/issues/<number> --jq .id`로 조회한다.
- 서버가 하위 이슈를 지원하지 않으면 부모 본문에 자식 이슈 체크리스트를 두고 자식 본문 첫머리에 `Part of #<parent>`를 적는다. 의존성을 지원하지 않으면 자식 본문 첫머리에 `Blocked by: #<number>, #<number>`를 적는다.
- 선행 작업이 모두 닫혀야 착수할 수 있다. 네이티브 의존성에서는 `issue_dependencies_summary.blocked_by`가 열린 선행 작업 수다.

## Wayfinding

- 지도는 `wayfinder:map` 라벨이 붙은 부모 이슈이며 Notes / Decisions-so-far / Fog를 담는다.
- 작업은 지도의 하위 이슈로 만든다. 종류는 `wayfinder:research`, `wayfinder:prototype`, `wayfinder:grilling`, `wayfinder:task` 라벨로 구분한다. 이 라벨은 해당 흐름을 처음 사용할 때 준비한다.
- 착수할 작업은 지도 순서대로, 열려 있고 선행 작업이 해결됐으며 담당자가 없는 이슈 중에서 고른다.
- 착수 시 첫 변경으로 `gh issue edit <number> --add-assignee '@me'`를 실행한다.
- 해결 시 답변을 댓글에 남기고 이슈를 닫은 뒤, 지도의 Decisions-so-far에 요약과 링크를 추가한다.

## Pull requests as a triage surface

**PRs as a request surface: no.**
