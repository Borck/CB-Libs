#!/usr/bin/env bash
# Works out the version of a release (the "Work out the version" step of .github/workflows/release.yml) and writes it to
# .release-env, which the later steps of the workflow source (a step cannot hand variables to the next one otherwise).
#
#   tag v<VersionPrefix>[-<suffix>]   e.g. v1.0.0-preview.2 or v1.0.0. The number must equal VersionPrefix in
#                                     Directory.Build.props; what follows the first '-' is the prerelease suffix, none means the
#                                     release version itself (Directory.Build.props' default suffix is cleared).
#   manual run                        variable SUFFIX (default preview.1; empty packs the release version).
#
# Publishing is decided here too: a tag and a deployment to the target nuget-org publish (see release.yaml).
#
# The tag is the version consumers see; the assemblies and the packages say VersionPrefix. Two numbers for one release is how a
# support conversation goes wrong.
set -eu

prefix="$(sed -n 's:.*<VersionPrefix>\(.*\)</VersionPrefix>.*:\1:p' Directory.Build.props | head -n1)"
if [ -z "$prefix" ]; then
  echo "Could not read VersionPrefix from Directory.Build.props." >&2
  exit 1
fi

# CI_COMMIT_TAG is only set for the tag event; a deployment of a tag pipeline carries the tag in the ref.
tag="${CI_COMMIT_TAG:-}"
ref="${CI_COMMIT_REF:-}"
if [ -z "$tag" ]; then
  case "$ref" in
    refs/tags/*) tag="${ref#refs/tags/}" ;;
  esac
fi

if [ -n "$tag" ]; then
  version_part="${tag#v}"
  tag_prefix="${version_part%%-*}"
  suffix=""
  if [ "$version_part" != "$tag_prefix" ]; then
    suffix="${version_part#*-}"
  fi
  if [ "$tag_prefix" != "$prefix" ]; then
    echo "Tag $tag does not match VersionPrefix $prefix in Directory.Build.props." >&2
    exit 1
  fi
else
  suffix="${SUFFIX-preview.1}"
fi

publish=false
if [ "${CI_PIPELINE_EVENT:-}" = tag ] && [ -n "$tag" ]; then
  publish=true
elif [ "${CI_PIPELINE_EVENT:-}" = deployment ] && [ "${CI_PIPELINE_DEPLOY_TARGET:-}" = nuget-org ]; then
  publish=true
fi

version="$prefix${suffix:+-$suffix}"
{
  echo "export RELEASE_TAG='$tag'"
  echo "export RELEASE_SUFFIX='$suffix'"
  echo "export RELEASE_VERSION='$version'"
  echo "export RELEASE_PUBLISH='$publish'"
} > .release-env
echo "Version $version, tag '${tag:-none}', publish=$publish"
