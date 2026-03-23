# Foundation Solution

## Overview
Foundation is a clean architecture-inspired .NET 8 solution composed of four core projects:
1. **Foundation.Domain** defines the domain entities (currently `Employee`).
2. **Foundation.Application** contains business logic services and DTOs for application layer operations.
3. **Foundation.Infrastructure** hosts data access, repositories, and persistence-related extensions.
4. **Foundation.API** exposes HTTP endpoints, security, observability, and middleware configuration.

## Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Visual Studio 2022+](https://visualstudio.microsoft.com/) / Visual Studio Code with C# extensions

## Solution structure
```
src/Foundation.Domain         // Core entity models (Employee)
src/Foundation.Application    // Application layer services, interfaces, DTOs
src/Foundation.Infrastructure // EF Core DbContext, repositories, persistence helpers
src/Foundation.API            // Minimal API, authentication, observability, health checks, metrics
```

## Getting Started
1. Clone the repository.
2. Navigate to the `src` directory.
3. Restore dependencies and build:
   ```bash
dotnet restore Foundation.sln
dotnet build Foundation.sln
```
4. Run the API project (from `src/Foundation.API`):
   ```bash
dotnet run --project Foundation.API/Foundation.API.csproj
```

## Configuration
### appsettings.json (required)
Create an `appsettings.json` file alongside `Program.cs` with relevant sections. A minimal example:
```json
{
  "Authentication": {
    "AzureAd": {
      "Authority": "https://login.microsoftonline.com/<TENANT_ID>/v2.0",
      "Audience": "api://<CLIENT_ID>"
    }
  },
  "Observability": {
    "ApplicationInsights": {
      "ConnectionString": "InstrumentationKey=<KEY>;IngestionEndpoint=https://<endpoint>/",
      "InstrumentationKey": "<KEY>"
    }
  },
  "ConnectionStrings": {
    "Default": "Server=.;Database=Foundation;Trusted_Connection=True;"
  }
}
```
Adjust values based on your environment.

## API Endpoints
| Method | Route | Description | Authorization |
| ------ | ----- | ----------- | ------------- |
| GET | `/health/live` | Liveness probe | Public |
| GET | `/health/ready` | Readiness probe | Public (requires DB connectivity) |
| GET | `/health` | Health check results (JSON) | Public |
| GET | `/metrics` | Prometheus metrics placeholder | Public |
| GET | `/employees` | List employees | Requires `Employee` role |
| GET | `/employees/{id}` | Get employee by ID | Requires `Leadership` role |
| POST | `/employees` | Create/update employee (placeholder) | Requires `Admin` role |

## Services & Persistence
- `Foundation.Infrastructure.Data.FoundationDbContext` uses EF Core to manage `Employee` entities.
- `Foundation.Application.Services.EmployeeService` orchestrates repository operations and maps to `EmployeeDto`.

## Observability & Security
- Application Insights telemetry is enabled via configuration values.
- JWT Bearer authentication relies on Azure AD authority and audience.
- Rate limiting is enabled globally (100 tokens per minute).
- CORS is restricted to `https://app.corp.com` (adjust as needed).

## Contributing
1. Create a branch from `main`.
2. Follow existing naming conventions.
3. Run `dotnet test` (if tests exist) and `dotnet build` before submitting a PR.
4. Submit a PR describing your changes.

## Notes
- Extend configuration for database provider (e.g., SQL Server connection string) and authentication as required.
- Implement additional endpoints/services/repositories in corresponding layers following the existing minimal API style.
