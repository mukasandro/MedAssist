# MedAssist

MVP REST API для врачебных сценариев (регистрация, пациенты, диалоги, сообщения, согласия) по принципам Clean Architecture, готов к контейнеризации.

## Стек и слои
- .NET 8, ASP.NET Core Controllers, Swagger/OpenAPI.
- Clean Architecture: `Api` (транспорт), `Application` (контракты), `Domain` (сущности), `Infrastructure` (in-memory реализация, DI).
- Хранение пока in-memory; в `docker-compose.yml` уже заведены Postgres/Redis для будущей БД/кеша.
- Наблюдаемость: health `GET /v1/health`, единый стиль ответов, нижний регистр роутов.

Структура решения:
- `src/MedAssist.Api` — REST контроллеры, composition root.
- `src/MedAssist.Application` — DTO, запросы, интерфейсы сервисов.
- `src/MedAssist.Domain` — сущности и enum (Doctor, Registration, Patient, Dialog, Message, Consent).
- `src/MedAssist.Infrastructure` — in-memory сервисы, модуль DI, заглушка текущего пользователя.

## Ключевые эндпоинты (v1)
- Регистрация: `POST /v1/registration` (имя, специализация, human-in-loop сразу), `GET /v1/registration`.
- Профиль: `GET /v1/me`, `PATCH /v1/me`.
- Пациенты: `GET /v1/patients`, `POST /v1/patients`, `GET /v1/patients/{id}`, `DELETE /v1/patients/{id}`, `POST /v1/patients/{id}/select`.
- Диалоги: `POST /v1/dialogs` (опционально `patientId`), `GET /v1/dialogs`, `GET /v1/dialogs/{id}`, `POST /v1/dialogs/{id}/close`.
- Сообщения: `GET /v1/dialogs/{dialogId}/messages`, `POST /v1/dialogs/{dialogId}/messages`.
- Справочники: `GET /v1/reference/specializations`.

## Локальный запуск
```bash
dotnet restore
dotnet run --project src/MedAssist.Api
# Swagger UI: http://localhost:5142/swagger (порт из лога запуска)
```

## Запуск в контейнерах
```bash
docker compose build
docker compose up
# API: http://localhost:8080/swagger
```

## Заметки и дальнейшие шаги
- Аутентификация заглушена: `StubCurrentUserContext` фиксирует одного врача; позже заменить на реальную авторизацию.
- Персистентность пока in-memory; подменить `InMemoryDataStore` на Postgres/Redis-репозитории, добавить миграции.
- Идемпотентность, валидацию и rate limit можно включить через middleware/Polly, когда определитесь с требованиями.
