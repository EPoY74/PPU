# PPU E2E tests

This test setup is intended to be reproducible on another developer machine after cloning the repository.

## What runs

The E2E environment uses Docker Compose for runtime dependencies and runs tests on the host machine:

```text
host dotnet test
  -> http://localhost:5055
  -> ppu-api container
  -> ppu-simulator:1502
  -> Python Modbus TCP simulator
```

## Prerequisites

- Docker Desktop is installed and running.
- .NET SDK 10 is installed.
- First run requires internet access for Docker images and NuGet packages.

The simulator image pins `pymodbus==3.11.3` because newer `pymodbus` versions changed the datastore API used by the local simulator.

## One-command run

From the `Ppu` directory:

```powershell
.\scripts\e2e-run.ps1
```

This script:

1. Builds and starts `ppu-simulator` and `ppu-api`.
2. Runs `Ppu.Tests.E2E`.
3. Stops and removes the compose environment.

## Manual run

From the `Ppu` directory:

```powershell
.\scripts\e2e-up.ps1
.\scripts\e2e-test.ps1
.\scripts\e2e-down.ps1
```

The manual mode is useful when debugging tests in Rider:

1. Run `.\scripts\e2e-up.ps1`.
2. Run or debug E2E tests from Rider.
3. Run `.\scripts\e2e-down.ps1`.

## Current coverage

The current E2E tests verify:

- `/health` returns API health information.
- `/` returns service metadata and endpoint links.
- `/openapi/v1.json` is published.
- `/last-read` eventually returns FC04 input registers from the Dockerized PLC simulator.

## Important files

- `docker-compose.e2e.yml`: E2E compose environment.
- `docker/ppu.Dockerfile`: PPU API container image.
- `docker/simulator.Dockerfile`: Python PLC simulator container image.
- `docker/e2e/simulator-static.json`: fixed simulator registers used by tests.
- `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs`: E2E test cases.
- `scripts/e2e-run.ps1`: one-command local E2E run.

## Expected result

Successful output should include:

```text
Passed: 4
Failed: 0
Total: 4
```

## Troubleshooting

If tests fail, inspect logs:

```powershell
docker compose -f docker-compose.e2e.yml logs ppu-api
docker compose -f docker-compose.e2e.yml logs ppu-simulator
```

If Docker images or NuGet packages changed after a merge, rebuild:

```powershell
docker compose -f docker-compose.e2e.yml down -v
docker compose -f docker-compose.e2e.yml up -d --build
dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj
docker compose -f docker-compose.e2e.yml down -v
```
