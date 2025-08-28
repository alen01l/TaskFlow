# TaskFlow

A simple task manager API built with .NET 8 (ASP.NET Core).

## Run locally
1. Open in Visual Studio 2022
2. Press F5 → Swagger opens at https://localhost:xxxx/swagger

## Endpoints
- `GET /api/tasks` – list tasks
- `POST /api/tasks` – create task `{ "title": "…" }`

## Roadmap
- [ ] EF Core + SQLite (persist tasks)
- [ ] Auth (Identity)
- [ ] React frontend (Vite + TS)