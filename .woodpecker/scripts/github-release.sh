#!/usr/bin/env bash
# Creates the GitHub release of a tag and attaches the packages (the "Create GitHub Release" step of release.yml:
# generated release notes, prerelease if the tag has a suffix). Expects RELEASE_TAG and RELEASE_VERSION (source .release-env
# first) and GITHUB_TOKEN: a token with "Contents: read and write" on this repository (Woodpecker secret github_token).
# Does nothing without a tag. Uses the GitHub REST API, so the repository has to be on github.com.
set -eu

if [ -z "${RELEASE_TAG:-}" ]; then
  echo "Not a tag: no GitHub release."
  exit 0
fi
if [ -z "${GITHUB_TOKEN:-}" ]; then
  echo "Set the github_token secret (Contents: read and write), enabled for the tag and deployment events." >&2
  exit 1
fi

api=https://api.github.com/repos/$CI_REPO
auth=(-H "Authorization: Bearer $GITHUB_TOKEN" -H "Accept: application/vnd.github+json" -H "X-GitHub-Api-Version: 2022-11-28")

prerelease=false
case "$RELEASE_TAG" in *-*) prerelease=true ;; esac

# An existing release of the tag (a re-run) is used as it is; the first "id" of the answer is the release's own.
if response=$(curl -fsS "${auth[@]}" "$api/releases/tags/$RELEASE_TAG" 2>/dev/null); then
  echo "Release of $RELEASE_TAG exists."
else
  body=$(printf '{"tag_name":"%s","name":"CB-Libs %s","draft":false,"prerelease":%s,"generate_release_notes":true}' \
    "$RELEASE_TAG" "$RELEASE_VERSION" "$prerelease")
  response=$(curl -fsS -X POST "${auth[@]}" -d "$body" "$api/releases")
fi
id=$(printf '%s' "$response" | grep -o '"id": *[0-9]*' | head -n1 | grep -o '[0-9]*')
if [ -z "$id" ]; then
  echo "Could not read the release id from the answer of GitHub." >&2
  exit 1
fi

failed=0
for file in artifacts/*.nupkg artifacts/*.snupkg; do
  [ -f "$file" ] || continue
  name=$(basename "$file")
  code=$(curl -sS -o /dev/null -w '%{http_code}' -X POST "${auth[@]}" -H "Content-Type: application/octet-stream" \
    --data-binary "@$file" "https://uploads.github.com/repos/$CI_REPO/releases/$id/assets?name=$name")
  # 422: an asset of that name is already there (a re-run); a re-run attaches what is missing and ignores the rest.
  case "$code" in
    201) echo "Attached $name" ;;
    422) echo "$name is already attached" ;;
    *)   echo "Attaching $name failed: HTTP $code" >&2; failed=1 ;;
  esac
done
exit $failed
