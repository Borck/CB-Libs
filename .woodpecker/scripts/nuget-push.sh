#!/usr/bin/env bash
# Pushes artifacts/*.nupkg to nuget.org (the "Push" step of release.yml). The .snupkg beside each .nupkg goes to the symbol
# server with it. --skip-duplicate: a re-run after a partial failure pushes what is missing.
# Needs NUGET_API_KEY: a nuget.org key with push rights on the CB.* packages (Woodpecker secret nuget_api_key).
set -eu

if [ -z "${NUGET_API_KEY:-}" ]; then
  echo "Set the nuget_api_key secret (a nuget.org key with push rights on the CB.* packages), enabled for the tag and deployment events." >&2
  exit 1
fi

for package in artifacts/*.nupkg; do
  dotnet nuget push "$package" --skip-duplicate --source https://api.nuget.org/v3/index.json --api-key "$NUGET_API_KEY"
done
