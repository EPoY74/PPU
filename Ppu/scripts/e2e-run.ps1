$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

Push-Location $ProjectRoot
try {
    docker compose -f docker-compose.e2e.yml up -d --build
    dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj --no-restore
}
finally {
    docker compose -f docker-compose.e2e.yml down -v
    Pop-Location
}
