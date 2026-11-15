#!/usr/bin/env bash
# net48 is only built, and its tests only run, on Windows; a compile error in them would otherwise first show there. Forcing the
# framework here compiles every test project (and the libraries under it) for net48 against its reference assemblies.
set -u

status=0
for dir in *Tests; do
  [ -d "$dir" ] || continue
  dotnet build "$dir/$dir.csproj" --configuration Release -p:TargetFrameworks=net48 || status=1
done
exit $status
