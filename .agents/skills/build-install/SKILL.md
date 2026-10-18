---
name: build-install
description: Build this repository's mod on Windows and install it into local Slay the Spire 2. Use when asked to patch the game with the current branch or reinstall the mod.
---

# Build and Local Installation

Package the current checkout and install it into the game's `mods/Sts2WD`. Use the repository-root `dev.ps1` build workflow.

## Confirm the Target and Environment

Start at the repository root. Record the branch, commit, and existing changes with `git status --short --branch` and `git log -1 --oneline`. Preserve the user's selected checkout and state in the completion report that uncommitted changes are included in the build.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 doctor
```

After `doctor` succeeds, use its `Sts2Path`, `ModsPath`, and `GodotPath`. Let `dev.ps1` select the SDK; do not pin the game path, SDK version, or BaseLib version in this skill. For missing dependencies, read the preparation instructions in `README.md` and resolve the installation blockers.

Check whether the game is running with `Get-Process -Name SlayTheSpire2 -ErrorAction SilentlyContinue`. Packaging can proceed while it runs. Before replacing installed files, ask the user to close the game and hold installation until it exits.

## Package

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\dev.ps1 publish
```

Pass `-Configuration Debug` or `-Configuration Release` if the user specified a configuration. `publish` performs the C# build, MegaDot resource import, and PCK export; a separate repeated `build` is unnecessary.

Proceed to installation only if the process exits with code 0 and all four files exist in the output directory `<ModsPath>/Sts2WD`. Do not substitute files left by a failed run or an older package.

- `Sts2WD.dll`
- `Sts2WD.pck`
- `Sts2WD.json`
- `Sts2WD.pdb`

Store long logs under the repository's ignored `artifacts/` directory and inspect the error section on failure. A resource-export completion message alone is not proof of success.

### Sandbox Failures

On this PC, MegaDot has reported `Failed to read the root certificate store` inside the sandbox and returned a failed export exit code. For this error or explicit access denial, retry that command once using the tool's escalation mechanism. If the retry fails, report the error and stop installation. Do not change system execution policy or certificate settings.

## Back Up, Then Install

Complete packaging before writing to the game installation. An installation request authorizes replacing the mod files; do not ask for the same confirmation again. Apply any required sandbox permission to the actual copy command.

Use `$ErrorActionPreference = 'Stop'` in PowerShell and `-LiteralPath` for paths.

1. Resolve source `<ModsPath>/Sts2WD` and destination `<Sts2Path>/mods/Sts2WD` from `doctor`. Pass paths as strings because they can contain spaces and `&`. Recheck that the game has exited immediately before replacing files.
2. Verify all four source files exist. Copy existing destination files with those names into `artifacts/runtime/local-install-backup-<timestamp>/` in the repository. Use a new backup directory for each run and print its path. Do not replace destination files if backup fails.
3. Create the destination directory and install only those four files with `Copy-Item -Force`. Game DLLs, BaseLib, and other mods are outside this package.
4. Run `Get-FileHash -Algorithm SHA256` for each source and installed file. Report installation failure and the backup path if any file is missing or differs. Installation is complete when all four hashes match.

Check `git status --short` after building. Before removing `.cs.uid` files created by this MegaDot run, confirm they were absent at the start and target only those files. Preserve existing changes and tracked resources.

## Completion Report

Briefly report the installed branch and commit, game destination, build and packaging results, four-file hash comparison, and backup path. Distinguish tests run from verification not performed. Do not describe building or matching file hashes as in-game play verification.

Launch the game or change mod settings only when requested. If activation could not be checked, tell the user that BaseLib and Sts2WD must be enabled in the game.
