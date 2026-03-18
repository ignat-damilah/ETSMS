# Foundation Project Documentation

## Overview
Foundation is a modular solution structured with separate projects for API, Application, Domain, and Infrastructure concerns. The API acts as the entry point, wiring together authentication, observability, database services, and HTTP endpoints. Infrastructure services manage persistence and shared concerns, while the application layer exposes services such as employee management consumed by the API. The domain layer defines core entities, such as `Employee`.

## Project Structure
- `Foundation.API`: Hosts the ASP.NET Core Web API, configures middleware, authentication, rate limiting, and endpoints.
- `Foundation.Application`: Implements business services used by the API endpoints.
- `Foundation.Domain`: Contains domain entities and models (e.g., `Employee`).
- `Foundation.Infrastructure`: Provides data access, dependency injection extensions, and supporting infrastructure services.

## API and Middleware Configuration
The `Program.cs` file defines the following key behaviors:

### Observability
- Application Insights telemetry is configured using values from `Observability:ApplicationInsights`.

### Database & Services
- Infrastructure and application services are registered via extension methods that consume configuration.

### Authentication & Authorization
- JWT Bearer authentication with Azure AD configuration (`Authentication:AzureAd`) is in place.
- Claims-based role evaluation is enforced with `Employee`, `Leadership`, and `Admin` policies.

### CORS Policy
- Default policy allows `https://app.corp.com` with any headers/methods and credentials, enabling secure client-side consumption.

### Rate Limiting
- A fixed token bucket limiter named `Global` allows 100 tokens per minute with no queueing to protect the API from excessive requests.

## Endpoints
- `GET /health/live`: Basic liveness check returning a healthy status.
- `GET /health/ready`: Readiness check that verifies database connectivity using the `FoundationDbContext`.
- `GET /health`: Aggregated health checks report with JSON response summarizing registered checks.
- `GET /metrics`: Placeholder metrics endpoint.
- `GET /employees`: Requires `Employee` role and lists all employees using the application service.
- `GET /employees/{id}`: Requires `Leadership` role and returns a single employee or 404 if not found.
- `POST /employees`: Requires `Admin` role; currently accepts an employee payload and immediately returns `202 Accepted`.

## Running and Configuration Notes
1. Ensure configuration files (e.g., `appsettings.json`) contain the required `Observability`, `Authentication`, and other required sections.
2. Application Insights key and connection string should be supplied through configuration or environment variables.
3. The database readiness check uses `FoundationDbContext`, so the database must be reachable when starting the API.
4. JWT tokens issued by Azure AD must include the correct audience and roles (`Employee`, `Leadership`, `Admin`) for authorization to succeed.
5. Rate limiting ensures that only 100 requests per minute are allowed per the global policy, preventing abuse.

## Deployment & Observability Recommendations
- Deploy the API behind a gateway that terminates TLS and forwards authenticated user tokens.
- Monitor Application Insights for telemetry, dependency calls, and failure alerts.
- Consider extending the `/metrics` endpoint with Prometheus-compatible data for richer metrics.
- Ensure health check endpoints are consumed by orchestrators for liveliness and readiness monitoring.
