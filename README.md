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
- Аутентификация: `POST /v1/auth/token` (`type=telegram_init_data` + `payload.initData` или `type=api_key` + пустой `payload`), для `api_key` ключ передаётся в `Authorization: ApiKey ...`, ответ — JWT Bearer.
- Регистрация: `POST /v1/registration` (никнейм, специализация), `DELETE /v1/registration` (удаляет врача и связанные данные; нужен JWT, `X-Telegram-User-Id` можно передать для совместимости).
- Профиль: `GET /v1/me`, `PATCH /v1/me`, `PUT /v1/me/active-patient`, `DELETE /v1/me/active-patient` (нужен JWT; `X-Telegram-User-Id` остается как fallback).
- Пациенты: `GET /v1/patients`, `POST /v1/patients`, `GET /v1/patients/{id}`, `PATCH /v1/patients/{id}`, `DELETE /v1/patients/{id}`, `POST /v1/patients/{id}/setactive` (устаревший, нужен JWT).
- Статика: `GET /v1/static-content/{code}` (для бота), `GET|POST /v1/static-content`, `PUT|DELETE /v1/static-content/{id}` (админка).
- Диалоги: `POST /v1/dialogs` (опционально `patientId`), `GET /v1/dialogs`, `GET /v1/dialogs/{id}`, `POST /v1/dialogs/{id}/close`.
- Сообщения: `GET /v1/dialogs/{dialogId}/messages`, `POST /v1/dialogs/{dialogId}/messages`.
- Справочники: `GET /v1/reference/specializations`.

## Локальный запуск
```bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=medassist;Username=medassist;Password=medassist"
export Auth__Jwt__SigningKey="replace-with-strong-32+-char-key"
export Auth__Telegram__BotToken="telegram_bot_token"
export Auth__Service__ApiKey="replace-with-service-api-key"
export LlmGateway__DeepSeek__ApiKey="your_deepseek_api_key"
dotnet restore
dotnet run --project src/MedAssist.Api
# Swagger UI: http://localhost:5142/swagger (выберите Admin или Bot в списке)
```

## Запуск в контейнерах
```bash
cp .env.example .env
# укажи реальный ключ:
# DEEPSEEK_API_KEY=...
# AUTH_SERVICE_API_KEY=...
# AUTH_TELEGRAM_BOT_TOKEN=...
docker compose build
docker compose up
# API: http://localhost:8080/swagger (выберите Admin или Bot в списке)
# Frontend (nginx): http://localhost:4173
```

## Заметки и дальнейшие шаги
- Для bot API добавлен JWT-вход (`/v1/auth/token`), но часть legacy-логики с `X-Telegram-User-Id` оставлена для совместимости.
- Персистентность: Postgres через EF Core (EnsureCreated). Подключение: `ConnectionStrings__Default`.
- Идемпотентность, валидацию и rate limit можно включить через middleware/Polly, когда определитесь с требованиями.
