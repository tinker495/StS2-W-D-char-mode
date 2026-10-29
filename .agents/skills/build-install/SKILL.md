---
name: build-install
description: Build this repository's mod on Windows and install it into local Slay the Spire 2. Use when asked to patch the game with the current branch or reinstall the mod.
---

# Build and Local Installation

Use the repository-root `dev.ps1` workflow. It owns packaging, installation, backups, and hash verification.

## Confirm the Target and Environment

Record the branch, commit, and existing changes with `git status --short --branch` and `git log -1 --oneline`. Preserve the selected checkout. Uncommitted product changes are included in the build; disclose them.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./dev.ps1 doctor
```

Use the resolved `Sts2Path`, `ModsPath`, and `GodotPath`. Let `dev.ps1` select dependencies; do not pin versions or paths in this skill. For missing dependencies, follow `README.md`.

Check `Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue`. Packaging may proceed while the game runs. If it is running, ask the user to close it and hold installation until it exits.

## Package or Install

For packaging only:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./dev.ps1 publish -ExecutionContextLabel sandbox
```

For an authorized installation:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./dev.ps1 deploy -ExecutionContextLabel sandbox
```

Use `-Configuration` only when requested. Set `-ExecutionContextLabel` to the actual invocation context: `sandbox`, `escalated`, or `local`. This label records the caller's knowledge; it does not grant or detect permissions. Omit it if unknown.

`publish` and `deploy` perform the C# build, MegaDot resource import, and PCK export. Animation rig checks run only through `dev.ps1 anim`; [check scope](../../../tests/rig-checks.md) documents them.

`deploy` packages first, checks the four package files and game process, backs up existing files, installs only the package, and compares SHA256 hashes. Apply required sandbox permission to the command. An installation request authorizes replacing the mod files; do not ask for the same confirmation again. Avoid separate copy commands or a redundant build/publish before deploy.

Completion requires exit code 0 and all four reported hash matches. Preserve the printed backup path. A resource-export completion line or old output files do not establish success. After a partial installation failure, report the backup path and inspect installed state before recovery.

## Diagnose Failures Without Installing Failed Output

Each `publish`/`deploy` run prints a unique `artifacts/runtime/packaging-...` directory and one result/log-path line per command. Full stdout/stderr remains in UTF-8 logs. Console output includes at most five diagnostic lines per command and eight tail lines on failure, each limited to 400 characters. An exit-zero command can still print engine diagnostics such as certificate-store errors; these remain visible and do not change the existing package-completion checks.

Read `run.json` and the failed step's JSON first, then retrieve a bounded excerpt from that step's log. Expand only around the relevant diagnostic; do not print all build/import/export logs together. Version steps record the actual SDK and MegaDot versions. Command records include executable, arguments, working directory, UTC times, and exit code. Do not dump credentials or the process environment into diagnostics.

Rig checks retain their `*.stdout.log`, `*.stderr.log`, `*.engine.log`, and `*.exitcode` files in the same diagnostic directory. For process-result capture and MegaDot startup-log crashes, use [Build execution and diagnostics](../../../docs/agents/build-troubleshooting.md).

For `Failed to read the root certificate store` or explicit access denial, retry once through the tool's escalation mechanism and mark the new run `escalated`. Keep both runs for comparison.

If the retry fails, hold installation of that failed output and continue safe diagnosis within the authorized task. Compare the actual error between attempts, narrow the failing command, and use the `diagnosing-bugs` workflow. A changed error needs a new hypothesis, not repeated blind retries. Leave system execution policy and certificate settings unchanged.

Once diagnosis or a fix produces a successful full workflow, continue the authorized installation. Ask for input only when a concrete missing decision or permission blocks further progress.

Check `git status --short` afterward. Remove generated `.cs.uid` files only if they were absent at task start and are confirmed to belong to this run. Preserve existing changes and tracked resources.

## Completion Report

Report branch/commit, destination, build/package result, four-file hash comparison, backup and diagnostics paths. Distinguish operational recovery from a verified root-cause fix: successful reruns alone do not establish cause or permanent remediation.

State tests actually run and verification not performed. Building and matching hashes are not in-game verification. Launch the game or change mod settings only when requested. If activation was not checked, tell the user that BaseLib and Sts2WD must be enabled.
