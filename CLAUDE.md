# CLAUDE.md

Guidance for working in this repository.

## What this is

Employee Management Tool — a multi-tenant employee / org-management system, split
into three components that share **one central PostgreSQL database**:

| Directory | What it is | Stack |
|---|---|---|
| [`employee-management-identity/`](employee-management-identity/) | Auth / identity service: ASP.NET Core Identity, JWT issuing, refresh tokens | .NET 9, EF Core 9, clean-architecture layering (`*.core`, `*.infrastructure`, `*.models`, `*.utility`, + web project) |
| [`employee-management-microservice/`](employee-management-microservice/) | Org-domain service: Tenant, Organization, Department, DomainUser, Employee, Manager, ReportingLine | .NET 9 (early — mostly scaffolding so far) |
| [`employee-management-tool/`](employee-management-tool/) | Frontend | Next.js 15 (App Router, Turbopack), React 19, TypeScript, ESLint 9 |

## Database

- Canonical schema: [`Employee Management Tool.postgres.sql`](Employee%20Management%20Tool.postgres.sql) (PostgreSQL).
  [`Employee Management Tool.sql`](Employee%20Management%20Tool.sql) is the older SQL Server original — treat it as **stale**.
- **Single central DB** shared by all components. The ASP.NET Identity tables
  (`AspNetUsers`, …) are created by EF Core migrations, not the SQL script.
- **Two distinct "user" concepts — keep them straight:**
  - **`IdentityUser`** — `ApplicationUser : IdentityUser<Guid>` (the `AspNetUsers` row).
    The **auth identity**: login/credentials only, no domain data. Its `Id` is the
    canonical user id across the platform (the JWT subject).
  - **`DomainUser`** — the **org/business person** (name, tenant, role). Lives in the
    domain service; links to the identity via `DomainUser.IdentityUserId`
    (a plain uuid — no cross-service FK).
- Inside the domain, join on `DomainUser.DomainUserId`; only
  `DomainUser.IdentityUserId` and `RefreshToken.IdentityUserId` reference the identity id.

### Domain invariants worth knowing

- **`Employee` = ICs only.** `Manager` is a distinct entity, *not* a kind of employee.
- **The org chart lives in `ReportingLine`** (`ReportId → DomainUser`, `ManagerId → Manager`),
  unifying IC→manager and manager→supervisor. PK on `ReportId` = one manager per person.
- **Soft-delete, never hard-delete people.** Off-boarding flips
  `Employee.EmploymentStatus` / `DomainUser.IsActive`, sets `EndDate`, revokes refresh
  tokens, and reassigns reports — rows stay for history and FK validity.
- **Deferred while this is a demo:** self-reporting and cycle detection in
  `ReportingLine` are app-level checks that don't exist yet. See
  [`employee-management-microservice/README.md`](employee-management-microservice/README.md).

## Common commands

Frontend (`employee-management-tool/`):

- `npm run dev` — dev server (Turbopack)
- `npm run build` / `npm run start`
- `npm run lint`

.NET services (`employee-management-identity/`, `employee-management-microservice/`):

- `dotnet build` / `dotnet run`
- `dotnet ef migrations add <Name>` / `dotnet ef database update` — the identity
  service owns the ASP.NET Identity tables.
