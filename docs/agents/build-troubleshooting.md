# Build execution and diagnostics

Use this reference when capturing build, publish or MegaDot observation results, or when a native crash or sandbox access error interrupts them. Resolve tools and paths with `dev.ps1 doctor` and the root development setup first. Each attempt gets a new local output directory; earlier logs and private helper scripts are never required inputs.

## Capture the command result before displaying logs

Record the child process's exit code immediately, before decoding or printing its output. Keep raw log bytes and the exit code separately. A log-display failure is not a build failure, and a completion message is not proof of a successful process.

The complete PowerShell example below runs `dev.ps1 publish` from the repository root. It uses the installed Python interpreter only as a process/log wrapper; no existing helper file is needed. `-X utf8` gives the wrapper UTF-8 stdout even on a Windows machine whose default Python encoding is CP949. The display decodes UTF-8 with replacement for incompatible bytes, while the saved log preserves the original bytes.

```powershell
$ErrorActionPreference = 'Stop'
$buildArtifacts = Join-Path (Get-Location).ProviderPath 'artifacts'
$buildOutput = Join-Path $buildArtifacts ('publish-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $buildOutput | Out-Null
New-Item -ItemType File -Path (Join-Path $buildOutput '.gdignore') | Out-Null

@'
import subprocess
import sys
from pathlib import Path

repo = Path.cwd()
output = Path(sys.argv[1])
log_path = output / "publish-output.txt"
command = [
    "powershell", "-NoProfile", "-ExecutionPolicy", "Bypass",
    "-File", str(repo / "dev.ps1"), "publish",
]
with log_path.open("wb") as log:
    result = subprocess.run(command, cwd=repo, stdout=log, stderr=subprocess.STDOUT)
(output / "publish.exitcode").write_text(str(result.returncode) + "\n", encoding="utf-8")
try:
    print(f"Publish process exit: {result.returncode}", flush=True)
    print(f"Raw log: {log_path}", flush=True)
    lines = log_path.read_text(encoding="utf-8", errors="replace").splitlines()
    print("\n".join(lines[-35:]), flush=True)
except (OSError, UnicodeError) as error:
    # The process result is already recorded; retain it even if display fails.
    print(f"Log display failed: {type(error).__name__}", file=sys.stderr)
sys.exit(result.returncode)
'@ | python -X utf8 - $buildOutput
$buildWrapperExit = $LASTEXITCODE
$buildExitPath = Join-Path $buildOutput 'publish.exitcode'
if (-not (Test-Path -LiteralPath $buildExitPath)) {
    throw "Publish result was not recorded; wrapper exit $buildWrapperExit. Inspect $buildOutput."
}
$buildProcessExit = [int](Get-Content -LiteralPath $buildExitPath -Encoding UTF8 -Raw)
if ($buildProcessExit -ne 0) {
    throw "Publish failed (process exit $buildProcessExit); inspect $buildOutput."
}
if ($buildWrapperExit -ne 0) {
    throw "Publish exited 0 but its wrapper failed (exit $buildWrapperExit); inspect logs before continuing."
}
```

For direct PowerShell native commands, save `$LASTEXITCODE` immediately after the command. For `Start-Process -Wait -PassThru`, use the returned process's `ExitCode`. Report the process result, script/resource errors, and any log-display problem separately. Inspect the relevant error section from the raw log; an abbreviated tail may omit an earlier error. When a wrapper alone fails, recover the recorded process result before deciding whether another build is needed.

## MegaDot crashes while opening its default log

On MegaDot `4.5.1.m.14.mono.custom_build`, a Windows memory-write error with process exit `0xC0000005` (signed `-1073741819`) can occur before project scripts execute. The identifying backtrace is `RefCounted::unreference` / `Ref<FileAccess>::unref` → `RotatedFileLogger::rotate_file` → `Main::setup`. Match the backtrace before applying this diagnosis to another crash.

A minimal `SceneTree` script that only prints a startup marker and calls `quit(0)` reproduced this failure twice consecutively with the repository's SDK environment. The same command succeeded twice outside the sandbox. Inside the sandbox, adding only `--log-file` with a fresh absolute path under the workspace also succeeded twice. This isolates the failure to default-log handling under restricted permissions; it occurs independently of rig loading or rendering. The observed default user-data directory was `%APPDATA%/Godot/app_userdata/Sts2WD`.

Use a new writable engine log for each process:

- `dev.ps1 anim` and `check` keep stage-specific `*.engine.log` files in their fresh observation directory.
- `dev.ps1 editor` creates `artifacts/editor-<id>/engine.log` and prints the path.
- The `GodotPublish` target creates `artifacts/godot-export-<id>/engine.log`, including when invoked by direct `dotnet publish`.
- For an ad hoc native command, create a fresh directory under `artifacts/` and pass its absolute `artifacts/<run>/engine.log` path through `--log-file`. Retain the engine crash handler and capture stdout, stderr and the actual process exit separately.

Run the affected `dev.ps1` command again and require both its normal completion markers and exit 0. Preserve earlier crash logs for comparison. This avoids the failing default-log path without changing editor settings or the engine binary. The startup diagnosis does not establish the cause of a crash with a different backtrace.

## Sandbox access failures

MegaDot on this PC can report `Failed to read the root certificate store` inside the sandbox. The line alone does not establish the command's result: headless observations have exited 0 with this message, while package export has failed with it.

1. Read the recorded process exit and error context. Report a certificate warning on an otherwise completed observation separately from script/resource failures.
2. If the command fails because of this certificate-store error or explicit access denial, retry the same command once using the tool's escalation mechanism. Keep both attempts' logs.
3. If the retry fails or cannot be authorized, report the exit code and error and stop the affected packaging or installation step. Continue work that does not depend on that output.

Preserve system execution policy and system/editor certificate settings. Keep resource-export failure handling enabled. A failed run does not authorize installing an older package or treating files left in the output directory as a new successful build.
