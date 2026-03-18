# Foundation Platform Documentation

## Overview
The Foundation solution is a .NET 8 Web API that exposes a minimal set of endpoints for managing employee data while integrating with infrastructure concerns such as authentication, observability, health monitoring, and rate limiting. The architecture separates concerns between API, application, domain, and infrastructure layers to keep the codebase extensible and testable.

## Solution Architecture
- **Foundation.API**: Defines the HTTP surface (endpoints, middleware, policies, and host configuration) and wires up application and infrastructure services. It configures observability, authentication/authorization, CORS, and rate limiting.
- **Foundation.Application**: Implements domain logic through services and interfaces. This layer exposes `IEmployeeService`, its DTOs, and the service registration extension used by the API layer. It depends on abstractions (e.g., `IEmployeeRepository`) defined alongside the logic to keep dependencies inverted.
- **Foundation.Domain**: Contains the entity definitions (currently `Employee`) and any domain-specific logic or invariants.
- **Foundation.Infrastructure**: Implements persistence using Entity Framework Core (`FoundationDbContext`) and data repositories (`EmployeeRepository`). It also exposes an extension to register infrastructure services and configuration required by the API layer.

## Requirements & Setup
### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/) database for the `FoundationDatabase` connection string
- Access to the configured Azure AD tenant (for authentication tokens)

### Configuration
The application reads from `appsettings.json` or environment variables. Key configuration sections:

```json
"Observability": {
  "ApplicationInsights": {
    "InstrumentationKey": "<Instrumentation Key if using direct key>",
    "ConnectionString": "<Optional Application Insights connection string>"
  }
},
"Authentication": {
  "AzureAd": {
    "Authority": "https://login.microsoftonline.com/{tenant}",
    "Audience": "api://{client-id}"
  }
},
"ConnectionStrings": {
  "FoundationDatabase": "Host=...;Database=...;Username=...;Password=..."
}
```

Ensure the `FoundationDatabase` connection string uses the correct runtime database provider (PostgreSQL) and credentials.

### Running the API Locally
1. Restore dependencies and build:
   ```bash
   dotnet restore src/Foundation.API/Foundation.API.csproj
   dotnet build src/Foundation.API/Foundation.API.csproj
   ```
2. Apply database migrations or ensure the schema exists against PostgreSQL.
3. Run the API:
   ```bash
   dotnet run --project src/Foundation.API/Foundation.API.csproj
   ```
4. The API listens on the configured HTTP port, typically `https://localhost:5001` and `http://localhost:5000`.

## API Surface
### Middleware & Supporting Features
- **Application Insights**: Enabled if configuration is provided via `Observability:ApplicationInsights`. Both instrumentation key and connection string are supported.
- **Authentication & Authorization**: Azure AD JWT bearer tokens are required. Roles define access via policies:
  - `EmployeePolicy` (requires `Employee` role)
  - `LeadershipPolicy` (requires `Leadership` role)
  - `AdminPolicy` (requires `Admin` role)
- **CORS**: Only allows `https://app.corp.com` by default with any header/method and Credentials.
- **Rate Limiting**: Global token bucket policy (100 tokens per minute, no queue) protects the API surface.
- **Health Checks**: Endpoints (`/health/live`, `/health/ready`, `/health`) expose liveness and readiness states plus the full health report.
- **Metrics**: A placeholder `/metrics` endpoint exists for Prometheus scraping.

### Endpoints
| Method | Path | Authorization | Description |
| --- | --- | --- | --- |
| `GET` | `/health/live` | None | Simple liveness probe returning `{ "status": "Healthy" }`. |
| `GET` | `/health/ready` | None | Validates database connectivity via EF Core. Responds `200 OK` if healthy, `503` otherwise. |
| `GET` | `/health` | None | Aggregated health checks with detailed status per check. Returns JSON payload containing overall status and entries. |
| `GET` | `/metrics` | None | Placeholder for Prometheus metrics (returns plain text). |
| `GET` | `/employees` | `EmployeePolicy` | Lists all employees (`EmployeeDto`) sorted by last name. |
| `GET` | `/employees/{id}` | `LeadershipPolicy` | Retrieves a single employee by ID. Returns `404` if not found. |
| `POST` | `/employees` | `AdminPolicy` | Currently returns `202 Accepted` while fetching the employee by `Id` (implementation is a placeholder for future creation logic). |

### Employee DTO
Employees returned by the API contain the following fields:
- `Id` (GUID)
- `FullName` (concatenation of first and last name)
- `Email`
- `Role`

## Contributing & Next Steps
- Add persistence unit tests by mocking `IEmployeeRepository`.
- Expand endpoint implementations (e.g., implement POST to create employees).
- Document additional operational requirements such as logging levels, secrets management, and CI/CD instructions if needed.

For any questions, review each project's source files (e.g., `Foundation.API/Program.cs`, `Foundation.Infrastructure`, etc.) to understand service wiring and dependency structure.