#!/usr/bin/env python3
"""
A simple build helper script for the dotnet-grpc-gateway repository.

Running this script will:

1. Restore NuGet packages.
2. Build the solution.
3. Run all unit tests.

It is intentionally lightweight and has no external dependencies beyond the
standard library and the .NET SDK (dotnet CLI). If any step fails, the script
will exit with a non‑zero status code and print the relevant error output.

Usage:
    python3 aider_buildcmd.py
"""

import subprocess
import sys
from pathlib import Path

def run_command(command: list[str], cwd: Path | None = None) -> None:
    """Run a command and stream its output. Raise on non‑zero exit."""
    try:
        result = subprocess.run(
            command,
            cwd=cwd,
            check=True,
            stdout=subprocess.PIPE,
            stderr=subprocess.STDOUT,
            text=True,
        )
        print(result.stdout)
    except subprocess.CalledProcessError as exc:
        print(f"Command {' '.join(command)} failed with exit code {exc.returncode}")
        print(exc.stdout)
        sys.exit(exc.returncode)

def main() -> int:
    repo_root = Path(__file__).parent.resolve()

    # 1. Restore packages
    print("Restoring NuGet packages...")
    run_command(["dotnet", "restore"], cwd=repo_root)

    # 2. Build the solution
    print("Building the solution...")
    run_command(["dotnet", "build", "--configuration", "Release", "--no-restore"], cwd=repo_root)

    # 3. Run tests
    print("Running unit tests...")
    run_command(["dotnet", "test", "--configuration", "Release", "--no-build", "--logger:trx"], cwd=repo_root)

    print("Build and test steps completed successfully.")
    return 0

if __name__ == "__main__":
    sys.exit(main())
