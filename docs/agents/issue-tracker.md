# Issue Tracker: GitHub

Issues and specifications are managed in GitHub Issues for `tinker495/StS2-W-D-char-mode`. Use the GitHub CLI.

## Execution Environment

- Run from the repository root. If `gh` is not on PATH, use `& ./.tools/gh/bin/gh.exe` in PowerShell.
- In the commands below, `gh` refers to that executable. Specify `--repo tinker495/StS2-W-D-char-mode` for issue, label, and PR commands.
- Use the existing GitHub CLI login or the process environment variable `GH_TOKEN`. On this PC, the credentials for this GitHub repository stored in Git Credential Manager are available. Pass tokens only within the process; never print them or save them to files.
- Write multiline bodies to a temporary UTF-8 file and pass `--body-file <path>`.
- Write human-facing issue, PR, and review content in Korean, following `AGENTS.md`. Keep CLI arguments and identifiers unchanged.

## Issue Operations

- Create: `gh issue create --title "..." --body-file <path>`.
- Read: `gh issue view <number> --json number,title,body,labels,comments`.
- List: `gh issue list --state open --json number,title,body,labels,comments`. Add `--label` and `--state` as needed.
- Edit the body: `gh issue edit <number> --body-file <path>`.
- Comment: `gh issue comment <number> --body-file <path>`.
- Add or remove labels: `gh issue edit <number> --add-label "..."` or `--remove-label "..."`. Resolve role names through `triage-labels.md`.
- Close: record the necessary result in a comment, then run `gh issue close <number>`.

When a skill says "publish to the issue tracker," create a GitHub issue. "Fetch the relevant ticket" means read the relevant issue.

## Parent Issues and Dependencies

- Sub-issues: `gh issue create --parent <parent> ...` or `gh issue edit <parent> --add-sub-issue <child>`.
- If the CLI does not support sub-issues, use `gh api --method POST repos/tinker495/StS2-W-D-char-mode/issues/<parent>/sub_issues -F sub_issue_id=<child-db-id>`.
- Register a prerequisite: `gh api --method POST repos/tinker495/StS2-W-D-char-mode/issues/<child>/dependencies/blocked_by -F issue_id=<blocker-db-id>`.
- API `*-db-id` values are numeric `id` values, not issue numbers or `node_id`. Read one with `gh api repos/tinker495/StS2-W-D-char-mode/issues/<number> --jq .id`.
- If the server does not support sub-issues, put a child-issue checklist in the parent body and `Part of #<parent>` at the start of each child body. If dependencies are unsupported, put `Blocked by: #<number>, #<number>` at the start of the child body.
- Begin only after all prerequisites are closed. For native dependencies, `issue_dependencies_summary.blocked_by` is the number of open prerequisites.

## Wayfinding

- The map is a parent issue labeled `wayfinder:map`, containing Notes / Decisions-so-far / Fog.
- Create work as sub-issues of the map. Classify it with `wayfinder:research`, `wayfinder:prototype`, `wayfinder:grilling`, or `wayfinder:task`. Prepare these labels when first using this workflow.
- In map order, select an open issue whose prerequisites are resolved and that has no assignee.
- As the first change when starting, run `gh issue edit <number> --add-assignee '@me'`.
- On resolution, leave the answer in a comment, close the issue, and add a summary and link to the map's Decisions-so-far.

## Pull Requests as a Triage Surface

**PRs as a request surface: no.**
