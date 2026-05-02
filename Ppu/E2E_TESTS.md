# PPU E2E Test Documentation

This file is the entry point for the PPU end-to-end test documentation.

- Russian: [docs/e2e-tests.ru.md](docs/e2e-tests.ru.md)
- English: [docs/e2e-tests.en.md](docs/e2e-tests.en.md)

Quick local run from the `Ppu` directory:

```powershell
.\scripts\e2e-run.ps1
```

Expected result:

```text
Passed: 5
Failed: 0
Total: 5
```

The E2E suite validates the Docker Compose based Modbus TCP integration path:

```text
host dotnet test
  -> http://localhost:5055
  -> ppu-api container
  -> ppu-simulator:1502
  -> Python Modbus TCP PLC simulator
```
