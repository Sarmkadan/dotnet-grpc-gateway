#!/usr/bin/env bash
# =============================================================================
# Compatibility build script for the sql-index-advisor path.
# This script forwards the call to the actual build.sh located in the repository root.
# =============================================================================

# Resolve the directory containing this script
DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Execute the real build script located in the sibling dotnet-grpc-gateway directory
"${DIR}/dotnet-grpc-gateway/build.sh"
