# API Layer Documentation

The `Foundation.API` project hosts the entry point for the web application (`Program.cs`) and performs all web-specific configuration.

## Key Responsibilities

1. **Observability**
   - Adds Application Insights telemetry with instrumentation keys and connection strings pulled from configuration under `Observability:ApplicationInsights`.

2. **Database & Services Wiring**
   - Calls `AddInfrastructureServices` and `AddApplicationServices` which register the EF Core context, repositories, and domain services.

3. **Authentication & Authorization**
   - Uses Azure AD JWT bearer tokens with audience/authority configured under `Authentication:AzureAd`.
   - Policies defined for roles:
     - `EmployeePolicy` requires role `Employee`.
     - `LeadershipPolicy` requires role `Leadership`.
     - `AdminPolicy` requires role `Admin`.

4. **CORS**
   - Default policy allowing `https://app.corp.com` to make cross-origin calls with credentials, headers, and all methods.

5. **Rate Limiting**
   - A global token bucket limiter named `Fixed` restricts requests to 100 tokens per minute with no queueing.

6. **Middleware Pipeline**
   - Applies rate limiting, CORS, authentication, and authorization middleware in sequence.

## Endpoints

- `GET /health/live` – Returns a simple `status: "Healthy"` response.
- `GET /health/ready` – Checks database connectivity via `FoundationDbContext.Database.CanConnect()`.
- `GET /health` – Health check endpoint (all registered checks) producing JSON payload with statuses.
- `GET /metrics` – Placeholder metrics endpoint returning a basic text response (Prometheus support is referenced via imports).
- `GET /employees` – Requires `EmployeePolicy`, returns all employees from the application service.
- `GET /employees/{id}` – Requires `LeadershipPolicy`, returns a single employee or `404`.
- `POST /employees` – Requires `AdminPolicy`, currently accepts a payload and calls `GetAsync` before returning `202 Accepted` (pending implementation details).

Additional API-specific extensions live under `Foundation.API.Endpoints` and `Foundation.API.Extensions`.
