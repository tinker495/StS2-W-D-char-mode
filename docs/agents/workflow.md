# Repository Work

## Scope and Settled Decisions

- Treat explicit user requirements, corrections, and earlier answers as settled decisions, including during design interviews. Preserve the request's other values and constraints.
- Ask only about unresolved choices whose alternatives would materially change the outcome. Once the required facts are available and every material decision is settled, summarize the result and continue work already authorized by the user's request. A skill's generic final-confirmation step does not reopen settled choices.
- A design-only request ends with the agreed design and its requested documentation. Invoking a skill alone does not authorize implementation or expand the task's scope.

## Read Within an Output Budget

- Use `rg --files` to locate files. For a topic index, open only the requested topic and its necessary dependencies; keep API references and inventories separate from routine production reads. Use `rg -n` to select headings or symbols within a longer reference, then retrieve the relevant UTF-8 line range.
- Reuse instructions already read in this session. Reread only changed sections or context that is no longer available.
- Resolve each skill once: honor an explicit user-provided path; for named dependencies, use the invoking skill's source (repository or plugin). Keep the resolved paths in task context and reuse their content. Consult another installed copy only when the selected source is unavailable.
- Parallelize independent reads, but budget their combined output below the tool response limit. Narrow each result before printing it; do not concatenate several complete long files into one response.
- For tool discovery, return matching tool names first, then inspect the selected tool's description. Keep unrelated descriptions out of the lookup output.
- If output is truncated, retrieve the missing selected range before making a decision that depends on it. Expand to adjacent sections only when a dependency requires them.

## Fact Finding and Delegation

- Resolve small local lookups directly with bounded searches and reads, including when an invoked skill prescribes delegation. Delegate substantial independent investigations when doing so reduces latency or context load.
- Give each agent a non-overlapping question and require source locations plus the relevant observations. Work on separate questions while it runs.
- Reuse returned evidence; reopen its sources only to resolve a concrete gap or contradiction. A running investigation is a prerequisite only for decisions that depend on its findings.

## Temporary Observations

Run one-off PowerShell reproductions through `dev.ps1 observe -ScriptPath <temporary-script>`. Keep preparation and execution in the same script so setup errors stop the run and are recorded. Invoke native tools through the shared `Invoke-RecordedCommand` helper to enforce exit codes and retain logs. Follow the script contract and evidence format in [development tools](../development-tools.md#임시-검증-실행).

Keep temporary scripts outside formal test directories and do not commit them. Their results are observations, not changes to product acceptance criteria; follow the root test ownership rules.

## PR Evidence

Create PR-only screenshots, comparison renders, evidence notes, and one-off render scripts under the ignored `artifacts/pr-<task>/` directory from the start. Keep product assets and reusable specifications in their normal tracked locations. Attach PR evidence directly to the PR when upload is available, or show local artifacts in the completion report. Keep evidence files out of Git even when upload is unavailable.

Before staging, confirm that each PR-only output is ignored with `git check-ignore -- <path>` and that `git ls-files -- <path>` returns no entries. Stage explicit product and documentation paths. For human-facing procedures, see [development tools](../development-tools.md#pr-자료-보관).

## Rebase and Verify

For a rebase request, inspect the branch and working tree, then fetch the requested remote target. Start with `dev.ps1 rebase -Upstream <target>` from the repository root; the default target is `origin/main`. Git requires a clean working tree. Preserve user changes and follow the session's existing authority for committing or stashing them.

The command preserves commit order and appends one final invocation of the repository's pre-commit hook to Git's rebase todo. That hook owns reference validation and the C# build. Git retains the pending check across conflicts and reschedules it after failure, even if the wrapper is absent from an intermediate commit.

- For a content conflict, resolve it within the authorized scope, stage the resolved files, and run `git rebase --continue`.
- For a failed final check, investigate and fix the implementation or documentation. Incorporate the fix into the rebased commit, then run `git rebase --continue` to rerun the queued check. Preserve protected tests and validation criteria.
- For an ordinary rebase started without this command, run `git -c core.hooksPath=.githooks hook run pre-commit` after Git finishes. The wrapper cannot retrofit its check into an already-running rebase.
- Report completion only when the final check succeeds, no rebase remains in progress, the requested target is an ancestor of HEAD, and the working-tree state is accounted for. Report unresolved failures separately and continue authorized fixes rather than equating rewritten commits with validated work.

Git can create rewritten commits before a failed final check; the check prevents this workflow from completing successfully, not the creation of every intermediate commit. The command does not fetch, push, or automatically stash changes.

## Shared Deployment Records

[Deployment status](../deployment-status.md) owns cross-character package, installation and verification records. Character asset guides own their file mappings and asset-specific application status, and link to the shared record. A historical deployment describes its original environment; it does not establish installation status for the current checkout.
