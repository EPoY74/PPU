# PPU E2E tests

These tests run on the host machine and expect Docker Compose to provide the external services:

- `ppu-simulator`: Python Modbus TCP simulator.
- `ppu-api`: ASP.NET Core PPU application configured to poll the simulator.

Start the E2E environment from the `Ppu` project directory:

```powershell
docker compose -f docker-compose.e2e.yml up -d --build
dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj
docker compose -f docker-compose.e2e.yml down -v
```

The default API URL is `http://localhost:5055`. Override it with `PPU_E2E_BASE_URL` when needed.
