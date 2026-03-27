# Foundation Project

This solution provides a set of services to manage employee data using a clean architecture approach.

## Repository Structure

- `Foundation.API`
  - Entry point for the application. Configures authentication, authorization, rate limiting, CORS, observability, and exposes HTTP endpoints.
- `Foundation.Application`
  - Contains business logic, application services, DTOs, and interfaces consumed by the API layer.
- `Foundation.Domain`
  - Domain entities such as `Employee` with foundational behavior.
- `Foundation.Infrastructure`
  - Implements persistence (EF Core) and registers infrastructure services.

## Developing with the Foundation Project

### Prerequisites

- .NET SDK 7.0 (or the version targeted by `Foundation.sln`).
- PostgreSQL database for persistence (connection string configured under `FoundationDatabase`).
- Azure AD tenant information to support JWT-based authentication.

### Local Setup

1. Restore and build the solution:
   ```bash
   dotnet restore
   dotnet build
   ```
2. Configure the application settings (
   `Foundation.API/appsettings.json` or environment variables) with the following sections:
   - `ConnectionStrings:FoundationDatabase`
   - `Authentication:AzureAd:Authority`
   - `Authentication:AzureAd:Audience`
   - `Observability:ApplicationInsights` (Instrumentation key or connection string).
3. Apply database migrations or ensure the schema exists before running the API.
4. Run the API project:
   ```bash
   dotnet run --project src/Foundation.API/Foundation.API.csproj
   ```

### Application Layers

- **Infrastructure**
  - Registers `FoundationDbContext` using PostgreSQL and implements `EmployeeRepository`.
- **Application**
  - Defines `IEmployeeService`, `IEmployeeRepository`, and DTO mapping logic in `EmployeeService`.
- **API**
  - Configures middleware: authentication, authorization policies, rate limiting, observability hooks, CORS, and health checks.

### Authentication & Authorization

- The API uses Azure AD JWT authentication (`JwtBearerDefaults.AuthenticationScheme`).
- Policies map Azure AD roles to local requirements:
  - `EmployeePolicy` requires `Employee` role.
  - `LeadershipPolicy` requires `Leadership` role.
  - `AdminPolicy` requires `Admin` role.

### Observability & Reliability

- Application Insights telemetry is enabled when `Observability:ApplicationInsights` is populated.
- Rate limiting policy (`Fixed`) restricts requests to 100 tokens per minute.
- Health checks:
  - Readiness and liveness probes are exposed at `/health/ready` and `/health/live`.
  - Composite health check at `/health` returns JSON summary.
- Metrics endpoint stub available at `/metrics` (integrate with Prometheus as needed).

### API Endpoints

| Method | Path | Authorization | Description |
| --- | --- | --- | --- |
| GET | `/health/live` | None | Liveness probe (simple ok response). |
| GET | `/health/ready` | None | Readiness probe verifying DB connectivity. |
| GET | `/health` | None | Aggregated health checks response. |
| GET | `/metrics` | None | Placeholder for metrics instrumentation. |
| GET | `/employees` | EmployeePolicy | List all employees via `IEmployeeService`. |
| GET | `/employees/{id}` | LeadershipPolicy | Fetch employee by ID (mapped to DTO). |
| POST | `/employees` | AdminPolicy | Accepts employee payload (currently returns `202 Accepted`). |

### Testing & Development Tips

- Services are registered with dependency injection using extension methods (`AddInfrastructureServices`, `AddApplicationServices`).
- DTO mapping occurs within `EmployeeService` to keep controllers thin.
- Authorization and authentication configuration is centralized in `Program.cs` for clarity.
- Consider using in-memory PostgreSQL containers or SQL migrations when running integration tests.
- Extend repositories, services, and DTOs as new features emerge.

## Contributing

1. Create feature branches from `main`.
2. Follow existing naming conventions for services, DTOs, and extension methods.
3. Ensure authentication policies and dependency injection registrations stay centralized for discoverability.
4. Add documentation if you introduce new endpoints or services.
