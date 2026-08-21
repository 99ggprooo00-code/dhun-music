# Builds and smoke-tests the unpackaged DHUN x64 executable.
# Requires Windows, .NET 10 and Windows App SDK build tooling.

$ErrorActionPreference = 'Stop'
$project = 'src\Dhun.WinUI\Dhun.WinUI.csproj'

Write-Host '[verify] Publishing DHUN x64 foundation...' -ForegroundColor Cyan
dotnet publish $project `
  -c Release -r win-x64 `
  -p:Platform=x64 `
  -p:PublishReadyToRun=true `
  -p:GenerateAppxPackageOnBuild=false `
  -p:AppxPackageSigningEnabled=False `
  -p:PackageCertificateThumbprint='' `
  -p:GenerateTemporaryStoreCertificate=False
if ($LASTEXITCODE -ne 0) { throw 'DHUN publish failed.' }

$exe = Get-ChildItem 'src\Dhun.WinUI\bin\Release' -Filter 'DHUN.exe' -Recurse |
  Where-Object { $_.FullName -match '[\\/]publish[\\/]DHUN\.exe$' } |
  Select-Object -First 1
if (-not $exe) { throw 'Published DHUN.exe was not found.' }

Write-Host "[verify] Launching $($exe.FullName)" -ForegroundColor Cyan
$process = Start-Process -FilePath $exe.FullName -PassThru
Start-Sleep -Seconds 20

if ($process.HasExited) {
  throw "DHUN exited during startup smoke test with code $($process.ExitCode)."
}

Stop-Process -Id $process.Id -Force
Write-Host 'DHUN_STARTUP_SMOKE=PASS' -ForegroundColor Green
