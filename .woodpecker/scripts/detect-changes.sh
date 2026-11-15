#!/usr/bin/env bash
# Decides whether a change touches anything but documents (the "What changed" job of .github/workflows/ci.yml).
#
# A path filter on the workflow would leave the required checks "expected" forever and block the merge of every docs-only pull
# request: a Woodpecker workflow that its `when` filters out reports no status at all. So the workflows always run, and a change
# that only touches documents makes every later step end early: this script writes the marker .ci-docs-only, the steps test it.
#
# The list of changed files is the one Woodpecker gets from the forge (CI_PIPELINE_FILES, a JSON array). It is empty for tags,
# manual runs, empty commits and more than 500 changed files; then there is nothing to compare and everything runs.
set -eu

marker=.ci-docs-only
rm -f "$marker"

files=$(printf '%s' "${CI_PIPELINE_FILES:-}" | grep -o '"[^"]*"' | tr -d '"' || true)
if [ -z "$files" ]; then
  echo "No list of changed files: everything runs."
  exit 0
fi

echo "$files"
if printf '%s\n' "$files" | grep -qvE '^docs/|\.md$|^images/'; then
  echo "Code changed: everything runs."
else
  echo "Only documents changed: build, test and pack are skipped."
  : > "$marker"
fi
