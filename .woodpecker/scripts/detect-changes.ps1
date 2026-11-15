# Windows twin of detect-changes.sh; see there for why the workflows always run and skip by marker file.
$ErrorActionPreference = 'Stop'

$marker = '.ci-docs-only'
Remove-Item -Force -ErrorAction SilentlyContinue $marker

$files = @([regex]::Matches([string]$env:CI_PIPELINE_FILES, '"([^"]*)"') | ForEach-Object { $_.Groups[1].Value })
if ($files.Count -eq 0) {
  Write-Host 'No list of changed files: everything runs.'
  exit 0
}

$files | ForEach-Object { Write-Host $_ }
$code = @($files | Where-Object { $_ -notmatch '^docs/|\.md$|^images/' })
if ($code.Count -gt 0) {
  Write-Host 'Code changed: everything runs.'
} else {
  Write-Host 'Only documents changed: build, test and pack are skipped.'
  New-Item -ItemType File -Force $marker | Out-Null
}
