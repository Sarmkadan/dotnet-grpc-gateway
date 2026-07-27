#!/usr/bin/env bash
# =============================================================================
# Build script for DotNetGrpcGateway repository
# Runs restore, build, and test steps using the .NET SDK.
# =============================================================================

set -euo pipefail

# Ensure the .NET SDK is available
if ! command -v dotnet >/dev/null 2>&1; then
    echo "Error: dotnet SDK is not installed or not in PATH."
    exit 1
fi

# Restore NuGet packages
echo "Restoring packages..."
dotnet restore

# Build the solution
echo "Building solution..."
dotnet build --configuration Release --no-restore

# Run tests
echo "Running tests..."
dotnet test --configuration Release --no-build --logger "console;verbosity=normal"

echo "Build and test completed successfully."
