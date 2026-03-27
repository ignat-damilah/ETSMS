# Developer Guide

## Architecture Overview
The Foundation solution targets a layered architecture to keep concerns separated:

1. **Foundation.API**
   - Minimal ASP.NET Core Web API application.
   - Registers middleware for CORS, rate limiting, authentication/authorization, health checks, and observability (Application Insights and Prometheus-style metrics endpoint).
   - Exposes HTTP endpoints to surface employee data based on role-based policies (`EmployeePolicy`, `LeadershipPolicy`, `AdminPolicy`).

2. **Foundation.Application**
   - Defines DTOs and service interfaces for the business use cases.
   - Implements the `EmployeeService` which maps domain entities to DTOs and orchestrates repository access.

3. **Foundation.Domain**
   - Contains core entities such as `Employee` with business-focused properties (e.g., `FullName`, `Role`).

4. **Foundation.Infrastructure**
   - Houses EF Core `FoundationDbContext`, repository implementations, and dependency injection extensions.
   - Configures PostgreSQL via `UseNpgsql` and exposes repository abstractions to the application layer.

## Configuration
- **appsettings.json** is required for configuration (already loaded in `Program.cs`).
- **Connection Strings**: make sure the `FoundationDatabase` connection string is set (expected to be PostgreSQL). The infrastructure layer throws an exception if the value is missing.
- **Authentication**: Azure AD configuration is read from the `Authentication:AzureAd` section with `Authority` and `Audience` values.
- **Observability**: Configure `Observability:ApplicationInsights:InstrumentationKey` or `ConnectionString` to enable Application Insights telemetry.

## Building & Running Locally
1. Restore dependencies:
   ```bash
   dotnet restore Foundation.sln
   ```
2. Update configuration (e.g., `appsettings.Development.json` or environment variables) to supply the database connection string and Azure AD values.
3. Run the API:
   ```bash
   dotnet run --project src/Foundation.API/Foundation.API.csproj
   ```
4. Swagger/UI is not wired up by default; use your own HTTP client against the endpoints.

## Important Middleware & Endpoints
- **Rate Limiting**: A global token bucket limiter `Fixed` allows up to 100 requests per minute with no queued requests. Enabled via `app.UseRateLimiter()`.
- **CORS**: Only `https://app.corp.com` is allowed with credentials support.
- **Authentication/Authorization**: JWT Bearer tokens validated against Azure AD and policies for `Employee`, `Leadership`, and `Admin` roles.
- **Health Checks**:
  - `/health/live`: simple liveness probe returning `Healthy`.
  - `/health/ready`: readiness probe checking database connectivity.
  - `/health`: aggregated health check endpoint emitting JSON with all checks.
- **Metrics**: `/metrics` returns a placeholder string, intended for hooking into Prometheus instrumentation.

## Data Access
- `EmployeeRepository` relies on EF Core with `DbSet<Employee>`.
- Queries use `AsNoTracking()` and sort by last/first name.
- Entities must have a unique `Id`, `Email`, and `Role` (with field length constraints).

## Extending the API
1. Add or modify DTOs/services in **Foundation.Application**.
2. Implement repository logic in **Foundation.Infrastructure.Repositories**.
3. Register new services in the extension methods (`AddInfrastructureServices`, `AddApplicationServices` in `Foundation.Application.Extensions`).
4. Update `Program.cs` endpoints to expose new functionality and apply appropriate policies.

## Observability
- Application Insights automatically captures telemetry if the instrumentation key or connection string is provided.
- Prometheus-style metrics should be connected via the `/metrics` endpoint once configured.
- Health checks provide quick visibility into liveness and readiness; integrate with your orchestration platform accordingly.
