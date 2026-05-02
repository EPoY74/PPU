$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

Push-Location $ProjectRoot
try {
    docker compose -f docker-compose.e2e.yml down -v
}
finally {
    Pop-Location
}
