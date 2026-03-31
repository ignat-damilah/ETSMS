# Foundation API Technical Overview

## Architecture
- **Presentation Layer (Foundation.API)**: Implements the HTTP pipeline, including authentication, authorization, CORS, rate limiting, observability, and routing to minimal API endpoints.
- **Application Layer (Foundation.Application)**: Hosts services and DTOs. The `EmployeeService` orchestrates repository interactions, transforms entities into DTOs, and exposes operations for listing and retrieving employees.
- **Domain Layer (Foundation.Domain)**: Contains core entity definitions such as `Employee`, encapsulating business attributes (e.g., `FullName` computed property).
- **Infrastructure Layer (Foundation.Infrastructure)**: Configures persistence via `FoundationDbContext`, and dependency injection extensions for data access implementations (repositories).

## Services & Dependency Injection
- Repositories implement `IEmployeeRepository` for data access, injected into `EmployeeService` in the Application layer.
- The API registers infrastructure and application services through extension methods under `Foundation.Infrastructure.Extensions.ServiceCollectionExtensions` and `Foundation.Application.Extensions.ServiceCollectionExtensions` respectively.
- This multilayered DI setup keeps the API decoupled from persistence details.

## Authentication & Authorization
- JWT Bearer authentication is configured using Azure AD settings from `appsettings.json` (authority, audience, role claim).
- Authorization policies enforce role-based access:
  - `EmployeePolicy` requires the `Employee` role.
  - `LeadershipPolicy` requires the `Leadership` role.
  - `AdminPolicy` requires the `Admin` role.

## Cross-cutting Concerns
- **Observability**: Application Insights telemetry is initialized with instrumentation key and connection string consumed from configuration.
- **CORS**: Restricts frontend origins to `https://app.corp.com`, allowing all headers/methods and credentials.
- **Rate Limiting**: Applies a global token bucket limiter permitting 100 tokens per minute to throttle traffic.
- **Health Checks**: Exposes liveness (`/health/live`) and readiness (`/health/ready`) endpoints plus `/health` that aggregates registered checks with JSON responses.
- **Metrics**: `/metrics` endpoint placeholder exists for integration with monitoring tools such as Prometheus.

## API Endpoints
| Endpoint | Method | Authorization | Description |
| --- | --- | --- | --- |
| `/health/live` | GET | None | Reports service liveness with a simple status response. |
| `/health/ready` | GET | None | Validates database connectivity via EF Core `CanConnect`. |
| `/health` | GET | None | Aggregated health report with check names and statuses. |
| `/metrics` | GET | None | Placeholder endpoint for Prometheus-style metrics. |
| `/employees` | GET | `EmployeePolicy` | Lists all employees via `EmployeeService.ListAsync`. |
| `/employees/{id}` | GET | `LeadershipPolicy` | Retrieves a single employee by ID via `EmployeeService.GetAsync`. |
| `/employees` | POST | `AdminPolicy` | Accepts employee payload (creates via service, though implementation currently returns `202 Accepted`). |

## Notes & Future Work
- The POST `/employees` endpoint currently returns `202 Accepted` without persisting; future work should wire up `EmployeeService` create methods.
- Implementing telemetry/metering around health checks and rate limiting could improve operational insights.
