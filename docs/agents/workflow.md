# Repository Work

## Scope and Settled Decisions

- Treat explicit user requirements, corrections, and earlier answers as settled decisions, including during design interviews. Preserve the request's other values and constraints.
- Ask only about unresolved choices whose alternatives would materially change the outcome. Once the required facts are available and every material decision is settled, summarize the result and continue work already authorized by the user's request. A skill's generic final-confirmation step does not reopen settled choices.
- A design-only request ends with the agreed design and its requested documentation. Invoking a skill alone does not authorize implementation or expand the task's scope.

## Complete Requested Work

Apply this procedure to implementation, asset delivery, and requests to create or update a PR, including ordinary requests such as "open a PR". Keep it within the user's requested action. For a PR-body-only request, use the supplied change and available evidence and identify unexecuted verification accurately.

1. Reconcile the current request and settled decisions with the final diff. Account for each requested behavior, card, image or other deliverable in its product files and actual entry points. Keep this working list in task context. Use the existing issue, design or asset record when a shared record is needed.
2. Resolve the actual owner and read the applicable topics in [CODING_STANDARDS.md](/CODING_STANDARDS.md). Compare the affected implementation with those conventions and its canonical specification. Inspect judgement-based requirements such as ownership of card behavior and cross-character coupling directly, and fix in-scope violations found during the work. A successful build or valid filename establishes only the conditions its check covers.
3. For artwork application, reuse the user's existing selection/application authority. Match every requested asset to its model or native visual owner, selected pixels, runtime file and current mapping record. Trace the actual resource selection through inheritance, overrides, resource lookup and fallback, including import/scene references where relevant. Resolve an omitted runtime file or an override selecting another image before reporting the requested application complete. Use the existing [asset delivery procedure](/.agents/skills/character-asset-pipeline/workflow.md#delivery-and-completion-records) for asset records, packaging and loading observations.
4. Select existing checks for the affected changes using the entry points below and the relevant test specifications. Use a command that includes the needed checks and reuse its results for the same inputs. Follow [test ownership](test-ownership.md) when a check fails or a specification conflicts with the request.

   | Affected work | Existing entry points |
   |---|---|
   | Tracked documentation, links and resource references | `dev.ps1 verify` checks tracked references and runtime card PNG dimensions/opacity. |
   | C# implementation | `dev.ps1 build` includes those checks and the C# build. `dev.ps1 check` is the existing locked-restore build route. |
   | Runtime images, scenes or localization | Use the reference/PNG checks above and `dev.ps1 publish` for Godot import and packaging, then the applicable loading observations. Follow the existing deployment procedure when installation is requested. |
   | Card previews, API lookup or animation | Use the applicable existing commands and specifications linked from [development tools](/docs/development-tools.md) and the affected topic. |

5. For conditions beyond mechanical validation, perform the comparison in the current task. Check request completeness against the final diff, conventions against the affected code, and artwork mappings against the resource selection. Record the relevant type/member, path or specification and the observation supporting the result. Correct omissions within the authorized scope. An agent's inspection and image review have the authority defined by the original request and existing approval rules.
6. When a tool or dependency is unavailable, continue the checks and source/resource inspection possible in the current environment. Report the unavailable command, concrete reason and the limits of the evidence obtained. Keep the user's requested PR action within its existing authorization. An environment limitation alone adds neither a separate verification environment nor a mandatory Draft stage. Actual failing checks and specification conflicts follow their existing rules.
7. Associate verification and inspection results with the final change. Record the commit when available and describe relevant working-tree changes. If code, artwork, mappings or other inputs change after a check, refresh the affected verification and conclusions. Reuse existing packaging and observation diagnostics, and keep enough command/input/result information in the PR for readers without local logs.
8. Before the final response or requested PR action, inspect the final diff for missing deliverables, unrelated changes and stale mappings. For an existing PR, reconcile review findings with the final change and record the correction, supported rebuttal or unresolved issue. Report requested outcomes, actual checks and direct inspection, remaining failures and unexecuted verification accurately. Execute the authorized PR action after this preparation. A body-writing request uses the final change and available evidence within that scope.

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

## Local Shell

Run repository scripts through `dev.ps1` or `powershell.exe` (Windows PowerShell 5.1); `pwsh` is not installed on this PC. CI runs the tracked-reference check under PowerShell 7, so keep shared scripts compatible with both.

## Temporary Observations

Run one-off PowerShell reproductions through `dev.ps1 observe -ScriptPath <temporary-script>`. Keep preparation and execution in the same script so setup errors stop the run and are recorded. Invoke native tools through the shared `Invoke-RecordedCommand` helper to enforce exit codes and retain logs. Follow the script contract and evidence format in [development tools](/docs/development-tools.md#임시-검증-실행).

Keep temporary scripts outside formal test directories and do not commit them. Their results are observations, not changes to product acceptance criteria; follow the root test ownership rules.

## PR Evidence

Create PR-only screenshots, comparison renders, evidence notes, and one-off render scripts under the ignored `artifacts/pr-<task>/` directory from the start. Keep product assets and reusable specifications in their normal tracked locations. Attach PR evidence directly to the PR when upload is available, or show local artifacts in the completion report. Keep evidence files out of Git even when upload is unavailable.

Before staging, confirm that each PR-only output is ignored with `git check-ignore -- <path>` and that `git ls-files -- <path>` returns no entries. Stage explicit product and documentation paths. For human-facing procedures, see [development tools](/docs/development-tools.md#pr-자료-보관).

## Rebase and Verify

For a rebase request, inspect the branch and working tree, then fetch the requested remote target. Start with `dev.ps1 rebase -Upstream <target>` from the repository root; the default target is `origin/main`. Git requires a clean working tree. Preserve user changes and follow the session's existing authority for committing or stashing them.

The command preserves commit order and appends one final invocation of the repository's pre-commit hook to Git's rebase todo. That hook owns reference validation and the C# build. Git retains the pending check across conflicts and reschedules it after failure, even if the wrapper is absent from an intermediate commit.

- For a content conflict, resolve it within the authorized scope, stage the resolved files, and run `git rebase --continue`.
- For a failed final check, investigate and fix the implementation or documentation. Incorporate the fix into the rebased commit, then run `git rebase --continue` to rerun the queued check. Preserve protected tests and validation criteria.
- For an ordinary rebase started without this command, run `git -c core.hooksPath=.githooks hook run pre-commit` after Git finishes. The wrapper cannot retrofit its check into an already-running rebase.
- Report completion only when the final check succeeds, no rebase remains in progress, the requested target is an ancestor of HEAD, and the working-tree state is accounted for. Report unresolved failures separately and continue authorized fixes rather than equating rewritten commits with validated work.

Git can create rewritten commits before a failed final check; the check prevents this workflow from completing successfully, not the creation of every intermediate commit. The command does not fetch, push, or automatically stash changes.

## Shared Deployment Records

[Deployment status](/docs/deployment-status.md) owns cross-character package, installation and verification records. Character asset guides own their file mappings and asset-specific application status, and link to the shared record. A historical deployment describes its original environment; it does not establish installation status for the current checkout.
