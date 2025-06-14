#!/usr/bin/env python3
"""
merge_telemetry.py

– Hierarchically list all envs and their timestamp-groups
– Single prompt “E.G” (e.g. 2.1) to pick env #2, group #1
– Merge only that group’s eco-log.csv via pandas
– Sort by endTime, renumber episodes, drop time cols
– Drop any extra columns listed in `removeList` (warn if missing)
– Write merged-eco-log_<PREFIX>.csv next to the runs
"""

import sys
from pathlib import Path
import pandas as pd

# ─── User-configurable list of columns to remove ──────────────────────────────
removeList = [
    "totalPredatorsSpawned", 
    "maxPredatorGeneration",
    "animalKilled",
    "diedFromHealthOver"
    # add any other column-names here
]
# ───────────────────────────────────────────────────────────────────────────────

def list_dirs(p: Path):
    return sorted([d for d in p.iterdir() if d.is_dir()], key=lambda x: x.name)

def group_by_prefix(runs):
    groups = {}
    for r in runs:
        prefix = r.name.rsplit("_", 1)[0]
        groups.setdefault(prefix, []).append(r)
    return dict(sorted(groups.items()))

def print_hierarchy(envs, all_groups):
    print("\nAvailable environments and timestamp-groups:\n")
    for ei, env in enumerate(envs, start=1):
        groups = all_groups[env.name]
        print(f"[{ei}] {env.name}")
        if not groups:
            print("    (no runs)\n")
            continue
        for gi, (prefix, runs) in enumerate(groups.items(), start=1):
            print(f"    [{ei}.{gi}] {prefix} ({len(runs)} runs)")
        print()

def parse_choice(choice, envs, all_groups):
    try:
        e_str, g_str = choice.split(".")
        ei = int(e_str) - 1
        gi = int(g_str) - 1
        env = envs[ei]
        prefixes = list(all_groups[env.name].keys())
        prefix = prefixes[gi]
        runs = all_groups[env.name][prefix]
        return env, prefix, runs
    except Exception:
        return None, None, None

def merge_group(env_dir: Path, runs, prefix: str):
    dfs = []
    for r in runs:
        f = r / "eco-log.csv"
        if not f.exists():
            print(f"  ▶ missing {r.name}/eco-log.csv – skipping")
            continue
        dfs.append(pd.read_csv(f))
    if not dfs:
        print("No data found; aborting.")
        return

    # concat & sort
    df = pd.concat(dfs, ignore_index=True)
    if "endTime" not in df:
        print("Missing endTime column; cannot sort.")
        return
    df = df.sort_values("endTime").reset_index(drop=True)

    # drop old episode & time cols
    for c in ("episode", "startTime", "endTime"):
        if c in df.columns:
            df.drop(columns=c, inplace=True)

    # drop user-specified columns, warn if absent
    for col in removeList:
        if col in df.columns:
            df.drop(columns=col, inplace=True)
        else:
            print(f"Warning: column '{col}' not found in merged data.")

    # Show all remaining columns so the user can inspect what's left
    print("\nColumns in merged data:")
    print(", ".join(df.columns.tolist()))

    # insert new episode number at front
    df.insert(0, "episode", df.index + 1)

    # write merged CSV
    out_name = f"merged-eco-log_{prefix}.csv"
    out_path = env_dir / out_name
    df.to_csv(out_path, index=False)
    print(f"\n✔ merged {len(df)} episodes from {len(dfs)} runs → {out_path}")

def main():
    # resolve project root from this script (Assets/Scripts/Ecosystem → up 3)
    script = Path(__file__).resolve()
    project = script.parents[3]
    telemetry = project / "Assets" / "Telemetry"

    if not telemetry.exists():
        print(f"Error: '{telemetry}' not found.")
        sys.exit(1)

    # build hierarchy
    envs = list_dirs(telemetry)
    all_groups = {env.name: group_by_prefix(list_dirs(env)) for env in envs}

    # display
    print_hierarchy(envs, all_groups)

    # single prompt
    choice = input("Enter selection (E.G), or blank to exit: ").strip()
    if not choice:
        print("Aborted.")
        return

    env, prefix, runs = parse_choice(choice, envs, all_groups)
    if not env:
        print("Invalid selection.")
        sys.exit(1)

    merge_group(env, runs, prefix)

if __name__ == "__main__":
    main()
