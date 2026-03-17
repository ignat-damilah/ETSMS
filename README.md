# Foundation Solution

## Overview
The Foundation solution is a .NET 8-based microservice composed of four primary projects:

- **Foundation.Domain** – Contains the core business entities (e.g., `Employee`). It is referenced by all other layers and defines the shape of the data model.
- **Foundation.Application** – Implements application services, DTOs, and interfaces that orchestrate domain logic for upstream consumers (such as the API). It exposes services like `IEmployeeService` that translate domain entities into data contracts consumed by Presentational layers.
- **Foundation.Infrastructure** – Implements persistence and supporting infrastructure. It wires DbContext configuration, repository implementations, and integrations such as Entity Framework Core scoped services.
- **Foundation.API** – Hosts the ASP.NET Core minimal API that provides HTTP endpoints, policies, authentication, and observability.

## Getting Started
### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- PostgreSQL database (the connection string is configured in `appsettings.json` or via environment variables)
- Azure AD tenant and registered application if running in a secured environment

### Configuration
1. Copy or create `appsettings.json` at the root of the `Foundation.API` project with at least the following sections:
   ```json
   {
     "ConnectionStrings": {
       "FoundationDatabase": "Host=...;Database=...;Username=...;Password=..."
     },
     "Authentication": {
       "AzureAd": {
         "Authority": "https://login.microsoftonline.com/...",
         "Audience": "api://..."
       }
     },
     "Observability": {
       "ApplicationInsights": {
         "ConnectionString": "...",
         "InstrumentationKey": "..."
       }
     }
   }
   ```
2. Update CORS origins inside `Program.cs` if clients are not hosted at `https://app.corp.com`.

### Running the API
```bash
cd src/Foundation.API
dotnet run
```
The API exposes the following endpoints:
- `GET /health/live` – General liveness probe.
- `GET /health/ready` – Readiness probe that validates database connectivity.
- `GET /health` – Detailed health check response with registered checks.
- `GET /metrics` – Placeholder metrics endpoint (currently returns `Metrics endpoint`).
- `GET /employees` – Requires the `Employee` policy.
- `GET /employees/{id}` – Requires the `Leadership` policy.
- `POST /employees` – Requires the `Admin` policy (currently returns HTTP 202 Accepted without persisting).

### Testing
No automated tests are included. Implementers should add unit/integration tests as appropriate for the application services and API surface.

## Architecture Notes
- **Security** – JWT Bearer authentication is configured via Azure AD with policies for Employees, Leadership, and Admin roles. Role claims use `ClaimTypes.Role`.
- **Observability** – Application Insights telemetry is registered using instrumentation key/connection string from configuration.
- **Rate Limiting** – A token bucket rate limiter is registered and applied globally.
- **Dependency Injection** – `Foundation.Infrastructure` registers the `FoundationDbContext` and `EmployeeRepository`, while `Foundation.Application` is responsible for registering application services (not shown but referenced in `Program.cs`).

Consult `docs/FOUNDATION-ARCHITECTURE.md` for more layer-by-layer explanations.
