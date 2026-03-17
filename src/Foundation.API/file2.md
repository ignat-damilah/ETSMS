# Foundation.API Documentation

This minimal API project exposes the employee-related endpoints, configures middleware, and sets up observability.

## Key Responsibilities
- Configure observability via Application Insights.
- Wire up authentication and authorization (Azure AD JWT, role-based policies).
- Configure CORS and rate limiting policies for HTTP traffic.
- Expose health checks, metrics, and employee endpoints.

## Configuration Highlights
- `Program.cs` loads `appsettings.json` and reads `Authentication:AzureAd` for JWT validation.
- Authorization policies (`EmployeePolicy`, `LeadershipPolicy`, `AdminPolicy`) ensure role-bound access to endpoints.
- Global rate limiting policy named `Fixed` limits to 100 requests per minute.
- CORS allows requests from `https://app.corp.com` with credentials.

## Endpoints
- `GET /health/live`: Returns `200 OK` with `{ status: "Healthy" }`.
- `GET /health/ready`: Checks `FoundationDbContext.Database.CanConnect` for readiness.
- `GET /health`: Aggregated health checks with statuses.
- `GET /metrics`: Placeholder metrics endpoint (returns `"Metrics endpoint"`).
- `GET /employees`: Requires `EmployeePolicy` and returns the result of `IEmployeeService.ListAsync`.
- `GET /employees/{id}`: Requires `LeadershipPolicy` and returns employee data when found.
- `POST /employees`: Requires `AdminPolicy`; currently fetches the employee by ID (placeholder logic).

## Dependency Injection
- Adds infrastructure services via `Foundation.Infrastructure.Extensions.ServiceCollectionExtensions`.
- Adds application services via `Foundation.Application.Extensions.ServiceCollectionExtensions`.
- Adds Application Insights telemetry with instrumentation key and connection string from configuration.

## Miscellaneous
- Uses ASP.NET Core rate limiting, authentication, and authorization middleware.
- Maps health checks and custom JSON response writer.
- Requests are forwarded through CORS, authentication, authorization, and then handled by endpoints.