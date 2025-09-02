# TaskFlow

A simple task manager API built with **.NET 8 (ASP.NET Core)** and **EF Core (SQLite)**.

## Run locally
1. Open in Visual Studio 2022  
2. Press F5 → Swagger opens at https://localhost:xxxx/swagger  

The API uses a local SQLite file (`taskflow.db`) with a connection string defined in `appsettings.json` under **ConnectionStrings:TaskFlowDb**.

## Demo User
A demo account is seeded automatically for testing:

- Email: `demo@taskflow.local`  
- Password: `Pass123$`  

Use this to log in via `POST /api/auth/login` before calling task endpoints.

## Endpoints

### Auth
- `POST /api/auth/register` – create a new user  
- `POST /api/auth/login` – log in (sets auth cookie)  
- `POST /api/auth/logout` – log out  
- `GET /api/auth/me` – get current logged-in user  

### Tasks (require authentication)
- `GET /api/tasks` – list all tasks (persisted in SQLite, sorted newest first in memory)  
- `GET /api/tasks/{id}` – get one task by ID  
- `POST /api/tasks` – create task `{ "title": "…" }`  
- `PUT /api/tasks/{id}` – replace an existing task (full update)  
- `PATCH /api/tasks/{id}` – partial update (title, priority, status, due date, mark complete)  
- `DELETE /api/tasks/{id}` – remove a task  

## Roadmap
- [x] In-memory tasks API  
- [x] EF Core + SQLite (persist tasks)  
- [x] Update (PUT/PATCH) & Delete endpoints  
- [x] Auth (Identity, per-user tasks, seeded demo user)  
- [ ] React frontend (Vite + TS)  

## Notes / Issues
- **SQLite + DateTimeOffset:**  
  SQLite doesn’t support ordering by `DateTimeOffset` directly, which caused a runtime error.  

  **Fix:** load tasks into memory and sort with LINQ in C#:  
  ```
  var items = await _db.Tasks.AsNoTracking().ToListAsync(ct);
  return Ok(items.OrderByDescending(t => t.CreatedAt));
  ```

-**Future improvement:** use SQL Server or store dates as Unix milliseconds to support server-side ordering.