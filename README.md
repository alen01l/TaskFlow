# TaskFlow API

A task manager API built with **.NET 8**, **ASP.NET Core Web API**, **ASP.NET Core Identity**, **EF Core**, and **SQLite**.

## Features

- Cookie-based authentication with ASP.NET Core Identity
- Demo user seeded automatically
- Per-user task ownership
- SQLite persistence
- EF Core migrations
- FluentValidation for request validation
- Swagger in development
- CORS configured for the Vite frontend

## Run locally

### Requirements

- .NET 8 SDK
- Visual Studio 2022 or terminal
- SQLite database is created locally as `taskflow.db`

### Start the API

From the API project folder:

```bash
dotnet restore
dotnet build
dotnet run
```

Or open the project in Visual Studio 2022 and press `F5`.

Swagger is available in development at:

```txt
https://localhost:<port>/swagger
```

## Database

The API uses a local SQLite database file:

```txt
taskflow.db
```

The connection string is defined in `appsettings.json` under:

```json
"ConnectionStrings": {
  "dbContext": "Data Source=taskflow.db"
}
```

Migrations are applied automatically on startup.

To create a fresh local database, delete `taskflow.db` and run the API again.

## Demo user

A demo account is seeded automatically:

```txt
Email: demo@taskflow.local
Password: Pass123$
```

Use this account to log in before calling authenticated task endpoints.

## Authentication

Authentication uses cookies.

Login:

```http
POST /api/auth/login
```

Body:

```json
{
  "email": "demo@taskflow.local",
  "password": "Pass123$"
}
```

After login, the API sets a `taskflow.auth` cookie.

## Endpoints

### Auth

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Create a new user |
| `POST` | `/api/auth/login` | Log in and set auth cookie |
| `POST` | `/api/auth/logout` | Log out |
| `GET` | `/api/auth/me` | Get the current logged-in user |

### Tasks

Task endpoints require authentication.

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/tasks` | List current user's tasks |
| `GET` | `/api/tasks/{id}` | Get one task |
| `POST` | `/api/tasks` | Create a task |
| `PUT` | `/api/tasks/{id}` | Replace a task |
| `PATCH` | `/api/tasks/{id}` | Partially update a task |
| `DELETE` | `/api/tasks/{id}` | Delete a task |

## Task examples

### Create task

```http
POST /api/tasks
```

```json
{
  "title": "Build TaskFlow"
}
```

### Patch task

```http
PATCH /api/tasks/{id}
```

```json
{
  "title": "Build TaskFlow API",
  "status": "InProgress",
  "priority": "High",
  "markComplete": false
}
```

Valid statuses:

```txt
Backlog
InProgress
Done
```

Valid priorities:

```txt
Low
Medium
High
```

## Validation

Request validation is handled with FluentValidation.

Examples:

- Task title is required
- Task title has a max length
- Auth email must be valid
- Register password must meet minimum length

Invalid requests return `400 Bad Request`.

## Notes

### SQLite and DateTimeOffset

SQLite does not support ordering by `DateTimeOffset` directly in EF Core queries.

For now, tasks are loaded first and then sorted in memory:

```csharp
var items = await UserTasks
    .AsNoTracking()
    .ToListAsync(ct);

return Ok(items.OrderByDescending(t => t.CreatedAt));
```

Future options:

- Store dates as UTC `DateTime`
- Store dates as Unix milliseconds
- Move to SQL Server or PostgreSQL for richer date/time support

## Roadmap

- [x] EF Core + SQLite persistence
- [x] ASP.NET Core Identity authentication
- [x] Per-user tasks
- [x] CRUD task endpoints
- [x] Enum JSON serialization
- [x] DTO contracts
- [x] FluentValidation
- [x] React frontend integration
- [ ] Service layer
- [ ] Filtering and search
- [ ] Pagination
- [ ] Due date improvements