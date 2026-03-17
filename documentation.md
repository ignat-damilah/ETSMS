# Foundation Project

The Foundation solution is a .NET 8-based microservice that exposes a minimal HTTP API to manage employees. It follows a clean architecture approach with distinct layers for the API surface, application logic, domain entities, and infrastructure concerns.

## Repository Structure

- `Foundation.API` – ASP.NET Core Web API hosting services, middleware, authentication, CORS, rate limiting, health checks, and Prometheus metrics.
- `Foundation.Application` – Application layer exposing DTOs, services, and interfaces for business logic such as listing and retrieving employees.
- `Foundation.Domain` – Contains domain entities such as `Employee` used throughout the solution.
- `Foundation.Infrastructure` – Integrates Entity Framework Core with PostgreSQL, implements the repository, and wires up infrastructure services.

## Key Features

- JWT authentication through Azure AD, with `Employee`, `Leadership`, and `Admin` policies applied to endpoints.
- Application Insights for observability, Prometheus metrics endpoint at `/metrics`, and built-in health check endpoints (`/health/live`, `/health/ready`, `/health`).
- Rate limiting via a global token bucket policy and CORS configured for `https://app.corp.com`.
- Clean segregation of responsibilities via dependency injection extension methods in `AddInfrastructureServices` and `AddApplicationServices`.

## Running the Project

1. Ensure a PostgreSQL instance is available and configure the connection string named `FoundationDatabase` in `appsettings.json` (or environment specific files).
2. Configure Azure AD settings under the `Authentication:AzureAd` section for `Authority` and `Audience`.
3. Optionally configure Application Insights using `Observability:ApplicationInsights:ConnectionString` / `InstrumentationKey`.
4. Restore and build the solution:
   ```bash
   dotnet restore Foundation.sln
   dotnet build Foundation.sln
   ```
5. Run the API from `src/Foundation.API`:
   ```bash
   cd src/Foundation.API
   dotnet run
   ```

## API Endpoints

- `GET /health/live` – Simple liveness probe.
- `GET /health/ready` – Verifies database connectivity via `FoundationDbContext`.
- `GET /health` – Aggregated health checks with JSON output.
- `GET /metrics` – Placeholder for Prometheus metrics.
- `GET /employees` – Lists employees (requires `EmployeePolicy`).
- `GET /employees/{id}` – Retrieves a single employee (requires `LeadershipPolicy`).
- `POST /employees` – Accepts employee creation (requires `AdminPolicy`).

## Data Access

Employees are persisted via Entity Framework Core in `FoundationDbContext`. The `EmployeeRepository` handles CRUD operations and ensures queries use `AsNoTracking` for read-only data.

## Testing

There are currently no automated tests included. Future work may add unit and integration tests.

## Deployment Notes

- Use secrets or environment variables for sensitive settings (`FoundationDatabase`, Azure AD credentials, App Insights keys).
- Monitor the `/metrics` endpoint via Prometheus if integrated.
- Health check endpoints should be used by orchestrators for liveness/readiness probes.
