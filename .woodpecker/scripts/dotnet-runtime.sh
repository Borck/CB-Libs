#!/usr/bin/env bash
# Installs an extra .NET runtime next to the SDK of the container, if it is not there yet (the `dotnet-version: 10.0.x` of
# actions/setup-dotnet in .github/workflows/ci.yml): global.json asks for the .NET 11 SDK, the net10.0 test projects need the
# .NET 10 runtime. A step's container is gone after the step, so run this in the step that runs the tests.
#
#   DOTNET_EXTRA_RUNTIME   channel to install, default 10.0; empty to skip.
#
# A custom CI image with the runtime baked in makes this script unnecessary (see .woodpecker/README.md).
set -eu

channel="${DOTNET_EXTRA_RUNTIME-10.0}"
[ -n "$channel" ] || exit 0

major="${channel%%.*}"
if dotnet --list-runtimes | grep -q "^Microsoft.NETCore.App ${major}\."; then
  echo ".NET $channel runtime is already installed."
  exit 0
fi

root="$(dirname "$(readlink -f "$(command -v dotnet)")")"
script="$(mktemp)"
trap 'rm -f "$script"' EXIT
curl -fsSL https://dot.net/v1/dotnet-install.sh -o "$script"
bash "$script" --runtime dotnet --channel "$channel" --install-dir "$root" --no-path
dotnet --list-runtimes
