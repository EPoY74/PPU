$ErrorActionPreference = "Stop"

$ProjectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

function Invoke-CheckedNativeCommand {
    param(
        [string] $FilePath,
        [string[]] $Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $LASTEXITCODE`: $FilePath $($Arguments -join ' ')"
    }
}

Push-Location $ProjectRoot
try {
    Invoke-CheckedNativeCommand "dotnet" @("test", ".\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj", "--no-restore")
}
finally {
    Pop-Location
}
