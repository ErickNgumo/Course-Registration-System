# Course Registration System

A full‑stack university registration portal: an **ASP.NET Core 8** modular‑monolith backend backed by **SQL Server** and **EF Core**, paired with a modern **React 18 + TypeScript** SPA. Students browse the catalog, register, manage waitlists, and track their academic progress; administrators manage students, courses, enrollments, grading, reporting, and audit.

> The backend is feature‑complete and stable. This phase adds the production‑quality frontend, integrates it with the existing API contract, and wires the whole stack to run with a single command.

---

## Table of contents

- [Highlights](#highlights)
- [Architecture](#architecture)
- [Repository layout](#repository-layout)
- [Tech stack](#tech-stack)
- [Quick start — one command](#quick-start--one-command)
- [Running the backend](#running-the-backend)
- [Running the frontend](#running-the-frontend)
- [Environment variables](#environment-variables)
- [Authentication](#authentication)
- [Frontend architecture](#frontend-architecture)
- [API integration](#api-integration)
- [Testing](#testing)
- [Building & deploying](#building--deploying)
- [Screenshots](#screenshots)
- [Default development accounts](#default-development-accounts)
- [Troubleshooting](#troubleshooting)

---

## Highlights

**Backend (ASP.NET Core 8)**
- Student & Administrator JWT authentication with role‑based authorization
- Course catalog with prerequisites, schedules, and capacity management
- Student registration with timetable‑conflict detection and automatic waitlisting
- Student dashboard (current credits, registered / waitlisted / completed courses, history)
- Administration module: students, courses, enrollments, grading, waitlist promotion
- Reporting (enrollment, capacity utilization, student status, waitlists) and tamper‑evident audit logs
- Global exception handling (RFC 7807 ProblemDetails), structured logging, URL‑segment API versioning, Swagger

**Frontend (React 18 + TypeScript)**
- Feature‑based, scalable architecture under `src/`
- Typed API layer — no HTTP calls in components; centralized Axios + endpoint definitions
- TanStack Query for all server state; React Query mutations with optimistic invalidation
- React Hook Form + Zod for forms with inline and server‑side validation
- React Router with `React.lazy` code‑splitting, protected & role‑based routes
- shadcn/ui + TailwindCSS components, dark/light/system themes
- Chart.js reports (enrollment trends, capacity utilization, status mix, credit distribution)
- Accessibility: keyboard navigation, ARIA labels, focus rings, color‑contrast tokens
- Loading skeletons, empty states, error boundaries (404 / 403 / 500), toast notifications

---

## Architecture

```text
┌───────────────┐        ┌────────────────────────────────────────────┐        ┌────────────┐
│   Browser     │  HTTP   │            ASP.NET Core 8 API              │  EF    │  SQL Server │
│  React SPA    │ ──────▶ │  ┌───────────────────────────────────────┐ │ ─────▶ │  2022       │
│ (Vite + nginx)│ ◀────── │  │ Controllers → Application Services →    │ │        │            │
│               │  JSON   │  │ Repository Abstractions → EF Core       │ │        └────────────┘
└───────────────┘         │  └───────────────────────────────────────┘ │
                          │  JWT auth · ProblemDetails · Swagger ·    │
                          │  audit · migrations · dev seeder           │
                          └───────────────────────────────────────────┘
```

Frontend layers:

```text
Pages ─▶ Hooks (use-queries) ─▶ Services (typed, centralized) ─▶ Axios api-client ─▶ API
  │        │ TanStack Query for cache + invalidation                 │
  └─ Components (ui/ shadcn primitives + shared features)
        └─ Contexts (Auth, Theme) and lib/ (utils, formats, schemas, query-keys)
```

No API calls live inside components. UI renders against queries; mutations invalidate the relevant cache keys (defined once in `lib/query-keys`).

---

## Repository layout

```text
.
├── docker-compose.yml            # database + api + web (one-command stack)
├── Dockerfile                    # backend (dotnet sdk → aspnet runtime)
├── src/StudentCourseRegistration.Api/      # modular monolith: Api / Application / Domain / Infrastructure
├── tests/StudentCourseRegistration.Tests/  # xUnit + Moq
├── .dockerignore
└── frontend/                     # React 18 + Vite SPA
    ├── package.json
    ├── vite.config.ts             # dev server proxy + Vitest config
    ├── tailwind.config.js
    ├── Dockerfile                 # node build → nginx SPA
    ├── nginx.conf                 # reverse proxies /api → api:8080 + history fallback
    └── src/
        ├── main.tsx  App.tsx
        ├── pages/                 # route-level pages (lazy-loaded)
        ├── layouts/               # StudentLayout, AdminLayout
        ├── components/            # shared: DataTable, Pagination, EmptyState, charts… + ui/ (shadcn primitives)
        ├── features/             # feature-scoped widgets (where helpful)
        ├── hooks/                # useAuth, useToast, useDebounce, use-queries (React Query hooks)
        ├── services/             # typed API services per area
        ├── contexts/             # AuthContext, (ThemeProvider)
        ├── lib/                  # api-client, api-error, jwt, utils, format, schemas, query-keys
        └── types/                # api.ts — every backend DTO mirrored as a TS type
```

---

## Tech stack

| Layer        | Technology |
|--------------|------------|
| Backend      | .NET 8, ASP.NET Core Web API, EF Core 8, JWT, URL‑segment versioning, Swagger |
| Database     | Microsoft SQL Server 2022 |
| Backend tests| xUnit, Moq |
| Frontend     | React 18, TypeScript, Vite, TailwindCSS, shadcn/ui, React Router, TanStack Query, Axios, React Hook Form, Zod, Lucide |
| Charts       | Chart.js + react‑chartjs‑2 |
| Frontend tests| Vitest, Testing Library, jsdom |
| Containers   | Docker, Docker Compose |

---

## Quick start — one command

This builds and runs the database, the API, and the frontend together.

```bash
# from the repository root
docker compose up --build
```

| Service      | URL                                  | Notes |
|--------------|--------------------------------------|-------|
| Frontend SPA | http://localhost:5173               | nginx serves the built React app; `/api` is proxied to the API |
| Backend API  | http://localhost:8080               | Swagger at `/swagger`, health at `/health` |
| SQL Server   | localhost:1433                       | data persisted in the `sqlserver-data` volume |

Open http://localhost:5173 and sign in with a seeded account (see [Default development accounts](#default-development-accounts)).

Tear down (keep data):
```bash
docker compose down
```
Tear down and delete the database volume:
```bash
docker compose down -v
```

---

## Running the backend

Prerequisites: .NET SDK 8, SQL Server (LocalDB or container), a development JWT signing key.

```bash
dotnet restore StudentCourseRegistration.sln
dotnet ef database update --project src/StudentCourseRegistration.Api
dotnet run --project src/StudentCourseRegistration.Api
```

The Development environment loads `appsettings.Development.json`. Environment variables override configuration, including nested values such as `ConnectionStrings__RegistrationDatabase` and `Jwt__SigningKey`. On startup in Development, pending migrations are applied and the idempotent `DevelopmentDatabaseSeeder` runs.

Set a non‑default local signing key before running outside disposable development environments:

```bash
dotnet user-secrets set "Jwt:SigningKey" "your-secure-development-key-with-at-least-32-characters" \
  --project src/StudentCourseRegistration.Api
```

Backend tests:
```bash
dotnet test tests/StudentCourseRegistration.Tests/StudentCourseRegistration.Tests.csproj
```

---

## Running the frontend

Prerequisites: Node 18+ and npm.

```bash
cd frontend
npm install
npm run dev          # http://localhost:5173 (proxies /api → :8080)
```

The Vite dev server proxies `/api` and `/health` to `http://localhost:8080`, so start the backend first (or run the docker stack). Other scripts:

```bash
npm run build        # type-check + production build → dist/
npm run preview      # serve the production build locally on :4173
npm run lint         # ESLint
npm test             # Vitest (run once)
npm run test:watch   # Vitest (watch mode)
```

---

## Environment variables

**Backend** (set via shell, `.env`, or docker compose)

| Variable | Default | Purpose |
|---|---|---|
| `MSSQL_SA_PASSWORD` | `YourStrong@Passw0rd` | SQL Server SA password |
| `ConnectionStrings__RegistrationDatabase` | (compose sets this) | EF Core connection string |
| `Jwt__Issuer` | `StudentCourseRegistration` | JWT `iss` claim |
| `Jwt__Audience` | `StudentCourseRegistration.Client` | JWT `aud` claim |
| `Jwt__SigningKey` | (development key) | Signing key (≥32 chars in production) |
| `Jwt__ExpiresInMinutes` | `60` | Token lifetime |
| `ASPNETCORE_ENVIRONMENT` | `Development` | ASPNETCORE environment |

**Frontend** (`frontend/.env` — Vite bundling-time only)

| Variable | Default | Purpose |
|---|---|---|
| `VITE_API_BASE_URL` | `/api/v1` | API base URL. The dev server and nginx both proxy `/api` to the backend. Point this at a remote API (e.g. `https://api.example.edu/api/v1`) to run the SPA against a deployed backend. |

A `frontend/.env.example` is provided. Never commit real secrets.

---

## Authentication

- Students sign in at `/login`; administrators at `/admin/login`. Both return a JWT.
- The JWT, role, user id, and display name persist in `localStorage` under `accessToken` / `userRole` / `userId` / `userName`.
- An Axios request interceptor attaches `Authorization: Bearer <token>` to every call.
- A response interceptor clears auth and bounces to the matching login page on `401`.
- `ProtectedRoute` gates role‑specific routes; a mismatched role redirects to `/unauthorized` (403).
- `AuthContext` polls for token expiry and logs the user out automatically.

---

## Frontend architecture

- **Feature‑based pages** — every route is a page in `src/pages/`, lazy‑loaded with `React.lazy` + `Suspense` for code splitting.
- **Typed API layer** — `src/services/*.service.ts` centralize endpoint definitions and return typed DTOs. No component imports axios directly.
- **React Query hooks** — `src/hooks/use-queries.ts` wraps every service call in `useQuery`/`useMutation`, and `src/lib/query-keys.ts` defines stable hierarchical cache keys so invalidation is consistent.
- **Forms** — React Hook Form + Zod schemas (`src/lib/schemas.ts`) drive accessible inline validation; server errors map onto fields via `setError`.
- **Error handling** — `ErrorBoundary` catches render errors; `QueryErrorBoundary` surfaces recoverable data errors; `ErrorPage` renders 404 / 403 / 500 states.
- **Theming** — `ThemeProvider` offers light/dark/system with CSS variables defined once in `index.css` and consumed by Tailwind tokens.
- **Components** — shadcn primitives live in `src/components/ui/`; shared feature components (`DataTable`, `Pagination`, `ConfirmDialog`, charts, `EmptyState`, `LoadingState`, `ProgressBar`) sit in `src/components/`.

---

## API integration

All endpoints are versioned under `/api/v1` and require a bearer token unless noted. Run the backend and open Swagger (`/swagger`) for the live, exhaustive endpoint catalog — the frontend was built against that contract and intentionally avoids duplicating it here so the source of truth stays the backend.

Mapped service files:
- `services/auth.service.ts` — student & admin login, current student
- `services/courses.service.ts` — student course catalog
- `services/enrollments.service.ts` — register/drop/dashboard
- `services/admin.service.ts` — admin dashboard, students, courses, enrollments, reports, audit

The Axios client reads `VITE_API_BASE_URL` (default `/api/v1`) and the dev proxy + nginx both forward `/api` to the backend, so no configuration is needed in local development.

---

## Testing

**Backend:**
```bash
dotnet test tests/StudentCourseRegistration.Tests
```
Covers authentication, course catalog, seeder idempotence, and persistence behaviour.

**Frontend** (Vitest + Testing Library):
```bash
cd frontend
npm test              # run once
npm run test:watch    # watch mode
```
The suite includes:
- Pure‑function tests for `api-error`, `utils`, `query-keys`, and Zod `schemas`
- Service tests (`api-service.test.ts`) with a mocked axios instance asserting correct paths/methods/payloads
- Component tests (`Pagination.test.tsx`)
- Hook tests (`useAuth`, `useToast`)
- A critical page test (`LoginPage.test.tsx`) covering validation, success flow, and API failure handling

Coverage is configured (V8). Run `npx vitest run --coverage` to produce an HTML/LCOV report in `frontend/coverage`.

---

## Building & deploying

**Frontend static build:**
```bash
cd frontend && npm ci && npm run build
# → dist/ is a static SPA. Serve with any static host or an nginx that
#   proxies /api to the backend and falls back to index.html for routes.
```

**Backend publish:**
```bash
dotnet publish src/StudentCourseRegistration.Api/StudentCourseRegistration.Api.csproj \
  -c Release -o ./publish
```

**Full stack via Docker:**
```bash
docker compose up --build
```
This builds three images — `database` (SQL Server), `api` (backend runtime), and `web` (nginx‑served SPA). The frontend container proxies `/api` to the `api` service, so the SPA only needs a single origin (`:5173`). For production, set `Jwt__SigningKey`, `MSSQL_SA_PASSWORD`, and `VITE_API_BASE_URL` to real values and run the stack behind TLS.

---

## Screenshots

> Screenshots are captured during a normal demo run and dropped into `docs/screenshots/`. Add `.png` files and they render below.

| Student Dashboard | Course Catalog | Admin Reports |
|---|---|---|
| `docs/screenshots/student-dashboard.png` | `docs/screenshots/course-catalog.png` | `docs/screenshots/admin-reports.png` |

---

## Default development accounts

The Development seeder creates these accounts (passwords are hashed). They are development‑only and must never reach a production environment.

| Account | Email | Password |
|---|---|---|
| Administrator | `admin@university.edu` | `Password123!` |
| Student | `john.doe@university.edu` | `Password123!` |

---

## Troubleshooting

- **`ECONNREFUSED` during `npm run dev`** — the API isn't running on `:8080`. Start the backend (or `docker compose up api database`) and reload.
- **401 immediately after login** — the local clock or token expiry is off; clear `localStorage` and sign in again (`AuthContext` auto‑logout may have fired).
- **Port conflicts** — `:5173` (SPA), `:8080` (API), `:1433` (SQL Server). Free the port or remap it in `docker-compose.yml`.
- **Migrations not applied** — in Development the app applies pending migrations on startup; ensure `ASPNETCORE_ENVIRONMENT=Development`.
- **Frontend can't reach a remote API** — set `VITE_API_BASE_URL` to the absolute API base and rebuild (Vite inlines it at build time).
