# Woodpecker CI

The pipeline of this repository for [Woodpecker CI](https://woodpecker-ci.org). It does what `.github/workflows/ci.yml`,
`.github/workflows/release.yml` and `azure-pipelines.yml` do, except the **Windows** build and tests (net48 and `CB.Win32Tests`):
Woodpecker has no Windows agent, so those stay in the Windows job of `ci.yml`, the only job of it that is still switched on.
The other old files are still there and still work, so the old CI can be switched off once this one has proved itself
([Switching the old CI off](#switching-the-old-ci-off)).
Woodpecker loads only the `.yaml`/`.yml` files of this folder (subfolders and other files are ignored): `scripts/` and this
file are not workflows.

| Woodpecker workflow | Replaces | Runs on |
| --- | --- | --- |
| `build-linux.yaml` | ci.yml `Build & Test (Linux)`, Azure build/test | Linux agent, Docker backend |
| `pack.yaml` | ci.yml `Pack the packages`, Azure pack | Linux agent, after `build-linux` |
| `release.yaml` | release.yml `Test & pack`, `Publish to nuget.org`, GitHub release | Linux agent |
| *(none)* | ci.yml `Build & Test (Windows)`, codeql.yml | stay on GitHub, see [Not migrated](#not-migrated-and-different) |

## Set up

1. **Activate the repository** in the Woodpecker UI (Woodpecker has to be connected to GitHub: OAuth app on the server,
   `WOODPECKER_GITHUB=true`). Leave the pipeline path empty; `.woodpecker/*.yaml` is found by default.
2. **Repository settings** (Settings -> General): *Allow pull requests* on; *Allow deployments* on (release publishes through a
   deployment, see below); *Cancel previous pipelines* on for `push` and `pull_request` (the `concurrency: cancel-in-progress`
   of ci.yml); *Require approval for* at least the default (forks).
3. **Agent.** Any agent with the Docker backend. The workflows ask for `platform=linux/amd64` and `backend=docker`; an agent that
   carries no labels matches everything, so give it its default labels.
4. **Secrets** (Settings -> Secrets), only needed for releases. Enable each for the event `deployment` only (not `pull_request`):

   | Secret | Content |
   | --- | --- |
   | `nuget_api_key` | nuget.org API key with push rights on the CB.* packages (the `NUGET_API_KEY` of the GitHub environment `nuget-org`) |
   | `github_token` | token with *Contents: read and write* on this repository, for the GitHub release |

5. **Required checks.** Woodpecker reports one status per workflow, named `ci/woodpecker/<event>/<workflow>`, for example
   `ci/woodpecker/pr/build-linux` and `ci/woodpecker/pr/pack`. Replace the old required checks `Build & Test (Linux)` and
   `Pack the packages` with these in the branch protection of `master`, after the first pull request has reported them (GitHub lists a
   check only once it has seen it). `Build & Test (Windows)` stays required: it is still produced by GitHub Actions.

## How it differs from GitHub Actions

- **Docs-only changes.** ci.yml skips the build for a change that only touches `docs/`, `images/` or `*.md`, as a skipped *job*
  and not as a path filter, so a required check never stays "expected". A Woodpecker workflow that its `when` filters out reports
  no status, so the same rule applies: the workflows always run, `scripts/detect-changes.sh` looks at the changed files Woodpecker got
  from GitHub (`CI_PIPELINE_FILES`) and, for a documents-only change, every later step ends at once (`.ci-docs-only` marker).
  Tags, manual runs and changes of more than 500 files run everything.
- **Deterministic builds.** `Directory.Build.props` sets `ContinuousIntegrationBuild` when `CI == 'true'`; Woodpecker sets
  `CI=woodpecker` and a step cannot overwrite it, so every step sets `ContinuousIntegrationBuild: true` itself. (Azure Pipelines
  sets neither, so its packages were never CI builds.)
- **Release approval.** GitHub's `nuget-org` environment with required reviewers has no counterpart. A tag only tests and packs.
  Publishing happens when somebody with push access opens the green pipeline of the tag and presses *Deploy* with the target
  `nuget-org`; the deployment rebuilds the commit (deterministically), pushes the packages with `--skip-duplicate` and creates the
  GitHub release (notes generated, prerelease when the tag has a suffix). A manual run takes the variable `SUFFIX` like the
  `workflow_dispatch` input (default `preview.1`, empty packs the release version) and never publishes by itself; to publish a
  preview from a branch, deploy that manual pipeline.
- **.NET 10 runtime.** `setup-dotnet` installed it next to the SDK; here `scripts/dotnet-runtime.sh` does in the test step. A custom
  image with the runtime baked in (see below) removes the download.
- **Test results, coverage and packages** are not kept: Woodpecker has no artifact storage, and files only live in the workspace of
  one workflow. See [Keeping files](#keeping-files-and-getting-them-into-a-build).
- **NuGet cache.** Nothing is cached between pipelines. Every Docker step is a new container and only the workspace (the cloned
  repository) is kept between the steps of a workflow, so packages restored into `~/.nuget/packages` by the `restore` step were gone in
  the `build` step (NETSDK1064, and MSB3644 for the net48 reference assemblies, which are a NuGet package too). Every step that runs
  dotnet therefore sets `NUGET_PACKAGES=$PWD/.nuget-packages`, inside the workspace. For a cache across pipelines, a cache plugin such as `meltwater/drone-cache` with S3/MinIO works.

## Not migrated, and different

- **Windows** (net48 tests, `CB.Win32Tests`): these need a Windows machine and Woodpecker has none, so `ci.yml` keeps its Windows job;
  its Linux and pack jobs are commented out because Woodpecker does them. The Linux workflow only *compiles* the tests for net48.
  A Windows workflow for Woodpecker (local backend on your own Windows agent) existed in commit `e4beb0a` as
  `.woodpecker/build-windows.yaml`, together with `detect-changes.ps1` and `test.ps1`, if you get an agent later.
- **CodeQL** (`codeql.yml`) is a GitHub feature (code scanning results, security tab) and has no Woodpecker counterpart. Keep it:
  disable the `Release` workflow on its own, not GitHub Actions as a whole. Without CodeQL, SonarQube/SonarCloud, or
  Semgrep in a Woodpecker step, are the usual replacements.
- **Dependabot.** `nuget` updates keep working (it is a GitHub service). The `github-actions` entry updates only workflows on GitHub:
  remove it once those are deleted. Woodpecker's images and plugin tags are updated by Renovate (it has a Woodpecker manager).
- **PR template.** Its *CI* section asks for the state of the required checks; read them from the `ci/woodpecker/...` statuses.

## Keeping files, and getting them into a build

Steps of one workflow share the workspace; nothing else is shared, and nothing outlives the pipeline. Everything that must live
longer, or reach another workflow or another repository, goes through storage you name.

**Keep test results, coverage and packages** (what `upload-artifact` did): upload them to S3/MinIO as the last step of a workflow.
`status: [success, failure]` is the `if: always()`:

```yaml
  - name: keep-test-results
    image: woodpeckerci/plugin-s3
    when:
      - status: [success, failure]
    settings:
      endpoint: https://minio.example.com
      path_style: true
      bucket: ci-artifacts
      source: TestResults/**/*
      target: /cb-libs/${CI_PIPELINE_NUMBER}/test-results-linux
      access_key:
        from_secret: s3_access_key
      secret_key:
        from_secret: s3_secret_key
```

The packages are better published than kept: a NuGet feed that every consumer (IVI-Framework, ...) already uses. Forgejo/Gitea and
GitHub Packages have one; the push is `dotnet nuget push artifacts/*.nupkg --source <feed> --api-key $KEY`.

### A vendor SDK such as the Zivid SDK

Not in this repository; if IVI-Framework or another project needs it, three ways work, in this order of preference:

1. **A CI image with the SDK in it** (Linux). The SDK is installed once, every run only pulls the image. Zivid ships `.deb` packages
   per Ubuntu release (take the URLs of the version you pin from [zivid.com/downloads](https://www.zivid.com/download-software) or
   keep the files in your own store; never commit them to git):

   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:11.0
   # the .NET 10 runtime for the net10.0 tests: dotnet-runtime.sh is then not needed
   COPY --from=mcr.microsoft.com/dotnet/runtime:10.0 /usr/share/dotnet/shared/Microsoft.NETCore.App /usr/share/dotnet/shared/Microsoft.NETCore.App
   ARG ZIVID_DEB_URLS
   RUN cd /tmp && for url in $ZIVID_DEB_URLS; do curl -fsSLO "$url"; done \
    && apt-get update && apt-get install -y ./*.deb && rm -rf ./*.deb /var/lib/apt/lists/*
   ```

   Build it in a workflow of its own with `woodpeckerci/plugin-docker-buildx` (`repo`, `registry`, `tags`, `dockerfile`, `build_args`,
   `username`/`password` from secrets; the plugin needs the server to list it in `WOODPECKER_PLUGINS_PRIVILEGED`), push it to a
   **private** registry (the SDK has a licence: do not publish it), add the registry login under *Registries* in the repository
   settings, and use it as `image:` of the steps that need it. Rebuild when the SDK version changes.
2. **A store the build downloads from.** Put the installer in S3/MinIO, a Forgejo/Gitea generic package registry or Nexus,
   and fetch it in the step with a secret, checking the hash (a cache of the file in the image or on the agent saves the download):

   ```yaml
     - name: fetch-sdk
       image: curlimages/curl
       environment:
         STORE_TOKEN:
           from_secret: sdk_store_token
       commands:
         - curl -fsSL -H "Authorization: Bearer $STORE_TOKEN" -o zivid.deb https://store.example.com/zivid/zivid_2.x_amd64.deb
         - echo "<sha256>  zivid.deb" | sha256sum -c -
   ```

3. **On the agent** (Windows, or anything that needs the camera). The installer of the Windows SDK runs once on the agent machine;
   steps of the local backend use what is installed. Give that agent a label (`WOODPECKER_AGENT_LABELS=zivid=true`) and ask for it
   in the workflow (`labels: { zivid: 'true' }`), so only workflows that need the SDK (or the camera, for hardware tests) land there.
   Keep the version in a file of the repository and check it in the first step, so the agent cannot drift unnoticed.

## Switching the old CI off

Run both for a while: until the Woodpecker statuses are green on a few pull requests, the required checks have been moved, and a
release rehearsal (a `v1.0.0-preview.N` tag, deployed to `nuget-org`) has worked. While both run, **GitHub's release.yml and
Woodpecker's release must not both be used for one tag** (`--skip-duplicate` makes a second push harmless, the GitHub release of an
existing tag is reused, but there is no reason to take the risk). Then:

1. **Required checks.** Settings -> Branches -> rule of `master`: remove `Build & Test (Linux)` and `Pack the packages`, add the
   `ci/woodpecker/pr/...` ones; keep `Build & Test (Windows)`. Move the `NUGET_API_KEY` secret (environment `nuget-org`) to Woodpecker.
2. **GitHub Actions**, per workflow, so the Windows job (in `CI`) and CodeQL stay: *Actions* tab -> workflow `Release` -> `...` ->
   *Disable workflow*, or `gh workflow disable release.yml`. A disabled workflow keeps its history and can be enabled again.
   Do **not** disable `CI` (its Windows job is the only Windows coverage) or switch Actions off in Settings -> Actions -> General.
3. **Azure Pipelines**, which the Windows job of `ci.yml` has made redundant. Azure DevOps -> Pipelines -> the pipeline -> `...` ->
   *Settings* -> *Processing of new run requests*: *Disabled* (or *Paused*); remove a *Build validation* branch policy that points at
   it (Repos -> Branches -> Branch policies), and, if the pipeline watches GitHub, uninstall the *Azure Pipelines* GitHub App from
   the repository (GitHub -> Settings -> Integrations). Without touching Azure DevOps, `trigger: none` and `pr: none` in
   `azure-pipelines.yml` stop it as well.
4. **Later, once nothing needs them:** delete `.github/workflows/release.yml` and `azure-pipelines.yml`, delete the commented-out jobs of
   `ci.yml`, delete the GitHub environment `nuget-org`. The `github-actions` ecosystem of `.github/dependabot.yml` stays: `ci.yml`
   and `codeql.yml` remain.

## First run: what to look at

None of this has run on a Woodpecker server yet. Look at: the pull request shows the two `ci/woodpecker/pr/...` statuses; a
documents-only pull request ends green in each with "Only documents changed"; the `version` step of a preview tag prints the right version; the *Deploy* button offers a target and `publish` sees
`CI_PIPELINE_DEPLOY_TARGET`. If the repository's Woodpecker is older than 3.x, check `labels`, `depends_on` and `when` against its docs.
