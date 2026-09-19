#!/bin/bash
# Builds Assemblies/PMM_Reptiles.dll via dotnet (PMM.Reptiles.csproj).
# The core ProjectMomo.csproj is built automatically through the ProjectReference.
set -e
cd "$(dirname "$0")"

dotnet build PMM.Reptiles.csproj -v:m

# The core build overwrites 'Project Momo/About/About.xml' with a generated
# minimal version. Restore the hand-written one so sync.sh deploys correctly.
git -C "$HOME/Desktop/Project Momo" checkout -- About/About.xml 2>/dev/null || true

echo "Built Assemblies/PMM_Reptiles.dll"
