# Foundation Project Documentation

## Solution Overview
`Foundation.sln` defines four primary projects:

1. **Foundation.API** – Hosts the minimal Web API built with ASP.NET Core. Program.cs wires up the entire application: configuration sources, observability, authentication, rate limiting, CORS, health checks, and endpoint routing.
2. **Foundation.Application** – Contains the higher-level business logic, including DTOs, interfaces, and services. `EmployeeService` implements the `IEmployeeService` contract, transforming domain entities into DTOs and delegating persistence to the injected repository.
3. **Foundation.Domain** – Defines core entities, such as the `Employee` entity with properties like `FullName`, `Email`, and `Role`, representing the data used across the application layers.
4. **Foundation.Infrastructure** – Provides the implementation for data access, telemetry, and any other infrastructure concerns consumed by the API and application layers (details live inside this project).

## Project Behavior
### Configuration & Observability
- Configuration is loaded from `appsettings.json` with live reload support.
- Application Insights telemetry is registered using the `Observability` configuration section (`ConnectionString` and `InstrumentationKey`).

### Authentication & Authorization
- JWT Bearer authentication is configured via the `Authentication:AzureAd` section. Tokens are validated against the provided authority and audience, and roles are pulled from the `ClaimTypes.Role` claim.
- Authorization policies define three role-based requirements: `EmployeePolicy`, `LeadershipPolicy`, and `AdminPolicy`.

### CORS & Rate Limiting
- CORS allows requests from `https://app.corp.com`, permitting any header/method and credentials.
- Rate limiting uses a single global token bucket limiter named `Fixed` limiting requests to 100 tokens per minute without queuing.

### Health & Metrics Endpoints
- `/health/live` – lightweight readiness check returning a simple healthy status.
- `/health/ready` – checks database connectivity via the infrastructure DbContext.
- `/health` – built-in ASP.NET Core health checks report all registered checks in a JSON payload.
- `/metrics` – placeholder endpoint for metrics (currently returns a static string).

### Employee Endpoints
- GET `/employees` – requires `EmployeePolicy` and returns a list of employees via `IEmployeeService.ListAsync`.
- GET `/employees/{id}` – requires `LeadershipPolicy` and returns a specific employee or 404.
- POST `/employees` – requires `AdminPolicy`. Currently records the requested data and immediately returns `Accepted` (persistence relies on future implementation inside `IEmployeeService`).

### Application Layer
`EmployeeService` provides data access through the injected `IEmployeeRepository`. It exposes:
- `ListAsync` – returns all employees as `EmployeeDto` instances.
- `GetAsync` – fetches an employee by id and converts to `EmployeeDto`, returning `null` if not found.

DTOs expose only surface data (`Id`, `FullName`, `Email`, `Role`) to consumers, ensuring mapping logic remains inside the service layer.

## Building & Running
1. Restore and build: `dotnet build Foundation.sln`.
2. Run the API: `dotnet run --project src/Foundation.API/Foundation.API.csproj`.
3. Ensure `appsettings.json` contains valid Azure AD settings and optional Application Insights keys before starting.

## Testing & Contribution Tips
- Add new DTOs/services inside `Foundation.Application` to keep business logic decoupled.
- Keep infrastructure concerns (e.g., EF Core DbContext, telemetry) inside `Foundation.Infrastructure` and expose registrations via extension methods consumed by `Foundation.API`.
- When extending endpoints, rely on typed services via `[FromServices]` to stay consistent with the minimal API style.

## Known Constraints
- POST `/employees` endpoint does not yet persist data; implement repository logic and service methods before enabling write operations.
- Metrics endpoint is a placeholder; integrate Prometheus or another scraper if needed.
