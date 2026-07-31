# Student Course Registration System

An ASP.NET Core 8 Web API for student authentication and course discovery. The application is designed as a modular monolith with clean, explicit boundaries so registration and academic workflows can be added without coupling them to HTTP or persistence details.

## Implemented capabilities

- Student JWT authentication
- Active Course Catalog
- Development database migration and seed data
- SQL Server health check
- URL-segment API versioning
- Structured JSON logging
- Docker-based local environment

## Architecture

```text
API Controllers → Application Services → Repository Abstractions → EF Core → SQL Server
```

Controllers translate HTTP concerns only. Application services own business policy. Repositories contain persistence queries only. Domain entities represent data and Entity Framework configurations define database mapping.

## Technologies

- .NET 8 and ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server 2022
- JWT Bearer authentication
- ASP.NET API Versioning
- Swagger / OpenAPI
- xUnit and Moq
- Docker Compose

## Folder structure

```text
src/StudentCourseRegistration.Api/
├── Api/                 HTTP controllers, contracts, middleware, request context
├── Application/         services, DTOs, abstractions, application exceptions
├── Domain/              data-only domain entities
└── Infrastructure/      EF Core, migrations, repositories, security, logging, seeding
tests/StudentCourseRegistration.Tests/
└── Application/ and Infrastructure/ unit tests
```

## Endpoints

All versioned API endpoints use `/api/v1` and require a bearer token unless noted otherwise.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/v1/auth/login` | Authenticate an active student and receive a JWT. |
| GET | `/api/v1/auth/me` | Return the authenticated student. |
| GET | `/api/v1/courses` | Return active courses ordered by code. |
| GET | `/api/v1/courses/{id}` | Return one active course; absent/inactive courses return `404`. |
| GET | `/health` | Anonymous SQL Server connectivity health check. |

The versioning configuration accepts future endpoint versions without changing the existing `v1` URLs.

## Running locally

Prerequisites: .NET SDK 8, SQL Server or SQL Server LocalDB, and a development JWT signing key.

```bash
dotnet restore StudentCourseRegistration.sln
dotnet ef database update --project src/StudentCourseRegistration.Api
dotnet run --project src/StudentCourseRegistration.Api
```

The Development environment loads `appsettings.Development.json`. Environment variables override configuration, including nested values such as `ConnectionStrings__RegistrationDatabase` and `Jwt__SigningKey`.

Set a non-default local signing key before running outside disposable development environments:

```bash
dotnet user-secrets set "Jwt:SigningKey" "your-secure-development-key-with-at-least-32-characters" --project src/StudentCourseRegistration.Api
```

## Running with Docker

Docker Compose starts SQL Server and the API, applies migrations, and seeds development data when the database becomes healthy.

```bash
docker compose up --build
```

Useful commands:

```bash
docker compose down
docker compose down -v
docker compose logs -f api
```

The API is available at `http://localhost:8080`. Override `MSSQL_SA_PASSWORD` and `JWT_SIGNING_KEY` through your shell environment or a local `.env` file; do not commit production secrets.

## Migrations and seed data

The Development startup path automatically applies pending migrations and runs the idempotent `DevelopmentDatabaseSeeder`. The seeder creates only missing records:

- One administrator
- One active student
- Ten active courses

Default development accounts:

| Account | Email | Password | Notes |
|---|---|---|---|
| Administrator | `admin@university.edu` | `Password123!` | Seeded for future administration features; administrator authentication is not implemented yet. |
| Student | `john.doe@university.edu` | `Password123!` | Can authenticate using the current login endpoint. |

Passwords are stored as hashes. These credentials are development-only and must never be deployed to a production environment.

To explicitly apply migrations:

```bash
dotnet ef database update --project src/StudentCourseRegistration.Api
```

## Tests

```bash
dotnet test tests/StudentCourseRegistration.Tests/StudentCourseRegistration.Tests.csproj
```

Tests cover authentication and Course Catalog services, plus development seeder idempotence and password hashing.

## Swagger and health

- Swagger UI: `http://localhost:8080/swagger` when using Docker.
- Health check: `http://localhost:8080/health` when using Docker.

Structured JSON logs are written to standard output. They intentionally exclude passwords, JWTs, SQL commands, and query parameters.
