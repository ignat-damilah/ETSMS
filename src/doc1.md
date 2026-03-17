# API Layer Overview

This layer contains the minimal host setup and endpoint wiring for the Foundation API. `Program.cs` configures the application pipeline and registers middleware:

- **Configuration**: Adds `appsettings.json` and reads sections for observability and authentication.
- **Observability**: Application Insights telemetry is enabled with configurable connection settings.
- **Services**: Infrastructure and application dependencies are registered via extension methods.
- **Security**: JWT Bearer authentication is configured with Azure AD settings, and authorization policies (Employee, Leadership, Admin) map to roles.
- **Cross-Origin**: A CORS policy is defined for `https://app.corp.com` to allow credentials.
- **Resilience**: A global fixed-rate token bucket limiter wraps the pipeline.
- **Endpoints**:
  - `/health/live` and `/health/ready` for liveness/readiness probes.
  - `/health` for aggregated health checks.
  - `/metrics` for custom telemetry.
  - `/employees` (Employee policy) lists employees.
  - `/employees/{id}` (Leadership policy) reads an employee.
  - `/employees` POST (Admin policy) accepts a new employee resource (currently returns `Accepted`).

This file is effectively the entry point that ties the entire application together and orchestrates dependencies, middleware, and routes.
