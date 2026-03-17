# Foundation Project Overview

## Purpose
The Foundation solution serves as a core enterprise API platform powering internal tooling across human resources, leadership reporting, and HR automation. It is built with modern .NET technologies and emphasizes security, observability, and reliability so that internal developers can quickly iterate on business-critical workflows.

## Core Components
- **Foundation.API**: Hosts the HTTP endpoints for employee management, health checks, metrics, and authentication. It wires up infrastructure, authentication, rate limiting, observability, and policy-driven authorization.
- **Foundation.Application**: Contains business logic, service abstractions, and DTO/mapping concerns. Teams implement feature behavior against application service interfaces to keep controllers thin.
- **Foundation.Infrastructure**: Provides data access, database contexts, and integrations to backing services. Infrastructure services are exposed via dependency injection for consumption by the API.
- **Foundation.Domain**: Defines the domain entities and value objects. It is intentionally lightweight to stay focused on business rules and models rather than implementation details.

## Primary Engineering Workflows
1. **Feature Development**
   - Implement new APIs by extending `Foundation.API` endpoints and corresponding services in `Foundation.Application`.
   - Share data models through the `Foundation.Domain` layer to keep contracts consistent.
   - Inject infrastructure dependencies like database contexts via `Foundation.Infrastructure` services.
   - Register new services and settings with the DI container in `Program.cs` and configuration files.

2. **Security/Compliance Workflows**
   - Configure Azure AD authentication and authorization policies in `Program.cs` to protect endpoints.
   - Ensure observability via Application Insights and Prometheus-compatible metrics.
   - Maintain rate limiting and CORS policies centrally in `Program.cs` for consistent enforcement.

3. **Operational Readiness**
   - Health and readiness probes (`/health`, `/health/live`, `/health/ready`) are exposed through the API to facilitate platform monitoring.
   - Use built-in logging, custom health check payloads, and metrics endpoints for diagnostics.

## Contextual Diagram (Textual)
```
[Foundation.API]
    |-- Depends on Foundation.Application services
    |-- Configures middleware: AuthN/AuthZ, CORS, Rate Limiting, Observability

[Foundation.Application]
    |-- Contains service interfaces/implementations
    |-- Relies on Foundation.Infrastructure for persistence

[Foundation.Infrastructure]
    |-- Hosts EF Core DbContext and repositories
    |-- Provides integrations required by application services

[Foundation.Domain]
    |-- Defines shared entity contracts used across layers
```

Refer to the setup guide for developer onboarding steps to run the solution locally and iterate quickly on these workflows.
