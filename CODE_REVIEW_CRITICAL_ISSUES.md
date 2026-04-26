# Code Review: Critical Issues

Дата ревью: 2026-04-26

Область проверки: backend-проект `Ppu`, конфигурация, polling worker, PLC reader, HTTP API, EF Core слой, структура solution.

Код не изменялся.

## 1. Конфиг PLC частично не применяется

Источник:

- [`Ppu/appsettings.json`](Ppu/appsettings.json#L10)
- [`Ppu/Config/PlcReaderOptions.cs`](Ppu/Config/PlcReaderOptions.cs#L11)
- [`Ppu/Config/PlcReaderOptions.cs`](Ppu/Config/PlcReaderOptions.cs#L13)

В `appsettings.json` указано `StartAdress`, а класс настроек ожидает `StartAddress`. Также в JSON задано `PollIntervalSeconds`, а в коде свойство называется `PollIntervalSecond`.

Из-за этого значения будут молча заменены дефолтами из `PlcReaderOptions`.

Почему критично:

- сервис может опрашивать не тот диапазон регистров;
- фактический polling interval может отличаться от ожидаемого;
- проблема не ловится компилятором и видна только в runtime;
- для промышленного оборудования чтение не того адреса может привести к неверной диагностике или неверным данным в интеграциях.

## 2. PLC-чтение выглядит async, но реально блокирующее и почти не отменяемое

Источник:

- [`Ppu/Services/PlcReaderService.cs`](Ppu/Services/PlcReaderService.cs#L28)
- [`Ppu/Services/PlcReaderService.cs`](Ppu/Services/PlcReaderService.cs#L36)
- [`Ppu/Services/PlcReaderService.cs`](Ppu/Services/PlcReaderService.cs#L41)
- [`Ppu/Services/PlcReaderService.cs`](Ppu/Services/PlcReaderService.cs#L47)
- [`Ppu/Services/PlcReaderService.cs`](Ppu/Services/PlcReaderService.cs#L60)

`RawReadAsync` принимает `CancellationToken`, но `Connect`, `ReadHoldingRegisters` и `ReadInputRegisters` вызываются синхронно и без token. `await Task.Yield()` не делает Modbus I/O асинхронным и не добавляет управляемую отмену.

Почему критично:

- при сетевых проблемах или зависшем PLC worker может надолго зависнуть на подключении или чтении;
- graceful shutdown может не сработать быстро;
- `/last-read` может продолжать отдавать устаревшие данные;
- сервис может выглядеть живым по HTTP, но фактически перестать выполнять основную функцию.

## 3. `/health` всегда возвращает `ok`, не проверяя PLC, worker и БД

Источник:

- [`Ppu/Program.cs`](Ppu/Program.cs#L58)
- [`Ppu/Program.cs`](Ppu/Program.cs#L60)
- [`Ppu/Program.cs`](Ppu/Program.cs#L65)

Health endpoint возвращает успешный статус без проверки последнего polling result, доступности PLC, состояния worker или БД.

Почему критично:

- мониторинг будет считать сервис здоровым даже при недоступном PLC;
- orchestration/deployment-системы не смогут отличить живой HTTP-процесс от реально работающего polling-сервиса;
- инциденты с потерей данных могут обнаруживаться поздно.

## 4. Слой БД зарегистрирован, но история чтений фактически не работает

Источник:

- [`Ppu/Program.cs`](Ppu/Program.cs#L26)
- [`Ppu/appsettings.json`](Ppu/appsettings.json#L2)
- [`Ppu/Services/PollingWorkers.cs`](Ppu/Services/PollingWorkers.cs#L33)
- [`Ppu/Data/PpuDbContext.cs`](Ppu/Data/PpuDbContext.cs#L13)
- [`Ppu/Data/Entities/RawReadHistoryEntry.cs`](Ppu/Data/Entities/RawReadHistoryEntry.cs#L5)

В `Program.cs` используется `builder.Configuration.GetConnectionString("PpuDb")`, но в конфиге секция названа `connectionString`, а стандартный ключ должен быть `ConnectionStrings`.

Кроме того, `PollingWorker` сохраняет результат только в `LastReadStore`, а не в `RawReadHistory`. Миграций или инициализации схемы БД в проекте не видно.

Почему критично:

- заявленная история/логирование чтений не сохраняется надежно;
- после рестарта сервиса последнее значение теряется;
- будущая интеграция, ожидающая исторические данные, получит пустую или неинициализированную БД;
- ошибка конфигурации строки подключения может проявиться только при первом реальном обращении к DbContext.

## 5. API и OpenAPI открыты без аутентификации

Источник:

- [`Ppu/Program.cs`](Ppu/Program.cs#L35)
- [`Ppu/Program.cs`](Ppu/Program.cs#L73)
- [`Ppu/appsettings.json`](Ppu/appsettings.json#L20)

`/last-read` и `/openapi/v1.json` доступны без authentication/authorization. OpenAPI публикуется всегда. `AllowedHosts` установлен в `*`.

Почему критично:

- если сервис будет доступен не только с localhost, наружу могут уйти технологические значения регистров;
- ошибки подключения к промышленному оборудованию могут раскрывать внутреннюю инфраструктуру;
- открытая OpenAPI-схема упрощает разведку API;
- для промышленного/SCADA-контекста даже read-only данные часто являются чувствительными.

## 6. Нет тестов и CI, поэтому критичные регрессии не ловятся

Источник:

- [`PPU.sln`](PPU.sln#L5)
- [`Ppu/Ppu.csproj`](Ppu/Ppu.csproj#L1)
- [`.github/workflows/`](.github/workflows/)

В solution подключен только проект `Ppu`. Тестовых проектов нет. Директория `.github/workflows` есть, но workflow-файлов в ней не найдено.

Почему критично:

- ошибки binding конфигурации не ловятся компилятором;
- polling failure, API contracts и health behavior не проверяются автоматически;
- регрессии будут обнаруживаться вручную или уже на оборудовании;
- для сервиса, который работает с PLC, отсутствие хотя бы базовых интеграционных и unit-тестов сильно повышает эксплуатационный риск.

## Проверки

Выполнено:

```powershell
dotnet build PPU.sln --no-restore
```

Результат: сборка успешна, предупреждений и ошибок нет.

Выполнено:

```powershell
dotnet test PPU.sln --no-restore --no-build
```

Результат: команда завершилась успешно, но тестовые проекты отсутствуют, поэтому фактически тесты не запускались.

Ограничение:

```powershell
git status --short
```

Не был доступен из sandbox-пользователя из-за `dubious ownership` для репозитория.
