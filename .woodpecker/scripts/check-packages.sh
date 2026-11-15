#!/usr/bin/env bash
# Every package in artifacts/ must carry the release version ("Check every package carries the version" of release.yml).
# Expects RELEASE_VERSION (source .release-env first).
set -eu

count=$(ls artifacts/*.nupkg 2>/dev/null | wc -l)
wrong=$(ls artifacts/*.nupkg 2>/dev/null | grep -v "\.${RELEASE_VERSION}\.nupkg$" || true)
echo "$count packages"
if [ "$count" -eq 0 ] || [ -n "$wrong" ]; then
  echo "Unexpected package versions: $wrong" >&2
  exit 1
fi
