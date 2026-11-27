"""Export compiled Realta rig-c packages into native Spine 4.2 (.spjson, .spatlas, .png) runtime assets."""

from __future__ import annotations

import argparse
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[2]
SCRIPTS_DIR = ROOT / "scripts"
if str(SCRIPTS_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPTS_DIR))

from export_spine42 import export_spine42


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("package", type=Path, help="Compiled rig-c output directory (containing build/ and optional select/build/)")
    parser.add_argument("output", type=Path, help="Target combat runtime directory (e.g. Sts2WD/animations/realta/combat)")
    args = parser.parse_args()

    package = args.package.resolve()
    combat_out = args.output.resolve()
    default_combat_out = (ROOT / "Sts2WD/animations/realta/combat").resolve()
    is_repo_target = combat_out == default_combat_out

    combat_info = export_spine42(
        build_dir=package / "build",
        out_dir=combat_out,
        stem="realta_skeleton",
        res_dir="res://Sts2WD/animations/realta/combat",
        sync_atlas_txt=(ROOT / "docs/characters/realta/animation/rigc/atlas.txt") if is_repo_target else None,
    )
    print(
        f"Realta combat/merchant Spine 4.2: {len(combat_info['animations'])} clips "
        f"({', '.join(combat_info['animations'])}), {combat_info['dimensions'][0]}x{combat_info['dimensions'][1]}."
    )

    select_build = package / "select" / "build"
    if select_build.is_dir():
        select_out = (ROOT / "Sts2WD/animations/realta/select").resolve() if is_repo_target else (combat_out.parent / "select")
        select_info = export_spine42(
            build_dir=select_build,
            out_dir=select_out,
            stem="realta_select_atlas",
            res_dir="res://Sts2WD/animations/realta/select",
            sync_atlas_txt=(ROOT / "docs/characters/realta/animation/rigc/select_atlas.txt") if is_repo_target else None,
        )
        print(
            f"Realta select Spine 4.2: {len(select_info['animations'])} clip "
            f"({', '.join(select_info['animations'])}), {select_info['dimensions'][0]}x{select_info['dimensions'][1]}."
        )

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
