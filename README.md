# Healthcare Operations Platform

Backend REST API for core hospital and clinic workflows — authentication, user and patient management, doctor profiles, appointments, prescriptions, and an append-only audit log.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-WebAPI-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-8-512BD4)
![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?logo=microsoftsqlserver&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-integration_tests-512BD4)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?logo=jsonwebtokens&logoColor=white)
![Swagger](https://img.shields.io/badge/Docs-Swagger-85EA2D?logo=swagger&logoColor=black)
![License](https://img.shields.io/badge/License-MIT-green)

---

## Overview

A backend service that models the operational heart of a small clinic: staff sign in, reception books appointments, doctors issue prescriptions, admins manage users and departments, and every change is captured in an audit trail.

Built with ASP.NET Core 8, Entity Framework Core, and SQL Server. JWT authentication with role-based authorisation (Admin, Doctor, Receptionist). Layered architecture keeps the API host thin and the domain testable.

## Architecture

Five-project solution, dependency direction flows inward toward the domain.

```
Healthcare.sln
├── src/
│   ├── Healthcare.Api              ASP.NET Core host — controllers, middleware, Swagger, JWT wiring
│   ├── Healthcare.Application      Service abstractions and application contracts
│   ├── Healthcare.Domain           Entities, enums, constants (no external dependencies)
│   ├── Healthcare.Infrastructure   EF Core DbContext, persistence, auth services, password hashing
│   └── Healthcare.Contracts        Request/response DTOs shared across the API boundary
├── tests/
│   └── Healthcare.Api.Tests        xUnit integration tests (Auth, Patients, Appointments, Audit)
├── database/                        Schema and seed scripts
├── docs/                            Functional, database, and API specifications
└── tools/                           Small utilities (password hash generator, etc.)
```

| Layer | Responsibility |
|---|---|
| **Api** | HTTP surface: routing, model binding, Swagger, JWT validation, exception middleware |
| **Application** | Orchestration — no EF Core, no HTTP — just interfaces and service contracts |
| **Domain** | Pure C#: entities, value objects, enums, domain constants |
| **Infrastructure** | EF Core `DbContext`, migrations, password hashing (BCrypt), JWT token issuance |
| **Contracts** | DTOs for the public API — kept separate so the domain never leaks over HTTP |

## Features

**Authentication & authorisation**
- JWT bearer tokens with configurable issuer, audience, lifetime
- Role-based access control: Admin, Doctor, Receptionist
- BCrypt password hashing
- `GET /auth/me` for current-user profile

**Clinical workflows**
- Patient registration, updates, and dependent relationships
- Doctor profiles and availability windows
- Appointment scheduling with availability + conflict validation
- Prescription issuance and per-patient history
- Department management

**Platform guarantees**
- Request validation via data annotations
- Soft-delete with global query filters (safe-by-default reads)
- Automatic `CreatedAt` / `UpdatedAt` / `CreatedBy` / `UpdatedBy` stamping
- Append-only audit log for create, update, delete, login, and status-change events
- Swagger UI with JWT bearer support at `/swagger`

## Tech stack

| Concern | Technology |
|---|---|
| Runtime | .NET 8 |
| Web framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server 2019+ (or LocalDB) |
| Auth | JWT bearer + BCrypt |
| API docs | Swashbuckle / Swagger UI |
| Tests | xUnit |
| Language | C# 12 |

## Getting started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server 2019+, SQL Server Express, or LocalDB
- EF Core CLI tool: `dotnet tool install --global dotnet-ef`

### First-time setup

```bash
# 1. Clone
git clone https://github.com/qurrat2/Healthcare-Operations-Platform---HOP.git
cd Healthcare-Operations-Platform---HOP

# 2. Copy the dev config template and edit as needed
cp src/Healthcare.Api/appsettings.Development.json.example \
   src/Healthcare.Api/appsettings.Development.json

# 3. Restore packages
dotnet restore

# 4. Apply EF Core migrations (creates the HealthcareDB database)
dotnet ef database update \
  --project src/Healthcare.Infrastructure \
  --startup-project src/Healthcare.Api

# 5. Run the API
dotnet run --project src/Healthcare.Api
```

The API will listen on `https://localhost:5001` (check the console output for the actual port).

Swagger UI: **https://localhost:5001/swagger**

### Running the tests

```bash
dotnet test
```

## API examples

### Log in and capture a JWT

```bash
curl -sk -X POST https://localhost:5001/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"YourSeededPassword"}'
```

Response (abbreviated):

```json
{
  "data": {
    "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "token_type": "Bearer",
    "expires_in": 3600,
    "user": { "id": 1, "username": "admin", "role": "Admin" }
  },
  "message": "Login successful"
}
```

### List patients (authenticated)

```bash
TOKEN="eyJhbGciOiJIUzI1NiIs..."
curl -sk https://localhost:5001/api/v1/patients \
  -H "Authorization: Bearer $TOKEN"
```

### Check appointment availability

```bash
curl -sk "https://localhost:5001/api/v1/appointments/availability?doctorId=1&date=2026-05-01" \
  -H "Authorization: Bearer $TOKEN"
```

See the full surface at `/swagger` once running. All endpoints use snake_case JSON and the envelope shape `{ data, message }` for success responses.

## Configuration

Secrets and environment-specific values live in `appsettings.Development.json` (gitignored) or user-secrets:

```bash
dotnet user-secrets set "Authentication:Jwt:SecretKey" "<your-32+-char-secret>" \
  --project src/Healthcare.Api
```

`appsettings.json` (checked in) holds production defaults — override via environment variables or user-secrets, never by editing the committed file.

## Project status

Active learning / portfolio project. The foundation is working: real database access, JWT login, seeded sample data, Swagger-based API exploration, end-to-end feature endpoints, request validation, soft-delete filters, and xUnit integration tests.

**Planned next**
- GitHub Actions CI (build + test on push)
- Docker Compose for one-command local SQL Server + API
- OpenAPI client generation for the demo UI
- Role-scoped integration test suite

## License

[MIT](LICENSE)
