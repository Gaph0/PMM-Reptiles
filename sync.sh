#!/bin/bash
# Syncs the mod into RimWorld's local Mods folder for in-game testing.
# Excludes dev-only files (git, tools, backups, plans) so the in-game copy is clean.
set -e
cd "$(dirname "$0")"

DEST="$HOME/.steam/steam/steamapps/common/RimWorld/Mods/Project Momo Reptiles"
mkdir -p "$DEST"

rsync -a --delete \
  --exclude='/.git' \
  --exclude='/Wiki' \
  --exclude='/Tools' \
  --exclude='Assemblies/*.bak*' \
  --exclude='*.zip' \
  --exclude='/PLAN.md' \
  --exclude='/*_Plan.md' \
  --exclude='/release.sh' \
  --exclude='/sync.sh' \
  ./ "$DEST/"

echo "Synced to: $DEST"
echo "Enable 'Project Momo Reptiles' (pmm.reptiles) in the mod list after Project Momo."
