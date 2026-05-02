$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

Push-Location $ProjectRoot
try {
    dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj --no-restore
}
finally {
    Pop-Location
}
