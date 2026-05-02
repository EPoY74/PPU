# E2E тесты PPU: Docker Compose, Modbus TCP, ASP.NET Core

Этот документ описывает, как запускать, поддерживать и расширять E2E тесты проекта PPU. Текст намеренно использует практичные формулировки, которые легко искать: `ASP.NET Core E2E tests`, `Docker Compose integration tests`, `Modbus TCP simulator`, `end-to-end testing`, `PLC polling integration test`, `xUnit E2E tests`, `Rider Docker Compose tests`.

## Что проверяют E2E тесты

E2E тесты проверяют не отдельный метод, а рабочую цепочку приложения:

```text
dotnet test на хосте
  -> http://localhost:5055
  -> ppu-api container
  -> PollingWorker
  -> PlcReaderService
  -> Modbus TCP
  -> ppu-simulator:1502
  -> SQLite history storage
```

Сценарии находятся в `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs`.

Текущее покрытие:

- `/health` возвращает статус API.
- `/` возвращает metadata и ссылки на endpoints.
- `/openapi/v1.json` публикует OpenAPI документ.
- `/last-read` возвращает успешное чтение FC04 input registers из Dockerized PLC simulator.
- `/history` возвращает сохранённые записи чтения из SQLite.

## Зависимости

Для локального запуска нужны:

- Docker Desktop.
- .NET SDK 10.
- PowerShell.
- Интернет при первом запуске для загрузки Docker images и NuGet packages.

Используемые технологии:

- `xUnit` для тестов.
- `dotnet test` как test runner.
- `docker compose` для запуска `ppu-api` и `ppu-simulator`.
- Python `pymodbus==3.11.3` внутри simulator image.
- SQLite внутри контейнера `ppu-api`.

Почему зафиксирован `pymodbus==3.11.3`: более новые версии изменили datastore API, а текущий локальный simulator использует старый API. Это зафиксировано в `docker/simulator.Dockerfile`, чтобы `Modbus TCP simulator tests` были воспроизводимыми.

## Как запустить одной командой

Открой терминал в папке `Ppu`:

```powershell
cd D:\repos-win\source\PPU\Ppu
.\scripts\e2e-run.ps1
```

Скрипт делает три действия:

```text
docker compose -f docker-compose.e2e.yml up -d --build
dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj --no-restore
docker compose -f docker-compose.e2e.yml down -v
```

Ожидаемый результат:

```text
Passed: 5
Failed: 0
Total: 5
```

## Как запускать из Rider

Самый простой workflow для Rider:

1. Запусти окружение:

```powershell
.\scripts\e2e-up.ps1
```

2. В Rider открой `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs`.
3. Запусти или отладь нужный тест зелёной кнопкой.
4. После работы останови окружение:

```powershell
.\scripts\e2e-down.ps1
```

Такой подход удобен для `Rider E2E tests with Docker Compose`, потому что контейнеры живут отдельно, а тесты можно запускать и debug-ить как обычные xUnit tests.

## Файлы, которые нужно знать

- `docker-compose.e2e.yml`: описывает `ppu-api` и `ppu-simulator`.
- `docker/ppu.Dockerfile`: собирает ASP.NET Core API container.
- `docker/simulator.Dockerfile`: собирает Python Modbus TCP simulator container.
- `docker/e2e/simulator-static.json`: фиксированные значения PLC registers.
- `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs`: E2E test cases.
- `scripts/e2e-run.ps1`: полный запуск одной командой.
- `scripts/e2e-up.ps1`: поднять окружение без запуска тестов.
- `scripts/e2e-test.ps1`: запустить только тесты.
- `scripts/e2e-down.ps1`: остановить окружение и удалить volume.

## Как устроен Docker Compose

Compose запускает два сервиса:

```text
ppu-simulator
  Python Modbus TCP simulator
  internal address: ppu-simulator:1502

ppu-api
  ASP.NET Core API
  reads PLC from ppu-simulator:1502
  publishes API to host localhost:5055
```

Важное правило: внутри Docker Compose нельзя использовать `localhost` для связи контейнеров. `ppu-api` должен ходить в simulator по имени сервиса:

```text
PlcReader__Host=ppu-simulator
PlcReader__Port=1502
```

Хостовые тесты ходят в API через:

```text
http://localhost:5055
```

## Как добавлять тест при новом функционале в опросчике

Если меняется `PollingWorker`, `PlcReaderService`, Modbus function code, register mapping, timeout handling или формат `/last-read`, добавляй E2E тест по этому чеклисту.

1. Определи внешний контракт.

Пример: добавили поддержку FC03 holding registers. Контракт:

```text
API /last-read returns functionCode=3
registers=[201,202,203,204]
```

2. Настрой simulator.

Файл `docker/e2e/simulator-static.json` уже содержит input и holding registers:

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

3. Настрой compose.

Для нового сценария можно добавить отдельный compose override, например:

```text
docker-compose.e2e.fc03.yml
```

В нём можно переопределить:

```yaml
services:
  ppu-api:
    environment:
      PlcReader__FunctionCode: 3
      PlcReader__RegisterCount: 4
```

4. Добавь отдельный тест с понятным именем.

Плохое имя:

```csharp
public async Task Test1()
```

Хорошее имя:

```csharp
[Fact(DisplayName = "E2E: /last-read returns FC03 holding registers from PLC simulator")]
public async Task LastRead_ReturnsHoldingRegisters_FromDockerizedSimulator()
```

5. Проверь, что тест падает при неправильном поведении.

Перед финальным коммитом временно измени expected registers или function code и убедись, что тест краснеет.

## Пример добавления проверки нового endpoint

Если добавлен endpoint `/metrics`, порядок такой:

1. Добавь endpoint в `Program.cs`.
2. Добавь ссылку в `EndpointLinksDto`, если endpoint должен быть виден из `/`.
3. Добавь E2E тест:

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

4. Добавь `/metrics` в OpenAPI assertion, если endpoint должен быть опубликован.
5. Обнови этот документ, если меняется workflow.

## Как проверить, что E2E тесты корректные

Недостаточно увидеть `Passed: 5`. Нужно доказать, что тесты ловят дефект.

Контрольная проверка:

1. В `tests/Ppu.Tests.E2E/PpuApiE2eTests.cs` временно замени:

```csharp
private static readonly ushort[] ExpectedRegisters = [101, 102, 103, 104];
```

на:

```csharp
private static readonly ushort[] ExpectedRegisters = [999, 102, 103, 104];
```

2. Запусти:

```powershell
.\scripts\e2e-run.ps1
```

3. Ожидаемый результат: тесты должны упасть.

Скрипт `e2e-run.ps1` тоже должен завершиться с ненулевым кодом выхода. Это важно для `CI E2E tests`, Rider run configuration и любой автоматизации: красный `dotnet test` не должен маскироваться успешным PowerShell запуском.

4. Верни правильные значения:

```csharp
private static readonly ushort[] ExpectedRegisters = [101, 102, 103, 104];
```

5. Запусти снова:

```powershell
.\scripts\e2e-run.ps1
```

6. Ожидаемый результат:

```text
Passed: 5
Failed: 0
Total: 5
```

Этот приём называется negative check, test validation или проверка чувствительности теста. Он полезен для любых `end-to-end tests`, `integration tests` и `Docker Compose test environment`.

## Troubleshooting

Если тесты не стартуют:

```powershell
docker compose -f docker-compose.e2e.yml ps
docker compose -f docker-compose.e2e.yml logs ppu-api
docker compose -f docker-compose.e2e.yml logs ppu-simulator
```

Если порт занят:

```text
localhost:5055 is already in use
```

Останови старое окружение:

```powershell
.\scripts\e2e-down.ps1
```

Если после merge поменялись Dockerfile, зависимости или appsettings:

```powershell
docker compose -f docker-compose.e2e.yml down -v
docker compose -f docker-compose.e2e.yml up -d --build
dotnet test .\tests\Ppu.Tests.E2E\Ppu.Tests.E2E.csproj
docker compose -f docker-compose.e2e.yml down -v
```

## Что не стоит делать

- Не удаляй assertion только потому, что тест упал.
- Не увеличивай timeout без диагностики логов.
- Не проверяй implementation details, если можно проверить внешний API contract.
- Не смешивай unit tests и E2E tests в одном проекте без причины.
- Не меняй `simulator-static.json` без обновления expected values в тестах.
