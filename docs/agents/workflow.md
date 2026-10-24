# Repository Work

## Read Within an Output Budget

- Use `rg --files` to locate files and `rg -n` to select headings or symbols before reading long files. Retrieve the relevant line ranges with explicit UTF-8 decoding.
- Reuse instructions already read in this session. Reread only changed sections or context that is no longer available.
- Parallelize independent reads, but budget their combined output below the tool response limit. Narrow each result before printing it; do not concatenate several complete long files into one response.
- If output is truncated, retrieve the missing selected range before making a decision that depends on it. Expand to adjacent sections only when a dependency requires them.

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
