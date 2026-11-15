#!/usr/bin/env bash
# Runs every *Tests project, one at a time: a fatal error in one must not stop the rest, and the exit code reports any failure.
# global.json at the repository root carries the Microsoft.Testing.Platform opt-in, so run this from the repository root.
#
#   SKIP_TEST_PROJECTS   space separated test project names to leave out, e.g. "CB.Win32Tests" (it calls the Windows API).
#   TEST_REPORT          true (default): write TRX and cobertura coverage reports to $TEST_RESULTS_DIR; false: just run.
#   TEST_RESULTS_DIR     default ./TestResults
set -u

skip=" ${SKIP_TEST_PROJECTS:-} "
report="${TEST_REPORT:-true}"
results="${TEST_RESULTS_DIR:-$PWD/TestResults}"

status=0
for dir in *Tests; do
  [ -d "$dir" ] || continue
  case "$skip" in
    *" $dir "*) echo "Skipping $dir."; continue ;;
  esac

  args=(test --project "$dir/$dir.csproj" --configuration Release --no-build)
  if [ "$report" = true ]; then
    args+=(-- --results-directory "$results/$dir" --report-trx --coverage --coverage-output-format cobertura)
  fi

  echo "=== $dir"
  dotnet "${args[@]}" || { echo "Tests failed in $dir"; status=1; }
done
exit $status
