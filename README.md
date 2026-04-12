# PLC Polling Utility (PPU)

**PLC Polling Utility (PPU)** — это лёгкий, надёжный и расширяемый сервис для опроса промышленного оборудования по протоколу Modbus TCP с возможностью логирования и предоставления данных через HTTP API.

---

## 🇷🇺 Описание (RU)

PPU предназначен для инженеров, пусконаладчиков и интеграторов, которым требуется быстрый и простой инструмент для:

* опроса PLC (Programmable Logic Controller — программируемый логический контроллер)
* чтения регистров Modbus TCP (FC03, FC04)
* диагностики связи с оборудованием
* логирования данных
* интеграции с другими системами через REST API

### Основные возможности

* 🔌 Подключение к PLC по Modbus TCP
* 📊 Чтение регистров (Holding / Input Registers)
* 🔁 Периодический опрос (polling)
* 🧾 Логирование результатов
* 🌐 HTTP API:

  * `/health` — состояние сервиса
  * `/last-read` — последний результат опроса
* ⚙️ Конфигурация через `appsettings.json`
* 🧩 Простая архитектура для дальнейшего расширения (SCADA, IoT, telemetry)

---

## 🇬🇧 Description (EN)

PPU is a lightweight, reliable, and extensible service for polling industrial equipment via Modbus TCP protocol with logging and HTTP API access.

It is designed for engineers, commissioning specialists, and system integrators who need a fast and simple tool for:

* PLC (Programmable Logic Controller) polling
* Modbus TCP register reading (FC03, FC04)
* connection diagnostics
* data logging
* integration with external systems via REST API

### Key Features

* 🔌 Modbus TCP connectivity
* 📊 Register reading (Holding / Input Registers)
* 🔁 Periodic polling loop
* 🧾 Logging support
* 🌐 HTTP API:

  * `/health` — service status
  * `/last-read` — last polling result
* ⚙️ Configuration via `appsettings.json`
* 🧩 Clean architecture ready for scaling (SCADA, industrial IoT, telemetry gateways)

---

## 🚀 Быстрый старт / Quick Start

### Требования / Requirements

* .NET 8 (или выше)
* Доступ к PLC (Modbus TCP)

### Запуск / Run

```bash
dotnet run
```

После запуска:

* https://localhost:7237/health;
* https://localhost:7237/last-read;

или

* http://localhost:5085/health";
* http://localhost:5085/last-read";

---

## ⚙️ Конфигурация / Configuration

Файл: `appsettings.json`

```json
{
  "PlcReader": {
    "Host": "10.0.6.10", 
    "Port": 502,
    "UnitId": 1,
    "FunctionCode": 3,
    "StartAddress": 0,
    "RegisterCount": 2,
    "PollIntervalSeconds": 5
  }
}
```

---

## 🧱 Архитектура / Architecture

```text
Ppu
├── Config
├── Domain
├── Services
├── Program.cs
```

### Основные компоненты:

* `PollingWorker` — фоновый цикл опроса
* `PlcReaderService` — чтение данных из PLC
* `LastReadStore` — хранение последнего результата
* `Minimal API` — доступ к данным через HTTP

---

## 📦 Use Cases / Сценарии использования

* Пусконаладка оборудования
* Диагностика PLC
* Быстрый мониторинг параметров
* Интеграция с SCADA/BI системами
* Промышленные IoT решения
* Локальные telemetry gateway

---

## 💼 Коммерческое использование / Commercial Use

Проект может использоваться:

* в коммерческих проектах
* как часть промышленного ПО
* для разработки SCADA/IoT решений
* как база для собственных продуктов

Подробности см. в файле [LICENSE](LICENSE).

---

## 🛣️ Roadmap

* [ ] Поддержка нескольких PLC
* [ ] История данных (PostgreSQL / SQLite)
* [ ] Декодирование сигналов (float, bit)
* [ ] Web UI
* [ ] Alerting (Telegram, Email)
* [ ] Docker deployment

---

## 🤝 Контрибьюция / Contributing

PR и идеи приветствуются.

---

## 📄 Лицензия / License

Этот проект распространяется под лицензией MIT.
Подробности: [LICENSE](LICENSE)

---

## 🔑 Ключевые слова (SEO)

industrial automation, PLC monitoring, Modbus TCP, SCADA tools, telemetry gateway, .NET industrial software, equipment monitoring, IoT gateway, data acquisition, industrial diagnostics

---
