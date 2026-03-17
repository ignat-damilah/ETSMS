# Foundation.API Documentation

This project configures the web hosting surface, authentication, observability, and HTTP endpoints for the Foundation service.

## Observability
- Reads Application Insights settings from configuration (`Observability:ApplicationInsights`).
- Registers Application Insights telemetry, enabling tracing and telemetry export.

## Infrastructure and Application Integration
- Calls `AddInfrastructureServices` to configure the database (PostgreSQL via `FoundationDbContext`) and repository registrations.
- Calls `AddApplicationServices` to wire application-level services such as `IEmployeeService`.

## Security
- Configures JWT Bearer authentication using Azure AD settings (`Authentication:AzureAd` section).
- Defines authorization policies for `Employee`, `Leadership`, and `Admin` roles.

## Middleware
- Applies rate limiting via a token bucket limiter (100 tokens per minute, no queuing).
- Enables CORS for `https://app.corp.com` with headers, methods, and credentials allowed.
- Adds request authentication and authorization.

## Endpoints
- `/health/live` and `/health/ready` for liveness and readiness probes.
- `/health` for aggregated health check results.
- `/metrics` placeholder endpoint.
- `/employees` (GET, EmployeePolicy) returns all employees.
- `/employees/{id}` (GET, LeadershipPolicy) returns a specific employee or 404.
- `/employees` (POST, AdminPolicy) accepts an employee payload, triggers a lookup, and returns 202 Accepted.
