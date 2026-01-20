# MedAssist

MVP REST API для врачебных сценариев (регистрация, пациенты, диалоги, сообщения, согласия) по принципам Clean Architecture, готов к контейнеризации.

## Стек и слои
- .NET 8, ASP.NET Core Controllers, Swagger/OpenAPI.
- Валидация: FluentValidation.
- Логирование: Serilog (консоль).
- Data: EF Core (InMemory по умолчанию, Npgsql при наличии connection string), модуль DI.
- Clean Architecture: `Api` (транспорт), `Application` (контракты), `Domain` (сущности), `Infrastructure` (in-memory реализация, DI).
- Хранение пока in-memory; в `docker-compose.yml` уже заведены Postgres/Redis для будущей БД/кеша.
- Наблюдаемость: единый стиль ответов, нижний регистр роутов.

Структура решения:
- `src/MedAssist.Api` — REST контроллеры, composition root.
- `src/MedAssist.Application` — DTO, запросы, интерфейсы сервисов.
- `src/MedAssist.Domain` — сущности и enum (Doctor, Registration, Patient, Dialog, Message, Consent).
- `src/MedAssist.Infrastructure` — in-memory сервисы, модуль DI, заглушка текущего пользователя.

## Ключевые эндпоинты (v1)
- Регистрация: `PUT /v1/registration` (никнейм, специализация, confirmed), `DELETE /v1/registration` (через заголовок `X-Telegram-User-Id`).
- Профиль: `GET /v1/me`, `PATCH /v1/me` (через заголовок `X-Telegram-User-Id`).
- Пациенты: `GET /v1/patients`, `POST /v1/patients`, `GET /v1/patients/{id}`, `DELETE /v1/patients/{id}`, `POST /v1/patients/{id}/setactive` (через заголовок `X-Telegram-User-Id`).
- Статика: `GET /v1/static-content/{code}` (для бота), `GET|POST /v1/static-content`, `PUT|DELETE /v1/static-content/{id}` (админка).
- Диалоги: `POST /v1/dialogs` (опционально `patientId`), `GET /v1/dialogs`, `GET /v1/dialogs/{id}`, `POST /v1/dialogs/{id}/close`.
- Сообщения: `GET /v1/dialogs/{dialogId}/messages`, `POST /v1/dialogs/{dialogId}/messages`.
- Справочники: `GET /v1/reference/specializations`.

## Локальный запуск
```bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=medassist;Username=medassist;Password=medassist"
dotnet restore
dotnet run --project src/MedAssist.Api
# Swagger UI: http://localhost:5142/swagger (выберите Admin или Bot в списке)
```

## Запуск в контейнерах
```bash
docker compose build
docker compose up
# API: http://localhost:8080/swagger (выберите Admin или Bot в списке)
# Frontend (nginx): http://localhost:4173
```

## Заметки и дальнейшие шаги
- Аутентификация заглушена: `StubCurrentUserContext` фиксирует одного врача; позже заменить на реальную авторизацию.
- Персистентность: Postgres через EF Core (EnsureCreated). Подключение: `ConnectionStrings__Default`.
- Идемпотентность, валидацию и rate limit можно включить через middleware/Polly, когда определитесь с требованиями.
