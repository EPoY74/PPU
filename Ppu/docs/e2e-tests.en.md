# PPU E2E Tests: Docker Compose, Modbus TCP, ASP.NET Core

This document explains how to run, maintain, and extend the PPU end-to-end tests. It intentionally includes searchable engineering phrases such as `ASP.NET Core E2E tests`, `Docker Compose integration tests`, `Modbus TCP simulator`, `end-to-end testing`, `PLC polling integration test`, `xUnit E2E tests`, and `Rider Docker Compose tests`.

## What The E2E Tests Validate

The E2E tests validate the running system, not just isolated methods:

```text
host dotnet test
  -> http://localhost:5055
  -> ppu-api container
  -> PollingWorker
  -> PlcReaderService
  -> Modbus TCP
  -> ppu-simulator:1502
  -> SQLite history storage
```

The test cases are located in `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs`.

Current coverage:

- `/health` returns API health data.
- `/` returns service metadata and endpoint links.
- `/openapi/v1.json` exposes the OpenAPI document.
- `/last-read` eventually returns FC04 input registers from the Dockerized PLC simulator.
- `/history` returns persisted raw read records from SQLite.

## Dependencies

Local prerequisites:

- Docker Desktop.
- .NET SDK 10.
- PowerShell.
- Internet access on the first run for Docker images and NuGet packages.

Technology stack:

- `xUnit` for tests.
- `dotnet test` as the test runner.
- `docker compose` for `ppu-api` and `ppu-simulator`.
- Python `pymodbus==3.11.3` inside the simulator image.
- SQLite inside the `ppu-api` container.

Why `pymodbus==3.11.3` is pinned: newer `pymodbus` versions changed the datastore API used by the local simulator. The pin is defined in `docker/simulator.Dockerfile` to keep the `Modbus TCP simulator tests` reproducible.

## One-Command Run

Open a terminal in the `Ppu` directory:

```powershell
cd D:\repos-win\source\PPU\Ppu
.\scripts\e2e-run.ps1
```

The script performs:

```text
docker compose -f docker-compose.e2e.yml up -d --build
dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj --no-restore
docker compose -f docker-compose.e2e.yml down -v
```

Expected result:

```text
Passed: 5
Failed: 0
Total: 5
```

## Running From Rider

Recommended Rider workflow:

1. Start the environment:

```powershell
.\scripts\e2e-up.ps1
```

2. Open `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs` in Rider.
3. Run or debug an individual xUnit test.
4. Stop the environment:

```powershell
.\scripts\e2e-down.ps1
```

This is practical for `Rider E2E tests with Docker Compose` because containers keep running while tests can be executed and debugged like normal xUnit tests.

## Important Files

- `docker-compose.e2e.yml`: Docker Compose E2E environment.
- `docker/ppu.Dockerfile`: ASP.NET Core API container image.
- `docker/simulator.Dockerfile`: Python Modbus TCP simulator container image.
- `docker/e2e/simulator-static.json`: fixed PLC register values used by tests.
- `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs`: E2E test cases.
- `scripts/e2e-run.ps1`: one-command E2E run.
- `scripts/e2e-up.ps1`: start the test environment.
- `scripts/e2e-test.ps1`: run only the tests.
- `scripts/e2e-down.ps1`: stop containers and remove volumes.

## Docker Compose Architecture

Compose starts two services:

```text
ppu-simulator
  Python Modbus TCP simulator
  internal address: ppu-simulator:1502

ppu-api
  ASP.NET Core API
  reads PLC data from ppu-simulator:1502
  publishes API to host localhost:5055
```

Important rule: do not use `localhost` for container-to-container communication. `ppu-api` must use the Compose service name:

```text
PlcReader__Host=ppu-simulator
PlcReader__Port=1502
```

Host-based tests call the API through:

```text
http://localhost:5055
```

## How To Add Tests When Polling Functionality Changes

When `PollingWorker`, `PlcReaderService`, Modbus function code, register mapping, timeout handling, or `/last-read` response format changes, add an E2E test using this checklist.

1. Define the external contract.

Example: FC03 holding registers support is added. Contract:

```text
API /last-read returns functionCode=3
registers=[201,202,203,204]
```

2. Configure the simulator.

`docker/e2e/simulator-static.json` already contains input and holding registers:

```json
{
  "input_registers": [
    { "address": 0, "values": [101, 102, 103, 104] }
  ],
  "holding_registers": [
    { "address": 0, "values": [201, 202, 203, 204] }
  ]
}
```

3. Configure Compose.

For a new scenario, add a compose override such as:

```text
docker-compose.e2e.fc03.yml
```

Example override:

```yaml
services:
  ppu-api:
    environment:
      PlcReader__FunctionCode: 3
      PlcReader__RegisterCount: 4
```

4. Add a clearly named test.

Bad name:

```csharp
public async Task Test1()
```

Good name:

```csharp
[Fact(DisplayName = "E2E: /last-read returns FC03 holding registers from PLC simulator")]
public async Task LastRead_ReturnsHoldingRegisters_FromDockerizedSimulator()
```

5. Verify that the test fails when the behavior is wrong.

Before the final commit, temporarily change expected registers or function code and make sure the test becomes red.

## Example: Adding A Test For A New Endpoint

If a `/metrics` endpoint is added:

1. Add the endpoint in `Program.cs`.
2. Add a link to `EndpointLinksDto` if it should be discoverable from `/`.
3. Add an E2E test:

```csharp
[Fact(DisplayName = "E2E: /metrics returns polling metrics")]
public async Task Metrics_ReturnsPollingMetrics()
{
    using var httpClient = CreateHttpClient();
    await WaitForApiAsync(httpClient);

    using var response = await httpClient.GetAsync("/metrics");

    response.EnsureSuccessStatusCode();
}
```

4. Add `/metrics` to the OpenAPI assertion if the endpoint should be published.
5. Update this documentation if the workflow changes.

## How To Verify That Tests Are Correct

Seeing `Passed: 5` is not enough. You should prove that the tests catch a real defect.

Negative check:

1. In `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs`, temporarily replace:

```csharp
private static readonly ushort[] ExpectedRegisters = [101, 102, 103, 104];
```

with:

```csharp
private static readonly ushort[] ExpectedRegisters = [999, 102, 103, 104];
```

2. Run:

```powershell
.\scripts\e2e-run.ps1
```

3. Expected result: tests must fail.

The `e2e-run.ps1` script must also finish with a non-zero exit code. This matters for `CI E2E tests`, Rider run configurations, and any automation: a red `dotnet test` result must not be hidden behind a successful PowerShell script execution.

4. Restore the correct values:

```csharp
private static readonly ushort[] ExpectedRegisters = [101, 102, 103, 104];
```

5. Run again:

```powershell
.\scripts\e2e-run.ps1
```

6. Expected result:

```text
Passed: 5
Failed: 0
Total: 5
```

This is a negative check, test validation, or test sensitivity check. It is useful for `end-to-end tests`, `integration tests`, and any `Docker Compose test environment`.

## Troubleshooting

If tests do not start:

```powershell
docker compose -f docker-compose.e2e.yml ps
docker compose -f docker-compose.e2e.yml logs ppu-api
docker compose -f docker-compose.e2e.yml logs ppu-simulator
```

If the port is already used:

```text
localhost:5055 is already in use
```

Stop the previous environment:

```powershell
.\scripts\e2e-down.ps1
```

If Dockerfiles, dependencies, or appsettings changed after a merge:

```powershell
docker compose -f docker-compose.e2e.yml down -v
docker compose -f docker-compose.e2e.yml up -d --build
dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj
docker compose -f docker-compose.e2e.yml down -v
```

## What Not To Do

- Do not delete assertions just because a test failed.
- Do not increase timeouts without reading logs.
- Do not test implementation details when an API contract can be tested.
- Do not mix unit tests and E2E tests in one project without a clear reason.
- Do not change `simulator-static.json` without updating expected values in tests.
