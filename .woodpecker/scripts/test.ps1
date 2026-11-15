# Windows twin of test.sh: every *Tests project, one at a time, with TRX and cobertura reports in .\TestResults.
# Run from the repository root (global.json carries the Microsoft.Testing.Platform opt-in).
$ErrorActionPreference = 'Continue'

$results = Join-Path (Get-Location) 'TestResults'
$status = 0
foreach ($dir in (Get-ChildItem -Directory -Filter '*Tests' | Sort-Object Name)) {
  $name = $dir.Name
  Write-Host "=== $name"
  dotnet test --project "$name/$name.csproj" --configuration Release --no-build -- --results-directory "$results/$name" --report-trx --coverage --coverage-output-format cobertura
  if ($LASTEXITCODE -ne 0) { Write-Host "Tests failed in $name"; $status = 1 }
}
exit $status
