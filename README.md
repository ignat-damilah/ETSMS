# Foundation Solution

This repository contains the Foundation suite made up of multiple .NET projects that implement a foundational set of APIs and services.

## Projects
- **Foundation.API**: ASP.NET Core Web API exposing employee endpoints, health checks, authentication, and observability.
- **Foundation.Application**: Application layer that defines DTOs, interfaces, and service abstractions.
- **Foundation.Infrastructure**: Infrastructure layer that wires up persistence, Entity Framework Core, and database migrations.
- **Foundation.Domain**: Domain entities shared across the layers.

## Getting Started
1. Ensure you have [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) installed.
2. Restore dependencies and build the solution:
   ```bash
   dotnet restore Foundation.sln
   dotnet build Foundation.sln
   ```
3. Run the API locally:
   ```bash
   dotnet run --project src/Foundation.API/Foundation.API.csproj
   ```

## Authentication & Authorization
The API uses Azure AD JWT Bearer authentication and defines policies for Employee, Leadership, and Admin roles. Configure the settings under `appsettings.json`.

## Observability
- Application Insights telemetry is enabled and configured via the `Observability` section of the configuration.
- Prometheus metrics endpoint is available at `/metrics`.

## Health & Metrics
- Liveness check: `GET /health/live`
- Readiness check: `GET /health/ready`
- Aggregate health checks: `GET /health`
- Metrics endpoint: `GET /metrics`

## CORS & Rate Limiting
- CORS is restricted to `https://app.corp.com` and allows all headers and methods with credentials.
- A token bucket rate limiter is registered with a global limit of 100 tokens per minute.

## Development Notes
- The Infrastructure and Application layers are wired through DI and referenced by the API.
- Extend the service implementations and repositories within the Application and Infrastructure layers as needed.
